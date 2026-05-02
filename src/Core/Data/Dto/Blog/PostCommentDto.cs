namespace GamaEdtech.Data.Dto.Blog
{
    public sealed class PostCommentDto
    {
        public long Id { get; set; }
        public string? CreationUser { get; set; }
        public string? CreationUserAvatar { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public string? Comment { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
    }
}
