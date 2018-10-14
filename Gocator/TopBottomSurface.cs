using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gocator
{
    public enum SurfaceType
    {
        Top,
        Bottom,
        TopBottom
    }
    public class TopBottomSurface
    {
        public ushort[] TopSurfaceData { get; set; }
        public ushort[] BottomSurfaceData { get; set; }
        public byte[] TopSurfaceIntensityData { get; set; }
        public byte[]BottomSurfaceIntensityData { get; set; }
        public SurfaceType SurfaceType { get; set; } = SurfaceType.Top;
        public TopBottomSurface(SurfaceType surfaceType)
        {
            SurfaceType = surfaceType;
        }
    }
}
