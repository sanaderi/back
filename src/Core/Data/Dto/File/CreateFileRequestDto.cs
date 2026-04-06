namespace GamaEdtech.Data.Dto.File
{
    using GamaEdtech.Domain.Enumeration;

    using Microsoft.AspNetCore.Http;

    public sealed class CreateFileRequestDto
    {
        public required IFormFile File { get; set; }
        public required ContainerType ContainerType { get; set; }
    }
}
