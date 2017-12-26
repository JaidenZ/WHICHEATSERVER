namespace WHICHEATSERVER.Core.Packets.Gateway
{
    using WHICHEATSERVER.Core.Network;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    /// <summary>
    /// 网关包消息
    /// </summary>
    public sealed class GatewayPacketMessage : EventArgs, IGatewayMessage
    {
        /// <summary>
        /// 网关包头
        /// </summary>
        public GatewayPacketHeader pHeader;
        /// <summary>
        /// 网关包体
        /// </summary>
        public GatewayPacketBody pBody;

        /// 发送数据的远程主机网路点
        /// </summary>
        public EndPoint pRemoteEP;

        /// <summary>
        /// 本地主机的网络点
        /// </summary>
        public EndPoint pLocalEP;

        private GatewayPacketMessage(long linkNo)
        {
            pHeader.qwLinkNo = linkNo;
        }

        private GatewayPacketMessage(GatewayPacketHeader header, GatewayPacketBody body, EndPoint remote, EndPoint local)
        {
            pBody = body;
            pRemoteEP = remote;
            pLocalEP = local;
            pHeader = header;
        }

        public static IGatewayMessage GetMessage(MessageReceivedEventArgs e)
        {
            GatewayPacketHeader header = GatewayPacketHeader.GetMessage(e); // 映射包的结构体
            GatewayPacketBody body = GatewayPacketBody.GetMessage(ref header, e);
            return new GatewayPacketMessage(header, body, e.RemoteEP, e.LocalEP);
        }

        public static IGatewayMessage CreateMessage(long linkNo)
        {
            GatewayPacketMessage message = new GatewayPacketMessage(linkNo);
            message.pBody = new GatewayPacketBody();
            return message;
        }

        public static byte[] CreateMessage(long link, byte[] message)
        {
            return GatewayPacketMessage.CreateMessage(new long[] { link }, message);
        }

        public static byte[] CreateMessage(IList<long> links, byte[] message)
        {
            if (links != null && links.Count > 0)
            {
                using (MemoryStream ms = new MemoryStream(8096))
                {
                    using (BinaryStreamWriter sw = new BinaryStreamWriter(ms))
                    {
                        sw.Write(GatewayProtocolHeader.Gateway_STX);
                        sw.Write((short)0);
                        sw.Write((ushort)GatewayCommand.CLIENT_INFO_QUEST);
                        sw.Write((byte)ClientPacketType.CLIENTPACKET_ALL);
                        sw.Write(links.Count);
                        foreach (long cid in links)
                        {
                            sw.Write(cid);
                        }
                        sw.Write(message);
                        //
                        byte[] buffer = ms.ToArray();
                        buffer[1] = (byte)buffer.Length;
                        buffer[2] = (byte)(buffer.Length >> 8);
                        //
                        return buffer;
                    }
                }
            }
            return null;
        }

        EndPoint IGatewayMessage.RemoteEP
        {
            get
            {
                return pRemoteEP;
            }
        }

        EndPoint IGatewayMessage.LocalEP
        {
            get
            {
                return pLocalEP;
            }
        }
        
        EndPoint IGatewayMessage.ClientEP
        {
            get
            {
                EndPoint ep = null;
                if (pBody != null && !string.IsNullOrEmpty(pBody.szClientIPc))
                {
                    IPAddress address;
                    if (!IPAddress.TryParse(pBody.szClientIPc, out address))
                        return null;
                    else
                        ep = new IPEndPoint(address, pBody.dwClientPort);
                }
                return ep;
            }
        }

        byte[] IGatewayMessage.GetMessage()
        {
            return pBody.szBuffer;
        }

        NBuffer IGatewayMessage.GetBuffer()
        {
            return new NBuffer(pBody.szBuffer, pBody.dwBufferOfs, pBody.szBufferLen);
        }

        long IGatewayMessage.LinkNo
        {
            get
            {
                return pHeader.qwLinkNo;
            }
            set
            {
                pHeader.qwLinkNo = value;
            }
        }

        int IGatewayMessage.GatewayMessageType
        {
            get
            {
                return (int)pHeader.wCommands;
            }
        }


        public int ClientType
        {
            get
            {
                return (int)pHeader.bClientPacketType;
            }
        }
    }
}
