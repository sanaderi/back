namespace GamaEdtech.Infrastructure.Provider.File
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Data.Dto.File;
    using GamaEdtech.Data.Dto.Provider.File;
    using GamaEdtech.Domain.Enumeration;
    using GamaEdtech.Infrastructure.Interface;

    using Microsoft.Extensions.Configuration;

    public abstract class FileProviderBase(Lazy<IConfiguration> configuration) : IFileProvider
    {
        protected Lazy<IConfiguration> Configuration => configuration;

        public abstract FileProviderType ProviderType { get; }

        public string? GetStaticFileUrl([NotNull] FileUriRequestDto requestDto) => $"{Configuration.Value.GetValue<string>("Core:Cdn")}/{GenerateBlobContainerName(requestDto.ContainerType)}/{requestDto.FileId}";

        public abstract Task<ResultData<Uri?>> GetFileUrlAsync([NotNull] FileUriRequestDto requestDto);

        public abstract Task<ResultData<bool>> RemoveFileAsync([NotNull] RemoveFileRequestDto requestDto);

        public abstract Task<ResultData<string?>> UploadFileAsync([NotNull] UploadFileRequestDto requestDto);

        protected static string GenerateBlobFileName(string? fileExtension) => $"{Guid.NewGuid():N}{fileExtension}";

        protected static string GenerateBlobContainerName([NotNull] ContainerType containerType) => containerType.Name.ToLowerInvariant();
    }
}
