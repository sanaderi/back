namespace GamaEdtech.Data.Dto.Identity
{
    public sealed class ValidateHandleRequestDto
    {
        public required int UserId { get; set; }
        public required string? Handle { get; set; }
    }
}
