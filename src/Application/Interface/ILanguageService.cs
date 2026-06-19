namespace GamaEdtech.Application.Interface
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.Localization;
    using GamaEdtech.Data.Dto.Language;

    [Injectable]
    public interface ILanguageService
    {
        Task<ResultData<ListDataSource<LanguageDto>>> GetLanguagesAsync(ListRequestDto<Language>? requestDto = null);
        Task<ResultData<List<LanguageDto>>> GetActiveLanguagesAsync();
        Task<ResultData<LanguageDto>> GetLanguageAsync([NotNull] ISpecification<Language> specification);
        Task<ResultData<int>> ManageLanguageAsync([NotNull] ManageLanguageRequestDto requestDto);
        Task<ResultData<bool>> RemoveLanguageAsync([NotNull] ISpecification<Language> specification);
    }
}
