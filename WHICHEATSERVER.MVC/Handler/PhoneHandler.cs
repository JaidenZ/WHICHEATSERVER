namespace WHICHEATSERVER.Mvc.Handler
{
    using Core.Packets.Gateway;
    using Core.Handler;
    using Core.Packets;
    using Core.Packets.Phone;
    using Controller;
    using Management;

    /// <summary>
    /// 平台处理器
    /// </summary>
    public sealed class PhoneHandler : IHandlerBase
    {
        private void ProcessMessage(IClientMessage messsage)
        {
            IController controller = ControllerContainer.Get<PhoneController>(messsage.MessageType);
            if (controller != null)
                controller.ProcessMessage(messsage);
        }

        int IHandlerBase.Flags
        {
            get { return (int)ClientPacketType.CLIENTPACKET_IPHONE; }
        }

        void IHandlerBase.ProcessMessage(IGatewayMessage message)
        {
            if (message != null)
            {
                NBuffer body = message.GetBuffer();
                PhonePacketHeader header = PhonePacketHeader.GetMessage(body);
                if (header != null)
                    this.ProcessMessage(new PhonePacketMessage(header, body, message));
            }
        }

        IClientMessage IHandlerBase.CreateMessage(long socket, int sequence)
        {
            return PhonePacketMessage.CreatMessage(sequence, socket);
        }
    }
}
