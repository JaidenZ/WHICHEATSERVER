namespace WHICHEATSERVER.Mvc
{
    using WHICHEATSERVER.Core.Component;
    using WHICHEATSERVER.Core.Handler;
    using WHICHEATSERVER.Core.Network;
    using WHICHEATSERVER.Core.Packets;
    using WHICHEATSERVER.Mvc.Gateway;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Diagnostics;

    public static class MvcApplication
    {

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static IGatewayCommunication _gatewayHandler = null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static ICommunication _communication = null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static Action[] _deployment = null;

        public static event UnhandledExceptionEventHandler UnhandledException = null;

        public static int Count = 0;

        static MvcApplication()
        {
            HandlerManager.Load(typeof(MvcApplication).Assembly);
        }

        public static IGatewayCommunication GetGateway()
        {
            return _gatewayHandler;
        }

        public static void RegisterCommunication(ICommunication communication)
        {
            if (communication == null)
            {
                throw new ArgumentNullException("communication");
            }
            _communication = communication;
            _gatewayHandler = new GatewayCommunication(communication);
            _gatewayHandler.Load += Communication_Load;
            _gatewayHandler.Received += Communication_Received;
        }

        private static void Communication_Load(object sender, EventArgs e)
        {
            if (_deployment != null)
                for (int i = 0; i < _deployment.Length; i++)
                    _deployment[i]();
        }

        public static void Run(string lockName, params Action[] deployment)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(lockName));
            _gatewayHandler.LockName = lockName;
            _deployment = deployment;
            _gatewayHandler.Start();
        }
        
        

        private static void Communication_Received(IGatewayMessage message)
        {
            if (message != null)
            {
                IHandlerBase handler = HandlerManager.Get(message.GatewayMessageType);
                if (handler != null)
                {
                    try
                    {
                        handler.ProcessMessage(message);
                    }
                    catch (Exception e)
                    {
                        if (MvcApplication.UnhandledException != null)
                        {
                            MvcApplication.UnhandledException(message, new UnhandledExceptionEventArgs(e, false));
                        }
                    }
                }
            }
        }
    }
}
