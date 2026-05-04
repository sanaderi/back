namespace GamaEdtech.Presentation.ViewModel.Blog
{
    using GamaEdtech.Common.Converter;
    using System.Text.Json.Serialization;

    using GamaEdtech.Domain.Enumeration;

    public sealed class PostCommentContributionListResponseViewModel
    {
        public long Id { get; set; }

        public string? CreationUser { get; set; }

        public DateTimeOffset CreationDate { get; set; }

        public long PostId { get; set; }

        [JsonConverter(typeof(EnumerationConverter<Status, byte>))]
        public Status Status { get; set; }
    }
}
