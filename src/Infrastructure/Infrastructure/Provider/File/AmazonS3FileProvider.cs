namespace GamaEdtech.Infrastructure.Provider.File
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Amazon.S3;
    using Amazon.S3.Model;

    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Data.Dto.File;
    using GamaEdtech.Data.Dto.Provider.File;
    using GamaEdtech.Domain.Enumeration;
    using GamaEdtech.Infrastructure.Interface;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using static GamaEdtech.Common.Core.Constants;

    public sealed class AmazonS3FileProvider(Lazy<ILogger<AmazonS3FileProvider>> logger, Lazy<IConfiguration> configuration) : IFileProvider
    {
        public FileProviderType ProviderType => FileProviderType.AmazonS3;

        public async Task<ResultData<Uri?>> GetFileUriAsync([NotNull] FileUriRequestDto requestDto)
        {
            try
            {
                var url = await Client.GetPreSignedURLAsync(new GetPreSignedUrlRequest
                {
                    BucketName = BucketName,
                    Key = requestDto.FileId,
                    Expires = DateTime.Now.AddMinutes(10),
                });

                return new(OperationResult.Succeeded) { Data = new Uri(url) };
            }
            catch (Exception exc)
            {
                logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message, }] };
            }
        }

        public async Task<ResultData<string?>> UploadFileAsync([NotNull] UploadFileRequestDto requestDto)
        {
            try
            {
                var name = $"{Guid.NewGuid():N}{requestDto.FileExtension}";

                InitiateMultipartUploadRequest initiateRequest = new()
                {
                    BucketName = BucketName,
                    Key = name,
                    ContentType = requestDto.ContentType,
                };
                var initResponse = await Client.InitiateMultipartUploadAsync(initiateRequest);
                var contentLength = requestDto.File.LongLength;
                var partSize = 5 * (long)Math.Pow(2, 20); // 5 MB
                long filePosition = 0;
                using var stream = new MemoryStream(requestDto.File);
                List<UploadPartResponse> uploadResponses = [];
                var partNumber = 1;
                while (filePosition < contentLength)
                {
                    UploadPartRequest uploadRequest = new()
                    {
                        BucketName = BucketName,
                        Key = name,
                        UploadId = initResponse.UploadId,
                        PartNumber = partNumber,
                        PartSize = partSize,
                        FilePosition = filePosition,
                        InputStream = stream,
                    };

                    // Upload a part and add the response to our list.
                    uploadResponses.Add(await Client.UploadPartAsync(uploadRequest));

                    filePosition += partSize;
                    partNumber++;
                }
                CompleteMultipartUploadRequest completeRequest = new()
                {
                    BucketName = BucketName,
                    Key = name,
                    UploadId = initResponse.UploadId
                };
                completeRequest.AddPartETags(uploadResponses);

                var completeUploadResponse = await Client.CompleteMultipartUploadAsync(completeRequest);

                return new(OperationResult.Succeeded) { Data = completeUploadResponse.Key };
            }
            catch (Exception exc)
            {
                logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message, }] };
            }
        }

        public async Task<ResultData<bool>> RemoveFileAsync([NotNull] RemoveFileRequestDto requestDto)
        {
            try
            {
                if (string.IsNullOrEmpty(requestDto.FileId))
                {
                    return new(OperationResult.Succeeded) { Data = true };
                }

                _ = await Client.DeleteObjectAsync(BucketName, requestDto.FileId);

                return new(OperationResult.Succeeded) { Data = true };
            }
            catch (Exception exc)
            {
                logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message, }] };
            }
        }

        private string BucketName => configuration.Value.GetValue<string>("FileProvider:AmazonS3:BucketName")!;

        private AmazonS3Client Client
        {
            get
            {
                var serviceUrl = configuration.Value.GetValue<string>("FileProvider:AmazonS3:ServiceURL");
                var secretAccessKey = configuration.Value.GetValue<string>("FileProvider:AmazonS3:SecretAccessKey");
                var accessKeyId = configuration.Value.GetValue<string>("FileProvider:AmazonS3:AccessKeyId");

                return new AmazonS3Client(accessKeyId, secretAccessKey, new AmazonS3Config
                {
                    ServiceURL = serviceUrl,
                    UseHttp = true,
                    ForcePathStyle = true,
                    AllowAutoRedirect = true,
                });
            }
        }
    }
}
