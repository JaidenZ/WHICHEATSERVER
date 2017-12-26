namespace WHICHEATSERVER.Core.Packets
{
    using System.IO;
    using System.Net;


    /// <summary>
    /// 客户端数据接口
    /// </summary>
    public interface IClientMessage
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        int MessageType { get; set; }

        /// <summary>
        /// 获取连接号
        /// </summary>
        long LinkNo { get; set; }

        /// <summary>
        /// 获取或设置序列号
        /// </summary>
        /// <returns></returns>
        int SerialNumber
        {
            get;
            set;
        }
        /// <summary>
        /// 获取或设置分包类别（0、不分包，1、标准分包，2、原子分包）
        /// </summary>
        byte PackageCategory
        {
            get;
            set;
        }
        /// <summary>
        /// 分包的索引
        /// </summary>
        int PackageIndex
        {
            get;
            set;
        }
        /// <summary>
        /// 分包的总数
        /// </summary>
        int PackageCount
        {
            get;
            set;
        }

        /// <summary>
        /// 创建一条消息
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        byte[] NewMessage(byte[] body);

        /// <summary>
        /// 获取消息数据
        /// </summary>
        /// <returns></returns>
        byte[] GetMessage();

        /// <summary>
        /// 获取消息体的数据
        /// </summary>
        /// <returns></returns>
        byte[] GetBuffer();

        /// <summary>
        /// 获取消息体的流
        /// </summary>
        /// <returns></returns>
        Stream GetStream();
        
        /// <summary>
        /// 网关消息
        /// </summary>
        IGatewayMessage GatewayMessage
        {
            get;
        }

    }
}
