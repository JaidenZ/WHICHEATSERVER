namespace WHICHEATSERVER.Core.Packets.Gateway
{
    using WHICHEATSERVER.Core.Network;
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// 网关数据包信息
    /// </summary>
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi, Size = 18)]
    public unsafe struct GatewayPacketHeader
    {
        /// <summary>
        /// 帧头
        /// </summary>
        [FieldOffset(0)]
        public byte bFrameHeader;

        /// <summary>
        /// 包长
        /// </summary>
        [FieldOffset(1)]
        public short wPacketLength;

        /// <summary>
        /// 网关命令
        /// </summary>
        [FieldOffset(3)]
        public GatewayCommand wCommands;

        /// <summary>
        /// 客户端类型
        /// </summary>
        [FieldOffset(5)]
        public ClientPacketType bClientPacketType;

        /// <summary>
        /// 连接编号
        /// </summary>
        [FieldOffset(6)]
        public long qwLinkNo;

        /// <summary>
        /// 客户端IP字符串长度
        /// </summary>
        [FieldOffset(14)]
        public int cbClientIPSize;


        public static GatewayPacketHeader GetMessage(MessageReceivedEventArgs message)
        {
            fixed (byte* ptr = message.Buffer)
            {
                return *(GatewayPacketHeader*)ptr;
            }
        }
    }
}
