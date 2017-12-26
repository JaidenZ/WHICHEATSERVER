namespace WHICHEATSERVER.Core.Packets.Phone
{
    using Core.Component;
    using Gateway;
    using System;
    using System.IO;
    using System.Net;



    public sealed class PhonePacketMessage : EventArgs, IphoneClientMessage
    {
        

        private byte[] g_pBodyBuffer = null;
        private Stream g_pBodyStream = null;
        /// <summary>
        /// 终端数据包头
        /// </summary>
        private PhonePacketHeader pHeader;

        /// <summary>
        /// 终端数据包体缓冲区
        /// </summary>
        private NBuffer pBody;

        /// <summary>
        /// 网关的消息体
        /// </summary>
        private IGatewayMessage pMsg;

        public PhonePacketMessage(PhonePacketHeader header, NBuffer body,IGatewayMessage message)
        {
            pHeader = header;
            pBody = body;
            pMsg = message;
        }

        public static IClientMessage CreatMessage(int serialNumber,long linkNo)
        {
            PhonePacketMessage message = new PhonePacketMessage(
                new PhonePacketHeader
                {
                    dwMsgSeq = serialNumber
                }, new NBuffer(0, 1),
                GatewayPacketMessage.CreateMessage(linkNo)
             );
            return message;
        }

        private byte[] GetMessage(byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream(8096))
            {
                using (BinaryWriter sw = new BinaryWriter(ms))
                {
                    sw.Write(PhoneProtocolFlags.STX);//键帧
                    {
                        PhonePacketHeader header = this.pHeader; //帧头
                        sw.Write((uint)header.messageType);
                        sw.Write(header.dwMsgSeq);
                        sw.Write(header.szDeviceNumber ?? string.Empty);
                        sw.Write(header.uMsgSplit);
                        if (header.uMsgSplit != 0)
                        {
                            sw.Write(header.dwMsgIndex);
                            sw.Write(header.dwMsgCount);
                        }
                        sw.Write(buffer);
                        {
                            buffer = ms.ToArray();
                            buffer[1] = (byte)buffer.Length;
                            buffer[2] = (byte)(buffer.Length >> 8);
                        }
                        return buffer;
                    }
                }
            }
        }

        byte[] IClientMessage.NewMessage(byte[] body)
        {
            if (body == null)
                return new byte[0];
            return GetMessage(body);
        }
        
        int IClientMessage.MessageType
        {
            get
            {
                Contract.Requires<InvalidOperationException>(pHeader != null);
                return (int)pHeader.messageType;
            }
            set
            {
                Contract.Requires<InvalidOperationException>(pHeader != null);
                pHeader.messageType = (PhoneMessageType)value;
            }
        }
        
        int IClientMessage.SerialNumber
        {
            get
            {
                Contract.Requires<InvalidOperationException>(pHeader != null);
                return pHeader.dwMsgSeq;
            }
            set
            {
                Contract.Requires<InvalidOperationException>(pHeader != null);
                pHeader.dwMsgSeq = value;
            }
        }
        byte IClientMessage.PackageCategory
        {
            get
            {
                Contract.Requires<InvalidOperationException>(pHeader != null);
                return pHeader.uMsgSplit;
            }
            set
            {
                Contract.Requires<InvalidOperationException>(pHeader != null);
                pHeader.uMsgSplit = (byte)value;
            }

        }
        int IClientMessage.PackageIndex
        {
            get
            {
                Contract.Requires<InvalidOperationException>(pHeader != null);
                return pHeader.dwMsgIndex;
            }
            set
            {
                Contract.Requires<InvalidOperationException>(pHeader != null);
                pHeader.dwMsgIndex = value;
            }
        }

        int IClientMessage.PackageCount
        {
            get
            {
                Contract.Requires<InvalidOperationException>(pHeader != null);
                return pHeader.dwMsgCount;
            }
            set
            {
                Contract.Requires<InvalidOperationException>(pHeader != null);
                pHeader.dwMsgCount = value;
            }
        }

        string IphoneClientMessage.DeviceNumber
        {
            get
            {
                Contract.Requires<InvalidOperationException>(pHeader != null);
                return pHeader.szDeviceNumber;
            }

            set
            {
                Contract.Requires<InvalidOperationException>(pHeader != null);
                pHeader.szDeviceNumber = value;
            }
        }

        long IClientMessage.LinkNo
        {
            get
            {
                Contract.Requires<InvalidOperationException>(pMsg != null);
                return pMsg.LinkNo;
            }

            set
            {
                Contract.Requires<InvalidOperationException>(pMsg != null);
                pMsg.LinkNo = value;
            }
        }

        IGatewayMessage IClientMessage.GatewayMessage
        {
            get
            {
                return pMsg;
            }
        }

        byte[] IClientMessage.GetBuffer()
        {
            Contract.Requires<InvalidOperationException>(pBody != null);
            return pBody.buffer;
        }

        unsafe byte[] IClientMessage.GetMessage()
        {
            if (g_pBodyBuffer == null)
            {
                fixed (byte* pinned = pBody.buffer)
                {
                    byte[] buffer = null;
                    if (pBody.offset <= 0)
                        buffer = pBody.buffer;
                    else
                    {
                        buffer = new byte[pBody.size];
                        byte* pX = pBody.GetBuffer();
                        for (int i = 0; i < pBody.size; i++)
                            buffer[i] = *pX++;
                    }
                    g_pBodyBuffer = buffer;
                }
            }
            return g_pBodyBuffer;
        }

        Stream IClientMessage.GetStream()
        {
            if (g_pBodyStream == null)
            {
                IClientMessage g_pMessage = this;
                g_pBodyStream = new MemoryStream();
                byte[] g_pBuffer = g_pMessage.GetBuffer();
                g_pBodyStream.Write(g_pBuffer, 0, g_pBuffer.Length);
            }
            return g_pBodyStream;
        }
    }
}
