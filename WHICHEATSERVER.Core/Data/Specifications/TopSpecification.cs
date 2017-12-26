namespace WHICHEATSERVER.Core.Data.Specifications
{
    public sealed class TopSpecification<T> : Specification<T>
    {
        private ISpecification<T> _ecx = null;
        private int? _count = null;

        internal TopSpecification(ISpecification<T> left,int? count)
        {
            _ecx = left;
            _count = count;
        }

        public override string SatisfiedBy()
        {
            string sql = _ecx.SatisfiedBy();
            if(_count != null)
            {
                sql += string.Format("TOP {0} ", _count);
            }
            return sql;
        }
    }
}
