namespace GamaEdtech.Data.Dto.ContentLocalization
{
    public sealed class ManageContentLocalizationRequestDto
    {
        public long? Id { get; set; }
        public required long ContentId { get; set; }
        public required string? ContentType { get; set; }
        public required string? Name { get; set; }
        public required string? Value { get; set; }
        public required int LanguageId { get; set; }
    }
}
