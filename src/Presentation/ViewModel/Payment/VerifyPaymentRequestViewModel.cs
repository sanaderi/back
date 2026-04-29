namespace GamaEdtech.Presentation.ViewModel.Payment
{
    using GamaEdtech.Common.DataAnnotation;

    public sealed class VerifyPaymentRequestViewModel
    {
        [Display]
        [Required]
        public string? TransactionId { get; set; }
    }
}
