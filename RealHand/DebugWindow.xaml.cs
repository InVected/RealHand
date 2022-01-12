
using System;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace RealHand
{
    
    public partial class DebugWindow : Window
    {

        public void UpdateImages(RelativHandLandmark[] LandmarkList, int[,] jointStructure, IntPtr depthData, int depthStride, int depthWidth, int depthHeight,
                  IntPtr colorData, int colorStride, int colorWidth, int colorHeight)
        {

            Action<RelativHandLandmark[], int[,], IntPtr, int, int, int, IntPtr, int, int, int> updateImage = new((LandmarkList, jointStructure, depthData, depthStride, depthWidth, depthHeight, colorData, colorStride, colorWidth, colorHeight) =>
           {
               
               var depthwbmp = imgDepth.Source as WriteableBitmap;
               var depthrect = new Int32Rect(0, 0, depthWidth, depthHeight);
               depthwbmp.WritePixels(depthrect, depthData, depthStride * depthHeight, depthStride);

               var colorwbmp = imgColor.Source as WriteableBitmap;
               var colorrect = new Int32Rect(0, 0, colorWidth, colorHeight);
               colorwbmp.WritePixels(colorrect, colorData, colorStride * colorHeight, colorStride);
               if (LandmarkList != null && LandmarkList.Length != 0) // Hand Detected
               {
                   float max = LandmarkList[0].Z;
                   float min = LandmarkList[0].Z;
                   foreach (RelativHandLandmark landmark in LandmarkList) // getting max and min
                   {
                       if (landmark.Z > max)
                       {
                           max = landmark.Z;
                       }

                       if (landmark.Z < min)
                       {
                           min = landmark.Z;
                       }
                   }

                   for (int i = 0; i < jointStructure.GetLength(0); i++) // Drawing Landmark Joints
                   {

                       RelativHandLandmark landmark1 = LandmarkList[jointStructure[i, 0]];
                       RelativHandLandmark landmark2 = LandmarkList[jointStructure[i, 1]];

                       if (landmark1.Visibility || landmark2.Visibility)
                       {
                           Vector3 colorFront = new(255, 225, 0);
                           Vector3 colorBack = new(139, 0, 139);
                           var proportionalDepth1 = (landmark1.Z - min) / (max - min);
                           var proportionalDepth2 = (landmark2.Z - min) / (max - min);
                           var color1V = proportionalDepth1 * colorBack + (1 - proportionalDepth1) * colorFront;
                           var color2V = proportionalDepth2 * colorBack + (1 - proportionalDepth2) * colorFront;
                           Color color1 = new();
                           color1.R = (byte)Math.Round(color1V.X);
                           color1.G = (byte)Math.Round(color1V.Y);
                           color1.B = (byte)Math.Round(color1V.Z);
                           Color color2 = new();
                           color2.R = (byte)Math.Round(color2V.X);
                           color2.G = (byte)Math.Round(color2V.Y);
                           color2.B = (byte)Math.Round(color2V.Z);

                            // landmark1, 2 Z for Colors.Orange, Colors.Blue
                            DrawLine(colorwbmp, landmark1.X, landmark1.Y, landmark2.X, landmark2.Y, 20, color1, color2);
                       }
                   }
                   foreach (RelativHandLandmark landmark in LandmarkList)
                   {
                       if (landmark.Visibility)
                       {
                           DrawPoint(colorwbmp, landmark.X, landmark.Y, 15, Colors.Green);
                       }
                       if (!landmark.Visibility)
                       {
                           DrawPoint(colorwbmp, landmark.X, landmark.Y, 15, Colors.Red);
                       }
                   }
               }

           });

            Dispatcher.Invoke(DispatcherPriority.Render, updateImage, LandmarkList, jointStructure, depthData, depthStride, depthWidth, depthHeight, colorData, colorStride, colorWidth, colorHeight);
            // Displayed Datapoints
            Dispatcher.Invoke(new Action(() =>
                 {
                     if (LandmarkList != null && LandmarkList.Length != 0) { 
                         wrist.Text = "Visibility: " + LandmarkList[0].Visibility + "\tX: " + LandmarkList[0].X + "  Y: " + LandmarkList[0].Y + "    Z: " + LandmarkList[0].Z ;
                    thumb_CMC.Text = "Visibility: " + LandmarkList[1].Visibility + "\tX: " + LandmarkList[1].X + "  Y: " + LandmarkList[1].Y + "    Z: " + LandmarkList[1].Z ;
                     thumb_MCP.Text = "Visibility: " + LandmarkList[2].Visibility + "\tX: " + LandmarkList[2].X + "  Y: " + LandmarkList[2].Y + "    Z: " + LandmarkList[2].Z ;
                     thumb_IP.Text = "Visibility: " + LandmarkList[3].Visibility + "\tX: " + LandmarkList[3].X + "  Y: " + LandmarkList[3].Y + "    Z: " + LandmarkList[3].Z ;
                     thumb_TIP.Text = "Visibility: " + LandmarkList[4].Visibility + "\tX: " + LandmarkList[4].X + "  Y: " + LandmarkList[4].Y + "    Z: " + LandmarkList[4].Z ;
                     index_Finger_MCP.Text = "Visibility: " + LandmarkList[5].Visibility + "\tX: " + LandmarkList[5].X + "  Y: " + LandmarkList[5].Y + "    Z: " + LandmarkList[5].Z;
                     index_Finger_PIP.Text = "Visibility: " + LandmarkList[6].Visibility + "\tX: " + LandmarkList[6].X + "  Y: " + LandmarkList[6].Y + "    Z: " + LandmarkList[6].Z ;
                     index_Finger_DIP.Text = "Visibility: " + LandmarkList[7].Visibility + "\tX: " + LandmarkList[7].X + "  Y: " + LandmarkList[7].Y + "    Z: " + LandmarkList[7].Z ;
                     index_Finger_TIP.Text = "Visibility: " + LandmarkList[8].Visibility + "\tX: " + LandmarkList[8].X + "  Y: " + LandmarkList[8].Y + "    Z: " + LandmarkList[8].Z ;
                     middle_Finger_MCP.Text = "Visibility: " + LandmarkList[9].Visibility + "\tX: " + LandmarkList[9].X + "  Y: " + LandmarkList[9].Y + "    Z: " + LandmarkList[9].Z ;
                     middle_Finger_PIP.Text = "Visibility: " + LandmarkList[10].Visibility + "\tX: " + LandmarkList[10].X + "  Y: " + LandmarkList[10].Y + "    Z: " + LandmarkList[10].Z ;
                     middle_Finger_DIP.Text = "Visibility: " + LandmarkList[11].Visibility + "\tX: " + LandmarkList[11].X + "  Y: " + LandmarkList[11].Y + "    Z: " + LandmarkList[11].Z ;
                     middle_Finger_TIP.Text = "Visibility: " + LandmarkList[12].Visibility + "\tX: " + LandmarkList[12].X + "  Y: " + LandmarkList[12].Y + "    Z: " + LandmarkList[12].Z;
                     ring_Finger_MCP.Text = "Visibility: " + LandmarkList[13].Visibility + "\tX: " + LandmarkList[13].X + "  Y: " + LandmarkList[13].Y + "    Z: " + LandmarkList[13].Z ;
                     ring_Finger_PIP.Text = "Visibility: " + LandmarkList[14].Visibility + "\tX: " + LandmarkList[14].X + "  Y: " + LandmarkList[14].Y + "    Z: " + LandmarkList[14].Z;
                     ring_Finger_DIP.Text = "Visibility: " + LandmarkList[15].Visibility + "\tX: " + LandmarkList[15].X + "  Y: " + LandmarkList[15].Y + "    Z: " + LandmarkList[15].Z ;
                     ring_Finger_TIP.Text = "Visibility: " + LandmarkList[16].Visibility + "\tX: " + LandmarkList[16].X + "  Y: " + LandmarkList[16].Y + "    Z: " + LandmarkList[16].Z;
                     pinky_MCP.Text = "Visibility: " + LandmarkList[17].Visibility + "\tX: " + LandmarkList[17].X + "  Y: " + LandmarkList[17].Y + "    Z: " + LandmarkList[17].Z ;
                     pinky_PIP.Text = "Visibility: " + LandmarkList[18].Visibility + "\tX: " + LandmarkList[18].X + "  Y: " + LandmarkList[18].Y + "    Z: " + LandmarkList[18].Z ;
                     pinky_DIP.Text = "Visibility: " + LandmarkList[19].Visibility + "\tX: " + LandmarkList[19].X + "  Y: " + LandmarkList[19].Y + "    Z: " + LandmarkList[19].Z ;
                     pinky_TIP.Text = "Visibility: " + LandmarkList[20].Visibility + "\tX: " + LandmarkList[20].X + "  Y: " + LandmarkList[20].Y + "    Z: " + LandmarkList[20].Z ;
                     }

                 }));
                
        }

        public void Update()
        {

            Dispatcher.Invoke(InvalidateVisual, DispatcherPriority.Render);
        }

        public DebugWindow(int depthWidth, int depthHeight, int colorWidth, int colorHeight)
        {
            InitializeComponent();
            imgDepth.Source = new WriteableBitmap(depthWidth, depthHeight, 96d, 96d, PixelFormats.Rgb24, null);
            imgColor.Source = new WriteableBitmap(colorWidth, colorHeight, 96d, 96d, PixelFormats.Rgb24, null);
        }

        public struct RGB
        {
            public byte R;
            public byte G;
            public byte B;
        }
        
        public unsafe RGB* GetPixelPointer()
        {
            var bmp = imgColor.Source as WriteableBitmap;
            unsafe
            {
                using var context = bmp.GetBitmapContext();

                RGB* pixelPointer = (RGB*)context.Pixels;
                return pixelPointer;
            }
        }
        //Draw Single Pixel on Image
        private void DrawPixel(int x, int y, Color color)
        {
            byte RColor = color.R;
            byte GColor = color.G;
            byte BColor = color.B;
            var bmp = imgColor.Source as WriteableBitmap;
            unsafe
            {
                using var context = bmp.GetBitmapContext();


                byte* pixels = (byte*)context.Pixels;
                var w = context.Width;
                var h = context.Height;

                var pixelStartByte = (x + y * w) * 3;
                pixels[pixelStartByte] = RColor;
                pixels[pixelStartByte + 1] = GColor;
                pixels[pixelStartByte + 2] = BColor;
            }
        }
        //Draw Single Point on Image
        private static void DrawPoint(WriteableBitmap bmp, int xc, int yc, int r, Color color)
        {
            byte RColor = color.R;
            byte GColor = color.G;
            byte BColor = color.B;
            unsafe
            {

                using var context = bmp.GetBitmapContext();


                byte* pixels = (byte*)context.Pixels;
                var w = context.Width;
                var h = context.Height;

                var startPointX = xc - r;
                var startPointY = yc - r;
                var endPointX = xc + r;
                var endPointY = yc + r;
                for (int iX = startPointX; iX <= endPointX; iX++)
                {
                    for (int iY = startPointY; iY <= endPointY; iY++)
                    {
                        if (iX >= 0 && iY >= 0 && iX < w && iY < h)
                        {
                            var distance = (iX - xc) * (iX - xc) + (iY - yc) * (iY - yc);
                            if (distance <= r * r)
                            {
                                var pixelStartByte = (iX + iY * w) * 3;
                                pixels[pixelStartByte] = RColor;
                                pixels[pixelStartByte + 1] = GColor;
                                pixels[pixelStartByte + 2] = BColor;
                            }
                        }
                    }
                }
            }
        }
        //Draw Single Line on Image
        private static void DrawLine(WriteableBitmap bmp, int xa, int ya, int xb, int yb, int thickness, Color color, Color color2)
        {
            var alphaAngle = -Math.Atan2(yb - ya, xb - xa);
            var distance = Math.Sqrt((xb - xa) * (xb - xa) + (yb - ya) * (yb - ya));
            float cosAlpha = (float)Math.Cos(alphaAngle);
            float sinAlpha = (float)Math.Sin(alphaAngle);
            float minusSinAlpha = sinAlpha * -1.0F;

            unsafe
            {

                using var context = bmp.GetBitmapContext();

                byte* pixels = (byte*)context.Pixels;
                var w = context.Width;
                var h = context.Height;

                for (float xCord = 0.0F; xCord <= distance; xCord += 0.5F)
                {
                    float proc = xCord / (float)distance;
                    Color res = color2 * proc + color * (1.0f - proc);
                    byte RColor = res.R;
                    byte GColor = res.G;
                    byte BColor = res.B;
                    for (float yCord = -thickness * 0.5F; yCord <= thickness * 0.5F; yCord += 0.5F)
                    {

                        // Rotations Matrix [cos(), sin()]  --> x rotation
                        //[-sin(), cos()] --> y rotation
                        float xRotated = cosAlpha * xCord + sinAlpha * yCord;
                        float yRotated = minusSinAlpha * xCord + cosAlpha * yCord;
                        int y = (int)Math.Round(yRotated + ya, 0);
                        int x = (int)Math.Round(xRotated + xa, 0);

                        if (x >= 0 && y >= 0 && x < w && y < h)
                        {
                            var pixelStartByte = (x + y * w) * 3;
                            pixels[pixelStartByte] = RColor;
                            pixels[pixelStartByte + 1] = GColor;
                            pixels[pixelStartByte + 2] = BColor;
                        }
                    }
                }
            }
        }
    }
}
