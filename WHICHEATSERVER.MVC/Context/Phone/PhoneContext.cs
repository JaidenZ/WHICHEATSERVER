namespace WHICHEATSERVER.Mvc.Context
{
    using WHICHEATSERVER.Core.Handler;
    using WHICHEATSERVER.Core.Packets;
    using WHICHEATSERVER.Core.Packets.Phone;
    using WHICHEATSERVER.Mvc.Controller;
    using WHICHEATSERVER.Mvc.Management;

    /// <summary>
    /// 平台控制器上下文
    /// </summary>
    public sealed class PhoneContext : Context<PhoneRequest, PhoneResponse>
    {
        protected override PhoneRequest CreateRequest(IClientMessage message, SNContext context, ControllerAttribute controller)
        {
            return new PhoneRequest(message, context, controller);
        }

        protected override PhoneResponse CreateResponse(IClientMessage message, SNContext context, ControllerAttribute controller)
        {
            return new PhoneResponse(message, context, controller);
        }

        public PhoneContext(IClientMessage message, ControllerAttribute controller)
            : base(message, controller, null)
        {

        }

        public PhoneContext(IClientMessage message, ControllerAttribute controller, SNContext context)
            : base(message, controller, context)
        {

        }

        /// <summary>
        /// 创建作业上下文（可能会抛出异常，你需要进行捕获）
        /// </summary>
        public static PhoneContext CreateContext(long linkNo)
        {
            SNContext sn = new SNContext();
            IHandlerBase handler = HandlerManager.Get(4);
            IClientMessage message = handler.CreateMessage(linkNo, sn.SN);
            return new PhoneContext(message, new ControllerAttribute(), sn);
        }
    }
}
