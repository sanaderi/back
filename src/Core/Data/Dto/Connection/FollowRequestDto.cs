namespace GamaEdtech.Data.Dto.Connection
{
    public sealed class FollowRequestDto
    {
        public required int ProfileId { get; set; }
        public required int UserId { get; set; }
        public required bool SubscribeToActivityFeed { get; set; }
    }
}
