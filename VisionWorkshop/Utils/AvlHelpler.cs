using AvlNet;
using Gocator;
namespace VisionWorkshop
{
    public class AvlHelpler
    {
        public EdgeModel EdgeModel { get; set; }
        public string EdgeModelParam { get; set; }


        public  Surface ZValueToSurface(ushort[] zValue, GocatorContext gocatorContext)
        {
            Surface outSurface = new Surface(gocatorContext.Width, gocatorContext.Height, zValue);
            outSurface.SetOffsetAndScales(gocatorContext.XOffset, gocatorContext.XResolution, gocatorContext.YOffset, gocatorContext.YResolution, gocatorContext.ZOffset, gocatorContext.ZResolution);
            return outSurface;
        }

        public void MacroParamsLoader(string path)
        {
            
        }
    }
}
