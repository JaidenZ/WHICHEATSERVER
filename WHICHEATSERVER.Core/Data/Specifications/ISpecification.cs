namespace WHICHEATSERVER.Core.Data.Specifications
{
    using System;
    using System.Linq.Expressions;

    public interface ISpecification<T>
    {
        string SatisfiedBy();

        ISpecification<T> OrderBy<E>(Expression<Func<E, object>> express, bool descending);

        ISpecification<T> Where(Expression<Func<T, bool>> express);

        ISpecification<T> Top(int? count);

        ISpecification<T> Count<E>(Expression<Func<E, object>> express, string alias);

    }
}
