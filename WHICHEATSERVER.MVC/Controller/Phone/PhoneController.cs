namespace WHICHEATSERVER.Mvc.Controller
{
    using Core.Packets;
    using Mvc.Context;

    public abstract class PhoneController:Controller
    {

        public abstract void ProcessRequest(PhoneContext context);

        protected override void ProcessMessage(IClientMessage message)
        {
            this.ProcessRequest(new PhoneContext(message, base.Attribute));
        }
    }
}
