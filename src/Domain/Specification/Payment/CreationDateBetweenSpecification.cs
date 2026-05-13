namespace GamaEdtech.Domain.Specification.Payment
{
    using System;
    using System.Linq.Expressions;

    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Domain.Entity;

    public sealed class CreationDateBetweenSpecification(DateOnly? start, DateOnly? end) : SpecificationBase<Payment>
    {
        public override Expression<Func<Payment, bool>> Expression()
        {
            DateTimeOffset? startDate = start.HasValue ? new DateTimeOffset(start.Value, new TimeOnly(0, 0), TimeSpan.Zero) : null;
            DateTimeOffset? endDate = end.HasValue ? new DateTimeOffset(end.Value, new TimeOnly(23, 59, 59, 999, 999), TimeSpan.Zero) : null;

            return t => (startDate == null || t.CreationDate >= startDate) && (endDate == null || t.CreationDate <= endDate);
        }
    }
}
