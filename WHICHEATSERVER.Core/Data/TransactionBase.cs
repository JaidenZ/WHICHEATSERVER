namespace WHICHEATSERVER.Core.Data
{
    using System;

    public abstract class TransactionBase
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess = false;

        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception ExceptionMsg = null;

        private DBWriter _DBWriter = null;

        private DBReader _DBReader = null;

        public TransactionBase()
        {

        }

        public void Execute()
        {
            try
            {
                _DBReader = new DBReader();
                PreTransactionBegin(_DBReader);

                _DBWriter = new DBWriter();
                ExecuteMethods(_DBWriter);

                _DBWriter.Commit();
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                _DBWriter.Rollback();
                ExceptionMsg = ex;
            }
            finally
            {
                if (_DBWriter != null)
                {
                    _DBWriter.Dispose();
                }
            }
        }

        protected abstract void ExecuteMethods(DBWriter dBWriter);

        /// <summary>
        /// 事务开启前执行
        /// </summary>
        protected virtual void PreTransactionBegin(DBReader dBReader)
        {
        }
    }
}
