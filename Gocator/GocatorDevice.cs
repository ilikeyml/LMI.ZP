using Lmi3d.GoSdk;
using Lmi3d.Zen;
using Lmi3d.Zen.Io;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Gocator
{
    public class GocatorDevice : IDevice
    {
        #region private menber
        GoSystem mSystem;
        GoSensor mSensor;
        List<KObject> mRawDataList = new List<KObject>();
        List<ushort[]> mResult = new List<ushort[]>();
        #endregion

        #region prop
        public string  IPAddr { get; set; }
        public int BufferSize { get; set; }
        #endregion

        #region event
        public event EventHandler<object> DeviceStatusEvent;
        public event EventHandler<object> OnDataReceivedEvent;
        #endregion

        #region backgroundWorker
        BackgroundWorker doDataWorker = new BackgroundWorker();
        #endregion

        #region ctor
        public GocatorDevice(string ipAddr, int bufferSize)
        {
            KApiLib.Construct();
            GoSdkLib.Construct();
            IPAddr = ipAddr;
            BufferSize = bufferSize;
            doDataWorker.DoWork += DoDataWorker_DoWork;
            doDataWorker.RunWorkerCompleted += DoDataWorker_RunWorkerCompleted;
            doDataWorker.ProgressChanged += DoDataWorker_ProgressChanged;
        }
        #endregion


        private void DoDataWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DeviceStatusEvent?.Invoke(this, $"Finished {e.ProgressPercentage} %");
        }

        private void DoDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnDataReceivedEvent?.Invoke(this, mResult);
        }

        private void DoDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ResolveRawDataList(mRawDataList);
        }


        #region Implement IDevice interface
        public bool InitialAcq()
        {
            mSystem = new GoSystem();
            mSensor = mSystem.FindSensorByIpAddress(KIpAddress.Parse(IPAddr));
            DeviceStatusEvent?.Invoke(this, $"Find device @ IP address {IPAddr}");
            mSensor.Connect();
            mSystem.EnableData(true);
            mSystem.SetDataHandler(OnData);
            return true;
        }

        private  void OnData(KObject data)
        {
            mRawDataList.Add(data);
            if (mRawDataList.Count == BufferSize)
            {
                doDataWorker.RunWorkerAsync();
            }
        }

        private List<ushort[]> ResolveRawDataList(List<KObject> mRawDataList)
        {
            Parallel.ForEach<KObject>(mRawDataList, (kData) => {
                ResolveRawData(kData);
                doDataWorker.ReportProgress((int)(mResult.Count / BufferSize * 1.0) * 100);
            });
            return mResult;        
        }

        private void ResolveRawData(KObject kData)
        {
            ushort[] data = new ushort[] { 1, 2, 3 };
            mResult.Add(data);
        }

        public bool ReleaseAcq()
        {
            throw new NotImplementedException();
        }

        public bool StartAcq()
        {
            return true;
        }

        public bool StopAcq()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
