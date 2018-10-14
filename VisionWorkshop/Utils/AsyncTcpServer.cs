﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace VisionWorkshop.Utils
{

        /// <summary>
        /// 异步TCP服务器
        /// </summary>
        public class AsyncTcpServer : IDisposable
        {
            #region Fields

            private TcpListener listener;
            private List<TcpClientState> clients;
            private bool disposed = false;

            #endregion

            #region Ctors

            /// <summary>
            /// 异步TCP服务器
            /// </summary>
            /// <param name="listenPort">监听的端口</param>
            public AsyncTcpServer(int listenPort)
              : this(IPAddress.Any, listenPort)
            {
            }

            /// <summary>
            /// 异步TCP服务器
            /// </summary>
            /// <param name="localEP">监听的终结点</param>
            public AsyncTcpServer(IPEndPoint localEP)
              : this(localEP.Address, localEP.Port)
            {
            }

            /// <summary>
            /// 异步TCP服务器
            /// </summary>
            /// <param name="localIPAddress">监听的IP地址</param>
            /// <param name="listenPort">监听的端口</param>
            public AsyncTcpServer(IPAddress localIPAddress, int listenPort)
            {
                Address = localIPAddress;
                Port = listenPort;
                this.Encoding = Encoding.Default;

                clients = new List<TcpClientState>();

                listener = new TcpListener(Address, Port);
                listener.AllowNatTraversal(true);
            }

            #endregion

            #region Properties

            /// <summary>
            /// 服务器是否正在运行
            /// </summary>
            public bool IsRunning { get; private set; }
            /// <summary>
            /// 监听的IP地址
            /// </summary>
            public IPAddress Address { get; private set; }
            /// <summary>
            /// 监听的端口
            /// </summary>
            public int Port { get; private set; }
            /// <summary>
            /// 通信使用的编码
            /// </summary>
            public Encoding Encoding { get; set; }

            #endregion

            #region Server

            /// <summary>
            /// 启动服务器
            /// </summary>
            /// <returns>异步TCP服务器</returns>
            public AsyncTcpServer Start()
            {
                if (!IsRunning)
                {
                    IsRunning = true;
                    listener.Start();
                    listener.BeginAcceptTcpClient(
                      new AsyncCallback(HandleTcpClientAccepted), listener);
                }
                return this;
            }

            /// <summary>
            /// 启动服务器
            /// </summary>
            /// <param name="backlog">
            /// 服务器所允许的挂起连接序列的最大长度
            /// </param>
            /// <returns>异步TCP服务器</returns>
            public AsyncTcpServer Start(int backlog)
            {
                if (!IsRunning)
                {
                    IsRunning = true;
                    listener.Start(backlog);
                    listener.BeginAcceptTcpClient(
                      new AsyncCallback(HandleTcpClientAccepted), listener);
                }
                return this;
            }

            /// <summary>
            /// 停止服务器
            /// </summary>
            /// <returns>异步TCP服务器</returns>
            public AsyncTcpServer Stop()
            {
                if (IsRunning)
                {
                    IsRunning = false;
                    listener.Stop();

                    lock (this.clients)
                    {
                        for (int i = 0; i < this.clients.Count; i++)
                        {
                            this.clients[i].TcpClient.Client.Disconnect(false);
                        }
                        this.clients.Clear();
                    }

                }
                return this;
            }

            #endregion

            #region Receive

            private void HandleTcpClientAccepted(IAsyncResult ar)
            {
                if (IsRunning)
                {
                    TcpListener tcpListener = (TcpListener)ar.AsyncState;

                    TcpClient tcpClient = tcpListener.EndAcceptTcpClient(ar);
                    byte[] buffer = new byte[tcpClient.ReceiveBufferSize];

                    TcpClientState internalClient
                      = new TcpClientState(tcpClient, buffer);
                    lock (this.clients)
                    {
                        this.clients.Add(internalClient);
                        RaiseClientConnected(tcpClient);
                    }

                    NetworkStream networkStream = internalClient.NetworkStream;
                    networkStream.BeginRead(
                      internalClient.Buffer,
                      0,
                      internalClient.Buffer.Length,
                      HandleDatagramReceived,
                      internalClient);

                    tcpListener.BeginAcceptTcpClient(
                      new AsyncCallback(HandleTcpClientAccepted), ar.AsyncState);
                }
            }

            private void HandleDatagramReceived(IAsyncResult ar)
            {
                if (IsRunning)
                {
                    TcpClientState internalClient = (TcpClientState)ar.AsyncState;
                    NetworkStream networkStream = internalClient.NetworkStream;

                    int numberOfReadBytes = 0;
                    try
                    {
                        numberOfReadBytes = networkStream.EndRead(ar);
                    }
                    catch
                    {
                        numberOfReadBytes = 0;
                    }

                    if (numberOfReadBytes == 0)
                    {
                        // connection has been closed
                        lock (this.clients)
                        {
                            this.clients.Remove(internalClient);
                            RaiseClientDisconnected(internalClient.TcpClient);
                            return;
                        }
                    }

                    // received byte and trigger event notification
                    byte[] receivedBytes = new byte[numberOfReadBytes];
                    Buffer.BlockCopy(
                      internalClient.Buffer, 0,
                      receivedBytes, 0, numberOfReadBytes);
                    RaiseDatagramReceived(internalClient.TcpClient, receivedBytes);
                    RaisePlaintextReceived(internalClient.TcpClient, receivedBytes);

                    // continue listening for tcp datagram packets
                    networkStream.BeginRead(
                      internalClient.Buffer,
                      0,
                      internalClient.Buffer.Length,
                      HandleDatagramReceived,
                      internalClient);
                }
            }

            #endregion

            #region Events

            /// <summary>
            /// 接收到数据报文事件
            /// </summary>
            public event EventHandler<TcpDatagramReceivedEventArgs<byte[]>> DatagramReceived;
            /// <summary>
            /// 接收到数据报文明文事件
            /// </summary>
            public event EventHandler<TcpDatagramReceivedEventArgs<string>> PlaintextReceived;

            private void RaiseDatagramReceived(TcpClient sender, byte[] datagram)
            {
                if (DatagramReceived != null)
                {
                    DatagramReceived(this, new TcpDatagramReceivedEventArgs<byte[]>(sender, datagram));
                }
            }

            private void RaisePlaintextReceived(TcpClient sender, byte[] datagram)
            {
                if (PlaintextReceived != null)
                {
                    PlaintextReceived(this, new TcpDatagramReceivedEventArgs<string>(
                      sender, this.Encoding.GetString(datagram, 0, datagram.Length)));
                }
            }

            /// <summary>
            /// 与客户端的连接已建立事件
            /// </summary>
            public event EventHandler<TcpClientConnectedEventArgs> ClientConnected;
            /// <summary>
            /// 与客户端的连接已断开事件
            /// </summary>
            public event EventHandler<TcpClientDisconnectedEventArgs> ClientDisconnected;

            private void RaiseClientConnected(TcpClient tcpClient)
            {
                if (ClientConnected != null)
                {
                    ClientConnected(this, new TcpClientConnectedEventArgs(tcpClient));
                }
            }

            private void RaiseClientDisconnected(TcpClient tcpClient)
            {
                if (ClientDisconnected != null)
                {
                    ClientDisconnected(this, new TcpClientDisconnectedEventArgs(tcpClient));
                }
            }

            #endregion

            #region Send

            /// <summary>
            /// 发送报文至指定的客户端
            /// </summary>
            /// <param name="tcpClient">客户端</param>
            /// <param name="datagram">报文</param>
            public void Send(TcpClient tcpClient, byte[] datagram)
            {
                if (!IsRunning)
                    throw new InvalidProgramException("This TCP server has not been started.");

                if (tcpClient == null)
                    throw new ArgumentNullException("tcpClient");

                if (datagram == null)
                    throw new ArgumentNullException("datagram");

                tcpClient.GetStream().BeginWrite(
                  datagram, 0, datagram.Length, HandleDatagramWritten, tcpClient);
            }

            private void HandleDatagramWritten(IAsyncResult ar)
            {
                ((TcpClient)ar.AsyncState).GetStream().EndWrite(ar);
            }

            /// <summary>
            /// 发送报文至指定的客户端
            /// </summary>
            /// <param name="tcpClient">客户端</param>
            /// <param name="datagram">报文</param>
            public void Send(TcpClient tcpClient, string datagram)
            {
                Send(tcpClient, this.Encoding.GetBytes(datagram));
            }

            /// <summary>
            /// 发送报文至所有客户端
            /// </summary>
            /// <param name="datagram">报文</param>
            public void SendAll(byte[] datagram)
            {
                if (!IsRunning)
                    throw new InvalidProgramException("This TCP server has not been started.");

                for (int i = 0; i < this.clients.Count; i++)
                {
                    Send(this.clients[i].TcpClient, datagram);
                }
            }

            /// <summary>
            /// 发送报文至所有客户端
            /// </summary>
            /// <param name="datagram">报文</param>
            public void SendAll(string datagram)
            {
                if (!IsRunning)
                    throw new InvalidProgramException("This TCP server has not been started.");

                SendAll(this.Encoding.GetBytes(datagram));
            }

            #endregion

            #region IDisposable Members

            /// <summary>
            /// Performs application-defined tasks associated with freeing, 
            /// releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases unmanaged and - optionally - managed resources
            /// </summary>
            /// <param name="disposing"><c>true</c> to release 
            /// both managed and unmanaged resources; <c>false</c> 
            /// to release only unmanaged resources.</param>
            protected virtual void Dispose(bool disposing)
            {
                if (!this.disposed)
                {
                    if (disposing)
                    {
                        try
                        {
                            Stop();

                            if (listener != null)
                            {
                                listener = null;
                            }
                        }
                        catch (SocketException ex)
                        {
                            //ExceptionHandler.Handle(ex);
                        }
                    }

                    disposed = true;
                }
            }

            #endregion
        }
    
}
