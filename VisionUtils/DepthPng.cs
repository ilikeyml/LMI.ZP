using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Gocator;
using System;
using System.Drawing;

namespace VisionUtils
{
    public class DepthPng
    {
        public static unsafe Image<Gray, ushort> ZValuesToDepthPng(ushort[] zValues, GocatorContext mContext )
        {
            int width = mContext.Width;
            int height = mContext.Height;
            int[] size = new int[] { height, width };
            fixed (ushort* p = zValues)
            {
                Mat depthPng = new Mat(size, DepthType.Cv16U, (IntPtr)p, null);
                return depthPng.ToImage<Gray,ushort>();
            }
        }
    }
}
