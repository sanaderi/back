namespace GamaEdtech.Data.Dto.Provider.PaymentGateway
{
    using GamaEdtech.Domain.Enumeration;

    public sealed class VerifyRequestDto
    {
        public required long PaymentId { get; set; }
        public required decimal Amount { get; set; }
        public required string? TransactionId { get; set; }
        public required Currency Currency { get; set; }
    }
}
