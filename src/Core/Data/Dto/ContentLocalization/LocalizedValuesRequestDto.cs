namespace GamaEdtech.Data.Dto.ContentLocalization
{
    public sealed class LocalizedValuesRequestDto
    {
        public required string? ContentType { get; set; }
        public required IEnumerable<long> ContentIds { get; set; }
    }
}
