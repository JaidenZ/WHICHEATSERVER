namespace WHICHEATSERVER.Core.Data.Specifications
{
    using System;
    using System.Linq.Expressions;

    public sealed class WhereSpecification<T> : Specification<T>
    {
        private ISpecification<T> _ecx = null;
        private Expression<Func<T, bool>> _exp = null;

        internal WhereSpecification(ISpecification<T> left, Expression<Func<T, bool>> express)
        {
            _ecx = left;
            _exp = express;
        }

        public override string SatisfiedBy()
        {
            string sql = _ecx.SatisfiedBy();
            string where = null;
            //
            if (_exp != null)
            {
                where = ExpressionSpecification.SatisfiedBy(_exp);
            }
            if (!string.IsNullOrEmpty(where))
            {
                where = string.Format("WHERE {0} ", where);
            }
            sql += where;
            return sql;
        }

    }
}
