using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VisionWorkshop.Utils
{
    public class TcpClientState
    {
        public byte[] Buffer { get; set; }
        public TcpClient TcpClient { get; set; }
        public NetworkStream NetworkStream { get; set; }
        public TcpClientState(TcpClient tcpClient, byte[] buffer)
        {
            this.TcpClient = tcpClient;
            this.Buffer = buffer;
            this.NetworkStream = tcpClient.GetStream();
        }
    }
}
