namespace GamaEdtech.Presentation.Api.Areas.Admin.Controllers
{
    using System.Diagnostics.CodeAnalysis;

    using Asp.Versioning;

    using GamaEdtech.Application.Interface;
    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Specification.Impl;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.Identity;
    using GamaEdtech.Data.Dto.ContentLocalization;
    using GamaEdtech.Domain.Entity;
    using GamaEdtech.Domain.Enumeration;
    using GamaEdtech.Presentation.ViewModel.ContentLocalization;

    using Microsoft.AspNetCore.Mvc;

    [Common.DataAnnotation.Area(nameof(Admin), "Admin")]
    [Route("api/v{version:apiVersion}/[area]/[controller]")]
    [ApiVersion("1.0")]
    [Permission(Roles = [nameof(Role.Admin)])]
    [Display(Name = "Content Localizations")]
    public class ContentLocalizationsController(Lazy<ILogger<ContentLocalizationsController>> logger, Lazy<IContentLocalizationService> contentLocalizationService)
        : ApiControllerBase<ContentLocalizationsController>(logger)
    {
        [HttpGet, Produces<ApiResponse<ListDataSource<ContentLocalizationsResponseViewModel>>>()]
        public async Task<IActionResult<ListDataSource<ContentLocalizationsResponseViewModel>>> GetContentLocalizations([NotNull, FromQuery] ContentLocalizationsRequestViewModel request)
        {
            try
            {
                var result = await contentLocalizationService.Value.GetContentLocalizationsAsync(new ListRequestDto<ContentLocalization>
                {
                    PagingDto = request.PagingDto,
                });
                return Ok<ListDataSource<ContentLocalizationsResponseViewModel>>(new(result.Errors)
                {
                    Data = result.Data.List is null ? new() : new()
                    {
                        List = result.Data.List.Select(t => new ContentLocalizationsResponseViewModel
                        {
                            Id = t.Id,
                            Name = t.Name,
                            LanguageId = t.LanguageId,
                            ContentType = t.ContentType,
                            ContentId = t.ContentId,
                        }),
                        TotalRecordsCount = result.Data.TotalRecordsCount,
                    }
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<ListDataSource<ContentLocalizationsResponseViewModel>>(new() { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpGet("{id:long}"), Produces<ApiResponse<ContentLocalizationResponseViewModel>>()]
        public async Task<IActionResult<ContentLocalizationResponseViewModel>> GetContentLocalization([FromRoute] long id)
        {
            try
            {
                var result = await contentLocalizationService.Value.GetContentLocalizationAsync(new IdEqualsSpecification<ContentLocalization, long>(id));
                return Ok<ContentLocalizationResponseViewModel>(new(result.Errors)
                {
                    Data = result.Data is null ? null : new()
                    {
                        Id = result.Data.Id,
                        ContentId = result.Data.ContentId,
                        ContentType = result.Data.ContentType,
                        LanguageId = result.Data.LanguageId,
                        Name = result.Data.Name,
                        Value = result.Data.Value,
                    }
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<ContentLocalizationResponseViewModel>(new() { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpPost, Produces<ApiResponse<ManageContentLocalizationResponseViewModel>>()]
        public async Task<IActionResult<ManageContentLocalizationResponseViewModel>> CreateContentLocalization([NotNull] ManageContentLocalizationRequestViewModel request)
        {
            try
            {
                var result = await contentLocalizationService.Value.ManageContentLocalizationAsync(new ManageContentLocalizationRequestDto
                {
                    ContentId = request.ContentId,
                    ContentType = request.ContentType,
                    LanguageId = request.LanguageId,
                    Name = request.Name,
                    Value = request.Value,
                });
                return Ok<ManageContentLocalizationResponseViewModel>(new(result.Errors)
                {
                    Data = new() { Id = result.Data, },
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<ManageContentLocalizationResponseViewModel>(new() { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpPut("{id:long}"), Produces<ApiResponse<ManageContentLocalizationResponseViewModel>>()]
        public async Task<IActionResult<ManageContentLocalizationResponseViewModel>> UpdateContentLocalization([FromRoute] long id, [NotNull, FromBody] ManageContentLocalizationRequestViewModel request)
        {
            try
            {
                var result = await contentLocalizationService.Value.ManageContentLocalizationAsync(new ManageContentLocalizationRequestDto
                {
                    Id = id,
                    ContentId = request.ContentId,
                    ContentType = request.ContentType,
                    LanguageId = request.LanguageId,
                    Name = request.Name,
                    Value = request.Value,
                });
                return Ok<ManageContentLocalizationResponseViewModel>(new(result.Errors)
                {
                    Data = new() { Id = result.Data, },
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<ManageContentLocalizationResponseViewModel>(new() { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpDelete("{id:long}"), Produces<ApiResponse<bool>>()]
        public async Task<IActionResult<bool>> RemoveContentLocalization([FromRoute] long id)
        {
            try
            {
                var result = await contentLocalizationService.Value.RemoveContentLocalizationAsync(new IdEqualsSpecification<ContentLocalization, long>(id));
                return Ok<bool>(new(result.Errors)
                {
                    Data = result.Data
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<bool>(new() { Errors = [new() { Message = exc.Message }] });
            }
        }
    }
}
