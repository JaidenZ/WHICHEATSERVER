namespace WHICHEATSERVER.Core.Handler
{
    using WHICHEATSERVER.Core.Packets;
    using System.Security;

    public interface IHandlerBase
    {
        int Flags
        {
            get;
        }

        [SecuritySafeCritical, SuppressUnmanagedCodeSecurity]
        void ProcessMessage(IGatewayMessage message);

        [SecuritySafeCritical]
        IClientMessage CreateMessage(long socket, int sequence);
    }
}
