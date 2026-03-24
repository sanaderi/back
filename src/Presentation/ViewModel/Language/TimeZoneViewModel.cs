namespace GamaEdtech.Presentation.ViewModel.Language
{
    public sealed class TimeZoneViewModel
    {
        public string? Id { get; set; }
        public string? DisplayName { get; set; }
        public TimeSpan BaseUtcOffset { get; set; }
    }
}
