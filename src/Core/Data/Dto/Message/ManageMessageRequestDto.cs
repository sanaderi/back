namespace GamaEdtech.Data.Dto.Message
{
    public sealed class ManageMessageRequestDto
    {
        public long? Id { get; set; }
        public required int UserId { get; set; }
        public required int ConnectionId { get; set; }
        public required string? Body { get; set; }
    }
}
