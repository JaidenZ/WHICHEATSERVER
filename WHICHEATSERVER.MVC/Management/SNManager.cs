namespace WHICHEATSERVER.Mvc.Management
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Concurrent;

    /// <summary>
    /// 流水号管理
    /// </summary>
    internal partial class SNManager
    {
        private static ConcurrentDictionary<long, SNContext> Contexts = new ConcurrentDictionary<long, SNContext>(300, 100000);

        /// <summary>
        /// 获取SN正文
        /// </summary>
        /// <returns></returns>
        public static SNContext Get(long linkNo, bool canadd = true)
        {
            DateTime now = DateTime.Now;
            SNContext sn = null;
            foreach(KeyValuePair< long, SNContext> pair in Contexts)
            {
                sn = pair.Value;
                if (sn.LinkNo == linkNo)
                {
                    sn.LiveTime = DateTime.Now;
                    return sn;
                }
                else if ((now - sn.LiveTime).TotalSeconds > 60) 
                {
                    SNContext sc = null;
                    Contexts.TryRemove(linkNo, out sc);
                }
            }
            if (canadd)
            {
                sn = new SNContext() { LinkNo = linkNo };
                SNManager.Contexts.TryAdd(linkNo, sn);
                return sn;
            }
            return null;
        }
    }
}
