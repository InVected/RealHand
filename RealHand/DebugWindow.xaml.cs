
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;





namespace RealHand
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {

        public void UpdateImages(HandLandmark[] LandmarkList, int[,] jointStructure, string debugString, IntPtr depthData, int depthStride, int depthWidth, int depthHeight,
                  IntPtr colorData, int colorStride, int colorWidth, int colorHeight)
        {

            Action<HandLandmark[], int[,], IntPtr, int, int, int, IntPtr, int, int, int> updateImage = new Action<HandLandmark[], int[,], IntPtr, int, int, int, IntPtr, int, int, int>((LandmarkList, jointStructure, depthData, depthStride, depthWidth, depthHeight, colorData, colorStride, colorWidth, colorHeight) =>
            {
                Trace.WriteLine("In UpdateImagesPoints");
                var depthwbmp = imgDepth.Source as WriteableBitmap;
                var depthrect = new Int32Rect(0, 0, depthWidth, depthHeight);
                depthwbmp.WritePixels(depthrect, depthData, depthStride * depthHeight, depthStride);

                var colorwbmp = imgColor.Source as WriteableBitmap;
                var colorrect = new Int32Rect(0, 0, colorWidth, colorHeight);
                colorwbmp.WritePixels(colorrect, colorData, colorStride * colorHeight, colorStride);
                 if (LandmarkList != null && LandmarkList.Length != 0) {
                    float max = LandmarkList[0].Z;
                    float min = LandmarkList[0].Z;
                    foreach (HandLandmark landmark in LandmarkList)
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

                    for (int i = 0; i < jointStructure.GetLength(0); i++)
                     {

                         HandLandmark landmark1 = LandmarkList[jointStructure[i, 0]];
                         HandLandmark landmark2 = LandmarkList[jointStructure[i, 1]];

                         if (landmark1.Visibility || landmark2.Visibility) {
                            Vector3 colorFront = new Vector3(255, 225, 0);
                            Vector3 colorBack = new Vector3(139,0,139);
                            var proportionalDepth1 = (landmark1.Z - min) / (max - min);
                            var proportionalDepth2 = (landmark2.Z - min) / (max - min);
                            var color1V = proportionalDepth1 * colorBack + (1 - proportionalDepth1) * colorFront;
                            var color2V = proportionalDepth2 * colorBack + (1 - proportionalDepth2) * colorFront;
                            Color color1 = new Color();
                            color1.R = (byte)Math.Round(color1V.X);
                            color1.G = (byte)Math.Round(color1V.Y);
                            color1.B = (byte)Math.Round(color1V.Z);
                            Color color2 = new Color();
                            color2.R = (byte)Math.Round(color2V.X);
                            color2.G = (byte)Math.Round(color2V.Y);
                            color2.B = (byte)Math.Round(color2V.Z);

                            // landmark1, 2 Z for Colors.Orange, Colors.Blue
                            DrawLine(colorwbmp, landmark1.X, landmark1.Y, landmark2.X, landmark2.Y, 20, color1, color2);
                         }
                     }
                     foreach (HandLandmark landmark in LandmarkList)
                     {
                         if (landmark.Visibility) {
                             DrawPoint(colorwbmp, landmark.X, landmark.Y, 15, Colors.Green);
                         }
                         if (!landmark.Visibility)
                         {
                             DrawPoint(colorwbmp, landmark.X, landmark.Y, 15, Colors.Red);
                         }
                     } 
                 }

            });
            
            Dispatcher.Invoke(DispatcherPriority.Render, updateImage, LandmarkList, jointStructure,depthData, depthStride, depthWidth, depthHeight, colorData, colorStride, colorWidth, colorHeight);
            /* Dispatcher.Invoke(new Action(() =>
             {
                 debugInfo.Text = debugString;
             }));
            */
        }


        public void update()
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
                using (var context = bmp.GetBitmapContext())
                {
                    RGB* pixelPointer = (RGB*)context.Pixels;
                    return pixelPointer;
                }
            }
        }
        private void DrawPixel(int x, int y, Color color)
        {
            byte RColor = color.R;
            byte GColor = color.G;
            byte BColor = color.B;
            var bmp = imgColor.Source as WriteableBitmap;
            unsafe
            {
                using (var context = bmp.GetBitmapContext())
                {

                    byte* pixels = (byte*)context.Pixels;
                    var w = context.Width;
                    var h = context.Height;

                    var pixelStartByte = (x + y * w) * 3;
                    pixels[pixelStartByte] = RColor;
                    pixels[pixelStartByte + 1] = GColor;
                    pixels[pixelStartByte + 2] = BColor;
                }
            }
        }
        private void DrawPoint(WriteableBitmap bmp, int xc, int yc, int r, Color color)
        {
            byte RColor = color.R;
            byte GColor = color.G;
            byte BColor = color.B;
            Trace.WriteLine(xc + " " + yc);
            unsafe
            {
                // Use refs for faster access (really important!) speeds up a lot!
                using (var context = bmp.GetBitmapContext())
                {

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
        }
        private void DrawLine(WriteableBitmap bmp, int xa, int ya, int xb, int yb, int thickness, Color color, Color color2)
        {




            

            var alphaAngle = -Math.Atan2(yb - ya, xb - xa);
            var distance = Math.Sqrt((xb - xa) * (xb - xa) + (yb - ya) * (yb - ya));
            float cosAlpha = (float)Math.Cos(alphaAngle);
            float sinAlpha = (float)Math.Sin(alphaAngle);
            float minusSinAlpha = sinAlpha * -1.0F;

            unsafe
            {
                // Use refs for faster access (really important!) speeds up a lot!
                using (var context = bmp.GetBitmapContext())
                {
                    byte* pixels = (byte*)context.Pixels;
                    var w = context.Width;
                    var h = context.Height;

                    for (float xCord = 0.0F; xCord <= distance; xCord = xCord + 0.5F)
                    {

                        float proc = xCord / (float)distance;
                        Color res = color2 * proc + color * (1.0f - proc);
                        byte RColor = res.R;
                        byte GColor = res.G;
                        byte BColor = res.B;
                        for (float yCord = -thickness * 0.5F; yCord <= thickness * 0.5F; yCord = yCord + 0.5F)
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
}
