namespace GamaEdtech.Data.Dto.Experience
{
    public sealed class ManageExperienceRequestDto
    {
        public long? Id { get; set; }
        public required int UserId { get; set; }
        public required DateTimeOffset StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public required string? Title { get; set; }
        public string? Description { get; set; }
    }
}
