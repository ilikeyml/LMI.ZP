using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Gocator;

namespace DemoUI.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {

            StartCommand = new RelayCommand(StartAction);
            gocatorDevice = new GocatorDevice(IPAddress, BufferSize);
            gocatorDevice.DeviceStatusEvent += GocatorDevice_DeviceStatusEvent;
            gocatorDevice.OnDataReceivedEvent += GocatorDevice_OnDataReceivedEvent;
            gocatorDevice.InitialAcq();

        }

        private void GocatorDevice_OnDataReceivedEvent(object sender, object e)
        {
            
        }

        private void GocatorDevice_DeviceStatusEvent(object sender, object e)
        {
            InfoMsg += $"{e.ToString()}  {Environment.NewLine}";
        }

        GocatorDevice gocatorDevice;

        public string IPAddress { get; set; } = "127.0.0.1";
        public int BufferSize { get; set; } = 4;
        public string InfoMsg { get; set; }
        public ICommand StartCommand { get; set; }


        private async void StartAction()
        {
            await Task.Run(() => { gocatorDevice.StartAcq(); });      
        }

    }
}
