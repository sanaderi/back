namespace GamaEdtech.Domain.Specification.Message
{
    using System.Linq.Expressions;

    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Domain.Entity;

    public sealed class SenderIdEqualsSpecification(int senderId) : SpecificationBase<Message>
    {
        public override Expression<Func<Message, bool>> Expression() => (t) => t.SenderId == senderId;
    }
}
