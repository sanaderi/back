namespace GamaEdtech.Presentation.ViewModel.ContentLocalization
{
    public sealed class ContentLocalizationResponseViewModel
    {
        public long Id { get; set; }
        public long ContentId { get; set; }
        public string? ContentType { get; set; }
        public string? Name { get; set; }
        public string? Value { get; set; }
        public int LanguageId { get; set; }
    }
}
