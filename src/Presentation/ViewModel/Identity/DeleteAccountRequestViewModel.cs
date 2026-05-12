namespace GamaEdtech.Presentation.ViewModel.Identity
{
    using GamaEdtech.Common.DataAnnotation;

    public sealed class DeleteAccountRequestViewModel
    {
        [Display]
        [Required]
        public string? Username { get; set; }

        [Display]
        [Required]
        public string? Password { get; set; }
    }
}
