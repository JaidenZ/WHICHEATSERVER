namespace WHICHEATSERVER.Core.Network
{
    using System;
    using System.Net;

    public class MessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// 收到的缓冲区数据
        /// </summary>
        public byte[] Buffer
        {
            get;
            set;
        }
        /// <summary>
        /// 发送数据的远程主机网路点
        /// </summary>
        public EndPoint RemoteEP
        {
            get;
            set;
        }

        /// <summary>
        /// 本地主机网路点
        /// </summary>
        public EndPoint LocalEP
        {
            get;
            set;
        }

        /// <summary>
        /// 具体发生行为的通信层
        /// </summary>
        public ICommunication Communication
        {
            get;
            set;
        }
    }
}
