namespace GamaEdtech.Data.Dto.Payment
{
    public sealed class CallbackGatewayRequestDto
    {
        public required long Id { get; set; }
        public required string? TransactionId { get; set; }
    }
}
