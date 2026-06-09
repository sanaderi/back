namespace GamaEdtech.Data.Dto.Connection
{
    public sealed class ToggleSubscriptionRequestDto
    {
        public required long ProfileId { get; set; }
        public required long UserId { get; set; }
    }
}
