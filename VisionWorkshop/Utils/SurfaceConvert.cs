using AvlNet;

namespace VisionWorkshop
{
    public class SurfaceConvert
    {
        public static void ScaleSurface(ref Surface inSurface, Gocator.GocatorContext mContext)
        {
            inSurface.SetOffsetAndScales(mContext.XOffset, mContext.XResolution, mContext.YOffset, mContext.YResolution, mContext.ZOffset, mContext.ZResolution);
        }
    }
}
