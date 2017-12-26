namespace WHICHEATSERVER.Core.DDD.Events
{
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;

    /// <summary>
    /// 领域事件总线
    /// </summary>
    public sealed class EventBus
    {
        private readonly ConcurrentDictionary<Type, Binding> m_binding = new ConcurrentDictionary<Type, Binding>(100, 300);

        private class Binding
        {
            public object sender;
            public Type declared;
            public Type eventtype;
            public Action<IEvent, Action<ICallback>> handler;
        }

        public static readonly EventBus Current = new EventBus();

        /// <summary>
        /// 订阅事件处理程序
        /// </summary>
        /// <typeparam name="TEvent">事件参数标识</typeparam>
        /// <param name="handler">事件处理程序</param>
        public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
        {
            if (handler == null)
                throw new ArgumentNullException("handler");
            if (!m_binding.ContainsKey(typeof(TEvent)))
            {
                Type clazz = handler.GetType();
                Type key = typeof(TEvent);
                MethodInfo mi = clazz.GetMethod("Handle");

                Binding binding = new Binding();
                binding.sender = handler;
                binding.declared = clazz;
                binding.eventtype = key;
                binding.handler = (Action<IEvent, Action<ICallback>>)Activator.CreateInstance(typeof(Action<IEvent, Action<ICallback>>), handler, mi.MethodHandle.GetFunctionPointer());

                if (!m_binding.TryAdd(key, binding))
                    throw new InvalidProgramException("binding");
            }
        }
        
        /// <summary>
        /// 发布用户事件源
        /// </summary>
        /// <typeparam name="T">事件处理器回调参数类型</typeparam>
        /// <param name="e">事件对象</param>
        /// <param name="callback">回调函数</param>
        public void Publish<T>(IEvent e, Action<T> callback) where T : class, ICallback
        {
            if (e == null)
                throw new ArgumentNullException("e");
            Type clazz = e.GetType();
            Binding binding = null;
            if (m_binding.TryGetValue(clazz, out binding))
            {
                Action<IEvent, Action<ICallback>> handler = binding.handler;
                handler(e, (r) =>
                {
                    if (callback != null)
                        callback(r as T);
                });
            }
        }
    }
}
