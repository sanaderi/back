namespace GamaEdtech.Data.Dto.Language
{
    public sealed class LanguageDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public bool IsEnable { get; set; }
        public string? Icon { get; set; }
    }
}
