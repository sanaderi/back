namespace GamaEdtech.Presentation.ViewModel.Game
{
    using System.Text.Json.Serialization;

    using GamaEdtech.Common.Converter;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Domain.Enumeration;

    public sealed class SpendPointsRequestViewModel
    {
        [Required]
        public long? Points { get; set; }

        [Required]
        public long? IdentifierId { get; set; }

        [JsonConverter(typeof(EnumerationConverter<ContentType, byte>))]
        [Required]
        public ContentType? ContentType { get; set; }
    }
}
