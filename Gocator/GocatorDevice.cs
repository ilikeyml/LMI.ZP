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
            IPAddr = ipAddr;
            BufferSize = bufferSize;
            doDataWorker.DoWork += DoDataWorker_DoWork;
            doDataWorker.RunWorkerCompleted += DoDataWorker_RunWorkerCompleted;
            doDataWorker.ProgressChanged += DoDataWorker_ProgressChanged;
        }
        #endregion


        private void DoDataWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
           
        }

        private void DoDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnDataReceivedEvent?.Invoke(this, mResult);
            mRawDataList.Clear();
            mResult.Clear();
        }

        private void DoDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ResolveRawDataList(mRawDataList);
        }


        #region Implement IDevice interface
        public bool InitialAcq()
        {
            KApiLib.Construct();
            GoSdkLib.Construct();
            mSystem = new GoSystem();
            mSensor = mSystem.FindSensorByIpAddress(KIpAddress.Parse(IPAddr));
            DeviceStatusEvent?.Invoke(this, $"Find device @ IP address {IPAddr}");
            mSensor.Connect();
            mSensor.EnableData(true);
            mSensor.SetDataHandler(OnData);
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
            });
            StopAcq();
            DeviceStatusEvent?.Invoke(this, $"Finished / Stop Acq/ Return Result");
            return mResult;        
        }

        private void ResolveRawData(KObject kData)
        {
            ushort[] data = new ushort[] { 1, 2, 3 };
            mResult.Add(data);
            DeviceStatusEvent?.Invoke(this, $"Finished {100*mResult.Count/BufferSize*1.0} %");
        }

        public bool ReleaseAcq()
        {
            this.DeviceStatusEvent = null;
            this.OnDataReceivedEvent = null;
            return true;
        }

        public bool StartAcq()
        {
            if (mSensor.State != GoState.Ready)
            {
                mSensor.Stop();
            }
            mSensor.Start();
            DeviceStatusEvent?.Invoke(this, $"Start Acq");
            return true;
        }

        public bool StopAcq()
        {
            mSensor.Stop();
            DeviceStatusEvent?.Invoke(this, $"Stop Acq");
            return true;
        }
        #endregion
    }
}
