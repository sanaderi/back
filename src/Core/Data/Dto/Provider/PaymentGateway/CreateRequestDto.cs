namespace GamaEdtech.Data.Dto.Provider.PaymentGateway
{
    using GamaEdtech.Domain.Enumeration;

    public sealed class CreateRequestDto
    {
        public required decimal Amount { get; set; }
        public required Currency Currency { get; set; }
        public required string? CallbackUrl { get; set; }
        public required long PaymentId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Email { get; set; }
    }
}
