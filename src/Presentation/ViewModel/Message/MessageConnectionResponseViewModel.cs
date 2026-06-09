namespace GamaEdtech.Presentation.ViewModel.Message
{
    public sealed class MessageConnectionResponseViewModel
    {
        public long ConnectionId { get; set; }
        public string? ConnectionName { get; set; }
        public int UnreadCount { get; set; }
    }
}
