namespace GamaEdtech.Presentation.ViewModel.Identity
{
    using System.Text.Json.Serialization;

    using GamaEdtech.Common.Converter;
    using GamaEdtech.Domain.Enumeration;

    public sealed class PublicProfileListResponseViewModel
    {
        public string? Avatar { get; set; }

        public string? FullName { get; set; }

        [JsonConverter(typeof(EnumerationConverter<OnlineStatus, byte>))]
        public OnlineStatus OnlineStatus { get; set; }

        [JsonConverter(typeof(EnumerationConverter<UserRateLevel, byte>))]
        public UserRateLevel UserRateLevel { get; set; }

        public IEnumerable<string?>? Skills { get; set; }

        public string? Handle { get; set; }
    }
}
