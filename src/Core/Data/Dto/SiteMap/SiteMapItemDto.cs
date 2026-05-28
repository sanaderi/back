namespace GamaEdtech.Data.Dto.SiteMap
{
    public sealed class SiteMapItemDto
    {
        public long Id { get; set; }
        public string? Path1 { get; set; }
        public string? Path2 { get; set; }
        public DateTimeOffset? LastModifyDate { get; set; }
    }
}
