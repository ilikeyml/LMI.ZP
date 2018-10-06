namespace Gocator
{
    interface IDevice
    {
        bool InitialAcq();
        bool StartAcq();
        bool StopAcq();
        bool ReleaseAcq();
    }
}
