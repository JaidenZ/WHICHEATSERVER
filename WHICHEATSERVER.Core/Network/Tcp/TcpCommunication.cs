namespace WHICHEATSERVER.Core.Network.Tcp
{
    using System;
    using System.Net.Sockets;
    using System.Net;
    using System.Collections.Generic;
    using System.Threading;
    using System.Runtime.InteropServices;

    /// <summary>
    /// TCP通信层
    /// </summary>
    public unsafe partial class TcpCommunication : ICommunication
    {
        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi, Size = 5)]
        private struct NETWORK_PACKET_HEADER
        {
            /// <summary>
            /// 键帧
            /// </summary>
            [FieldOffset(0)]
            public byte bkey;
            /// <summary>
            /// 帧长
            /// </summary>
            [FieldOffset(1)]
            public ushort wlen;
            /// <summary>
            /// 命令
            /// </summary>
            [FieldOffset(3)]
            public ushort wcmd;

            /// <summary>
            /// 帧头
            /// </summary>
            public const int STX = 0x6B;
        }

        /// <summary>
        /// 套接字侦听器
        /// </summary>
        private Socket g_pSocket = null;
        /// <summary>
        /// 缓冲区管理
        /// </summary>
        private byte[] g_buffer = null;
        /// <summary>
        /// 接受工作线程
        /// </summary>
        private Thread g_recvWorkThread = null;
        /*
        /// <summary>
        /// 发送工作线程
        /// </summary>
        private Thread g_sendWorkThread = null;
        /// <summary>
        /// 发送工作队列
        /// </summary>
        private Queue<byte[]> g_sendWorkPools = new Queue<byte[]>();
        */
        /// <summary>
        /// 接受工作大小
        /// </summary>
        private int g_recvWorkSize = sizeof(NETWORK_PACKET_HEADER);
        /// <summary>
        /// 接受的包头
        /// </summary>
        private NETWORK_PACKET_HEADER? g_recvWorkHeader = null;
        /// <summary>
        /// 接受的包工作偏移
        /// </summary>
        private int g_recvWorkOfs = 0;
        /// <summary>
        /// 接受的完整一包
        /// </summary>
        private byte[] g_recvWorkPacket = null;
        /// <summary>
        /// 数据到达
        /// </summary>
        private IList<EventHandler<MessageReceivedEventArgs>> g_evtMsgRecvs = new List<EventHandler<MessageReceivedEventArgs>>();
        /// <summary>
        /// 退出工作线程
        /// </summary>
        private bool g_exitWorkThread = true;
        /// <summary>
        /// 文本主机名
        /// </summary>
        private string g_strHostName = string.Empty;
        /// <summary>
        /// 主机端口号
        /// </summary>
        private int g_portHostValue = 0;

        public event EventHandler Disconnected;

        void ICommunication.Connect(string host, int port)
        {
            lock (this)
            {
                g_portHostValue = port;
                g_strHostName = host;
                if (g_pSocket == null)
                {
                    g_pSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    g_pSocket.NoDelay = true;
                    g_pSocket.SendBufferSize = 524288;
                }
                if (g_buffer == null)
                    g_buffer = new byte[sizeof(NETWORK_PACKET_HEADER)];
                if (!g_pSocket.Connected)
                    g_pSocket.Connect(host, port);
                this.CleanUp();
            }
        }

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetGetConnectedState(ref int dwFlag, int dwReserved);

        private bool Connected
        {
            get
            {
                int dwFlags = 0;
                if (!InternetGetConnectedState(ref dwFlags, 0))
                    return false;
                return !(((g_pSocket.Poll(0, SelectMode.SelectRead) && g_pSocket.Available <= 0) || !g_pSocket.Connected));
            }
        }

        private unsafe byte[] CleanUp()
        {
            lock (this)
            {
                g_recvWorkSize = sizeof(NETWORK_PACKET_HEADER);
                g_recvWorkOfs = 0;
                byte[] data = g_recvWorkPacket;
                g_recvWorkPacket = null;
                g_recvWorkHeader = null;
                return data;
            }
        }

        private unsafe void RecvWorkMethod()
        {
            while (!g_exitWorkThread)
            {
                byte[] data = null;
                int len = 0;
                try
                {
                    if (g_pSocket == null || !this.Connected || g_pSocket.Available < g_recvWorkSize)
                        Thread.Sleep(1);
                    else
                    {
                        len = g_pSocket.Receive((g_recvWorkHeader == null ? g_buffer : g_recvWorkPacket),
                            g_recvWorkOfs, g_recvWorkSize, SocketFlags.None);
                        if (g_recvWorkHeader == null)
                        {
                            NETWORK_PACKET_HEADER* header = (NETWORK_PACKET_HEADER*)Marshal.UnsafeAddrOfPinnedArrayElement(g_buffer, 0);
                            if (header->bkey == NETWORK_PACKET_HEADER.STX)
                            {
                                g_recvWorkSize = (header->wlen - len);
                                g_recvWorkOfs = len;
                            }
                            g_recvWorkHeader = *header;
                            g_recvWorkPacket = new byte[header->wlen];
                            Buffer.BlockCopy(g_buffer, 0, g_recvWorkPacket, 0, len);
                            if (g_recvWorkSize <= 0)
                            {
                                data = this.CleanUp();
                            }
                        }
                        else if ((len - g_recvWorkSize) <= 0)
                        {
                            data = this.CleanUp();
                        }
                    }
                    if (g_pSocket != null && !this.Connected)
                    {
                        this.Close();
                        this.OnDisconnected();
                    }
                }
                catch
                {
                    this.Close();
                    this.OnDisconnected();
                }
                if (data != null)
                {
                    MessageReceivedEventArgs args = new MessageReceivedEventArgs();
                    args.Buffer = data;
                    args.Communication = this;
                    this.OnReceived(args);
                }
            }
            lock (this)
            {
                this.g_recvWorkThread = null;
            }
        }

        /*
        private void SendWorkMethod()
        {
            while (!g_exitWorkThread)
            {
                if (g_pSocket == null || g_sendWorkPools.Count <= 0)
                    Thread.Sleep(1);
                else if (g_sendWorkPools.Count > 0)
                {
                    byte[] buffer = null;
                    lock (g_sendWorkPools)
                        buffer = g_sendWorkPools.Dequeue();
                    if (buffer != null)
                    {
                        try
                        {
                            if (g_pSocket != null)
                                g_pSocket.Send(buffer);
                        }
                        catch
                        {
                            lock (g_sendWorkPools)
                                g_sendWorkPools.Enqueue(buffer);
                            this.Close();
                            this.OnDisconnected();
                        }
                    }
                }
            }
        }*/

        private void OnDisconnected()
        {
            lock (this)
            {
                ICommunication communication = this;
                while (true)
                {
                    try
                    {
                        communication.Connect(g_strHostName, g_portHostValue);
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
            if (Disconnected != null)
                Disconnected(this, EventArgs.Empty);
        }

        void ICommunication.Start()
        {
            lock (this)
            {
                g_exitWorkThread = false;
                if (g_recvWorkThread == null)
                {
                    g_recvWorkThread = new Thread(RecvWorkMethod) { IsBackground = true, Priority = ThreadPriority.Highest };
                    g_recvWorkThread.Start();
                }
                /*
                if (g_sendWorkThread == null)
                {
                    g_sendWorkThread = new Thread(SendWorkMethod) { IsBackground = true, Priority = ThreadPriority.Highest };
                    g_sendWorkThread.Start();
                }*/
                this.CleanUp();
            }
        }

        void ICommunication.Stop()
        {
            lock (this)
            {
                g_exitWorkThread = true;
                if (g_recvWorkThread != null)
                {
                    g_recvWorkThread = null;
                }
                /*
                if (g_sendWorkThread != null)
                {
                    g_sendWorkThread = null;
                }*/
                if (g_pSocket != null)
                {
                    SocketExtension.Close(g_pSocket);
                    g_pSocket = null;
                }
                this.CleanUp();
            }
        }

        void ICommunication.Send(byte[] buffer)
        {
            /*
            lock (g_sendWorkPools)
            {
                g_sendWorkPools.Enqueue(buffer);
            }*/
            if (buffer != null && buffer.Length > 0)
            {
                try
                {
                    if (g_pSocket != null)
                    {
                        SocketError error = SocketError.SocketError;
                        g_pSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, out error, (result) =>
                        {
                            g_pSocket.EndSend(result, out error);
                            if (error != SocketError.Success)
                            {
                                //Debugger.Break();

                                this.Close();
                                this.OnDisconnected();
                            }
                        }, null);
                        if (error != SocketError.Success)
                        {
                            //Debugger.Break();

                            this.Close();
                            this.OnDisconnected();
                        }
                    }
                }
                catch(Exception)
                {
                    this.Close();
                    this.OnDisconnected();
                }
            }
        }

        private void Close()
        {
            lock (this)
            {
                if (g_pSocket != null)
                {
                    g_pSocket.Close();
                    g_pSocket = null;
                }
                this.CleanUp();
            }
        }

        event EventHandler<MessageReceivedEventArgs> ICommunication.Received
        {
            add
            {
                var buffer = g_evtMsgRecvs;
                if (!buffer.Contains(value))
                {
                    buffer.Add(value);
                }
            }
            remove
            {
                var buffer = g_evtMsgRecvs;
                int i = buffer.IndexOf(value);
                if (i >= 0)
                {
                    buffer.RemoveAt(i);
                }
            }
        }
    }

    public partial class TcpCommunication : ICommunication
    {
        /// <summary>
        /// 数据到达通知函数可重写
        /// </summary>
        protected virtual void OnReceived(MessageReceivedEventArgs e)
        {
            var buffer = g_evtMsgRecvs;
            if (buffer.Count > 0)
            {
                foreach (EventHandler<MessageReceivedEventArgs> handler in buffer)
                {
                    handler(this, e);
                }
            }
        }
    }
}
