namespace GamaEdtech.Presentation.ViewModel.Language
{
    using GamaEdtech.Common.DataAnnotation;

    public sealed class ManageLanguageRequestViewModel
    {
        [Display]
        [Required]
        public string? Name { get; set; }

        [Display]
        [Required]
        [Culture]
        public string? Code { get; set; }

        [Display]
        public string? Icon { get; set; }

        [Display]
        [Required]
        public bool? IsEnable { get; set; }
    }
}
