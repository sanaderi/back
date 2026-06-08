namespace GamaEdtech.Data.Dto.Identity
{
    public sealed class GetUserTokenRequestDto
    {
        public required long UserId { get; set; }

        public required string TokenProvider { get; set; }

        public required string Purpose { get; set; }
    }
}
