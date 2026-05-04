namespace GamaEdtech.Data.Dto.Message
{
    using System;

    public sealed class MessageDto
    {
        public long Id { get; set; }
        public string? Body { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public bool IsRead { get; set; }
    }
}
