namespace WHICHEATSERVER.Core.Packets
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public class GroupPackage
    {
        public static Dictionary<string, PacketInfo> _Dic = new Dictionary<string, PacketInfo>();

        private static Timer _Timer = null;

        private static object _Lock = new object();
        public static void AddPackages<T>(string key, int allcount, IList<T> datas, Action action)
        {
            lock (_Lock)
            {
                if (_Dic.ContainsKey(key))
                {
                    ((List<T>)_Dic[key].Data).AddRange(datas);
                }
                else
                {
                    _Dic.Add(key, new PacketInfo() { CallBack = action, AllCount = allcount, Data = datas, ExpirationDate = DateTime.Now.AddMinutes(1) });
                }

                PacketInfo temp = _Dic[key];
                if (temp.CallBack != null && temp.AllCount == ((List<T>)temp.Data).Count)
                {
                    action.Invoke();
                }
            }
        }

        static GroupPackage()
        {
            _Timer = new Timer(CheckExpirationDate, null, 0, 1000 * 60);
        }

        public static PacketInfo GetPacketInfo(string key)
        {
            lock (_Lock)
            {
                if (!_Dic.ContainsKey(key))
                {
                    return null;
                }

                return _Dic[key];
            }
        }

        public static List<T> GetDatas<T>(string key)
        {
            lock (_Lock)
            {
                if (!_Dic.ContainsKey(key))
                {
                    return null;
                }

                PacketInfo temp = _Dic[key];

                if (temp.Data == null)
                {
                    return null;
                }

                return (List<T>)temp.Data;
            }
        }

        private static void CheckExpirationDate(object obj)
        {
            lock (_Lock)
            {
                IList<string> keys = new List<string>();
                foreach (var item in _Dic)
                {
                    if (DateTime.Now > item.Value.ExpirationDate)
                    {
                        keys.Add(item.Key);
                    }
                }

                foreach (var item in keys)
                {
                    _Dic.Remove(item);
                }
            }
        }
    }


    public class PacketInfo
    {
        public int AllCount { get; set; }

        /// <summary>
        /// 失效时间
        /// </summary>
        public DateTime ExpirationDate { get; set; }

        public object Data { get; set; }

        public Action CallBack { get; set; }
    }
}
