using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Rectangle = System.Drawing.Rectangle;
using Resource = SharpDX.DXGI.Resource;
using ResultCode = SharpDX.DXGI.ResultCode;

namespace DXGI_DesktopDuplication
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Texture2D acquiredDesktopImage = null;

        public MainWindow()
        {
            InitializeComponent();

            //todo test code here
            //Capture();
            FrameData data;
            new DuplicationManager().GetFrame(out  data);

            //Debug.WriteLine("size = {0}", Marshal.SizeOf(typeof(OutputDuplicateMoveRectangle)));
        }

        public void Capture()
        {
            // # of graphics card adapter
            const int numAdapter = 0;

            // # of output device (i.e. monitor)
            const int numOutput = 0;

            const string outputFileName = "ScreenCapture.bmp";

            // Create DXGI Factory1
            var factory = new Factory1();
            Adapter1 adapter = factory.GetAdapter1(numAdapter);

            // Create device from Adapter
            var device = new Device(adapter);

            // Get DXGI.Output
            Output output = adapter.GetOutput(numOutput);
            var output1 = output.QueryInterface<Output1>();

            // Width/Height of desktop to capture
            int width = output.Description.DesktopBounds.Width;
            int height = output.Description.DesktopBounds.Height;

            // Create Staging texture CPU-accessible
            var textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = {Count = 1, Quality = 0},
                Usage = ResourceUsage.Staging
            };
            var screenTexture = new Texture2D(device, textureDesc);

            // Duplicate the output
            OutputDuplication duplicatedOutput = output1.DuplicateOutput(device);

            bool captureDone = false;

            for (int i = 0; !captureDone; i++)
            {
                try
                {
                    Resource screenResource;
                    OutputDuplicateFrameInformation duplicateFrameInformation;

                    // Try to get duplicated frame within given time
                    duplicatedOutput.AcquireNextFrame(10000, out duplicateFrameInformation, out screenResource);
                    //get new frame

                    //TODO if still holding old frame, destroy it


                    //TODO QI for IDXGIResource


                    //get metadata
                    if (duplicateFrameInformation.TotalMetadataBufferSize > 0)
                    {
                        //TODO old buffer size too small


                        //TODO get move rectangles
                        int bufferSize = duplicateFrameInformation.TotalMetadataBufferSize;
                        // duplicatedOutput.GetFrameMoveRects();
                    }


                    if (i > 0)
                    {
                        // copy resource into memory that can be accessed by the CPU
                        using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                            device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);

                        // Get the desktop capture texture
                        DataBox mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read,
                            MapFlags.None);

                        // Create Drawing.Bitmap
                        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                        var boundsRect = new Rectangle(0, 0, width, height);

                        // Copy pixels from screen capture Texture to GDI bitmap
                        BitmapData mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                        IntPtr sourcePtr = mapSource.DataPointer;
                        IntPtr destPtr = mapDest.Scan0;
                        for (int y = 0; y < height; y++)
                        {
                            // Copy a single line 
                            Utilities.CopyMemory(destPtr, sourcePtr, width*4);

                            // Advance pointers
                            sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                            destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                        }

                        // Release source and dest locks
                        bitmap.UnlockBits(mapDest);
                        device.ImmediateContext.UnmapSubresource(screenTexture, 0);

                        // Save the output
                        bitmap.Save(outputFileName);

                        // Capture done
                        captureDone = true;
                    }

                    screenResource.Dispose();
                    duplicatedOutput.ReleaseFrame();
                }
                catch (SharpDXException e)
                {
                    if (e.ResultCode.Code != ResultCode.WaitTimeout.Result.Code)
                    {
                        throw e;
                    }
                }
            }

            // Display the texture using system associated viewer
            Process.Start(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, outputFileName)));

            // TODO: We should cleanp up all allocated COM objects here
        }
    }

    public struct FrameData
    {
        public OutputDuplicateMoveRectangle[] MoveRectangles;
        public SharpDX.Rectangle[] DirtyRectangles;
        public Texture2D Frame;
        public OutputDuplicateFrameInformation FrameInfo;
        
    }

    //[Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c")]
    public class DuplicationManager
    {
        private Device device;
        private OutputDuplication duplicatedOutput;
        private int height;
        private Texture2DDescription textureDesc;
        private int width;
        //private Texture2D acquiredDesktopImage = null;

        public DuplicationManager()
        {
            Init();
        }

        private void Init()
        {
            // # of graphics card adapter
            const int numAdapter = 0;

            // # of output device (i.e. monitor)
            const int numOutput = 0;

            // Create DXGI Factory1
            var factory = new Factory1();
            Adapter1 adapter = factory.GetAdapter1(numAdapter);

            // Create device from Adapter
            device = new Device(adapter);

            // Get DXGI.Output
            Output output = adapter.GetOutput(numOutput);
            var output1 = output.QueryInterface<Output1>();

            // Width/Height of desktop to capture
            width = output.Description.DesktopBounds.Width;
            height = output.Description.DesktopBounds.Height;

            // Create Staging texture CPU-accessible
            textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = {Count = 1, Quality = 0},
                Usage = ResourceUsage.Staging
            };


            // Duplicate the output
            duplicatedOutput = output1.DuplicateOutput(device);
        }

        public void GetFrame
            (out FrameData data)
        {
            data = new FrameData();
            bool captureDone = false;
            for (int i = 0; !captureDone; i++)
                try
                {
                    var screenTexture = new Texture2D(device, textureDesc);

                    Resource screenResource;
                    OutputDuplicateFrameInformation duplicateFrameInformation;

                    // Try to get duplicated frame within given time
                    duplicatedOutput.AcquireNextFrame(10000, out duplicateFrameInformation, out screenResource);

                    

                    if (i > 0)
                    {
                        // copy resource into memory that can be accessed by the CPU
                        using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                            device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);

                        //TODO
                        Debug.WriteLine("START------");
                        #region new code added here

                        int bufSize = duplicateFrameInformation.TotalMetadataBufferSize;
                        var moveRectangles =
                            new OutputDuplicateMoveRectangle[
                                bufSize/Marshal.SizeOf(typeof (OutputDuplicateMoveRectangle))];
                        //get move rects
                        duplicatedOutput.GetFrameMoveRects(bufSize, moveRectangles, out bufSize);
                        data.MoveRectangles = moveRectangles;

                        bufSize = duplicateFrameInformation.TotalMetadataBufferSize - bufSize;
                        var dirtyRectangles = new SharpDX.Rectangle[bufSize / Marshal.SizeOf(typeof(Rectangle))];
                        //get dirty rects
                        duplicatedOutput.GetFrameDirtyRects(bufSize, dirtyRectangles, out bufSize);
                        data.DirtyRectangles = dirtyRectangles;

                        data.Frame = screenTexture;
                        data.FrameInfo = duplicateFrameInformation;
                        
                        #endregion
                        Debug.WriteLine("DONE ----------");
                        //TODO
                        
                        // Get the desktop capture texture
                        DataBox mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read,
                            MapFlags.None);

                        // Create Drawing.Bitmap
                        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                        var boundsRect = new Rectangle(0, 0, width, height);

                        // Copy pixels from screen capture Texture to GDI bitmap
                        BitmapData mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                        IntPtr sourcePtr = mapSource.DataPointer;
                        IntPtr destPtr = mapDest.Scan0;
                        for (int y = 0; y < height; y++)
                        {
                            // Copy a single line 
                            Utilities.CopyMemory(destPtr, sourcePtr, width*4);

                            // Advance pointers
                            sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                            destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                        }

                        // Release source and dest locks
                        bitmap.UnlockBits(mapDest);
                        device.ImmediateContext.UnmapSubresource(screenTexture, 0);

                        // Save the output
                        bitmap.Save("save.bmp");

                        // Capture done
                        captureDone = true;
                    }


                    screenTexture.Dispose();

                    screenResource.Dispose();
                    duplicatedOutput.ReleaseFrame();
                }
                catch (SharpDXException e)
                {
                    if (e.ResultCode.Code != ResultCode.WaitTimeout.Result.Code)
                    {
                        throw e;
                    }
                }

            // Display the texture using system associated viewer
            //Process.Start(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "save.bmp")));
        }
    }
}