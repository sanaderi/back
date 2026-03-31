namespace GamaEdtech.Common.Localization
{
    using GamaEdtech.Common.DataAnnotation;

    [Injectable]
    public interface ILanguageService
    {
        IReadOnlyList<string?> GetActiveLanguages();
        Task<IReadOnlyList<string?>> GetActiveLanguagesAsync();
    }
}
