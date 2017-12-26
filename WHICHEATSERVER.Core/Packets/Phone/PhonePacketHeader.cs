namespace WHICHEATSERVER.Core.Packets.Phone
{
    using System.Runtime.InteropServices;
    [StructLayout(LayoutKind.Sequential,CharSet = CharSet.Ansi)]
    public partial class PhonePacketHeader
    {

        /// <summary>
        /// 消息类型
        /// </summary>
        public PhoneMessageType messageType;

        /// <summary>
        /// 消息序列号
        /// </summary>
        /// <returns></returns>
        public int dwMsgSeq;

        /// <summary>
        /// 设备编号
        /// </summary>
        public string szDeviceNumber { get; set; }

        /// <summary>
        /// 是否消息已分包
        /// </summary>
        public byte uMsgSplit;

        /// <summary>
        /// 分包的索引
        /// </summary>
        public int dwMsgIndex;

        /// <summary>
        /// 分包的总包数
        /// </summary>
        public int dwMsgCount;



        public static unsafe PhonePacketHeader GetMessage(NBuffer stream)
        {
            fixed (byte* pinned = stream.buffer)
            {
                byte* ptr = stream.GetBuffer();
                int ofs = sizeof(byte);
                if (*ptr++ != PhoneProtocolFlags.STX)
                    return null;
                PhonePacketHeader header = new PhonePacketHeader();
                header.messageType = *(PhoneMessageType*)ptr;
                ptr += sizeof(PhoneMessageType);
                ofs += sizeof(PhoneMessageType);
                header.dwMsgSeq = *(int*)ptr;
                ptr += sizeof(int);
                ofs += sizeof(int);
                int size = *ptr++;
                ofs += sizeof(byte);
                header.szDeviceNumber = NString.To(ptr, ofs, size);
                ptr += size;
                ofs += size;
                header.uMsgSplit = *(byte*)ptr++;
                ofs += sizeof(bool);
                int* i = (int*)ptr;
                if (header.uMsgSplit != 0)
                {
                    header.dwMsgIndex = *i++;
                    header.dwMsgCount = *i++;
                    ptr += sizeof(int) << 1;
                    ofs += sizeof(int) << 1;
                }
                stream.offset += ofs;
                stream.size = (stream.size - ofs);
                return header;
            }
        }

    }
}
