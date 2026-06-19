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
        public long? SchoolId { get; set; }

        [Display]
        public string? Description { get; set; }
    }
}
