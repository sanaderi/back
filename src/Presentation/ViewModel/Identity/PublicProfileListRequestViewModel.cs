namespace GamaEdtech.Presentation.ViewModel.Identity
{
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAnnotation;

    public sealed class PublicProfileListRequestViewModel
    {
        [Display]
        public PagingDto? PagingDto { get; set; } = new() { PageFilter = new(), };

        [Display]
        public string? FullName { get; set; }

        [Display]
        public string? Skill { get; set; }
    }
}
