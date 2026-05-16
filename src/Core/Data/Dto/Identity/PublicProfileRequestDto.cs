namespace GamaEdtech.Data.Dto.Identity
{
    public sealed class PublicProfileRequestDto
    {
        public required string ProfileHandle { get; set; }
        public required int UserId { get; set; }
    }
}
