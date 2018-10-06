﻿using Lmi3d.GoSdk;
using Lmi3d.GoSdk.Messages;
using Lmi3d.Zen;
using Lmi3d.Zen.Io;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
        public string IPAddr { get; set; }
        public int BufferSize { get; set; }
        public GocatorContext mContext { get; private set; }
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
        }
        #endregion
        #region backgroundworker event handler
        private void DoDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnDataReceivedEvent?.Invoke(this, mResult);
            mRawDataList.Clear();
        }
        private void DoDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ResolveRawDataList(mRawDataList);
        }
        #endregion
        #region data resolve
        private void OnData(KObject data)
        {
            mRawDataList.Add(data);
            if (mRawDataList.Count == BufferSize)
            {
                mResult.Clear();
                doDataWorker.RunWorkerAsync();
            }
        }
        private List<ushort[]> ResolveRawDataList(List<KObject> mRawDataList)
        {
            #region for loop
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < mRawDataList.Count; i++)
            {
                mResult.Add(ResolveRawData(mRawDataList[i]));
            }
            Debug.WriteLine($"for loop consuming {sw.ElapsedMilliseconds}");
            #endregion
            #region parallel loop
            //Stopwatch sw = Stopwatch.StartNew();
            //Task.Run(() =>
            //{
            //    Parallel.For(0, BufferSize, (index) =>
            //    {
            //        mResult.Add(ResolveRawData(mRawDataList[index]));
            //    });
            //}).Wait();
            //Debug.WriteLine($"Parallel for consuming {sw.ElapsedMilliseconds}");
            #endregion
            StopAcq();
            DeviceStatusEvent?.Invoke(this, $"Finished / Stop Acq/ Return Result");
            return mResult;
        }
        private ushort[] ResolveRawData(KObject kData)
        {
            GoDataSet dataSet = (GoDataSet)kData;
            ushort[] zValue = new ushort[0];
            mContext = new GocatorContext();
            for (UInt32 i = 0; i < dataSet.Count; i++)
            {
                GoDataMsg dataObj = (GoDataMsg)dataSet.Get(i);
                switch (dataObj.MessageType)
                {
                    #region SurfaceMsg
                    case GoDataMessageType.Surface:

                        GoUniformSurfaceMsg surfaceMsg = (GoUniformSurfaceMsg)dataObj;
                        long width = surfaceMsg.Width;
                        long height = surfaceMsg.Length;
                        long bufferSize = width * height;
                        mContext.XResolution = (double)surfaceMsg.XResolution / 1000000;
                        mContext.ZResolution = (double)surfaceMsg.ZResolution / 1000000;
                        mContext.XOffset = (double)surfaceMsg.XOffset / 1000;
                        mContext.ZOffset = (double)surfaceMsg.ZOffset / 1000;
                        mContext.YResolution = (double)surfaceMsg.YResolution / 1000;
                        mContext.Width = (int)width;
                        mContext.Height = (int)height;
                        IntPtr bufferPointer = surfaceMsg.Data;
                        short[] ranges = new short[bufferSize];
                        zValue = new ushort[bufferSize];
                        Marshal.Copy(bufferPointer, ranges, 0, ranges.Length);
                        Parallel.For(0, bufferSize, (index) =>
                        {
                            zValue[index] = (ushort)(ranges[index] + 32768);
                        });
                        break;
                        #endregion

                }
            }
            DeviceStatusEvent?.Invoke(this, $"Finished {100 * (mResult.Count + 1) / BufferSize * 1.0} %");
            return zValue;
        }
        #endregion
        #region Implement IDevice interface
        public bool InitialAcq()
        {
            KApiLib.Construct();
            GoSdkLib.Construct();
            mSystem = new GoSystem();
            mSensor = mSystem.FindSensorByIpAddress(KIpAddress.Parse(IPAddr));
            DeviceStatusEvent?.Invoke(this, $"Find device @ IP address {IPAddr}");
        
            mSystem.Connect();
            mSystem.EnableData(true);
            mSystem.SetDataHandler(OnData);
            return true;
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
