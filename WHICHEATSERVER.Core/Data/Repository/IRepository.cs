namespace WHICHEATSERVER.Core.Data.Repository
{
    using System.Collections;

    /// <summary>
    /// 领域仓储服务实现规约
    /// </summary>
    /// <typeparam name="TEntity">仓储实体类型</typeparam>
    public interface IRepository<TEntity> : IQueryable<TEntity> where TEntity : AggregateRoot
    {
        /// <summary>
        /// 添加到仓储
        /// </summary>
        /// <param name="entity">仓储实体类型</param>
        /// <returns></returns>
        int Add(TEntity entity);
        /// <summary>
        /// 保存到仓库
        /// </summary>
        /// <param name="entity">仓储实体类型</param>
        /// <returns></returns>
        int Update(TEntity entity);
        /// <summary>
        /// 从仓储中移除
        /// </summary>
        /// <param name="entity">仓储实体类型</param>
        /// <returns></returns>
        int Remove(TEntity entity);
        /// <summary>
        /// 添加到仓储(为了复杂的多表类型存储，做出的设计上的让步尽管如此你也必须要限定仓储的工作范围）
        /// </summary>
        /// <param name="entity">仓储实体类型</param>
        /// <returns></returns>
        int Add(AggregateRoot entity);
        /// <summary>
        /// 保存到仓库(为了复杂的多表类型存储，做出的设计上的让步尽管如此你也必须要限定仓储的工作范围）
        /// </summary>
        /// <param name="entity">仓储实体类型</param>
        /// <returns></returns>
        int Update(AggregateRoot entity);
        /// <summary>
        /// 从仓储中移除(为了复杂的多表类型存储，做出的设计上的让步尽管如此你也必须要限定仓储的工作范围）
        /// </summary>
        /// <param name="entity">仓储实体类型</param>
        /// <returns></returns>
        int Remove(AggregateRoot entity);
    }
}
