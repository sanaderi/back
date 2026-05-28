namespace GamaEdtech.Data.Dto.Identity
{
    using GamaEdtech.Domain.Enumeration;

    public sealed class PublicProfileDto
    {
        public string? Avatar { get; set; }
        public string? FullName { get; set; }
        public OnlineStatus OnlineStatus { get; set; }
        public UserRateLevel UserRateLevel { get; set; }
        public IEnumerable<string?>? Skills { get; set; }
        public string? Handle { get; set; }
    }
}
