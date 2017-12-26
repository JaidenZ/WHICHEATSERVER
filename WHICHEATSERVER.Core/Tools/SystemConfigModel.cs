namespace WHICHEATSERVER.Core.Tools
{
    public class SystemConfigModel
    {
        /// <summary>
        /// 网关服务器IP
        /// </summary>
        public string GatewayServerIp { get; set; }
        /// <summary>
        /// 网关服务器端口
        /// </summary>
        public int GatewayServerPort { get; set; }
        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DataBase { get; set; }
        /// <summary>
        /// 数据库地址
        /// </summary>
        public string DataBaseIp { get; set; }
        /// <summary>
        /// 基础数据库端口
        /// </summary>
        public int DataBasePort { get; set; }
        /// <summary>
        /// 基础数据库用户名
        /// </summary>
        public string DataBaseUserName { get; set; }
        /// <summary>
        /// 基础数据库密码
        /// </summary>
        public string DataBaseUserPwd { get; set; }

    }
}
