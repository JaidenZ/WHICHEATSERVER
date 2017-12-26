namespace WHICHEATSERVER.Core.Packets
{
    using System.Net;
    public interface IGatewayMessage
    {

        /// <summary>
        /// 网关消息类型
        /// </summary>
        int GatewayMessageType { get; }

        /// <summary>
        /// 远程地址
        /// </summary>
        EndPoint RemoteEP { get; }
        /// <summary>
        /// 本地地址
        /// </summary>
        EndPoint LocalEP { get; }
        /// <summary>
        /// 客户端地址
        /// </summary>
        EndPoint ClientEP { get; }
        /// <summary>
        /// 链接号
        /// </summary>
        long LinkNo { get; set; }
        /// <summary>
        /// 获取消息流
        /// </summary>
        /// <returns></returns>
        byte[] GetMessage();
        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <returns></returns>
        NBuffer GetBuffer();

    }
}
