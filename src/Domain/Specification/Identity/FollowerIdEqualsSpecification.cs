namespace GamaEdtech.Domain.Specification.Identity
{
    using System.Linq.Expressions;

    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Domain.Entity;

    public sealed class FollowerIdEqualsSpecification(long followerId) : SpecificationBase<Connection>
    {
        public override Expression<Func<Connection, bool>> Expression() => (t) => t.SourceUserId == followerId && t.Status == Enumeration.ConnectionStatus.Confirmed;
    }
}
