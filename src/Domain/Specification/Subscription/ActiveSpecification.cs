namespace GamaEdtech.Domain.Specification.Subscription
{
    using System.Linq.Expressions;

    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Domain.Entity;

    public sealed class ActiveSpecification() : SpecificationBase<SubscriptionPlan>
    {
        public override Expression<Func<SubscriptionPlan, bool>> Expression() => (t) => t.IsActive;
    }
}
