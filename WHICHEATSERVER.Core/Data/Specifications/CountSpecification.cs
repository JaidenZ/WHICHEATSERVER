namespace WHICHEATSERVER.Core.Data.Specifications
{
    using System;
    using System.Linq.Expressions;

    public sealed class CountSpecification<T,E> : Specification<T>
    {

        private ISpecification<T> _ecx = null;
        private Expression<Func<E, object>> _exp = null;
        private string _alias = null;

        internal CountSpecification(ISpecification<T> left, Expression<Func<E,object>> express, string alias)
        {
            _ecx = left;
            _exp = express;
            _alias = alias;
        }

        public override string SatisfiedBy()
        {
            string sql = _ecx.SatisfiedBy();
            sql += string.Format("COUNT({0}) ", ExpressionSpecification.SatisfiedBy(_exp));
            if (!string.IsNullOrEmpty(_alias))
            {
                sql += string.Format("AS [{0}] ", _alias);
            }
            return sql;
        }


    }
}
