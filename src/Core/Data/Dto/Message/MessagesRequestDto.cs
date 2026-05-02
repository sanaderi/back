namespace GamaEdtech.Data.Dto.Message
{
    using GamaEdtech.Common.Data;

    public sealed class MessagesRequestDto
    {
        public required int ConnectionId { get; set; }
        public required int UserId { get; set; }
        public PagingDto? PagingDto { get; set; }
    }
}
