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
        public ushort[] TopData { get; set; }
        public ushort[] BottomData { get; set; }
        public SurfaceType SurfaceType { get; set; } = SurfaceType.Top;
        public TopBottomSurface(SurfaceType surfaceType)
        {
            SurfaceType = surfaceType;
        }
    }
}
