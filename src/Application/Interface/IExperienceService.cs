namespace GamaEdtech.Application.Interface
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Data.Dto.Experience;
    using GamaEdtech.Domain.Entity;

    [Injectable]
    public interface IExperienceService
    {
        Task<ResultData<ListDataSource<ExperienceDto>>> GetExperiencesAsync(ListRequestDto<Experience>? requestDto = null);
        Task<ResultData<ExperienceDto>> GetExperienceAsync([NotNull] ISpecification<Experience> specification);
        Task<ResultData<long>> ManageExperienceAsync([NotNull] ManageExperienceRequestDto requestDto);
        Task<ResultData<bool>> RemoveExperienceAsync([NotNull] ISpecification<Experience> specification);
    }
}
