namespace GamaEdtech.Infrastructure.Provider.File
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Data.Dto.File;
    using GamaEdtech.Data.Dto.Provider.File;
    using GamaEdtech.Domain.Enumeration;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using static GamaEdtech.Common.Core.Constants;

    public sealed class LocalFileProvider(Lazy<ILogger<LocalFileProvider>> logger
        , Lazy<IConfiguration> configuration, Lazy<IWebHostEnvironment> environment, Lazy<HttpContextAccessor> httpContextAccessor) : FileProviderBase
    {
        public override FileProviderType ProviderType => FileProviderType.Local;

        public override async Task<ResultData<Uri?>> GetFileUriAsync([NotNull] FileUriRequestDto requestDto)
        {
            try
            {
                var path = $"{GetDirectoryPath(requestDto.ContainerType, false)}/{requestDto.FileId}";
                return new(OperationResult.Succeeded) { Data = await Task.FromResult(new Uri(path)) };
            }
            catch (Exception exc)
            {
                logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message, }] };
            }
        }

        public override async Task<ResultData<string?>> UploadFileAsync([NotNull] UploadFileRequestDto requestDto)
        {
            try
            {
                var dir = GetDirectoryPath(requestDto.ContainerType, true);
                if (!Directory.Exists(dir))
                {
                    _ = Directory.CreateDirectory(dir);
                }

                var value = GenerateBlobFileName(requestDto.FileExtension);
                var savedFilePath = Path.Combine(dir, value);

                await System.IO.File.WriteAllBytesAsync(savedFilePath, requestDto.File);

                return new(OperationResult.Succeeded) { Data = value };
            }
            catch (Exception exc)
            {
                logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message, }] };
            }
        }

        public override async Task<ResultData<bool>> RemoveFileAsync([NotNull] RemoveFileRequestDto requestDto)
        {
            try
            {
                if (string.IsNullOrEmpty(requestDto.FileId))
                {
                    return new(OperationResult.Succeeded) { Data = await Task.FromResult(true) };
                }

                var file = Path.Combine(GetDirectoryPath(requestDto.ContainerType, true), requestDto.FileId);
                if (System.IO.File.Exists(file))
                {
                    System.IO.File.SetAttributes(file, FileAttributes.Normal);
                    System.IO.File.Delete(file);
                }

                return new(OperationResult.Succeeded) { Data = await Task.FromResult(true) };
            }
            catch (Exception exc)
            {
                logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message, }] };
            }
        }

        private string GetDirectoryPath([NotNull] ContainerType containerType, bool physicalPath)
        {
            var path = configuration.Value.GetValue<string>("FileProvider:Local:Path")!;
            var container = GenerateBlobContainerName(containerType);
            if (physicalPath)
            {
                List<string> lst = [environment.Value.WebRootPath];
                lst.AddRange(path.Split('/'));
                lst.Add(container);
                return Path.Combine([.. lst]);
            }

            var hostUrl = $"{httpContextAccessor.Value.HttpContext?.Request.Scheme}://{httpContextAccessor.Value.HttpContext?.Request.Host}";
            return hostUrl + "/" + path + "/" + container;
        }
    }
}
