namespace GamaEdtech.Domain.Specification.Identity
{
    using System.Linq.Expressions;

    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Domain.Entity.Identity;
    using GamaEdtech.Domain.Enumeration;

    public sealed class ProfileVisibilityEqualsSpecification(ProfileVisibility profileVisibility) : SpecificationBase<ApplicationUser>
    {
        public override Expression<Func<ApplicationUser, bool>> Expression() => (t) => t.ProfileVisibility == profileVisibility;
    }
}
