namespace WHICHEATSERVER.Mvc.Context
{
    using WHICHEATSERVER.Core.Packets;
    using WHICHEATSERVER.Core.Packets.Phone;
    using WHICHEATSERVER.Core.Serialization;
    using WHICHEATSERVER.Core.Utilits;
    using WHICHEATSERVER.Mvc.Controller;
    using WHICHEATSERVER.Mvc.Management;
    using System;

    /// <summary>
    /// 平台请求信息
    /// </summary>
    public sealed partial class PhoneRequest : Request
    {
        protected IphoneClientMessage GetRaw()
        {
            return base.GetRaw<IphoneClientMessage>();
        }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNumber
        {
            get
            {
                return GetRaw().DeviceNumber;
            }
        }
        

        public PhoneRequest(IClientMessage message, SNContext context, ControllerAttribute controller)
            : base(message, context, controller)
        {
            base.DefaultSingleSerialize = false;
        }

        protected override object DeserializeTo(byte[] buffer, Type clazz, bool single)
        {
            object value = buffer.Deserialize(clazz);
            if (value != null)
                return value;
            return BinaryFormatter.Deserialize(buffer, clazz, single);
        }

        protected override T DeserializeTo<T>(byte[] buffer, bool single)
        {
            T value = buffer.Deserialize<T>();
            if (value != null)
                return value;
            return BinaryFormatter.Deserialize<T>(buffer, single);
        }
    }
}
