namespace WHICHEATSERVER.Mvc.Context
{
    using WHICHEATSERVER.Core.Component;
    using WHICHEATSERVER.Core.Network;
    using WHICHEATSERVER.Core.Packets;
    using WHICHEATSERVER.Core.Serialization;
    using WHICHEATSERVER.Core.Utilits;
    using WHICHEATSERVER.Mvc.Controller;
    using WHICHEATSERVER.Mvc.Management;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    public abstract class Response
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IClientMessage raw = null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private long linkNo = -1;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SNContext sn = null;

        /// <summary>
        /// 客户端的连接号
        /// </summary>
        public long LinkNo
        {
            get
            {
                return linkNo;
            }
            set
            {
                linkNo = value;
            }
        }

        /// <summary>
        /// 允许自增分包索引
        /// </summary>
        public bool PackageIncrementIndex
        {
            get;
            set;
        }

        /// <summary>
        /// 返回到的命令行
        /// </summary>
        public int ResponseCommands
        {
            get
            {
                if (this.raw == null)
                    throw new InvalidOperationException();
                return raw.MessageType;
            }
            set
            {
                if (this.raw == null)
                    throw new InvalidOperationException();
                raw.MessageType = value;
            }
        }

        /// <summary>
        /// 返回到的消息流水号
        /// </summary>
        public int SerialNumber
        {
            get
            {
                if (this.raw == null)
                    throw new InvalidOperationException();
                return raw.SerialNumber;
            }
            set
            {
                if (this.raw == null)
                    throw new InvalidOperationException();
                raw.SerialNumber = value;
            }
        }

        /// <summary>
        /// 获取或设置分包类别（0、不分包，1、标准分包，2、原子分包）
        /// </summary>
        public int PackageCategory
        {
            get
            {
                if (this.raw == null)
                    throw new InvalidOperationException();
                return raw.PackageCategory;
            }
            set
            {
                if (this.raw == null)
                    throw new InvalidOperationException();
                raw.PackageCategory = (byte)value;
            }
        }

        /// <summary>
        /// 分包的索引
        /// </summary>
        public int PackageIndex
        {
            get
            {
                if (this.raw == null)
                    throw new InvalidOperationException();
                return raw.PackageIndex;
            }
            set
            {
                if (this.raw == null)
                    throw new InvalidOperationException();
                raw.PackageIndex = value;
            }
        }

        /// <summary>
        /// 分包的总数
        /// </summary>
        public int PackageCount
        {
            get
            {
                if (this.raw == null)
                    throw new InvalidOperationException();
                return raw.PackageCount;
            }
            set
            {
                if (this.raw == null)
                    throw new InvalidOperationException();
                raw.PackageCount = value;
            }
        }

        /// <summary>
        /// 自定义标记（短持久）
        /// </summary>
        public object Tag
        {
            get;
            set;
        }

        /// <summary>
        /// 自定义用户令牌（短持久）
        /// </summary>
        public object UserToken
        {
            get;
            set;
        }

        /// <summary>
        /// 控制器特性
        /// </summary>
        public ControllerAttribute Controller
        {
            get;
            private set;
        }

        protected IClientMessage Raw
        {
            get
            {
                return this.raw;
            }
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
            T message = this.raw as T;
            Contract.Requires<InvalidOperationException>(message != null);
            return message;
        }

        public Response(IClientMessage message, SNContext context, ControllerAttribute controller)
        {
            try
            {
                this.raw = message;
            }
            finally
            {
                this.linkNo = message.LinkNo;
                this.PackageCategory = 0;
                this.PackageIndex = 0;
                this.PackageCount = 0;
                this.sn = context;
                this.PackageIncrementIndex = true;
                this.ResponseCommands = (this.Controller = controller).ResponseCommands;
            }
        }

        /// <summary>
        /// 写入对象到客户端
        /// </summary>
        public void Write(object value)
        {
            this.Write(value, this.DefaultSingleSerialize);
        }

