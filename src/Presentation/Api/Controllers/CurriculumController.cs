namespace GamaEdtech.Presentation.Api.Controllers
{
    using System.Text.Json.Nodes;

    using Asp.Versioning;

    using GamaEdtech.Application.Interface;
    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.Identity;
    using GamaEdtech.Data.Dto.Curriculum;
    using GamaEdtech.Domain.Enumeration;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The curriculum model for the website (phase 3): boards and what is under them, a board's filter panel with live counts,
    /// SEO texts, the lesson tree, schools by board and the landing pages. Each answer has <c>native</c>: the website uses
    /// the data only when it is true (<c>Migration:Routes:Curriculum.Website</c> Native) and keeps its own code otherwise.
    /// Anyone may read it; cached briefly like the search's lists. See docs/business/curriculum.md.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Permission(policy: null)]
    [AllowAnonymous]
    public class CurriculumController(Lazy<ILogger<CurriculumController>> logger, Lazy<ICurriculumPublicService> service, Lazy<ICurriculumContentService> contentService)
        : ApiControllerBase<CurriculumController>(logger)
    {
        private const int CacheSeconds = 300;

        /// <summary>The curriculum's switches: whether the website reads the model, and whether it is edited here.</summary>
        [HttpGet("status"), Produces<ApiResponse<CurriculumStatusDto>>()]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public IActionResult<CurriculumStatusDto> GetStatus() => Ok<CurriculumStatusDto>(new() { Data = contentService.Value.GetStatus() });

        [HttpGet("boards"), Produces<ApiResponse<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicItemDto>>>>()]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = CacheSeconds)]
        public Task<IActionResult<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicItemDto>>>> GetBoards() => AnswerAsync(() => service.Value.GetBoardsAsync());

        /// <summary>A board's levels.</summary>
        [HttpGet("boards/{id:int}/levels"), Produces<ApiResponse<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicItemDto>>>>()]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = CacheSeconds)]
        public Task<IActionResult<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicItemDto>>>> GetLevels([FromRoute] int id) => AnswerAsync(() => service.Value.GetChildrenAsync("levels", id));

        /// <summary>A level's subjects (syllabi).</summary>
        [HttpGet("levels/{id:int}/subjects"), Produces<ApiResponse<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicItemDto>>>>()]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = CacheSeconds)]
        public Task<IActionResult<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicItemDto>>>> GetSubjects([FromRoute] int id) => AnswerAsync(() => service.Value.GetChildrenAsync("subjects", id));

        /// <summary>A subject's topics, units and chapters in order.</summary>
        [HttpGet("subjects/{id:int}/topics"), Produces<ApiResponse<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicItemDto>>>>()]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = CacheSeconds)]
        public Task<IActionResult<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicItemDto>>>> GetTopics([FromRoute] int id) => AnswerAsync(() => service.Value.GetChildrenAsync("topics", id));

        /// <summary>The forms' lists by today's ids: <c>type</c> base (a board's levels), lesson (a level's subjects) or
        /// topic (a subject's topics), <c>parent</c> the parent's lookup id.</summary>
        [HttpGet("types"), Produces<ApiResponse<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicItemDto>>>>()]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = CacheSeconds, VaryByQueryKeys = ["*"])]
        public Task<IActionResult<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicItemDto>>>> GetTypeList([FromQuery] string type, [FromQuery] int parent) =>
            AnswerAsync(() => service.Value.GetTypeListAsync(type, parent));

        /// <summary>A board's filter panel for today's search ids (<c>section</c>, <c>base</c>, <c>lesson</c>), with counts;
        /// <c>isPaper</c> false for the study materials tab.</summary>
        [HttpGet("filters"), Produces<ApiResponse<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicFilterDto>>>>()]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = CacheSeconds, VaryByQueryKeys = ["*"])]
        public Task<IActionResult<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicFilterDto>>>> GetFilters([FromQuery] int section, [FromQuery(Name = "base")] int? grade,
            [FromQuery] int? lesson, [FromQuery] bool isPaper = true) => AnswerAsync(() => service.Value.GetFiltersAsync(section, grade, lesson, isPaper));

        /// <summary>A page's title and description: <c>page</c>, the board (<c>section</c>), and every other query key a value
        /// (<c>board</c>, <c>qualification</c>, <c>subject</c>, <c>title</c>...).</summary>
        [HttpGet("seo"), Produces<ApiResponse<CurriculumAnswerDto<CurriculumSeoResultDto>>>()]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = CacheSeconds, VaryByQueryKeys = ["*"])]
        public Task<IActionResult<CurriculumAnswerDto<CurriculumSeoResultDto>>> GetSeo([FromQuery] CurriculumSeoPage? page, [FromQuery] int? section)
        {
            Dictionary<string, string?> values = new(StringComparer.Ordinal);
            foreach (var (key, value) in Request.Query.Where(t => t.Key is not ("page" or "section")))
            {
                values[key] = value.ToString();
            }

            return AnswerAsync(() => service.Value.GetSeoAsync(page ?? CurriculumSeoPage.PastPaperList, section, values));
        }

        /// <summary>A past paper's title and description, worked out from the model.</summary>
        [HttpGet("seo/paper/{id:long}"), Produces<ApiResponse<CurriculumAnswerDto<CurriculumSeoResultDto>>>()]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = CacheSeconds)]
        public Task<IActionResult<CurriculumAnswerDto<CurriculumSeoResultDto>>> GetPaperSeo([FromRoute] long id) => AnswerAsync(() => service.Value.GetPaperSeoAsync(id));

        /// <summary>A subject's lesson tree with linked tutorials (by today's subject id), in the shape of <c>search/tutorials/lessonTree</c>.</summary>
        [HttpGet("tree/{subjectId:int}"), Produces<ApiResponse<CurriculumAnswerDto<JsonObject>>>()]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = CacheSeconds)]
        public Task<IActionResult<CurriculumAnswerDto<JsonObject>>> GetLessonTree([FromRoute] int subjectId) => AnswerAsync(() => service.Value.GetLessonTreeAsync(subjectId));

        /// <summary>The schools teaching a board, by name, optionally in one city.</summary>
        [HttpGet("boards/{id:int}/schools"), Produces<ApiResponse<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicSchoolDto>>>>()]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = CacheSeconds, VaryByQueryKeys = ["*"])]
        public Task<IActionResult<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicSchoolDto>>>> GetSchools([FromRoute] int id, [FromQuery] int? cityId,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 24) => AnswerAsync(() => service.Value.GetSchoolsAsync(id, cityId, page, pageSize));

        /// <summary>A board, qualification or syllabus landing page (<c>/boards/cambridge/igcse/0580-mathematics</c>).</summary>
        [HttpGet("landing/{board}/{level?}/{subject?}"), Produces<ApiResponse<CurriculumAnswerDto<CurriculumLandingDto>>>()]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = CacheSeconds)]
        public Task<IActionResult<CurriculumAnswerDto<CurriculumLandingDto>>> GetLanding([FromRoute] string board, [FromRoute] string? level, [FromRoute] string? subject) =>
            AnswerAsync(() => service.Value.GetLandingAsync(board, level, subject));

        private async Task<IActionResult<T>> AnswerAsync<T>(Func<Task<ResultData<T>>> call)
        {
            try
            {
                var result = await call();
                return Ok<T>(new(result.Errors) { Data = result.Data });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return Ok<T>(new(new Error { Message = exc.Message }));
            }
        }
    }
}
