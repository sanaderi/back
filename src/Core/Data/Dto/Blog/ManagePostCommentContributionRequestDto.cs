namespace GamaEdtech.Data.Dto.Blog
{
    public sealed class ManagePostCommentContributionRequestDto
    {
        public long? Id { get; set; }
        public required long PostId { get; set; }
        public required int UserId { get; set; }

        public required PostCommentContributionDto CommentContribution { get; set; }
    }
}
