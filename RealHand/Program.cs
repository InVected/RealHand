using Intel.RealSense;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using System.Numerics;

namespace RealHand
{
    class Program
    {


        


        [STAThread]
        static void Main(string[] args)
        {
            List<HandLandmark> handData = new List<HandLandmark>();
            System.Diagnostics.Process process = new System.Diagnostics.Process();//System Process Object
            Colorizer colorizer = new Colorizer();
            Console.WriteLine("output UpdateImages");
            bool DebugMode = true;
            DebugWindow debugWindow = null;
            var mediaPipeHand = new MediaPipeHand();
            var realHand = new RealHand(mediaPipeHand.Structure);


            // Create and config the pipeline to strem color and depth frames.
            Pipeline pipeline = new Pipeline();

                var ctx = new Context();
                var devices = ctx.QueryDevices();
                var dev = devices[0];

                Console.WriteLine("\nUsing device 0, an {0}", dev.Info[CameraInfo.Name]);
                Console.WriteLine("    Serial number: {0}", dev.Info[CameraInfo.SerialNumber]);
                Console.WriteLine("    Firmware version: {0}", dev.Info[CameraInfo.FirmwareVersion]);

                var sensors = dev.QuerySensors();
                var depthSensor = sensors[0];
                var colorSensor = sensors[1];

                var depthProfile = depthSensor.StreamProfiles
                                    .Where(p => p.Stream == Stream.Depth)
                                    .OrderBy(p => p.Framerate)
                                    .Select(p => p.As<VideoStreamProfile>()).First();

                var colorProfile = colorSensor.StreamProfiles
                                    .Where(p => p.Stream == Stream.Color)
                                    .OrderBy(p => p.Framerate)
                                    .Select(p => p.As<VideoStreamProfile>()).First();

                var cfg = new Config();
                cfg.EnableStream(Stream.Depth, depthProfile.Width, depthProfile.Height, depthProfile.Format, 30);
                cfg.EnableStream(Stream.Color, colorProfile.Width, colorProfile.Height, colorProfile.Format, 30);


            var server = new NamedPipeServerStream("NPtest");

            Console.WriteLine("Waiting for connection...");
            //======================================================
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "python.exe";
            startInfo.Arguments = "C:\\dev\\RealHand\\python\\main.py";
            process.StartInfo = startInfo;
            process.Start();
            //======================================================
            server.WaitForConnection();

            Console.WriteLine("Connected.");
            var br = new System.IO.BinaryReader(server);
            var bw = new System.IO.BinaryWriter(server);
            var pp = pipeline.Start(cfg);
               
                if (DebugMode)
                {
                    debugWindow = new DebugWindow(depthProfile.Width, depthProfile.Height, colorProfile.Width, colorProfile.Height);
                debugWindow.Show();
                }


                    while (true)
                    {

                // We wait for the next available FrameSet and using it as a releaser object that would track
                // all newly allocated .NET frames, and ensure deterministic finalization
                // at the end of scope. 
                using (var frames = pipeline.WaitForFrames())
                {
                    VideoFrame colorFrame = frames.ColorFrame.DisposeWith(frames); //------------------- getting the frames
                    DepthFrame depthFrame = frames.DepthFrame.DisposeWith(frames);//--------------------


                    var colorizedDepth = colorizer.Process<VideoFrame>(depthFrame).DisposeWith(frames);
                    //=====================================
                    byte[] byteArray = new byte[colorFrame.DataSize];
                    Marshal.Copy(colorFrame.Data, byteArray, 0, colorFrame.DataSize);
                    IntPtr dataTest = colorFrame.Data;
                    string str = "Test";
                    byte[] buf = byteArray;
                    bw.Write((uint)colorFrame.Width);
                    bw.Write((uint)colorFrame.Height);

                    // Write string length
                    bw.Write(buf);                              // Write string
                    Console.WriteLine("Wrote: \"{0}\"", str);


                    int len = (int)br.ReadUInt32();           // Read string length
                    string debugString = "";
                    if (len != 0)
                    {
                        str = new string(br.ReadChars(len));    // Read string

                        
                        mediaPipeHand.setJsonData(str);
                        var elData = mediaPipeHand.Data;
                        debugString = "X:" + elData[8].X.ToString() + "Y:" + elData[8].Y.ToString() + "Z:" + elData[8].Z.ToString();

                        realHand.calculateWithDepth(mediaPipeHand.Data, depthFrame, colorFrame);
                     


                    }

                            //=====================================
                            if (DebugMode)
                            {
                                debugWindow.UpdateImages(debugString, colorizedDepth.Data, colorizedDepth.Stride, colorizedDepth.Width, colorizedDepth.Height, colorFrame.Data, colorFrame.Stride, colorProfile.Width, colorProfile.Height);
                            }

                        }
                    }
            Console.WriteLine("Client disconnected.");
            server.Close();
            server.Dispose();





        }
    }
    public class HandLandmark
    {
        //public string Name { get; set; }
        public int ID { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public bool Visibility { get; set; }
        public HandLandmark(int id, float x, float y, float z)
        {
            ID = id;
            X = x;
            Y = y;
            Z = z;
            Visibility = true;

        }

    }
    public class MediaPipeHand
    {
        //public string Name { get; set; }
        public List<HandLandmark> Data { get; set; }
        public int[,] Structure { get; set; }

