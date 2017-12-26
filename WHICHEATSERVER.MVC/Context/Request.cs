namespace WHICHEATSERVER.Mvc.Context
{
    using WHICHEATSERVER.Core.Component;
    using WHICHEATSERVER.Core.Packets;
    using WHICHEATSERVER.Core.Serialization;
    using WHICHEATSERVER.Mvc.Controller;
    using WHICHEATSERVER.Mvc.Management;
    using System;
    using System.IO;

    public abstract class Request
    { 
        /// <summary>
        /// 客户端请求控制器命令值
        /// </summary>
        public int RequestCommands
        {
            get;
            private set;
        }

        /// <summary>
        /// 设备链接号
        /// </summary>
        public long LinkNo
        {
            get;
            private set;
        }

        /// <summary>
        /// 消息流水号
        /// </summary>
        public int SerialNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取分包类别（0、不分包，1、标准分包，2、原子分包）
        /// </summary>
        public int PackageCategory
        {
            get;
            private set;
        }

        /// <summary>
        /// 分包的索引
        /// </summary>
        public int PackageIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// 分包的总数
        /// </summary>
        public int PackageCount
        {
            get;
            private set;
        }

        /// <summary>
        /// 平台数据包信息
        /// </summary>
        public IClientMessage Raw
        {
            get;
            private set;
        }
        
        /// <summary>
        /// 默认单字节序列化
        /// </summary>
        protected bool DefaultSingleSerialize
        {
            get;
            set;
        }

        protected T GetRaw<T>() where T : class, IClientMessage
        {
            T message = this.Raw as T;
            Contract.Requires<InvalidOperationException>(message != null);
            return message;
        }

        public Request(IClientMessage message, SNContext context, ControllerAttribute controller)
        {
            IGatewayMessage gateway = message.GatewayMessage;
            this.Raw = message;
            //
            this.RequestCommands = message.MessageType;
            this.SerialNumber = message.SerialNumber;
            this.PackageIndex = message.PackageIndex;
            this.PackageCount = message.PackageCount;
            this.PackageCategory = message.PackageCategory;
            //
            this.LinkNo = message.LinkNo;
        }

        /// <summary>
        /// 获取请求流
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            if (this.Raw == null)
                throw new InvalidOperationException();
            return Raw.GetStream();
        }

        /// <summary>
        /// 获取请求的缓冲区
        /// </summary>
        /// <returns></returns>
        public byte[] GetBuffer()
        {
            if (this.Raw == null)
                throw new InvalidOperationException();
            return Raw.GetBuffer();
        }

        /// <summary>
        /// 从请求流内读入对象
        /// </summary>
        /// <returns></returns>
        public object ReadObject(Type clazz)
        {
            return this.ReadObject(clazz, this.DefaultSingleSerialize);
        }

        /// <summary>
        /// 从请求流内读入对象
        /// </summary>
        /// <returns></returns>
        public object ReadObject(Type clazz, bool single)
        {
            if (clazz == null)
                throw new ArgumentNullException("clazz");
            byte[] buffer = this.GetBuffer();
            return this.DeserializeTo(buffer, clazz, single);
        }

        /// <summary>
        /// 从请求流内读入对象
        /// </summary>
        /// <returns></returns>
        public T ReadObject<T>() where T : class, new()
        {
            return this.ReadObject<T>(this.DefaultSingleSerialize);
        }

        /// <summary>
        /// 从请求流内读入对象
        /// </summary>
        /// <returns></returns>
        public T ReadObject<T>(bool single) where T : class, new()
        {
            byte[] buffer = this.GetBuffer();
            return this.DeserializeTo<T>(buffer, single);
        }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <typeparam name="T">二进制流</typeparam>
        /// <param name="buffer">二进制流</param>
        /// <param name="single">单字节长度</param>
        /// <returns></returns>
        protected abstract T DeserializeTo<T>(byte[] buffer, bool single) where T : class, new();

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <param name="buffer">二进制流</param>
        /// <param name="clazz">读取的类型</param>
        /// <param name="single">单字节长度</param>
        /// <returns></returns>
        protected abstract object DeserializeTo(byte[] buffer, Type clazz, bool single);
    }
}
