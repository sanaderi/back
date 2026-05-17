namespace GamaEdtech.Data.Dto.Experience
{
    public sealed class ManageExperienceRequestDto
    {
        public long? Id { get; set; }
        public required int UserId { get; set; }
        public required DateTimeOffset StartDate { get; set; }
        public required long SchoolId { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public string? Description { get; set; }
    }
}
