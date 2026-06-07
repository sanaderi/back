namespace GamaEdtech.Domain.Specification.Identity
{
    using System.Linq.Expressions;

    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Domain.Entity;

    public sealed class FollowingIdEqualsSpecification(long followingId) : SpecificationBase<Connection>
    {
        public override Expression<Func<Connection, bool>> Expression() => (t) => t.DestinationUserId == followingId && t.Status == Enumeration.ConnectionStatus.Confirmed;
    }
}
