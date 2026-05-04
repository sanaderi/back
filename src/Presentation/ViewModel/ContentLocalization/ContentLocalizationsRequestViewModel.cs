namespace GamaEdtech.Presentation.ViewModel.ContentLocalization
{
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAnnotation;

    public sealed class ContentLocalizationsRequestViewModel
    {
        [Display]
        public PagingDto? PagingDto { get; set; }
    }
}
