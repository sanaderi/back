namespace GamaEdtech.Presentation.ViewModel.Message
{
    public sealed class MessagesResponseViewModel
    {
        public long Id { get; set; }
        public string? Body { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public bool IsRead { get; set; }
    }
}
