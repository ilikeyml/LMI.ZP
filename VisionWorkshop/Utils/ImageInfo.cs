using System.Drawing;
using AvlNet;
namespace VisionWorkshop
{
    public class ImageInfo
    {
        public ImageInfo()
        {

        }
        public ImageInfo(AvlNet.Image image, string  info)
        {
            Image = image;
            Info = info;
        }
        public AvlNet.Image Image { get; set; }
        public string Info { get; set; }


    }
}
