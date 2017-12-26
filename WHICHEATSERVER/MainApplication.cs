namespace WHICHEATSERVER
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using WHICHEATSERVER.Core.Tools;
    using WHICHEATSERVER.Core.Data;
    using WHICHEATSERVER.Core.Data.Connection;
    using Core.Network;
    using Core.Network.Tcp;
    using Mvc;
    using Core.Component;
    using System.Text.RegularExpressions;
    using System.Diagnostics;
    using System.Text;
    using Mvc.Context;

    public partial class MainApplication
    {

        static ICommunication _communication;

        [STAThread]
        static void Main(string[] args)
        {
            SystemConfigModel config = SystemConfigLoader.Current;
            MainApplication.Prepared(config);
            Console.WriteLine("Server is prepared...");
            while (true)
            {
                string command = Console.ReadLine();
                Help(command);
                Clear(command);
                Exit(command);
            }
            Console.ReadKey(false);
        }



        public static void Prepared(SystemConfigModel config)
        {
            DBConnection.Current.Database = config.DataBase;
            DBConnection.Current.Server = config.DataBaseIp + "," + config.DataBasePort;
            DBConnection.Current.LoginUser = config.DataBaseUserName;
            DBConnection.Current.Password = config.DataBaseUserPwd;

            _communication = new TcpCommunication();
            _communication.Connect(config.GatewayServerIp, config.GatewayServerPort);

            MvcApplication.RegisterCommunication(_communication);
            try
            {

            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);

            }
            finally
            {
                MvcApplication.Run("zhj", Deployement);
            }

        }

        private static void Deployement()
        {
            try
            {
                Thread work = Thread.CurrentThread;
                lock (work)
                {
                    if (WorkThreadDictionary.Get(work) == null)
                    {
                        WorkThreadDictionary.Create(Thread.CurrentThread); // 对当前线程部署工作容器
                        DBConnection.Current.Deployment(); // 对当前线程部署数据库连接（长连接）
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }


    /// <summary>
    /// CMD
    /// </summary>
    public partial class MainApplication
    {
        private static void Help(string command)
        {
            foreach (string cmd in new string[] { "\\?", "/help", "help" })
            {
                if (Regex.IsMatch(command, cmd, RegexOptions.IgnoreCase))
                {
                    Console.WriteLine("cls                                      清屏");
                    Console.WriteLine("lc                                       加载缓存");
                    Console.WriteLine("exit                                     退出服务器程序");
                    Console.WriteLine();
                }
            }
        }

        private static void Clear(string command)
        {
            if (Regex.IsMatch(command, "cls", RegexOptions.IgnoreCase))
                Console.Clear();
        }

        private static void Exit(string command)
        {
            if (Regex.IsMatch(command, "exit", RegexOptions.IgnoreCase))
            {
                Process.GetCurrentProcess().Kill();
            }
        }


    }

}
