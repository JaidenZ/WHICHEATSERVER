namespace WHICHEATSERVER.Core.Packets.Phone
{
    public interface IphoneClientMessage : IClientMessage
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        string DeviceNumber
        {
            get;
            set;
        }

    }
}
