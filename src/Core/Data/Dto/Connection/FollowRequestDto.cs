namespace GamaEdtech.Data.Dto.Connection
{
    public sealed class FollowRequestDto
    {
        public required long ProfileId { get; set; }
        public required long UserId { get; set; }
        public required bool SubscribeToActivityFeed { get; set; }
    }
}
