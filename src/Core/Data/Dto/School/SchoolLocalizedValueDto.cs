namespace GamaEdtech.Data.Dto.School
{
    public sealed class SchoolLocalizedValueDto
    {
        public required int LanguageId { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
    }
}
