namespace GamaEdtech.Data.Dto.Message
{
    public sealed class ManageMessageRequestDto
    {
        public long? Id { get; set; }
        public required long UserId { get; set; }
        public required long ConnectionId { get; set; }
        public required string? Body { get; set; }
    }
}
