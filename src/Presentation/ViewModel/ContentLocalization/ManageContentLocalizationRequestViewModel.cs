namespace GamaEdtech.Presentation.ViewModel.ContentLocalization
{
    using GamaEdtech.Common.DataAnnotation;

    public sealed class ManageContentLocalizationRequestViewModel
    {
        [Display]
        [Required]
        public required long ContentId { get; set; }

        [Display]
        [Required]
        public required string? ContentType { get; set; }

        [Display]
        [Required]
        public required string? Name { get; set; }

        [Display]
        [Required]
        public required string? Value { get; set; }

        [Display]
        [Required]
        public required int LanguageId { get; set; }
    }
}
