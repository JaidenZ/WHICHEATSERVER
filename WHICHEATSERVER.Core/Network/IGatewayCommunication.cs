namespace WHICHEATSERVER.Core.Network
{
    using System.Collections.Generic;
    using System;
    using Core.Packets;
    public interface IGatewayCommunication
    {
        /// <summary>
        /// 网关操作员
        /// </summary>
        string LockName
        {
            get;
            set;
        }

        void Start();

        /// <summary>
        /// 发送消息
        /// </summary>
        void SendMessage(long link, byte[] message);

        /// <summary>
        /// 发送消息
        /// </summary>
        void SendMessage(IList<long> links, byte[] message);

        /// <summary>
        /// 数据到达
        /// </summary>
        Action<IGatewayMessage> Received
        {
            get;
            set;
        }

        /// <summary>
        /// 工作线程加载
        /// </summary>
        event EventHandler<EventArgs> Load;
    }
}