        /// <summary>
        /// 写入对象到客户端
        /// </summary>
        public void Write(object value, bool single)
        {
            this.Write(value, new long[] { this.linkNo }, single);
        }

        /// <summary>
        /// 写入对象到客户端
        /// </summary>
        public void Write(object value, IList<long> linkNos)
        {
            this.Write(value, linkNos, this.DefaultSingleSerialize);
        }

        /// <summary>
        /// 写入对象到客户端
        /// </summary>
        public void Write(object value, IList<long> linkNos, bool single)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            this.Write(this.SerializeTo(value, single), linkNos);
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="obj">欲被序列化的对象</param>
        /// <param name="single">单子接长度</param>
        /// <returns></returns>
        protected abstract byte[] SerializeTo(object obj, bool single);

        /// <summary>
        /// 写到流到客户端
        /// </summary>
        public void Write(Stream s)
        {
            this.Write(s, new long[] { this.linkNo });
        }

        /// <summary>
        /// 写到流到客户端
        /// </summary>
        public void Write(params byte[] buffer)
        {
            this.Write(buffer, new long[] { this.linkNo });
        }

        /// <summary>
        /// 写到流到客户端
        /// </summary>
        public void Write(Stream s, IList<long> linkNos)
        {
            if (s == null || !s.CanRead)
                throw new ArgumentException("s");
            long size = s.Position - s.Length;
            if (size < 0)
                throw new ArgumentException("s");
            byte[] buffer = new byte[size];
            s.Read(buffer, 0, buffer.Length);
            this.Write(buffer, linkNos);
        }

        /// <summary>
        /// 密封一个原子消息迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerable Message(byte[] buffer, bool atom)
        {
            Contract.Requires<ArgumentNullException>(buffer != null, "buffer");
            Contract.Requires<InvalidOperationException>(raw != null && sn != null);
            IList<byte[]> splits = NBufferSplit.Split(buffer);
            if (splits.Count > 1) // 如果只存在一个包说明不需要分包
            {
                this.PackageCategory = 1;
                this.PackageCount = (ushort)splits.Count;
                this.PackageIndex = 0;
            }
            foreach (byte[] split in splits)
            {
                if (this.PackageIncrementIndex)
                {
                    if (this.PackageCategory != 0)
                        this.PackageIndex++; // 递加分包索引
                }
                if (!atom)
                    buffer = raw.NewMessage(split);
                yield return buffer;
            }
        }

        /// <summary>
        /// 密封一个原子消息迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerable Message(byte[] buffer)
        {
            return this.Message(buffer, false);
        }

        /// <summary>
        /// 发送原子消息
        /// </summary>
        /// <param name="message">原子消息迭代器</param>
        public void Write(IList<long> linkNos, IEnumerable message)
        {
            if (ListUnit.IsNullOrEmpty(linkNos) || message == null)
                throw new ArgumentException();
            IGatewayCommunication handler = MvcApplication.GetGateway();
            foreach (byte[] buffer in message)
                handler.SendMessage(linkNos, buffer);
        }

        /// <summary>
        /// 发送原子消息
        /// </summary>
        /// <param name="message">原子消息迭代器</param>
        public void Write(IEnumerable message)
        {
            this.Write(new long[] { this.linkNo }, message);
        }

        /// <summary>
        /// 写到流到客户端
        /// </summary>
        public void Write(byte[] buffer, bool atom, IList<long> linkNos)
        {
            this.Write(this.Message(buffer, atom), linkNos);  
        }

        /// <summary>
        /// 写到流到客户端
        /// </summary>
        public void Write(byte[] buffer, bool atom)
        {
            this.Write(this.Message(buffer, atom), new long[] { this.linkNo });
        }

        /// <summary>
        /// 写到流到客户端
        /// </summary>
        public void Write(byte[] buffer, IList<long> linkNos)
        {
            this.Write(linkNos, this.Message(buffer, false));
        }
    }
}
