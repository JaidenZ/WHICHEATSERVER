namespace WHICHEATSERVER.Core.Data.Specifications
{
    using System;
    using System.Linq.Expressions;

    public sealed class OrderBySpecification<T,E>:Specification<T>
    {

        private ISpecification<T> _ecx = null;
        private Expression<Func<E, object>> _exp = null;
        private bool _descending = false;

        internal OrderBySpecification(ISpecification<T> left, Expression<Func<E, object>> express, bool descending)
        {
            _ecx = left;
            _exp = express;
            _descending = descending;
        }

        public override string SatisfiedBy()
        {
            string sql = _ecx.SatisfiedBy();
            string orderby = ExpressionSpecification.SatisfiedBy(_exp);
            sql += string.Format("ORDER BY {0} {1} ", orderby, (_descending ? "DESC" : "ASC"));
            return sql;
        }

    }
}
