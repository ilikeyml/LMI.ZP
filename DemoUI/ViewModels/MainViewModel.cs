using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using Gocator;
using VisionUtils;
namespace DemoUI
{
    class MainViewModel : BaseViewModel
    {
        #region Private members
        /// <summary>
        /// sensor object
        /// </summary>
        GocatorDevice gocatorDevice;
        /// <summary>
        /// UI sync context
        /// </summary>
        SynchronizationContext curContext;
        /// <summary>
        /// Storage all image to be display
        /// </summary>
        List<ImageSource> imageQueue = new List<ImageSource>();

        #endregion
        #region ctor
        public MainViewModel()
        {

            curContext = SynchronizationContext.Current;
            StartCommand = new RelayCommand(StartAction);
            ImageProcessCommand = new RelayCommand(ShowProcessImageView);
           
            gocatorDevice = new GocatorDevice(IPAddress, BufferSize);
            gocatorDevice.DeviceStatusEvent += GocatorDevice_DeviceStatusEvent;
            gocatorDevice.OnDataReceivedEvent += GocatorDevice_OnDataReceivedEvent;
            gocatorDevice.InitialAcq();
        }
        #endregion
        #region device event
        private void GocatorDevice_OnDataReceivedEvent(object sender, object e)
        {
            List<ushort[]> result = new List<ushort[]>();
            result = (List<ushort[]>)e;
            curContext.Post(new SendOrPostCallback((state) =>
            {
                if (!IsColorMapChecked)
                {
                    Image<Gray, ushort> image = EmguToWpfImage.ZValuesToDepthPng(result[1], gocatorDevice.mContext);
                    DisplayImage = EmguToWpfImage.ToBitmapSource(image);
                }
                else
                {
                    Image<Rgb, byte> image = EmguToWpfImage.ZValueToColorMap(result[1], gocatorDevice.mContext);
                    DisplayImage = EmguToWpfImage.ToBitmapSource(image);
                }

            }), null);
        }
        private void GocatorDevice_DeviceStatusEvent(object sender, object e)
        {
            InfoMsg += $"{e.ToString()}  {Environment.NewLine}";
        }
        #endregion
        #region prop
        public string IPAddress { get; set; } = "127.0.0.1";
        public int BufferSize { get; set; } = 16;
        public string InfoMsg { get; set; }
        public ImageSource DisplayImage { get; set; }
        public ICommand StartCommand { get; set; }
        public ICommand ImageProcessCommand { get; set; }
        public ICommand ColorMapCommand { get; set; }

        public bool IsColorMapChecked { get; set; }

        #endregion
        #region Command methods
        private async void StartAction()
        {
            await Task.Run(() => { gocatorDevice.StartAcq(); });
        }

        private void ShowProcessImageView()
        {
            new ProcessImageView().ShowDialog();
        }


        #endregion
    }
}
