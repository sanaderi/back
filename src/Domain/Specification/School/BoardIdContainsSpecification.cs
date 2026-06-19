namespace GamaEdtech.Domain.Specification.School
{
    using System.Linq;
    using System.Linq.Expressions;

    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Domain.Entity;

    public sealed class BoardIdContainsSpecification(IEnumerable<int> ids) : SpecificationBase<School>
    {
        public override Expression<Func<School, bool>> Expression() => (t) => t.SchoolBoards.Any(b => ids.Contains(b.BoardId));
    }
}
