namespace WHICHEATSERVER.Core.Handler
{
    using WHICHEATSERVER.Core.Component;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public partial class HandlerManager
    {
        private static IList<IHandlerBase> Handlers
        {
            get;
            set;
        }

        static HandlerManager()
        {
            HandlerManager.Handlers = new List<IHandlerBase>();
        }
    }

    public partial class HandlerManager
    {
        /// <summary>
        /// 加载一个通信层插件
        /// </summary>
        public static Assembly Load(FileInfo file)
        {
            if (file == null || !file.Exists)
                throw new ArgumentException("您提供的处理器插件不存在");
            Assembly assembly = Assembly.LoadFrom(file.FullName);
            HandlerManager.Load(assembly);
            return assembly;
        }

        public static void Load(Assembly assembly)
        {
            Contract.Requires<ArgumentNullException>(assembly != null);
            foreach (Type clazz in assembly.GetExportedTypes())
            {
                if (typeof(IHandlerBase).IsAssignableFrom(clazz))
                {
                    ConstructorInfo ctor = clazz.GetConstructor(Type.EmptyTypes);
                    if (ctor == null)
                    {
                        continue;
                    }
                    IHandlerBase handler = (IHandlerBase)ctor.Invoke(null);
                    HandlerManager.Handlers.Add(handler);
                }
            }
        }

        /// <summary>
        /// 从插件列表中获取一个处理器抽象(T is ITermianlHandler, IPlatformHandler)
        /// </summary>
        /// <returns></returns>
        public static IHandlerBase Get(int flags)
        {
            IList<IHandlerBase> handlers = HandlerManager.Handlers;
            for (int i = 0; i < handlers.Count; i++)
            {
                IHandlerBase handler = handlers[i];
                if (handler.Flags == flags)
                    return handler;
            }
            return null;
        }
    }
}
