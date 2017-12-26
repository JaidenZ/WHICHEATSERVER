namespace WHICHEATSERVER.Mvc.Management
{
    using WHICHEATSERVER.Core.Component;
    using WHICHEATSERVER.Mvc.Controller;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Collections.Concurrent;

    /// <summary>
    /// 控制器对象容器
    /// </summary>
    public partial class ControllerContainer
    {
        private readonly static IDictionary<int, object> m_controllers = null;

        static ControllerContainer()
        {
            ControllerContainer.m_controllers = new Dictionary<int, object>(300);
        }
        /// <summary>
        /// 导入控制器
        /// </summary>
        /// <param name="assembly"></param>
        public static Assembly Load(FileInfo file)
        {
            Contract.Requires<IOException>(file != null && file.Exists);
            Assembly assembly = Assembly.LoadFrom(file.FullName);
            ControllerContainer.Load(assembly);
            return assembly;
        }

        /// <summary>
        /// 导入控制器
        /// </summary>
        /// <param name="assembly"></param>
        public static void Load(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentException("assembly");
            foreach (Type clazz in assembly.GetExportedTypes())
                if (typeof(IController).IsAssignableFrom(clazz))
                {
                    ConstructorInfo ctor = clazz.GetConstructor(Type.EmptyTypes);
                    if (ctor == null)
                        continue;
                    ControllerAttribute attr = ControllerAttribute.Get(clazz);
                    if (attr != null)
                    {
                        if (ControllerContainer.m_controllers.ContainsKey(attr.RequestCommands))
                        {
                            throw new ArgumentException("key");
                        }
                        ControllerContainer.m_controllers.Add(attr.RequestCommands, ctor.Invoke(null));
                    }
                }
        }

        /// <summary>
        /// 获取控制器(T is TerminalController, PlatformController)
        /// </summary>
        /// <returns></returns>
        public static T Get<T>(int cmd) where T : class
        {
            object obj = null;
            if (ControllerContainer.m_controllers.TryGetValue(cmd, out obj))
                return (T)obj;
            return default(T);
        }
    }
}
