namespace WHICHEATSERVER.Core.Packets.Phone
{
    public static class PhoneProtocolFlags
    {
        public const byte STX = 0x2B;

        /// <summary>
        /// 消息体长度
        /// </summary>
        public const int BODYLEN = 0x3FF;
    }
}
