namespace WHICHEATSERVER.Core.Data
{
    using Core.Utilits;
    using WHICHEATSERVER.Core.Data.Connection;
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Reflection;

    public static partial class SDLHelper
    {
        /// <summary>
        /// 生成一个Insert语句
        /// </summary>
        /// <returns></returns>
        public static SqlCommand CreateInsert(object value, string table)
        {
            if (value == null || string.IsNullOrEmpty(table))
            {
                throw new ArgumentException();
            }
            string sql = "INSERT INTO {0}({1}) VALUES({2})";
            SqlCommand cmd = new SqlCommand();

            SqlParameterCollection args = cmd.Parameters;
            PropertyInfo[] props = (value.GetType()).GetProperties();
            int len = props.Length - 1;
            string fileds = null, values = null;
            for (int i = 0; i <= len; i++)
            {
                PropertyInfo prop = props[i];
                if (i >= len)
                {
                    values += ("@" + prop.Name);
                    fileds += string.Format("[{0}]", prop.Name);
                }
                else
                {
                    values += string.Format("@{0},", prop.Name);
                    fileds += string.Format("[{0}],", prop.Name);
                }
                object val = prop.GetValue(value, null);
                if (val == null)
                {
                    val = DBNull.Value;
                }
                args.Add(new SqlParameter(string.Format("@{0}", prop.Name), val));
            }
            sql = string.Format(sql, table, fileds, values);
            cmd.CommandText = sql;
            return cmd;
        }

        /// <summary>
        /// 创建更新执行语句(动态)
        /// </summary>
        /// <param name="value">映射的对象</param>
        /// <param name="table">表</param>
        /// <param name="key">主键</param>
        /// <param name="where">条件</param>
        /// <returns></returns>
        public static SqlCommand CreateUpdate(object value, string table, string key, string where)
        {
            if (value == null || string.IsNullOrEmpty(table) || string.IsNullOrEmpty(key))
            {
                throw new ArgumentException();
            }
            string when = string.Empty, sql = string.Format("UPDATE {0} SET", table);
            SqlCommand cmd = new SqlCommand();
            SqlParameterCollection args = cmd.Parameters;
            PropertyInfo[] props = (value.GetType()).GetProperties();
            int len = props.Length - 1;
            for (int i = 0; i <= len; i++)
            {
                PropertyInfo prop = props[i];
                if (prop.Name != key)
                {
                    sql += string.Format(i >= len ? "[{0}]=@{1}" : " [{0}]=@{1},", prop.Name, prop.Name);
                }
                else
                {
                    when += string.Format(" WHERE {0}=@{1}", key, key);
                }
                object val = prop.GetValue(value, null);
                if (val == null)
                {
                    val = DBNull.Value;
                }
                args.Add(new SqlParameter(string.Format("@{0}", prop.Name), val));
            }
            if (!string.IsNullOrEmpty(where))
            {
                when += string.Format(" AND {0} ", where);
            }
            cmd.CommandText = (sql += when);
            return cmd;
        }
    }

    public static partial class SDLHelper
    {
        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <returns></returns>
        public static int ExecuteNonQuery(SqlCommand cmd, SqlConnection connection)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            try
            {
                cmd.Connection = connection;
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                return cmd.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                if (!SDLHelper.Failback(e))
                {
                    throw e;
                }
                return SDLHelper.ExecuteNonQuery(cmd, connection);
            }
        }

        /// <summary>
        /// 执行SQL语句  返回首行首列
        /// </summary>
        /// <returns></returns>
        public static object ExecuteScalar(SqlCommand cmd, SqlConnection connection)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            try
            {
                cmd.Connection = connection;
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                return cmd.ExecuteScalar();
            }
            catch (SqlException e)
            {
                if (!SDLHelper.Failback(e))
                {
                    throw e;
                }
                return SDLHelper.ExecuteScalar(cmd, connection);
            }
        }
    }

    public static partial class SDLHelper
    {
        /// <summary>
        /// 获取SQL参数
        /// </summary>
        /// <param name="value">参数</param>
        /// <returns></returns>
        public static SqlParameter[] GetParameters(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type clazz = value.GetType();
            PropertyInfo[] properties = clazz.GetProperties();
            SqlParameter[] parameters = new SqlParameter[properties.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                PropertyInfo prop = properties[i];
                object obj = prop.GetValue(value, null);
                if (obj == null)
                {
                    obj = DBNull.Value;
                }
                parameters[i] = new SqlParameter(string.Format("@{0}", prop.Name), obj);
            }
            return parameters;
        }

        /// <summary>
        /// 获取SQL参数
        /// </summary>
        /// <param name="procedure">存储过程</param>
        /// <returns></returns>
        public static SqlParameter[] GetParameters(string procedure)
        {
            SqlConnection connection = DBConnection.Current;
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = procedure;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = connection;
                //
                SqlCommandBuilder.DeriveParameters(cmd); // 探测存储过程参数
                //
                SqlParameterCollection args = cmd.Parameters;
                SqlParameter[] buffer = new SqlParameter[args.Count];
                for (int i = 0; i < args.Count; i++) // 交接且设置默认值
                {
                    SqlParameter arg = args[i];
                    arg.Value = DBNull.Value;
                    buffer[i] = arg;
                }
                return buffer;
            }
        }
    }

    /// <summary>
    /// 过滤器
    /// </summary>
    public static partial class SDLHelper
    {
        /// <summary>
        /// 检查sql关键字 目前主要检查是否含有 '
        /// </summary>
        /// <param name="args">参数</param>
        public static void RequireParameter(params string[] args)
        {
            if (!args.IsNullOrEmpty<string>())
            {
                return;
            }

            foreach (var item in args)
            {
                if (item.IndexOf("'") > 0)
                {
                    throw new Exception(string.Format("{0},数据无效", item));
                }
            }
        }

        /// <summary>
        /// 参数验证
        /// </summary>
        /// <param name="obj"></param>
        public static void RequireParameter(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            Type type = obj.GetType();
            foreach (PropertyInfo item in type.GetProperties())
            {
                Type itemtype = item.PropertyType;
                if (itemtype == typeof(string))
                {
                    var value = item.GetValue(obj, null);
                    if (value != null)
                    {
                        RequireParameter(value.ToString());
                    }
                }
            }
        }

        public static bool Failback(SqlException exception)
        {
            if (exception == null)
                return false;
            return exception.Class >= 20; // 严重错误
        }
    }
}
