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

    public abstract class FileProviderBase : IFileProvider
    {
        public abstract FileProviderType ProviderType { get; }

        public abstract Task<ResultData<Uri?>> GetFileUriAsync([NotNull] FileUriRequestDto requestDto);
        public abstract Task<ResultData<bool>> RemoveFileAsync([NotNull] RemoveFileRequestDto requestDto);
        public abstract Task<ResultData<string?>> UploadFileAsync([NotNull] UploadFileRequestDto requestDto);

        protected static string GenerateBlobFileName(string? fileExtension) => $"{Guid.NewGuid():N}{fileExtension}";

        protected static string GenerateBlobContainerName([NotNull] ContainerType containerType) => containerType.Name.ToLowerInvariant();
    }
}
