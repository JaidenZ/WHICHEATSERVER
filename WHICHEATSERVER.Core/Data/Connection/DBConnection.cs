namespace WHICHEATSERVER.Core.Data.Connection
{
    using Component;
    using System;
    using System.Data;
    using System.Data.SqlClient;

    public sealed partial class DBConnection
    {
        private static DBConnection DB_CONNECTION = null;

        public static DBConnection Current
        {
            get
            {
                if (DB_CONNECTION == null)
                {
                    DB_CONNECTION = new DBConnection();
                }
                return DB_CONNECTION;
            }
        }

        private DBConnection() { }
    }

    public partial class DBConnection
    {
        /// <summary>
        /// 服务器
        /// </summary>
        public string Server
        {
            get;
            set;
        }
        /// <summary>
        /// 数据库
        /// </summary>
        public string Database
        {
            get;
            set;
        }
        /// <summary>
        /// 登陆用户
        /// </summary>
        public string LoginUser
        {
            get;
            set;
        }
        /// <summary>
        /// 登陆密码
        /// </summary>
        public string Password
        {
            get;
            set;
        }

        public override string ToString()
        {
            return string.Format("Data Source={0}; Initial Catalog={1}; User Id={2}; Password={3};",
                this.Server, this.Database, this.LoginUser, this.Password);
        }

        public static implicit operator string(DBConnection value)
        {
            if (value == null)
                return null;
            return value.ToString();
        }

        private const string WORK_KEY_NAME = "DBConnection";

        public SqlConnection Deployment()
        {
            WorkThreadDictionary work = WorkThreadDictionary.Get();
            SqlConnection connection = null;
            if (work != null)
            {
                connection = work.Get<SqlConnection>(WORK_KEY_NAME);
            }
            if (connection == null)
            {
                connection = new SqlConnection();
                if (work != null)
                {
                    work.Set(WORK_KEY_NAME, connection);
                }
                //connection.FireInfoMessageEventOnUserErrors = true;
                connection.Disposed += Connection_Disposed;
                //connection.InfoMessage += Connection_InfoMessage;
            }
            if (connection.State != ConnectionState.Open)
            {
                connection.ConnectionString = this.ToString();
            }
            return connection;
        }

        private static void Connection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            using (SqlConnection connection = (SqlConnection)sender)
            {
                Connection_Disposed(sender, e);
            }
        }

        public static implicit operator SqlConnection(DBConnection value)
        {
            if (value == null)
            {
                return null;
            }
            return value.Deployment();
        }

        private static void Connection_Disposed(object sender, EventArgs e)
        {
            WorkThreadDictionary work = WorkThreadDictionary.Get();
            if (work != null)
            {
                SqlConnection connection = new SqlConnection();
                work.Set(WORK_KEY_NAME, connection);
            }
        }

    }
}
