namespace WHICHEATSERVER.Core.Data.Repository
{
    using WHICHEATSERVER.Core.Data.Specifications;
    using System.Collections.Generic;

    public interface IQueryable<TEntity> where TEntity : AggregateRoot
    {        
        /// <summary>
        /// 查询全部满足规约的对象
        /// </summary>
        /// <typeparam name="T">输出类型</typeparam>
        /// <param name="specification">检索的规约</param>
        /// <returns></returns>
        IList<T> FindAll<T>(ISpecification<T> specification) where T : AggregateRoot;
        /// <summary>
        /// 查询单个满足规约的对象
        /// </summary>
        /// <typeparam name="T">输出类型</typeparam>
        /// <param name="specification">检索的规约</param>
        /// <returns></returns>
        T Find<T>(ISpecification<T> specification) where T : AggregateRoot;
    }
}
