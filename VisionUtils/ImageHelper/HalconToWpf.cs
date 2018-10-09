using System;
using HalconDotNet;
using Gocator;
namespace VisionUtils.ImageHelper
{
    public class HalconToWpf
    {
        public  static unsafe HImage ZValueToDepthImage(ushort[] zValue, GocatorContext context)
        {
            HImage image = new HImage();
            fixed (ushort* p = zValue)
            {
                try
                {
                    image.GenImage1("uint2", context.Width, context.Height, (IntPtr)p);
                }
                catch (HalconException ee)
                {
                    throw;
                }
            }
            return image;
        }
    }
}
