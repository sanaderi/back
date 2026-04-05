namespace GamaEdtech.Data.Dto.Provider.File
{
    using GamaEdtech.Domain.Enumeration;

    public sealed class UploadFileRequestDto
    {
        public required byte[] File { get; set; }
        public required string FileExtension { get; set; }
        public required string ContentType { get; set; }
        public required ContainerType ContainerType { get; set; }
    }
}
