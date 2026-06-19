namespace GamaEdtech.Data.Dto.File
{
    using GamaEdtech.Domain.Enumeration;

    public sealed class FileUriRequestDto
    {
        public required string? FileId { get; set; }
        public required ContainerType ContainerType { get; set; }
    }
}
