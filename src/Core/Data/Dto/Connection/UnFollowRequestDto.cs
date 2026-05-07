namespace GamaEdtech.Data.Dto.Connection
{
    public sealed class UnFollowRequestDto
    {
        public required int ProfileId { get; set; }
        public required int UserId { get; set; }
        public required bool TwoWayRevoke { get; set; }
    }
}
