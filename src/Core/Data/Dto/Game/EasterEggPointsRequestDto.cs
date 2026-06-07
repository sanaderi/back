namespace GamaEdtech.Data.Dto.Game
{
    public sealed class EasterEggPointsRequestDto
    {
        public required long UserId { get; set; }
        public required Guid Id { get; set; }
    }
}
