namespace GamaEdtech.Data.Dto.Language
{
    public sealed class ManageLanguageRequestDto
    {
        public int? Id { get; set; }
        public required string? Name { get; set; }
        public required string? Code { get; set; }
        public required bool IsEnable { get; set; }
        public string? Icon { get; set; }
    }
}
