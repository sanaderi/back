namespace GamaEdtech.Presentation.ViewModel.Experience
{
    using GamaEdtech.Common.DataAnnotation;

    public sealed class ManageExperienceRequestViewModel
    {
        [Display]
        [Required]
        public DateTimeOffset? StartDate { get; set; }

        [Display]
        public DateTimeOffset? EndDate { get; set; }

        [Display]
        [Required]
        public string? Title { get; set; }

        [Display]
        public string? Description { get; set; }
    }
}
