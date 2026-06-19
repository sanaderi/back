namespace GamaEdtech.Domain.Specification.Message
{
    using System.Linq.Expressions;

    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Domain.Entity;

    public sealed class IsReadEqualsSpecification(bool isRead) : SpecificationBase<Message>
    {
        public override Expression<Func<Message, bool>> Expression() => (t) => t.IsRead == isRead;
    }
}
