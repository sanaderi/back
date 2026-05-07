namespace GamaEdtech.Data.Dto.Identity
{
    using GamaEdtech.Domain.Enumeration;

    public sealed class PublicProfileResponseDto
    {
        public string? Avatar { get; set; }
        public Role? Roles { get; set; }
        public long ProfileView { get; set; }
        public DateTimeOffset? RegistrationDate { get; set; }
        public OnlineStatus OnlineStatus { get; set; }
    }
}
