namespace WHICHEATSERVER.Core.Packets.Gateway
{
    using WHICHEATSERVER.Core.Network;
    using System;
    using System.Text;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe partial class GatewayPacketBody
    {

        /// <summary>
        /// 客户端IP地址
        /// </summary>
        public string szClientIPc;

        /// <summary>
        /// 客户端端口
        /// </summary>
        public int dwClientPort;

        /// <summary>
        /// 缓冲区数据
        /// </summary>
        public byte[] szBuffer;

        /// <summary>
        /// 缓冲区长度
        /// </summary>
        public int szBufferLen;

        /// <summary>
        /// 缓冲区数据位于流的偏移量
        /// </summary>
        public int dwBufferOfs;

        public byte* GetBuffer()
        {
            fixed (byte* ptr = &szBuffer[dwBufferOfs])
            {
                return ptr;
            }
        }

        public static GatewayPacketBody GetMessage(ref GatewayPacketHeader header, MessageReceivedEventArgs args)
        {
            byte[] message = args.Buffer;
            int offset = sizeof(GatewayPacketHeader); // 位于包头的偏移
            if (offset > message.Length)
                return null;
            GatewayPacketBody body = new GatewayPacketBody();
            body.szClientIPc = PacketTextEncoder.GetEncoding(message, --offset, header.cbClientIPSize);
            offset += header.cbClientIPSize; // 迭代偏移到客户端IP后
            fixed (byte* ptr = &args.Buffer[offset])
            {
                body.dwClientPort = *((int*)ptr);
            }
            offset += sizeof(int);
            body.szBuffer = message; // 网关发过来的数据（客户端数据+网关信息）
            body.dwBufferOfs = offset; // 客户端数据开始索引
            body.szBufferLen = message.Length - offset; // 客户端数据长度
            return body;
        }
    }
}
