namespace GamaEdtech.Data.Dto.Message
{
    public sealed class ToggleMessageRequestDto
    {
        public required long Id { get; set; }
        public required int UserId { get; set; }
    }
}
