namespace GamaEdtech.Data.Dto.Experience
{
    public sealed class ExperienceDto
    {
        public long Id { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public long SchoolId { get; set; }
        public string? SchoolTitle { get; set; }
        public string? Description { get; set; }
    }
}
