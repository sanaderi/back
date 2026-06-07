namespace GamaEdtech.Data.Dto.Connection
{
    public sealed class ConfirmFollowRequestRequestDto
    {
        public required long Id { get; set; }
        public required long UserId { get; set; }
        public required bool TwoWay { get; set; }
    }
}
