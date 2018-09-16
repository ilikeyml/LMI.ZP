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
namespace DemoUI.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        #region Private members
        GocatorDevice gocatorDevice;
        SynchronizationContext curContext;
        #endregion
        #region ctor
        public MainViewModel()
        {
            curContext = SynchronizationContext.Current;
            StartCommand = new RelayCommand(StartAction);
            gocatorDevice = new GocatorDevice(IPAddress, BufferSize);
            gocatorDevice.DeviceStatusEvent += GocatorDevice_DeviceStatusEvent;
            gocatorDevice.OnDataReceivedEvent += GocatorDevice_OnDataReceivedEvent;
            gocatorDevice.InitialAcq();
        }
        #endregion
        #region device event
        private void GocatorDevice_OnDataReceivedEvent(object sender, object e)
        {
            List<ushort[]> result = (List<ushort[]>)e;
            for (int i = 0; i < result.Count; i++)
            {
                Image<Gray, ushort> image = EmguToWpfImage.ZValuesToDepthPng(result[0], gocatorDevice.mContext);
                curContext.Post(new SendOrPostCallback((o) =>
                {
                    DisplayImage = EmguToWpfImage.ToBitmapSource(image);
                }), null);
            }
        }
        private void GocatorDevice_DeviceStatusEvent(object sender, object e)
        {
            InfoMsg += $"{e.ToString()}  {Environment.NewLine}";
        }
        #endregion
        #region prop
        public string IPAddress { get; set; } = "127.0.0.1";
        public int BufferSize { get; set; } = 4;
        public string InfoMsg { get; set; }
        public ImageSource DisplayImage { get; set; }
        public ICommand StartCommand { get; set; }
        #endregion
        #region Command methods
        private async void StartAction()
        {
            await Task.Run(() => { gocatorDevice.StartAcq(); });
        }
        #endregion
    }
}
