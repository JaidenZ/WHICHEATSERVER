namespace WHICHEATSERVER.Mvc.Context
{
    using WHICHEATSERVER.Core.Component;
    using WHICHEATSERVER.Core.Network;
    using WHICHEATSERVER.Core.Packets;
    using WHICHEATSERVER.Mvc.Controller;
    using WHICHEATSERVER.Mvc.Management;
    using System;
    using System.Net;
    using System.Diagnostics;

    public abstract class Context<TRequest, TResponse> where TRequest : Request where TResponse : Response
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SNContext sn = null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IClientMessage message = null;

        /// <summary>
        /// 发送消息的网关地址
        /// </summary>
        public EndPoint RemoteEP
        {
            get
            {
                Contract.Requires<InvalidOperationException>(message != null);
                IGatewayMessage gateway = message.GatewayMessage;
                Contract.Requires<InvalidOperationException>(gateway != null);
                return gateway.RemoteEP;
            }
        }
        /// <summary>
        /// 本地服务器网路地址
        /// </summary>
        public EndPoint LocalEP
        {
            get
            {
                Contract.Requires<InvalidOperationException>(message != null);
                IGatewayMessage gateway = message.GatewayMessage;
                Contract.Requires<InvalidOperationException>(gateway != null);
                return gateway.LocalEP;
            }
        }
        /// <summary>
        /// 客户端网路地址
        /// </summary>
        public EndPoint ClientEP
        {
            get
            {
                Contract.Requires<InvalidOperationException>(message != null);
                IGatewayMessage gateway = message.GatewayMessage;
                Contract.Requires<InvalidOperationException>(gateway != null);
                return gateway.ClientEP;
            }
        }
        /// <summary>
        /// 当前服务器登陆用户ID
        /// </summary>
        public string LoginID
        {
            get
            {
                IGatewayCommunication handler = MvcApplication.GetGateway();
                Contract.Requires<InvalidOperationException>(handler != null);
                return handler.LockName;
            }
        }
        
        /// <summary>
        /// 数据请求上下文
        /// </summary>
        public TRequest Request
        {
            get;
            set;
        }
        /// <summary>
        /// 数据响应上下文
        /// </summary>
        public TResponse Response
        {
            get;
            set;
        }
        /// <summary>
        /// 托管的上下文
        /// </summary>
        protected SNContext GetContext()
        {
            return sn;
        }
        /// <summary>
        /// 抽象请求的创建
        /// </summary>
        /// <returns></returns>
        protected abstract TRequest CreateRequest(IClientMessage message, SNContext context, ControllerAttribute controller);
        /// <summary>
        /// 抽象响应的创建
        /// </summary>
        /// <returns></returns>
        protected abstract TResponse CreateResponse(IClientMessage message, SNContext context, ControllerAttribute controller);

        public Context(IClientMessage message, ControllerAttribute controller)
            : this(message, controller, null)
        {
            // TD:OD 
        }

        public Context(IClientMessage message, ControllerAttribute controller, SNContext context)
        {
            Contract.Requires<ArgumentNullException>(controller != null && message != null);
            if (context == null)
            {
                context = SNManager.Get(message.LinkNo);
                this.Request = this.CreateRequest(message, context, controller);
            }
            this.sn = context;
            this.message = message;
            this.Response = this.CreateResponse(message, context, controller);
        }
    }
}
