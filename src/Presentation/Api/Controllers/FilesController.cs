namespace GamaEdtech.Presentation.Api.Controllers
{
    using System.Threading.Tasks;

    using Asp.Versioning;

    using GamaEdtech.Application.Interface;
    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Domain.Enumeration;

    using Microsoft.AspNetCore.Mvc;

    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class FilesController(Lazy<ILogger<FilesController>> logger, Lazy<IFileService> fileService)
        : ApiControllerBase<FilesController>(logger)
    {
        [HttpGet("{id}"), Produces<ApiResponse<string>>()]
        public async Task<IActionResult> GetFile([FromRoute] string id)
        {
            try
            {
                var result = await fileService.Value.GetFileUriAsync(new() { FileId = id, ContainerType = ContainerType.Default, });
                return result is null
                    ? Ok<string>(new())
                    : Redirect(result.ToString());
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return Ok<string>(new(new Error { Message = exc.Message }));
            }
        }
    }
}
