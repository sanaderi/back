namespace GamaEdtech.Application.Interface
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Data.Dto.Subscription;
    using GamaEdtech.Domain.Entity;

    [Injectable]
    public interface ISubscriptionService
    {
        Task<ResultData<ListDataSource<SubscriptionPlanDto>>> GetSubscriptionPlansAsync(ListRequestDto<SubscriptionPlan>? requestDto = null);
        Task<ResultData<SubscriptionPlanDto>> GetSubscriptionPlanAsync([NotNull] ISpecification<SubscriptionPlan> specification);
        Task<ResultData<long>> ManageSubscriptionPlanAsync([NotNull] ManageSubscriptionPlanRequestDto requestDto);
        Task<ResultData<bool>> RemoveSubscriptionPlanAsync([NotNull] ISpecification<SubscriptionPlan> specification);
    }
}
