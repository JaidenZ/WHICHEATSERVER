namespace WHICHEATSERVER.Core.Data.Repository
{
    using WHICHEATSERVER.Core.Data.Specifications;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : AggregateRoot
    {
        private string _table = null;
        /// <summary>
        /// 仓储的工作区域
        /// </summary>
        public string TableName
        {
            get
            {
                return _table;
            }
        }
        /// <summary>
        /// 实例化一个数据仓储
        /// </summary>
        /// <param name="table">作用区域</param>
        public Repository(string table)
        {
            if (string.IsNullOrEmpty(table))
            {
                throw new ArgumentNullException("table");
            }
            _table = table;
        }

        public virtual int Add(TEntity entity)
        {
            return this.Add(entity as AggregateRoot);
        }

        public virtual int Update(TEntity entity)
        {
            return this.Update(entity as AggregateRoot);
        }

        public virtual int Remove(TEntity entity)
        {
            return this.Remove(entity as AggregateRoot);
        }

        public virtual int Add(AggregateRoot entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException();
            }
            return this.OnAddToCollection(new DBWriter(), entity);
        }

        public virtual int Update(AggregateRoot entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException();
            }
            return this.OnUpdateToCollection(new DBWriter(), entity);
        }

        public virtual int Remove(AggregateRoot entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException();
            }
            return this.OnRemoveToCollection(new DBWriter(), entity);
        }
        /// <summary>
        /// 添加到集合
        /// </summary>
        /// <param name="writer">数据写入器</param>
        /// <param name="entity">实体聚合根</param>
        protected abstract int OnAddToCollection(DBWriter writer, AggregateRoot entity);
        /// <summary>
        /// 从集合中移除
        /// </summary>
        /// <param name="writer">数据写入器</param>
        /// <param name="entity">实体聚合根</param>
        protected abstract int OnRemoveToCollection(DBWriter writer, AggregateRoot entity);
        /// <summary>
        /// 更新到集合
        /// </summary>
        /// <param name="writer">数据写入器</param>
        /// <param name="entity">实体聚合根</param>
        protected abstract int OnUpdateToCollection(DBWriter writer, AggregateRoot entity);
        /// <summary>
        /// 查询全部满足规约的对象
        /// </summary>
        /// <typeparam name="T">输出类型</typeparam>
        /// <param name="specification">检索的规约</param>
        /// <returns></returns>
        public virtual IList<T> FindAll<T>(ISpecification<T> specification) where T : AggregateRoot
        {
            if (specification == null)
            {
                throw new ArgumentNullException();
            }
            DBReader r = new DBReader();
            return r.Select<T>(specification.SatisfiedBy());
        }
        /// <summary>
        /// 查询单个满足规约的对象
        /// </summary>
        /// <typeparam name="T">输出类型</typeparam>
        /// <param name="specification">检索的规约</param>
        /// <returns></returns>
        public virtual T Find<T>(ISpecification<T> specification) where T : AggregateRoot
        {
            IList<T> values = FindAll<T>(specification);
            if (values == null || values.Count <= 0)
            {
                return null;
            }
            return values[0];
        }
    }
}
