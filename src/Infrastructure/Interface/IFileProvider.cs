namespace GamaEdtech.Infrastructure.Interface
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.Service.Factory;
    using GamaEdtech.Data.Dto.File;
    using GamaEdtech.Data.Dto.Provider.File;
    using GamaEdtech.Domain.Enumeration;

    [Injectable]
    public interface IFileProvider : IProvider<FileProviderType>
    {
        Task<ResultData<Uri?>> GetFileUriAsync([NotNull] FileUriRequestDto requestDto);
        Task<ResultData<string?>> UploadFileAsync([NotNull] UploadFileRequestDto requestDto);
        Task<ResultData<bool>> RemoveFileAsync([NotNull] RemoveFileRequestDto requestDto);
    }
}
