namespace GamaEdtech.Presentation.ViewModel.Language
{
    public sealed class ActiveLanguageViewModel
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Icon { get; set; }
        public bool IsDefault { get; set; }
        public bool Rtl { get; set; }
    }
}
