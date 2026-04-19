namespace GamaEdtech.Presentation.ViewModel.School
{
    using GamaEdtech.Common.DataAnnotation;

    public sealed class SchoolLocalizedValueViewModel
    {
        [Display]
        [Required]
        public int? LanguageId { get; set; }

        [Display]
        public string? Description { get; set; }

        [Display]
        public string? Name { get; set; }

        [Display]
        public string? Address { get; set; }
    }
}
