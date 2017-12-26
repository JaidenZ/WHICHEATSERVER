namespace WHICHEATSERVER.Core.AOP.Proxy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public static class Program
    {
        public interface IFoo
        {
            void Say();
        }

        public class FooInvocationHandler : InvocationHandler
        {
            object InvocationHandler.InvokeMember(object obj, int rid, string name, params object[] args)
            {
                MethodBase met = typeof(IFoo).Module.ResolveMethod(rid);
                if (met.Name == "Say")
                {
                    Console.WriteLine("hello world!");
                }
                return null;
            }
        }

        static void Main()
        {
            IFoo foo = InterfaceProxy.New<IFoo>(new FooInvocationHandler());
            foo.Say();
            Console.ReadKey(false);
        }
    }
}
