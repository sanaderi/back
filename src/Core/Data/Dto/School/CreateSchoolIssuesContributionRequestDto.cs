namespace GamaEdtech.Data.Dto.School
{
    public sealed class CreateSchoolIssuesContributionRequestDto
    {
        public long SchoolId { get; set; }
        public long CreationUserId { get; set; }
        public string? Description { get; set; }
    }
}
