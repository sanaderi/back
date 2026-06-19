namespace GamaEdtech.Data.Dto.Identity
{
    public sealed class ValidateHandleRequestDto
    {
        public required long UserId { get; set; }
        public required string? Handle { get; set; }
    }
}
