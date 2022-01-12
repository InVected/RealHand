using Intel.RealSense;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Threading;


namespace RealHand
{
    class Program
    {
        static bool DebugMode = true;
        [STAThread]
        static void Main(string[] args)
        {
            var completeWatch = new System.Diagnostics.Stopwatch();
            bool firstRound = true;
            completeWatch.Start();
            
            connectionData startupData = StartProgram(); //initialisation of Camera, Pipe and WPF UI
            //var unityPipe = new NamedPipeServerStream("unityPipe"); // Pipe to Unity
            //unityPipe.WaitForConnection(); // connects Pipe to Unity

            Trace.WriteLine("Connected Unity");
            //var unityWriter = new System.IO.BinaryWriter(unityPipe);
            while (true)
            {
                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                RelativHandLandmark[] outputData = loadProgram(startupData); //generating Data for 
                if (outputData is not null) {

                    string[] jsonArray = new string[21];
                  
                    for (int i= 0;i< outputData.Length; i++)
                {
                        jsonArray[i] = JsonConvert.SerializeObject(outputData[i]);

                    }
                    string jsonArrayJ = JsonConvert.SerializeObject(jsonArray); ;
                    //Trace.WriteLine(jsonArrayJ);
                    //unityWriter.Write(jsonArrayJ);  //sends Data to unity
                }
                watch.Stop();
                if (firstRound)
                {
                    completeWatch.Stop();
                    Trace.WriteLine($"First Round with Startup Time: {completeWatch.ElapsedMilliseconds} ms");
                    firstRound = false;
                }
                Trace.WriteLine($"Latest Round Time: {watch.ElapsedMilliseconds} ms");
                

            }
        }



        static connectionData StartProgram()
        {
            
            List<RelativHandLandmark> handData = new();
            System.Diagnostics.Process process = new();//System Process Object
            
            Trace.WriteLine("output UpdateImages");

            // Create and config the pipeline to strem color and depth frames.
            Pipeline pipeline = new();

            // --------- initialisation RealSense Cameras --------------v
            var ctx = new Context();
            var devices = ctx.QueryDevices();
            var dev = devices[0];

            Trace.WriteLine("\nUsing device 0, an {0}", dev.Info[CameraInfo.Name]);
            Trace.WriteLine("\nUSB-Type: {0}", dev.Info[CameraInfo.UsbTypeDescriptor]);
            Trace.WriteLine("\nSerial number: {0}", dev.Info[CameraInfo.SerialNumber]);
            Trace.WriteLine("\nFirmware version: {0}", dev.Info[CameraInfo.FirmwareVersion]);

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
            // --------- initialisation RealSense Cameras --------------^

            //-----------------------------------------
            var server = new NamedPipeServerStream("NPtest");

            Trace.WriteLine("Waiting for connection...");
            //======================================================v Python
            System.Diagnostics.ProcessStartInfo startInfo = new();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "python.exe";
            startInfo.Arguments = "C:\\dev\\RealHand\\python\\main.py";
            process.StartInfo = startInfo;
            process.Start();
            //======================================================^ Python
            server.WaitForConnection();

            Trace.WriteLine("Connected.");
            var br = new System.IO.BinaryReader(server);
            var bw = new System.IO.BinaryWriter(server);
            var pp = pipeline.Start(cfg);
            Intrinsics intrinsics = pp.GetStream(Stream.Color).As<VideoStreamProfile>().GetIntrinsics(); // intrinsics of Colorframe

            DebugWindow debugWindow = null;
            Thread windowThread; 
            bool testBool = false;
            
            if (Program.DebugMode)
            {
                windowThread = new Thread(() =>
                {
                    debugWindow = new DebugWindow(colorProfile.Width, colorProfile.Height, colorProfile.Width, colorProfile.Height);
                    testBool = true;
                    debugWindow.ShowDialog();
                });
                windowThread.SetApartmentState(ApartmentState.STA);
                windowThread.IsBackground = true;
                windowThread.Start();
            }
            while (!testBool) ;

            connectionData cData = new();
            cData.br = br;
            cData.bw = bw;
            cData.pipeline = pipeline;
            cData.intrinsics = intrinsics;
            cData.debugWindow = debugWindow;
            cData.colorProfile = colorProfile;
            return cData;
            
            
            Trace.WriteLine("Client disconnected.");
            server.Close();
            server.Dispose();
            


        }
        static RelativHandLandmark[] loadProgram(connectionData startupData) {
            System.IO.BinaryReader br = startupData.br;
            System.IO.BinaryWriter bw = startupData.bw;
            Pipeline pipeline = startupData.pipeline;
            Intrinsics intrinsics = startupData.intrinsics;
            DebugWindow debugWindow = startupData.debugWindow;
            VideoStreamProfile colorProfile = startupData.colorProfile;

            
            MediaPipeHand mediaPipeHand = new();  
            TransformToReal realHand = new(mediaPipeHand.Structure, mediaPipeHand.GetJointCount()); 
            Colorizer colorizer = new();  


            using var frames = pipeline.WaitForFrames();
            Align align = new Align(Stream.Color).DisposeWith(frames); 

            
            Intel.RealSense.Frame aligned = align.Process(frames).DisposeWith(frames);
            FrameSet alignedFrameset = aligned.As<FrameSet>().DisposeWith(frames);
            VideoFrame colorFrame = alignedFrameset.ColorFrame.DisposeWith(alignedFrameset); //------------------- getting the frames
            DepthFrame alignedDepthFrame = alignedFrameset.DepthFrame.DisposeWith(alignedFrameset);//--------------------
            DepthFrame depthFrame = frames.DepthFrame.DisposeWith(frames);
            var colorizedDepth = colorizer.Process<VideoFrame>(alignedDepthFrame).DisposeWith(alignedFrameset);
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
            mediaPipeHand.SetData(null);
            RelativHandLandmark[] onlyVisisibleList = null;
            RelativHandLandmark[] realHandOutput = null;

            if (len != 0)
            {
                str = new string(br.ReadChars(len));    // Read string

                mediaPipeHand.SetJsonData(str);
                var elData = mediaPipeHand.Data;
                onlyVisisibleList = realHand.GetVisibleList(mediaPipeHand.Data, colorFrame);
                realHandOutput = realHand.CalculateWithDepth(onlyVisisibleList, depthFrame, colorFrame, intrinsics); // OOOOOOOOOOOOOO ready for Output OOOOOOOOOOOOOOO
                
            }
            unsafe
            {

                int[,] jointStructure = mediaPipeHand.Structure;
                if (Program.DebugMode)
                {
                   
                    debugWindow.UpdateImages(onlyVisisibleList, jointStructure, colorizedDepth.Data, colorizedDepth.Stride, colorizedDepth.Width, colorizedDepth.Height, colorFrame.Data, colorFrame.Stride, colorProfile.Width, colorProfile.Height);


                }
            }
            return realHandOutput; 
        }
        public struct connectionData
        {
            public System.IO.BinaryReader br;
            public System.IO.BinaryWriter bw;
            public Pipeline pipeline;
            public Intrinsics intrinsics;
            public DebugWindow debugWindow;
            public VideoStreamProfile colorProfile;
        }
    }

}