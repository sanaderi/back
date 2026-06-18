namespace GamaEdtech.Data.Dto.School
{
    public sealed class SchoolImageInfoDto
    {
        public long Id { get; set; }
        public string? FileUri { get; set; }
        public long CreationUserId { get; set; }
        public string? CreationUser { get; set; }
        public string? TagName { get; set; }
        public string? TagIcon { get; set; }
        public long? TagId { get; set; }
        public bool IsDefault { get; set; }
    }
}
