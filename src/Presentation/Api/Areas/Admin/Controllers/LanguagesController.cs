namespace GamaEdtech.Presentation.Api.Areas.Admin.Controllers
{
    using System.Diagnostics.CodeAnalysis;

    using Asp.Versioning;

    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Specification.Impl;
    using GamaEdtech.Common.Identity;
    using GamaEdtech.Common.Localization;
    using GamaEdtech.Domain.Enumeration;
    using GamaEdtech.Presentation.ViewModel.Language;

    using Microsoft.AspNetCore.Mvc;

    [Common.DataAnnotation.Area(nameof(Admin), "Admin")]
    [Route("api/v{version:apiVersion}/[area]/[controller]")]
    [ApiVersion("1.0")]
    [Permission(Roles = [nameof(Role.Admin)])]
    public class LanguagesController(Lazy<ILogger<LanguagesController>> logger, Lazy<Application.Interface.ILanguageService> languageService)
        : ApiControllerBase<LanguagesController>(logger)
    {
        [HttpGet, Produces<ApiResponse<ListDataSource<LanguageResponseViewModel>>>()]
        public async Task<IActionResult<ListDataSource<LanguageResponseViewModel>>> GetLanguages([NotNull, FromQuery] LanguagesRequestViewModel request)
        {
            try
            {
                var result = await languageService.Value.GetLanguagesAsync(new ListRequestDto<Language>
                {
                    PagingDto = request.PagingDto,
                });
                return Ok<ListDataSource<LanguageResponseViewModel>>(new(result.Errors)
                {
                    Data = result.Data.List is null ? new() : new()
                    {
                        List = result.Data.List.Select(t => new LanguageResponseViewModel
                        {
                            Id = t.Id,
                            Name = t.Name,
                            Code = t.Code,
                            Icon = t.Icon,
                            IsEnable = t.IsEnable,
                            IsDefault = t.IsDefault,
                        }),
                        TotalRecordsCount = result.Data.TotalRecordsCount,
                    }
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return Ok<ListDataSource<LanguageResponseViewModel>>(new() { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpGet("{id:int}"), Produces<ApiResponse<LanguageResponseViewModel>>()]
        public async Task<IActionResult<LanguageResponseViewModel>> GetLanguage([FromRoute] int id)
        {
            try
            {
                var result = await languageService.Value.GetLanguageAsync(new IdEqualsSpecification<Language, int>(id));
                return Ok<LanguageResponseViewModel>(new(result.Errors)
                {
                    Data = result.Data is null ? null : new()
                    {
                        Id = result.Data.Id,
                        Name = result.Data.Name,
                        Code = result.Data.Code,
                        Icon = result.Data.Icon,
                        IsEnable = result.Data.IsEnable,
                        IsDefault = result.Data.IsDefault,
                    }
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<LanguageResponseViewModel>(new() { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpPost, Produces<ApiResponse<ManageLanguageResponseViewModel>>()]
        public async Task<IActionResult<ManageLanguageResponseViewModel>> CreateLanguage([NotNull] ManageLanguageRequestViewModel request)
        {
            try
            {
                var result = await languageService.Value.ManageLanguageAsync(new()
                {
                    Name = request.Name,
                    Code = request.Code,
                    Icon = request.Icon,
                    IsEnable = request.IsEnable.GetValueOrDefault(),
                    IsDefault = request.IsDefault.GetValueOrDefault(),
                });
                return Ok<ManageLanguageResponseViewModel>(new(result.Errors)
                {
                    Data = new() { Id = result.Data, },
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<ManageLanguageResponseViewModel>(new() { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpPut("{id:int}"), Produces<ApiResponse<ManageLanguageResponseViewModel>>()]
        public async Task<IActionResult<ManageLanguageResponseViewModel>> UpdateLanguage([FromRoute] int id, [NotNull, FromBody] ManageLanguageRequestViewModel request)
        {
            try
            {
                var result = await languageService.Value.ManageLanguageAsync(new()
                {
                    Id = id,
                    Name = request.Name,
                    Code = request.Code,
                    Icon = request.Icon,
                    IsEnable = request.IsEnable.GetValueOrDefault(),
                    IsDefault = request.IsDefault.GetValueOrDefault(),
                });
                return Ok<ManageLanguageResponseViewModel>(new(result.Errors)
                {
                    Data = new() { Id = result.Data, },
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<ManageLanguageResponseViewModel>(new() { Errors = [new() { Message = exc.Message }] });
            }
        }

        [HttpDelete("{id:int}"), Produces<ApiResponse<bool>>()]
        public async Task<IActionResult<bool>> RemoveLanguage([FromRoute] int id)
        {
            try
            {
                var result = await languageService.Value.RemoveLanguageAsync(new IdEqualsSpecification<Language, int>(id));
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

        [HttpGet("cultures"), Produces<ApiResponse<IEnumerable<CultureViewModel>>>()]
        public IActionResult<IEnumerable<CultureViewModel>> GetCultures()
        {
            try
            {
                return Ok<IEnumerable<CultureViewModel>>(new()
                {
                    Data = Globals.AllCultures.Select(t => new CultureViewModel
                    {
                        Code = t.Name,
                        DisplayName = t.NativeName,
                    }),
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return Ok<IEnumerable<CultureViewModel>>(new(new Error { Message = exc.Message }));
            }
        }
    }
}
