namespace GamaEdtech.Data.Dto.Connection
{
    public sealed class UnFollowRequestDto
    {
        public required long ProfileId { get; set; }
        public required long UserId { get; set; }
        public required bool TwoWayRevoke { get; set; }
    }
}
