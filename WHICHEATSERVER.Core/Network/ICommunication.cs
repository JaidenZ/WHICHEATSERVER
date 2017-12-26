namespace WHICHEATSERVER.Core.Network
{
    using System;

    /// <summary>
    /// 通信层插件开发规范
    /// </summary>
    public partial interface ICommunication
    {
        event EventHandler Disconnected;
        /// <summary>
        /// 链接服务器
        /// </summary>
        void Connect(string host, int port);
        /// <summary>
        /// 开启通信层
        /// </summary>
        void Start();
        /// <summary>
        /// 关闭通信层
        /// </summary>
        void Stop();
        /// <summary>
        /// 发送数据包
        /// </summary>
        void Send(byte[] buffer);
        /// <summary>
        /// 数据到达
        /// </summary>
        event EventHandler<MessageReceivedEventArgs> Received;
    }
}
