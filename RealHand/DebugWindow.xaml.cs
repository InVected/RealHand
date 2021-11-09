
using System;
using System.Collections.Generic;
using System.Linq;
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
        private static Action EmptyDelegate = delegate () { };
        public void UpdateImages(string debugString, IntPtr depthData, int depthStride, int depthWidth, int depthHeight,
                          IntPtr colorData, int colorStride, int colorWidth, int colorHeight)
        {
            
            Action<IntPtr, int, int, int, IntPtr, int, int, int> updateImage = new Action< IntPtr, int, int, int, IntPtr, int, int, int>((depthData, depthStride, depthWidth, depthHeight, colorData, colorStride, colorWidth, colorHeight) =>
            {
               
                var depthwbmp = imgDepth.Source as WriteableBitmap;
                var depthrect = new Int32Rect(0, 0, depthWidth, depthHeight);
                depthwbmp.WritePixels(depthrect, depthData, depthStride * depthHeight, depthStride);

                var colorwbmp = imgColor.Source as WriteableBitmap;
                var colorrect = new Int32Rect(0, 0, colorWidth, colorHeight);
                colorwbmp.WritePixels(colorrect, colorData, colorStride * colorHeight, colorStride);
            });









            Dispatcher.Invoke(DispatcherPriority.Render, updateImage, depthData, depthStride, depthWidth, depthHeight,colorData, colorStride, colorWidth, colorHeight);
            Dispatcher.Invoke(new Action(() =>
            {
                debugInfo.Text = debugString;
            }));
        }

        public DebugWindow(int depthWidth, int depthHeight, int colorWidth, int colorHeight)
        {
            InitializeComponent();
            imgDepth.Source = new WriteableBitmap(depthWidth, depthHeight, 96d, 96d, PixelFormats.Rgb24, null);

            imgColor.Source = new WriteableBitmap(colorWidth, colorHeight, 96d, 96d, PixelFormats.Rgb24, null);

        }


    }
}
