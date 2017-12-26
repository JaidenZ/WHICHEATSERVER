namespace WHICHEATSERVER.Mvc.Management
{
    using WHICHEATSERVER.Mvc.Context;
    using System;
    using System.Collections.Generic;

    public sealed class SNContext
    {
        /// <summary>
        /// 流水号
        /// </summary>
        public int SN;
        /// <summary>
        /// 连接号
        /// </summary>
        public long LinkNo;

        public DateTime LiveTime;

        public SNContext()
        {
            LiveTime = DateTime.Now;
        }
    }
}
