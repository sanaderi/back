namespace GamaEdtech.Domain.Specification.Subscription
{
    using System.Linq.Expressions;

    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Domain.Entity;

    using NetTopologySuite.Geometries;

    public sealed class CoordinateInsideSpecification(Point? point) : SpecificationBase<SubscriptionPlan>
    {
        public override Expression<Func<SubscriptionPlan, bool>> Expression() => (t) => t.Polygon == null || (point != null && t.Polygon.Within(point));
    }
}
