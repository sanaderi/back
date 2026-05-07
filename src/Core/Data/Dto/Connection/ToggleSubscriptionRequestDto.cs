namespace GamaEdtech.Data.Dto.Connection
{
    public sealed class ToggleSubscriptionRequestDto
    {
        public required int ProfileId { get; set; }
        public required int UserId { get; set; }
    }
}
