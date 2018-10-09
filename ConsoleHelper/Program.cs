using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gocator;
using HalconDotNet;
using VisionUtils.ImageHelper;
namespace ConsoleHelper
{
    class Program
    {
        static GocatorDevice gocatorDevice = new GocatorDevice("127.0.0.1", 32);
        static void Main(string[] args)
        {
            gocatorDevice.DeviceStatusEvent += GocatorDevice_DeviceStatusEvent;
            gocatorDevice.OnDataReceivedEvent += GocatorDevice_OnDataReceivedEvent;
            gocatorDevice.InitialAcq();
            gocatorDevice.StartAcq();
            Console.ReadLine();
        }
        private static void GocatorDevice_OnDataReceivedEvent(object sender, object e)
        {
            List<ushort[]> data = (List<ushort[]>)e;
            var obj = new Object();
            //Image<Gray, UInt16> image = EmguToWpfImage.ZValuesToDepthPng(data[0], gocatorDevice.mContext);
            Parallel.For(0, data.Count, (index) => {

                lock (obj)
                {
                    HImage image = HalconToWpf.ZValueToDepthImage(data[index], gocatorDevice.mContextTop);
                    image.WriteImage("png", 0, $@"C:\ImageData\Bottom\bottom_{index}.png");

                }

            });

            Console.WriteLine("Done");
        }
        private static void GocatorDevice_DeviceStatusEvent(object sender, object e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}
