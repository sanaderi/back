namespace GamaEdtech.Data.Dto.Blog
{
    public sealed class PostCommentReactionRequestDto
    {
        public required long PostId { get; set; }
        public required long CommentId { get; set; }
        public required int UserId { get; set; }
    }
}
