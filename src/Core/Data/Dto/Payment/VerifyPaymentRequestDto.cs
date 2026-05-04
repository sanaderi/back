namespace GamaEdtech.Data.Dto.Payment
{
    public sealed class VerifyPaymentRequestDto
    {
        public required long Id { get; set; }
        public required string? TransactionId { get; set; }
    }
}