        public MediaPipeHand()
        {
           
            Structure = new int[,]
            {
                    {0, 1},
                    {1, 2},
                    {2, 3},
                    {3, 4},
                    {0, 5},
                    {5, 6},
                    {6, 7},
                    {7, 8},
                    {5, 9},
                    {9, 10},
                    {10, 11},
                    {11, 12},
                    {9, 13},
                    {13, 14},
                    {14, 15},
                    {15, 16},
                    {13, 17},
                    {0, 17},
                    {17, 18},
                    {18, 19},
                    {19, 20},

            };
            

        }
        public void setJsonData(string json)
        {
            dynamic jsonData = JsonConvert.DeserializeObject(json);

            List<HandLandmark> handData = new List<HandLandmark>();
            foreach (var landmark in jsonData) // creates landmark Objects and stores them in a List
            {

                int landmarkID = landmark.GetValue("id").ToObject<int>();
                float landmarkX = landmark.GetValue("x").ToObject<float>();
                float landmarkY = landmark.GetValue("y").ToObject<float>();
                float landMarkZ = landmark.GetValue("z").ToObject<float>();

                HandLandmark handLandmark = new HandLandmark(landmarkID, landmarkX, landmarkY, landMarkZ);
                handData.Add(handLandmark);
            }
            Data = handData;
        }
        public void setData(List<HandLandmark> data)
        {
            Data = data;
        }
        public int getJointCount()
        {
            return Structure.Length;
        }

    }
    public class RealHand
    {
        public int[,] Structure;
        public List<List<int>> ReStructure;
        public List<HandLandmark> Data { get;}
        public RealHand(int[,] structure)
        {
            Structure = structure;
            List<List<int>> tempList = new List<List<int>>();
            for (int i = 0; i > structure.Length; i++)
            {
                List<int> innerList = new List<int>();
                for (int j = 0; j > structure.Length; j++)
                {
                    if (structure[j,0] == i)
                    {
                        innerList.Add(structure[i, 1]);
                    }
                    if (structure[j,1] == i)
                    {
                        innerList.Add(structure[i, 0]);
                    }

                }
                tempList.Add(innerList);
            }
            ReStructure = tempList;
            

        }
        public void calculateWithDepth(List<HandLandmark> landmarkList, DepthFrame depthFrame, VideoFrame colorFrame)
        {

            List<HandLandmark> SortedLandmarkList = landmarkList.OrderBy(landmark => landmark.Z).ToList();
            SortedLandmarkList = BorderControle(SortedLandmarkList, colorFrame);
            List<HandLandmark> VisibleLandmarkList = OcclusionControle(SortedLandmarkList);
            foreach(HandLandmark handLandmark in VisibleLandmarkList)
            {
                if (handLandmark.Visibility)
                {
                    //get Real coordinates
                }
            }
            // calculate formula to interpolate Occludet landmarks
            foreach(HandLandmark handLandmark in VisibleLandmarkList)
            {
                if (!handLandmark.Visibility)
                {
                    //calculate Real coordinates with formular
                }
            }

        }
        public bool hasData()
        {
            if (Data != null) return false;
            else return true;
        }
        private List<HandLandmark> BorderControle(List<HandLandmark> SortedLandmarkList, VideoFrame colorFrame)
        {

            foreach (HandLandmark handLandmark in SortedLandmarkList)
            {
                if (handLandmark.Visibility)
                {
                    if (handLandmark.X < 0|| handLandmark.X  >= colorFrame.Width || handLandmark.Y < 0 || handLandmark.Y >= colorFrame.Height)
                    {
                        handLandmark.Visibility = false;
                    }

                }

            }
            return SortedLandmarkList;
        }
        private List<HandLandmark> OcclusionControle(List<HandLandmark> SortedLandmarkList)
        {
            Double factor = 0.5;
            for (int k = 0; k > SortedLandmarkList.Count; k++)
            {
                var breaker = false;
                HandLandmark backPoint = SortedLandmarkList[k];
                for(int m = k+1; m > SortedLandmarkList.Count; m++)
                {
                    HandLandmark frontPoint = SortedLandmarkList[m];
                    Double distance = Math.Sqrt(Math.Pow(backPoint.X-frontPoint.X,2)+Math.Pow(backPoint.Y-frontPoint.Y,2));
                    if (distance <= factor)
                    {
                        SortedLandmarkList[k].Visibility = false;
                        break;
                    }
                    foreach (int joint in ReStructure[m])
                    {
                        HandLandmark jointPoint = SortedLandmarkList[joint];
                        Vector2 pointJoint = new Vector2(jointPoint.X, jointPoint.Y);
                        Vector2 pointFront = new Vector2(frontPoint.X, frontPoint.Y);
                        Vector2 pointBack = new Vector2(backPoint.X, backPoint.Y);
                        Vector2 jointLine = Vector2.Subtract(pointJoint, pointFront);
                        Vector2 ortogonal = new Vector2(jointLine.Y, -jointLine.X);
                        Vector2 einheitsOrtogonal = Vector2.Normalize(ortogonal);
                        Vector2 u = Vector2.Subtract(pointFront, pointBack);
                        var reziprokeDeterminante = 1.0 / (jointLine.X * einheitsOrtogonal.Y - jointLine.Y * einheitsOrtogonal.X);
                        var lambda =  reziprokeDeterminante * (u.X*einheitsOrtogonal.Y-u.Y*einheitsOrtogonal.X); // relativ position of intersection on jointLine
                        var delta = reziprokeDeterminante * (u.Y*einheitsOrtogonal.X-u.X*einheitsOrtogonal.Y); //distance point to jointLine
                        if(lambda <= 1 && lambda >= 0 && delta >= factor && delta <= -factor)
                        {
                            SortedLandmarkList[k].Visibility = false;
                            breaker = true;
                            break;
                        }

                    }
                    if (breaker) break;
                }
            }
                return SortedLandmarkList;
        }

    }
}
