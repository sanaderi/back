namespace GamaEdtech.Presentation.ViewModel.Experience
{
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAnnotation;

    public sealed class ExperiencesRequestViewModel
    {
        [Display]
        public PagingDto? PagingDto { get; set; } = new() { PageFilter = new(), };
    }
}
