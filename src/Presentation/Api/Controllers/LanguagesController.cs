namespace GamaEdtech.Presentation.Api.Controllers
{
    using System.Globalization;

    using Asp.Versioning;

    using GamaEdtech.Application.Interface;
    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.TimeZone;
    using GamaEdtech.Presentation.ViewModel.Language;

    using Microsoft.AspNetCore.Mvc;

    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class LanguagesController(Lazy<ILogger<LanguagesController>> logger, Lazy<ILanguageService> languageService, Lazy<ITimeZoneProvider> timeZoneProvider)
        : ApiControllerBase<LanguagesController>(logger)
    {
        [HttpGet, Produces<ApiResponse<IEnumerable<ActiveLanguageViewModel>>>()]
        public async Task<IActionResult<IEnumerable<ActiveLanguageViewModel>>> GetActiveLanguages()
        {
            try
            {
                var result = await languageService.Value.GetActiveLanguagesAsync();
                return Ok<IEnumerable<ActiveLanguageViewModel>>(new(result.Errors)
                {
                    Data = result.Data is null
                    ? []
                    : result.Data.Select(t => new ActiveLanguageViewModel
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Code = t.Code,
                        Icon = t.Icon,
                        IsDefault = t.IsDefault,
                        Rtl = new CultureInfo(t.Code!).TextInfo.IsRightToLeft,
                    }),
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return Ok<IEnumerable<ActiveLanguageViewModel>>(new(new Error { Message = exc.Message }));
            }
        }

        [HttpGet("time-zones"), Produces<ApiResponse<IEnumerable<TimeZoneViewModel>>>()]
        public IActionResult<IEnumerable<TimeZoneViewModel>> GetTimeZones()
        {
            try
            {
                var result = timeZoneProvider.Value.GetTimeZones();
                return Ok<IEnumerable<TimeZoneViewModel>>(new()
                {
                    Data = result is null
                    ? []
                    : result.Select(t => new TimeZoneViewModel
                    {
                        Id = t.Id,
                        DisplayName = t.DisplayName,
                        BaseUtcOffset = t.BaseUtcOffset,
                    }),
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return Ok<IEnumerable<TimeZoneViewModel>>(new(new Error { Message = exc.Message }));
            }
        }
    }
}
