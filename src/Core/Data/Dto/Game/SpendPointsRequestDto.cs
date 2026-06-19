namespace GamaEdtech.Data.Dto.Game
{
    using GamaEdtech.Domain.Enumeration;

    public sealed class SpendPointsRequestDto
    {
        public required long UserId { get; set; }
        public required long Points { get; set; }
        public required long IdentifierId { get; set; }
        public required ContentType ContentType { get; set; }
    }
}
