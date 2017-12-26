namespace WHICHEATSERVER.Mvc.Controller
{
    using WHICHEATSERVER.Core.Packets;
    using System.Diagnostics;

    public abstract class Controller : IController
    {
        public object Tag
        {
            get;
            set;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal ControllerAttribute Attribute
        {
            get;
            set;
        }

        public Controller()
        {
            this.Attribute = ControllerAttribute.Get(base.GetType());
        }

        void IController.ProcessMessage(IClientMessage message)
        {
            this.ProcessMessage(message);
        }

        protected abstract void ProcessMessage(IClientMessage message);
    }
}
