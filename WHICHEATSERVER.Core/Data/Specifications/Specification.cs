namespace WHICHEATSERVER.Core.Data.Specifications
{
    using System;
    using System.Linq.Expressions;

    public abstract class Specification<T> : ISpecification<T>
    {
        public abstract string SatisfiedBy();

        public ISpecification<T> Count<E>(Expression<Func<E, object>> express, string alias)
        {
            return new CountSpecification<T, E>(this, express, alias);
        }

        public ISpecification<T> OrderBy<E>(Expression<Func<E, object>> express, bool descending)
        {
            return new OrderBySpecification<T, E>(this, express, descending);
        }

        public ISpecification<T> Top(int? count)
        {
            return new TopSpecification<T>(this, count);
        }

        public ISpecification<T> Where(Expression<Func<T, bool>> express)
        {
            return new WhereSpecification<T>(this, express);
        }
    }
}
