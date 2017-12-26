namespace WHICHEATSERVER.Mvc.Gateway
{
    using WHICHEATSERVER.Core.Component;
    using WHICHEATSERVER.Core.Network;
    using WHICHEATSERVER.Core.Packets;
    using WHICHEATSERVER.Core.Packets.Gateway;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// 网关通信层
    /// </summary>
    internal unsafe partial class GatewayCommunication : IGatewayCommunication
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="links">链接号</param>
        /// <param name="message">缓冲区数据</param>
        void IGatewayCommunication.SendMessage(IList<long> links, byte[] message)
        {
            if ((message = GatewayPacketMessage.CreateMessage(links, message)) != null)
                this.Communication.Send(message);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="cids">链接号</param>
        /// <param name="message">缓冲区数据</param>
        void IGatewayCommunication.SendMessage(long link, byte[] message)
        {
            IGatewayCommunication gateway = this;
            gateway.SendMessage(new long[] { link }, message);
        }

        public event EventHandler<EventArgs> Load = null;

        public Action<IGatewayMessage> Received
        {
            get;
            set;    
        }
    }

    internal unsafe partial class GatewayCommunication
    {
        private ICommunication communication = null;

        public ICommunication Communication
        {
            get
            {
                return this.communication;
            }
            private set
            {
                Contract.Requires<ArgumentNullException>(value != null);
                value.Disconnected += Communication_Disconnected;
                this.communication = value;
            }
        }

        private void Communication_Disconnected(object sender, EventArgs e)
        {
            lock (this)
            {
                this.Logined = false;
                this.SendMessage(GatewayCommand.SERVER_BEATHEART_QUEST);
            }
        }

        /// <summary>
        /// 派发消息池
        /// </summary>
        private Queue<IGatewayMessage> DistributedMessagePools
        {
            get;
            set;
        }

        /// <summary>
        /// 派发工作线程
        /// </summary>
        private IList<Thread> DistributedWorkThreads
        {
            get;
            set;
        }

        /// <summary>
        /// 退出派发工作线程
        /// </summary>
        private bool ExitDistributedWorkThread
        {
            get;
            set;
        }

        public GatewayCommunication(ICommunication communication)
        {
            this.Communication = communication;
            this.Communication.Received += Communication_Received;
            //
            this.DistributedMessagePools = new Queue<IGatewayMessage>();
            this.ExitDistributedWorkThread = true;
            {
                this.ProtectedWorkThread = new Thread(this.ProtectedWorkMethod)
                {
                    IsBackground = false,
                    Priority = ThreadPriority.Lowest
                };
            }
        }

        /// <summary>
        /// 派发工作线程函数
        /// </summary>
        private void DistributedWorkMethod()
        {
            if (this.Load != null)
                this.Load(this, EventArgs.Empty);
            while (true)
            {
                if (this.ExitDistributedWorkThread)
                    Thread.Sleep(1);
                else
                {
                    Queue<IGatewayMessage> pools = this.DistributedMessagePools;
                    IGatewayMessage message = null;
                    lock (pools)
                    {
                        if (pools.Count > 0)
                            message = pools.Dequeue();
                    }
                    if (message == null)
                        Thread.Sleep(1);
                    else
                    {
                        this.ProcessReceived(message);
                    }
                }
            }
        }

        private void ProtectedWorkMethod()
        {
            while (true)
            {
                lock (this)
                {
                    TimeSpan span = DateTime.Now - this.ReceivedDateTime;
                    if (span.TotalSeconds > 20) // 如果超过二十秒则证明掉线
                        this.Logined = false; // 已掉线
                    if (this.Logined)
                        this.SendMessage(GatewayCommand.SERVER_BEATHEART_QUEST);
                    else
                        this.SendMessage(GatewayCommand.SERVER_LOGIN_QUEST); // 重新登陆
                }
                Thread.Sleep(15000);
            }
        }
    }

    internal unsafe partial class GatewayCommunication
    {
        /// <summary>
        /// 是否已登陆到网关
        /// </summary>
        public bool Logined
        {
            get;
            set;
        }
        /// <summary>
        /// 心跳工作线程
        /// </summary>
        private Thread ProtectedWorkThread
        {
            get;
            set;
        }

        /// <summary>
        /// 接收时间
        /// </summary>
        private DateTime ReceivedDateTime
        {
            get;
            set;
        }
    }

    internal unsafe partial class GatewayCommunication
    {
        /// <summary>
        /// 登陆到网关的ID
        /// </summary>
        public string LockName
        {
            get;
            set;
        }

        /// <summary>
        /// 启动网关通信层
        /// </summary>
        public void Start()
        {
            lock (this)
            {
                try
                {
                    if (this.Communication == null)
                    {
                        throw new InvalidOperationException();
                    }
                    this.Communication.Start();
                    if (this.DistributedWorkThreads == null)
                    {
                        int len = Environment.ProcessorCount * 3 + 1;
                        this.DistributedWorkThreads = new List<Thread>(len);
                        for (int i = 0; i < len; i++)
                        {
                            Thread work = new Thread(this.DistributedWorkMethod)
                            {
                                IsBackground = false,
                                Priority = ThreadPriority.Highest,
                                Name = "GatewayCommunication"
                            };
                            work.Start();
                            this.DistributedWorkThreads.Add(work);
                        }
                    }
                    this.ExitDistributedWorkThread = false;
                }
                finally
                {
                    this.ProtectedWorkThread.Start(); // 运行心跳包线程
                }
            }
        }

        /// <summary>
        /// 中止网关通信层
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                if (this.Communication == null)
                {
                    throw new InvalidOperationException();
                }
                this.Communication.Stop();
                this.ExitDistributedWorkThread = true;
            }
        }
    }

    internal unsafe partial class GatewayCommunication
    {
        /// <summary>
        /// 发送命令
        /// </summary>
        /// <returns></returns>
        private void SendMessage(GatewayCommand cmd)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryStreamWriter sw = new BinaryStreamWriter(ms))
                {
                    sw.Write(GatewayProtocolHeader.Gateway_STX);
                    sw.Write((short)0);
                    sw.Write((ushort)cmd);
                    sw.Write((byte)ClientPacketType.CLIENTPACKET_ALL);//客户端类型
                    //
                    sw.Write(0);
                    sw.Write(this.LockName);
                    //
                    byte[] message = ms.ToArray();
                    //
                    message[1] = (byte)message.Length;
                    message[2] = (byte)(message.Length << 8);
                    //
                    this.Communication.Send(message);
                }
            }
        }
    }

    internal unsafe partial class GatewayCommunication
    {
        /// <summary>
        /// 通信层数据到达
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Communication_Received(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                IGatewayMessage message = GatewayPacketMessage.GetMessage(e);
                switch ((GatewayCommand)message.GatewayMessageType)
                {
                    case GatewayCommand.SERVER_LOGIN_ANSWER:
                        // 登陆包
                        this.Logined = true;
                        return;
                    case GatewayCommand.SERVER_BEATHEART_ANSWER:
                        // 心跳包
                        return;
                    case GatewayCommand.CLIENT_INFO_QUEST:
                        // 跳出
                        break;
                    default: // 不支持
                        return;
                }
                lock (this.DistributedMessagePools)
                {
                   this.DistributedMessagePools.Enqueue(message);
                }
            }
            finally
            {
                this.ReceivedDateTime = DateTime.Now; // 收到数据的时间
            }
        }

        /// <summary>
        /// 处理接收的数据
        /// </summary>
        private unsafe void ProcessReceived(IGatewayMessage message)
        {
            if (this.Received != null)
                this.Received(message);
        }
    }
}
