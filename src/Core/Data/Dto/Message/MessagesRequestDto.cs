namespace GamaEdtech.Data.Dto.Message
{
    using GamaEdtech.Common.Data;

    public sealed class MessagesRequestDto
    {
        public required long ConnectionId { get; set; }
        public required long UserId { get; set; }
        public PagingDto? PagingDto { get; set; }
    }
}
