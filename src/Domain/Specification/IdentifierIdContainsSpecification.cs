namespace GamaEdtech.Domain.Specification
{
    using System.Linq.Expressions;

    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Domain.Entity;

    public sealed class IdentifierIdContainsSpecification<TClass>(IEnumerable<long> identifierIds) : SpecificationBase<TClass>
        where TClass : IIdentifierId
    {
        public override Expression<Func<TClass, bool>> Expression() => (t) => identifierIds.Contains(t.IdentifierId!.Value);
    }
}
