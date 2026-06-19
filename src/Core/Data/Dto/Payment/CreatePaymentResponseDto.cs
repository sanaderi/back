namespace GamaEdtech.Data.Dto.Payment
{
    public sealed class CreatePaymentResponseDto
    {
        public long PaymentId { get; set; }
        public string? Url { get; set; }
    }
}
