namespace GamaEdtech.Data.Dto.Identity
{
    public sealed class PublicProfileRequestDto
    {
        public required string ProfileHandle { get; set; }
        public required long UserId { get; set; }
    }
}
