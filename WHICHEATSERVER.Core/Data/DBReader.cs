namespace WHICHEATSERVER.Core.Data
{
    using WHICHEATSERVER.Core.AOP.Data;
    using WHICHEATSERVER.Core.Data.Connection;
    using WHICHEATSERVER.Core.Data.Converter;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text;

    /// <summary>
    /// 数据库读取装置
    /// </summary>
    public partial class DBReader
    {
        private IList<T> ToList<T>(DataTable table) where T : class
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            DataRowCollection rows = table.Rows;
            if (rows.Count <= 0)
            {
                return new List<T>(0);
            }
            if (typeof(T) == typeof(object))
            {
                DataModelProxyConverter proxy = DataModelProxyConverter.GetInstance();
                return (IList<T>)proxy.ToList(table);
            }
            else
            {
                return DataModelConverter.ToList<T>(table);
            }
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        public DataTable Select(SqlCommand cmd)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }
            cmd.Connection = DBConnection.Current;
            lock (cmd.Connection)
            {
                try
                {
                    if (cmd.Connection.State != ConnectionState.Open)
                    {
                        cmd.Connection.Open();
                    }
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
                catch (SqlException e)
                {
                    if (!SDLHelper.Failback(e)) // 在MSSQL建立链接时出现网路相关问题
                    {
                        throw e; 
                    }
                    return this.Select(cmd);
                }
            }
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T">映射类型</typeparam>
        /// <param name="cmd">命令行</param>
        /// <returns></returns>
        public IList<T> Select<T>(SqlCommand cmd) where T : class
        {
            using (DataTable dt = this.Select(cmd))
            {
                return this.ToList<T>(dt);
            }
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sql">语句</param>
        /// <returns></returns>
        public DataTable Select(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentException("sql");
            }
            using (SqlCommand cmd = new SqlCommand(sql))
            {
                return this.Select(cmd);
            }
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sql">语句</param>
        /// <returns></returns>
        public DataTable Select(StringBuilder sql)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql");
            }
            return this.Select(Convert.ToString(sql));
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T">映射类型</typeparam>
        /// <param name="sql">语句</param>
        /// <returns></returns>
        public IList<T> Select<T>(string sql) where T : class
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentException("sql");
            }
            using (SqlCommand cmd = new SqlCommand(sql))
            {
                return this.Select<T>(cmd);
            }
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T">映射类型</typeparam>
        /// <param name="sql">语句</param>
        /// <returns></returns>
        public IList<T> Select<T>(StringBuilder sql) where T : class, new()
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql");
            }
            return this.Select<T>(Convert.ToString(sql));
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="procedure">存储过程</param>
        /// <param name="parameters">函数参数</param>
        /// <returns></returns>
        public DataTable Select(string procedure, params SqlParameter[] parameters)
        {
            if (string.IsNullOrEmpty(procedure))
            {
                throw new ArgumentException("procedure");
            }
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procedure;
                //
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                return this.Select(cmd);
            }
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedure">存储过程</param>
        /// <param name="parameters">函数参数</param>
        /// <returns></returns>
        public IList<T> Select<T>(string procedure, params SqlParameter[] parameters) where T : class, new()
        {
            using (DataTable dt = this.Select(procedure, parameters))
            {
                return this.ToList<T>(dt);
            }
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="procedure">存储过程</param>
        /// <param name="value">参数</param>
        /// <returns></returns>
        public DataTable Select(string procedure, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return this.Select(procedure, SDLHelper.GetParameters(value));
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="procedure">存储过程</param>
        /// <param name="value">参数</param>
        /// <returns></returns>
        public IList<T> Select<T>(string procedure, object value) where T : class, new()
        {
            using (DataTable dt = this.Select(procedure, value))
            {
                return this.ToList<T>(dt);
            }
        }

        /// <summary>
        /// 统计数量
        /// </summary>
        /// <param name="sql">语句</param>
        /// <returns></returns>
        public int Count(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentException("sql");
            }
            using (DataTable dt = this.Select(sql))
            {
                DataRowCollection rows = dt.Rows;
                if (rows.Count > 0)
                {
                    return Convert.ToInt32(rows[0][0]);
                }
                return default(int);
            }
        }

        /// <summary>
        /// 统计数量
        /// </summary>
        /// <param name="sql">语句</param>
        /// <returns></returns>
        public int Count(StringBuilder sql)
        {
            if (sql == null)
            {
                throw new ArgumentException("sql");
            }
            return this.Count(sql.ToString());
        }

        /// <summary>
        /// 统计数量
        /// </summary>
        /// <param name="table">表</param>
        /// <param name="where">条件</param>
        /// <returns></returns>
        public int Count(string table, string where)
        {
            string sql = string.Format("SELECT COUNT(1) FROM {0} ", table);
            if (!string.IsNullOrEmpty(where))
            {
                sql += string.Format("WHERE {0}", where);
            }
            return this.Count(sql);
        }
    }
}
