namespace GamaEdtech.Domain.Specification.Identity
{
    using GamaEdtech.Common.DataAccess.Specification;

    using System.Linq.Expressions;
    using GamaEdtech.Domain.Entity.Identity;

    public sealed class FirstNameContainsSpecification(string firstName) : SpecificationBase<ApplicationUser>
    {
        public override Expression<Func<ApplicationUser, bool>> Expression() => (t) => t.FirstName!.Contains(firstName);
    }
}
