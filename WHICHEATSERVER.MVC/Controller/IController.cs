namespace WHICHEATSERVER.Mvc.Controller
{
    using WHICHEATSERVER.Core.Packets;

    public interface IController
    {
        object Tag 
        {
            get;
            set; 
        }

        void ProcessMessage(IClientMessage message);
    }
}
