using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace DXGI_DesktopDuplication
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DuplicationManager duplicationManager = null;


        public static UpdateUI RefreshUI;

        public MainWindow()
        {
            InitializeComponent();

            RefreshUI = UpdateImage;

            //test code here
            Console.WriteLine("{0}, {1}", SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
            Console.WriteLine("{0}, {1}", SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);

            Console.WriteLine(Marshal.SizeOf(typeof (Vertex)));

            duplicationManager = DuplicationManager.GetInstance(Dispatcher);

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
            Console.WriteLine("Click");
            //CaptureFrame();//TODO 已知bug：只有写成CaptureFrame时不会抛异常
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);


        public void UpdateImage(Bitmap bitmap)
        {
            IntPtr pointer = bitmap.GetHbitmap();

            Image.Source = Imaging.CreateBitmapSourceFromHBitmap(pointer, IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(pointer);
        }
    }
}