namespace GamaEdtech.Presentation.ViewModel.Identity
{
    using System.Text.Json.Serialization;

    using GamaEdtech.Common.Converter;
    using GamaEdtech.Domain.Enumeration;

    public sealed class PublicProfileResponseViewModel
    {
        [JsonConverter(typeof(FlagsEnumerationConverter<Role>))]
        public Role? Roles { get; set; }

        public long ProfileView { get; set; }

        public string? Avatar { get; set; }

        public DateTimeOffset? RegistrationDate { get; set; }

        [JsonConverter(typeof(EnumerationConverter<OnlineStatus, byte>))]
        public OnlineStatus OnlineStatus { get; set; }
    }
}
