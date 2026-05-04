namespace GamaEdtech.Presentation.ViewModel.Message
{
    public sealed class MessageConnectionResponseViewModel
    {
        public int ConnectionId { get; set; }
        public string? ConnectionName { get; set; }
        public int UnreadCount { get; set; }
    }
}
