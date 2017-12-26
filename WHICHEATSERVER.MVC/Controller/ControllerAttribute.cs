namespace WHICHEATSERVER.Mvc.Controller
{
    using System;

    /// <summary>
    /// 控制器请求特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ControllerAttribute : Attribute
    {
        /// <summary>
        /// 控制器接收的请求命令代码
        /// </summary>
        public int RequestCommands
        {
            get;
            set;
        }

        /// <summary>
        /// 控制器响应的请求代码
        /// </summary>
        public int ResponseCommands
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置对象的标记
        /// </summary>
        public object Tag
        {
            get;
            set;
        }

        /// <summary>
        /// 请求的控制器名
        /// </summary>
        public string RequestName
        {
            get;
            set;
        }

        /// <summary>
        /// 允许请求映射
        /// </summary>
        [Obsolete("此属性已废弃，请各位同僚在工程中移除对此属性的依赖 This deprecated attribute members please in the project removes the dependency on this property")]
        public bool RequestMapping
        {
            get;
            set;
        }

        public static ControllerAttribute Get<T>()
        {
            return ControllerAttribute.Get(typeof(T));
        }

        public static ControllerAttribute Get(Type clazz)
        {
            if (clazz == null)
            {
                return null;
            }
            object[] attr = clazz.GetCustomAttributes(typeof(ControllerAttribute), false);
            if (attr.Length <= 0)
            {
                return null;
            }
            return attr[0] as ControllerAttribute;
        }
    }
}
