namespace GamaEdtech.Data.Dto.Blog
{
    public sealed class ConfirmPostCommentContributionRequestDto
    {
        public required long ContributionId { get; set; }
        public required bool NotifyUser { get; set; }
    }
}
