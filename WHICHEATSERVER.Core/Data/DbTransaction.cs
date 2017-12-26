namespace WHICHEATSERVER.Core.Data
{
    using System;
    using System.Data;

    /// <summary>
    /// 数据库事务处理基类,对数据库提供统一管理  只有出现异常才会回滚 例如：影响行数为0  主动抛出异常 便会回滚
    /// </summary>
    public abstract class DbTransaction
    {
        /// <summary>
        /// IDbTransaction 事务对象
        /// </summary>
        private IDbTransaction transaction = null;

        /// <summary>
        /// IDbConnection 连接对象
        /// </summary>
        private IDbConnection connection = null;

        /// <summary>
        /// 是否启数据库动事务（IDbConnection.BeginTransaction）
        /// </summary>
        private bool isBeginTransaction = true;

        /// <summary>
        /// Transaction 事务隔离级别
        /// </summary>
        private IsolationLevel isolationLevel;

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public Exception ErrorInfo { get; private set; }

        /// <summary>
        /// 无参数构造函数(初始化TransaionBase)
        /// </summary>
        public DbTransaction()
        {
            this.isolationLevel = IsolationLevel.ReadCommitted;
        }

        /// <summary>
        /// 构造函数，提供DbConnection 连接对象
        /// </summary>
        /// <param name="conn">IDbConnection 连接对象</param>
        public DbTransaction(IDbConnection conn)
            : this()
        {
            this.connection = conn;
        }

        /// <summary>
        /// 获得或者设置 IDbTransaction 事务对象
        /// </summary>
        protected IDbTransaction Transaction
        {
            get
            {
                return this.transaction;
            }
        }

        /// <summary>
        /// IDbConnection 连接对象
        /// </summary>
        protected IDbConnection Connection
        {
            get
            {
                return this.connection;
            }

            set
            {
                this.connection = value;
            }
        }

        /// <summary>
        /// 是否启数据库动事务（IDbConnection.BeginTransaction）
        /// </summary>
        protected bool IsBeginTransaction
        {
            get
            {
                return this.isBeginTransaction;
            }

            set
            {
                this.isBeginTransaction = value;
            }
        }

        /// <summary>
        /// Transaction 事务隔离级别
        /// </summary>
        protected IsolationLevel IsolationLevel
        {
            get
            {
                return this.isolationLevel;
            }

            set
            {
                this.isolationLevel = value;
            }
        }

        /// <summary>
        /// 执行（Execute）
        /// </summary>
        public void Execute()
        {

            try
            {
                this.connection.Open();

                if (this.isBeginTransaction)
                {
                    this.PreTransactionBegin();

                    this.transaction = this.connection.BeginTransaction(this.isolationLevel);
                }

                this.ExecuteMethods();

                if (this.transaction != null)
                {
                    this.transaction.Commit();

                    this.Committed();
                }

                this.IsSuccess = true;
            }
            catch (Exception ex)
            {
                if (this.transaction != null)
                {
                    this.transaction.Rollback();

                    this.Rollbacked();
                }

                ErrorInfo = ex;
            }
            finally
            {
                if (this.transaction != null)
                {
                    this.transaction.Dispose();
                }

                if (this.connection != null)
                {
                    this.connection.Close();
                }
            }
        }

        /// <summary>
        /// ExecuteMethods业务处理函数
        /// </summary>
        protected abstract void ExecuteMethods();

        /// <summary>
        /// 事务提交后处理动作，如果不要求开启事物(isBeginTransaction=false)将不会执行此方法
        /// </summary>
        protected virtual void Committed()
        {
        }

        /// <summary>
        /// 事务回滚后处理动作，如果不要求开启事物(isBeginTransaction=false)将不会执行此方法
        /// </summary>
        protected virtual void Rollbacked()
        {
        }

        /// <summary>
        /// 开启事务（BeginTransaction）之前执行的方法，如果不要求开启事物(isBeginTransaction=false)将不会执行此方法
        /// </summary>
        protected virtual void PreTransactionBegin()
        {
        }
    }
}
