namespace GamaEdtech.Application.Interface
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Data.Dto.ContentLocalization;
    using GamaEdtech.Domain.Entity;

    [Injectable]
    public interface IContentLocalizationService
    {
        Task<ResultData<ListDataSource<ContentLocalizationsDto>>> GetContentLocalizationsAsync(ListRequestDto<ContentLocalization>? requestDto = null);
        Task<ResultData<ContentLocalizationDto>> GetContentLocalizationAsync([NotNull] ISpecification<ContentLocalization> specification);
        Task<ResultData<long>> ManageContentLocalizationAsync([NotNull] ManageContentLocalizationRequestDto requestDto);
        Task<ResultData<bool>> RemoveContentLocalizationAsync([NotNull] ISpecification<ContentLocalization> specification);
        Task<ResultData<List<LocalizedValueDto>>> GetLocalizedValuesAsync([NotNull] LocalizedValuesRequestDto requestDto);
    }
}
