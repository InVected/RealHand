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
using System.Diagnostics;
using System.Windows.Media;
using System.Threading;

namespace RealHand
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {



 

            
            bool DebugMode = true;
            List<HandLandmark> handData = new List<HandLandmark>();
            System.Diagnostics.Process process = new System.Diagnostics.Process();//System Process Object
            Colorizer colorizer = new Colorizer();
            Trace.WriteLine("output UpdateImages");
            
            //DebugWindow debugWindow = null;
            var mediaPipeHand = new MediaPipeHand();
            var realHand = new RealHandmark(mediaPipeHand.Structure, mediaPipeHand.getJointCount());

            // Create and config the pipeline to strem color and depth frames.
            Pipeline pipeline = new Pipeline();

            var ctx = new Context();
            var devices = ctx.QueryDevices();
            var dev = devices[0];

            Trace.WriteLine("\nUsing device 0, an {0}", dev.Info[CameraInfo.Name]);
            Trace.WriteLine("    Serial number: {0}", dev.Info[CameraInfo.SerialNumber]);
            Trace.WriteLine("    Firmware version: {0}", dev.Info[CameraInfo.FirmwareVersion]);

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

            Trace.WriteLine("Waiting for connection...");
            //======================================================
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "python.exe";
            startInfo.Arguments = "C:\\dev\\RealHand\\python\\main.py";
            process.StartInfo = startInfo;
            process.Start();
            //======================================================
            server.WaitForConnection();

            Trace.WriteLine("Connected.");
            var br = new System.IO.BinaryReader(server);
            var bw = new System.IO.BinaryWriter(server);
            var pp = pipeline.Start(cfg);

            DebugWindow debugWindow = null;

            if (DebugMode)
            {
                var windowThread = new Thread(() =>
                {

                    debugWindow = new DebugWindow(depthProfile.Width, depthProfile.Height, colorProfile.Width, colorProfile.Height);
                    debugWindow.ShowDialog();
                });
                windowThread.SetApartmentState(ApartmentState.STA);
                windowThread.IsBackground = true;
                windowThread.Start();
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
                    string str;
                    byte[] buf = byteArray;
                    bw.Write((uint)colorFrame.Width);
                    bw.Write((uint)colorFrame.Height);

                    // Write string length
                    bw.Write(buf);                              // Write string

                    int len = (int)br.ReadUInt32();           // Read string length
                    string debugString = "";
                    mediaPipeHand.setData(null);
                    if (len != 0)
                    {
                        str = new string(br.ReadChars(len));    // Read string

                        mediaPipeHand.setJsonData(str);
                        var elData = mediaPipeHand.Data;
                        debugString = "X:" + elData[8].X.ToString() + "\n" + "Y:" + elData[8].Y.ToString() + "\n" + "Z:" + elData[8].Z.ToString() + "\n";
                    }
                    unsafe
                    {
                        HandLandmark[] onlyVisisibleList = realHand.getVisibleList(mediaPipeHand.Data, colorFrame, debugWindow); //change to not only
                        realHand.calculateWithDepth(mediaPipeHand.Data, depthFrame, colorFrame, debugWindow);


                        int[,] jointStructure = mediaPipeHand.Structure;

                        if (DebugMode)
                        {
                            debugWindow.UpdateImages(onlyVisisibleList, jointStructure, debugString, colorizedDepth.Data, colorizedDepth.Stride, colorizedDepth.Width, colorizedDepth.Height, colorFrame.Data, colorFrame.Stride, colorProfile.Width, colorProfile.Height);
                            

                        }


                    }

                }
            }
            Trace.WriteLine("Client disconnected.");
            server.Close();
            server.Dispose();

            
        }


    }

}
