namespace GamaEdtech.Data.Dto.Identity
{
    public sealed class LoginHistoryRequestDto
    {
        public required int UserId { get; set; }
        public required string? IpAddress { get; set; }
        public required string? UserAgent { get; set; }
    }
}
