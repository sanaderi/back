namespace GamaEdtech.Domain.Specification.Post
{
    using System.Linq.Expressions;

    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Domain.Entity;

    public sealed class PostIdEqualsSpecification<TClass>(long postId) : SpecificationBase<TClass>
        where TClass : IPostId
    {
        public override Expression<Func<TClass, bool>> Expression() => (t) => t.PostId.Equals(postId);
    }
}
