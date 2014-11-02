using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Rectangle = SharpDX.Rectangle;
using Resource = SharpDX.DXGI.Resource;
using ResultCode = SharpDX.DXGI.ResultCode;

namespace DXGI_DesktopDuplication
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DuplicationManager duplicationManager = DuplicationManager.getInstance();


        public MainWindow()
        {
            InitializeComponent();

            //test code here
            Console.WriteLine("{0}, {1}", SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
            Console.WriteLine("{0}, {1}", SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);

            Console.WriteLine(Marshal.SizeOf(typeof (Vertex)));

            CaptureFrame();
        }


        public static void UpdateBimtapWithFrameData(ref Bitmap sourceBitmap, FrameData data)
        {
            Graphics graphics = Graphics.FromImage(sourceBitmap);
            //TODO
        }


        public void CaptureFrame()
        {
            FrameData frameData;

            duplicationManager.GetFrame(out frameData);
            // duplicationManager.GetChangedRects(ref frameData); //TODO pending
        }


        public void CapturedChangedRects()
        {
            FrameData data = null;
            duplicationManager.GetChangedRects(ref data);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //TODO test code here
            CapturedChangedRects();
        }
    }

}