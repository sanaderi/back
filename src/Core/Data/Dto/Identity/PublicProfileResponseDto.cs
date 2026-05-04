namespace GamaEdtech.Data.Dto.Identity
{
    using GamaEdtech.Domain.Enumeration;

    public sealed class PublicProfileResponseDto
    {
        public Role? Roles { get; set; }
        public int PageViewsCount { get; set; }
        public DateTimeOffset? RegistrationDate { get; set; }
        public OnlineStatus OnlineStatus { get; set; }
    }
}
