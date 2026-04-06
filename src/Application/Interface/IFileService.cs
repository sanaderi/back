namespace GamaEdtech.Application.Interface
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Data.Dto.File;

    [Injectable]
    public interface IFileService
    {
        Task<Uri?> GetFileUriAsync([NotNull] FileUriRequestDto requestDto);
        Task<ResultData<string?>> CreateFileAsync([NotNull] CreateFileRequestDto requestDto);
        Task<ResultData<bool>> RemoveFileAsync([NotNull] RemoveFileRequestDto requestDto);
    }
}
