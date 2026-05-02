namespace GamaEdtech.Data.Dto.Message
{
    public sealed class MessageConnectionDto
    {
        public int ConnectionId { get; set; }
        public string? ConnectionName { get; set; }
        public int UnreadCount { get; set; }
    }
}
