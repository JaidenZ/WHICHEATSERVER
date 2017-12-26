namespace WHICHEATSERVER.Mvc.Context
{
    using WHICHEATSERVER.Core.Component;
    using WHICHEATSERVER.Core.Packets;
    using WHICHEATSERVER.Core.Packets.Phone;
    using WHICHEATSERVER.Core.Serialization;
    using WHICHEATSERVER.Mvc.Controller;
    using WHICHEATSERVER.Mvc.Management;
    using System;
    using Core;
    using ssc;

    /// <summary>
    /// 平台响应报文
    /// </summary>
    public sealed class PhoneResponse : Response
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

        public PhoneResponse(IClientMessage message, SNContext context, ControllerAttribute controller)
            : base(message, context, controller)
        {
            base.DefaultSingleSerialize = false;
        }

        protected override byte[] SerializeTo(object obj, bool single)
        {
            ISerializable s = obj as ISerializable;
            if (s != null)
                return s.Serialize();
            return BinaryFormatter.Serialize(obj, single);
        }
    }
}
