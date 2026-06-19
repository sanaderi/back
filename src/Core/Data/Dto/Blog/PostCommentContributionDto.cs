namespace GamaEdtech.Data.Dto.Blog
{
    using System;

    public sealed class PostCommentContributionDto
    {
        public long PostId { get; set; }
        public string? Comment { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public long CreationUserId { get; set; }
    }
}
