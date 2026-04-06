namespace GamaEdtech.Presentation.ViewModel.Language
{
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAnnotation;

    public sealed class LanguagesRequestViewModel
    {
        [Display]
        public PagingDto? PagingDto { get; set; }
    }
}
