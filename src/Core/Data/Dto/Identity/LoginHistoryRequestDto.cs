namespace GamaEdtech.Data.Dto.Identity
{
    public sealed class LoginHistoryRequestDto
    {
        public required long UserId { get; set; }
        public required string? IpAddress { get; set; }
        public required string? UserAgent { get; set; }
    }
}
