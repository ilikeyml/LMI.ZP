using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DemoUI
{
    class ProcessImageViewModel : BaseViewModel
    {
        public ProcessImageViewModel()
        {
            for (int i = 0; i < 10; i++)
            {
                ImageQueueData item = new ImageQueueData(new BitmapImage(new Uri(@"pack://application:,,,/resources/logo.png")), "image");
                ImageItems.Add(item);
            }


        }

        public List<ImageQueueData> ImageItems { get; set; } = new List<ImageQueueData>();

        public class ImageQueueData
        {
            public ImageQueueData(ImageSource source,  string tips)
            {
                ImageItem = source;
                ImageTips = tips;
            }
            public ImageSource ImageItem { get; set; }
            public string ImageTips { get; set; }

        }
    }
}
