namespace GamaEdtech.Presentation.ViewModel.Blog
{
    public sealed class PostCommentContributionReviewViewModel
    {
        public long Id { get; set; }
        public string? PostTitle { get; set; }
        public long PostId { get; set; }
        public string? Comment { get; set; }
    }
}
