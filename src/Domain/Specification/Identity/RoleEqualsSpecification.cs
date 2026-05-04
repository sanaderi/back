namespace GamaEdtech.Domain.Specification.Identity
{
    using System.Linq.Expressions;

    using GamaEdtech.Common.Data.Enumeration;
    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Domain.Entity.Identity;
    using GamaEdtech.Domain.Enumeration;

    public sealed class RoleEqualsSpecification(Role role) : SpecificationBase<ApplicationUser>
    {
        public override Expression<Func<ApplicationUser, bool>> Expression()
        {
            var normalizedNames = role.GetNames().Select(t => t.ToUpperInvariant());
            return (t) => t.UserRoles!.Any(r => normalizedNames.Contains(r.Role!.NormalizedName));
        }
    }
}
