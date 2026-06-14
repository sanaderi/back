namespace GamaEdtech.Presentation.ViewModel.Transaction
{
    using GamaEdtech.Common.DataAnnotation;

    public sealed class CreateTransactionRequestViewModel
    {
        [Display]
        [Required]
        public long? UserId { get; set; }

        [Display]
        [Required]
        public bool? IsDebit { get; set; }

        [Display]
        [Required]
        public long? Points { get; set; }

        [Display]
        [Required]
        public string? Description { get; set; }
    }
}
