namespace GamaEdtech.Presentation.ViewModel.Ticket
{
    using System;
    using System.Collections.Generic;

    public sealed class TicketReplyResponseViewModel
    {
        public long Id { get; set; }
        public string? Body { get; set; }
        public string? CreationUser { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public string? FileUri { get; set; }
        public IEnumerable<string?>? Receivers { get; set; }
    }
}
