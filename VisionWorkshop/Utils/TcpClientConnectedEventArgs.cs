using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VisionWorkshop.Utils
{
    public class TcpClientConnectedEventArgs : EventArgs
    {
        public TcpClient TcpClient { get; set; }
        public TcpClientConnectedEventArgs(TcpClient tcp)
        {
            TcpClient = tcp;
        }
    }
}
