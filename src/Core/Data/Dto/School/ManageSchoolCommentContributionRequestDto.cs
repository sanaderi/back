namespace GamaEdtech.Data.Dto.School
{
    public sealed class ManageSchoolCommentContributionRequestDto
    {
        public long? Id { get; set; }
        public required long SchoolId { get; set; }
        public required long UserId { get; set; }

        public required SchoolCommentContributionDto CommentContribution { get; set; }
    }
}
