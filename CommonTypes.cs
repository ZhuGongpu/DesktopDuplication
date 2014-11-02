using System.IO;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace DXGI_DesktopDuplication
{
    public class FrameData
    {
        public int DirtyCount;
        public Rectangle[] DirtyRectangles;
        //TODO public Bitmap FrameBitmap;
        public Texture2D Frame;
        public OutputDuplicateFrameInformation FrameInfo;
        public int MoveCount;
        public OutputDuplicateMoveRectangle[] MoveRectangles;

        public void WriteToStream(Stream stream)
        {
            //TODO 
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public class Vertex
    {
        public float Pos_X;
        public float Pos_Y;
        public float Pos_Z;

        public float TexCoord_X;
        public float TexCoord_Y;

        public void SetPos(float x, float y, float z)
        {
            Pos_X = x;
            Pos_Y = y;
            Pos_Z = z;
        }

        public void SetTexCoord(float x, float y)
        {
            TexCoord_X = x;
            TexCoord_Y = y;
        }
    }
}