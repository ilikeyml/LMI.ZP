using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VisionWorkshop.Utils
{
    public class TcpDatagramReceivedEventArgs<T> : EventArgs
    {
        public TcpClient TcpClient { set; get; }
        public T Datagram { set; get; }
        public TcpDatagramReceivedEventArgs(TcpClient tcpClient, T datagram)
        {
            this.TcpClient = tcpClient;
            this.Datagram = datagram;
        }
    }
}
