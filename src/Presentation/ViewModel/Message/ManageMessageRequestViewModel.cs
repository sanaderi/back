namespace GamaEdtech.Presentation.ViewModel.Message
{
    using GamaEdtech.Common.DataAnnotation;

    public sealed class ManageMessageRequestViewModel
    {
        [Display]
        [Required]
        public string? Body { get; set; }

        [Display]
        [Required]
        public int? ConnectionId { get; set; }
    }
}
