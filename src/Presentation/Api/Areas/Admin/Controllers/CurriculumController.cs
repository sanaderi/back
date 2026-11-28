namespace GamaEdtech.Presentation.Api.Areas.Admin.Controllers
{
    using System.Diagnostics.CodeAnalysis;

    using Asp.Versioning;

    using GamaEdtech.Application.Interface;
    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.Identity;
    using GamaEdtech.Data.Dto.Curriculum;
    using GamaEdtech.Domain.Enumeration;

    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The curriculum model's admin screens (phase 1: nothing visitors see reads it yet): boards and their profile,
    /// qualifications, subjects, syllabi, papers, document types, topic trees and links, SEO templates, and previews of the
    /// filter panel, the linked lists and the SEO texts. Admins and anyone given the delegated permission, like the topic
    /// matches: a board edit never touches money or users. Copied rows are refused (<c>notNative</c>) until
    /// <c>Migration:WriteRoutes:Curriculum</c> is <c>Native</c>. See docs/business/curriculum.md.
    /// </summary>
    [Common.DataAnnotation.Area(nameof(Admin), "Admin")]
    [Route("api/v{version:apiVersion}/[area]/[controller]")]
    [ApiVersion("1.0")]
    [Permission(Roles = [nameof(Role.Admin)])]
    [DelegablePermission]
    public class CurriculumController(Lazy<ILogger<CurriculumController>> logger, Lazy<ICurriculumAdminService> adminService, Lazy<ICurriculumService> curriculumService)
        : ApiControllerBase<CurriculumController>(logger)
    {
        #region Boards

        [HttpGet("boards"), Produces<ApiResponse<IReadOnlyList<CurriculumBoardDto>>>()]
        public Task<IActionResult<IReadOnlyList<CurriculumBoardDto>>> GetBoards() => AnswerAsync(() => adminService.Value.GetBoardsAsync());

        /// <summary>A board with its whole profile.</summary>
        [HttpGet("boards/{id:int}"), Produces<ApiResponse<CurriculumBoardDetailDto>>()]
        public Task<IActionResult<CurriculumBoardDetailDto>> GetBoard([FromRoute] int id) => AnswerAsync(() => adminService.Value.GetBoardAsync(id));

        /// <summary>Creates a board (hidden until <c>isEnabled</c>); answers its id.</summary>
        [HttpPost("boards"), Produces<ApiResponse<int>>()]
        public Task<IActionResult<int>> CreateBoard([NotNull, FromBody] CurriculumBoardDto request)
        {
            request.Id = 0;
            return AnswerAsync(() => adminService.Value.SaveBoardAsync(request, User.UserId()));
        }

        [HttpPut("boards/{id:int}"), Produces<ApiResponse<int>>()]
        public Task<IActionResult<int>> UpdateBoard([FromRoute] int id, [NotNull, FromBody] CurriculumBoardDto request)
        {
            request.Id = id;
            return AnswerAsync(() => adminService.Value.SaveBoardAsync(request, User.UserId()));
        }

        /// <summary>Removes a board added here that nothing points at.</summary>
        [HttpDelete("boards/{id:int}"), Produces<ApiResponse<bool>>()]
        public Task<IActionResult<bool>> DeleteBoard([FromRoute] int id) => AnswerAsync(() => adminService.Value.DeleteBoardAsync(id));

        /// <summary>Replaces the board's filters (which, their labels, styles and order).</summary>
        [HttpPut("boards/{id:int}/filters"), Produces<ApiResponse<bool>>()]
        public Task<IActionResult<bool>> SaveFilters([FromRoute] int id, [NotNull, FromBody] IReadOnlyList<CurriculumBoardFilterDto> request) =>
            AnswerAsync(() => adminService.Value.SaveBoardFiltersAsync(id, request));

        /// <summary>Saves the board's tab scopes (own / own and GAMA / all) and its order of the other boards.</summary>
        [HttpPut("boards/{id:int}/links"), Produces<ApiResponse<bool>>()]
        public Task<IActionResult<bool>> SaveLinks([FromRoute] int id, [NotNull, FromBody] CurriculumBoardLinksDto request) =>
            AnswerAsync(() => adminService.Value.SaveBoardLinksAsync(id, request));

        /// <summary>Replaces the board's series types (an item with <c>id</c> 0 is added, one left out is removed).</summary>
        [HttpPut("boards/{id:int}/series-types"), Produces<ApiResponse<bool>>()]
        public Task<IActionResult<bool>> SaveSeriesTypes([FromRoute] int id, [NotNull, FromBody] IReadOnlyList<CurriculumSeriesTypeDto> request) =>
            AnswerAsync(() => adminService.Value.SaveSeriesTypesAsync(id, request));

        /// <summary>Replaces the board's variant kinds with their options.</summary>
        [HttpPut("boards/{id:int}/variant-kinds"), Produces<ApiResponse<bool>>()]
        public Task<IActionResult<bool>> SaveVariantKinds([FromRoute] int id, [NotNull, FromBody] IReadOnlyList<CurriculumVariantKindDto> request) =>
            AnswerAsync(() => adminService.Value.SaveVariantKindsAsync(id, request));

        /// <summary>The filter panel as visitors will see it (the wizard's live preview). Counts come in phase 3.</summary>
        [HttpGet("boards/{id:int}/panel"), Produces<ApiResponse<IReadOnlyList<CurriculumPanelFilterDto>>>()]
        public Task<IActionResult<IReadOnlyList<CurriculumPanelFilterDto>>> GetPanel([FromRoute] int id, [FromQuery] int? qualificationId, [FromQuery] int? syllabusId) =>
            AnswerAsync(() => curriculumService.Value.GetFilterPanelAsync(id, qualificationId, syllabusId));

        /// <summary>What a tab of the board lists for one of its topics: own, GAMA's, the other boards', and the related.</summary>
        [HttpGet("boards/{id:int}/linked-topics/{topicId:int}"), Produces<ApiResponse<CurriculumLinkedTopicsDto>>()]
        public Task<IActionResult<CurriculumLinkedTopicsDto>> GetLinkedTopics([FromRoute] int id, [FromRoute] int topicId, [FromQuery] CurriculumContentTab? tab) =>
            AnswerAsync(() => curriculumService.Value.GetLinkedTopicsAsync(id, topicId, tab ?? CurriculumContentTab.Tutorials));

        #endregion

        #region Qualifications, subjects, syllabi, papers, document types

        [HttpGet("boards/{boardId:int}/qualifications"), Produces<ApiResponse<IReadOnlyList<CurriculumQualificationDto>>>()]
        public Task<IActionResult<IReadOnlyList<CurriculumQualificationDto>>> GetQualifications([FromRoute] int boardId) =>
            AnswerAsync(() => adminService.Value.GetQualificationsAsync(boardId));

        [HttpPost("qualifications"), Produces<ApiResponse<int>>()]
        public Task<IActionResult<int>> CreateQualification([NotNull, FromBody] CurriculumQualificationDto request)
        {
            request.Id = 0;
            return AnswerAsync(() => adminService.Value.SaveQualificationAsync(request, User.UserId()));
        }

        [HttpPut("qualifications/{id:int}"), Produces<ApiResponse<int>>()]
        public Task<IActionResult<int>> UpdateQualification([FromRoute] int id, [NotNull, FromBody] CurriculumQualificationDto request)
        {
            request.Id = id;
            return AnswerAsync(() => adminService.Value.SaveQualificationAsync(request, User.UserId()));
        }

        [HttpDelete("qualifications/{id:int}"), Produces<ApiResponse<bool>>()]
        public Task<IActionResult<bool>> DeleteQualification([FromRoute] int id) => AnswerAsync(() => adminService.Value.DeleteQualificationAsync(id));

        [HttpGet("subjects"), Produces<ApiResponse<IReadOnlyList<CurriculumSubjectDto>>>()]
        public Task<IActionResult<IReadOnlyList<CurriculumSubjectDto>>> GetSubjects() => AnswerAsync(() => adminService.Value.GetSubjectsAsync());

        [HttpPost("subjects"), Produces<ApiResponse<int>>()]
        public Task<IActionResult<int>> CreateSubject([NotNull, FromBody] CurriculumSubjectDto request)
        {
            request.Id = 0;
            return AnswerAsync(() => adminService.Value.SaveSubjectAsync(request));
        }

        [HttpPut("subjects/{id:int}"), Produces<ApiResponse<int>>()]
        public Task<IActionResult<int>> UpdateSubject([FromRoute] int id, [NotNull, FromBody] CurriculumSubjectDto request)
        {
            request.Id = id;
            return AnswerAsync(() => adminService.Value.SaveSubjectAsync(request));
        }

        [HttpDelete("subjects/{id:int}"), Produces<ApiResponse<bool>>()]
        public Task<IActionResult<bool>> DeleteSubject([FromRoute] int id) => AnswerAsync(() => adminService.Value.DeleteSubjectAsync(id));

        [HttpGet("qualifications/{qualificationId:int}/syllabi"), Produces<ApiResponse<IReadOnlyList<CurriculumSyllabusDto>>>()]
        public Task<IActionResult<IReadOnlyList<CurriculumSyllabusDto>>> GetSyllabi([FromRoute] int qualificationId) =>
            AnswerAsync(() => adminService.Value.GetSyllabiAsync(qualificationId));

        [HttpPost("syllabi"), Produces<ApiResponse<int>>()]
        public Task<IActionResult<int>> CreateSyllabus([NotNull, FromBody] CurriculumSyllabusDto request)
        {
            request.Id = 0;
            return AnswerAsync(() => adminService.Value.SaveSyllabusAsync(request, User.UserId()));
        }

        [HttpPut("syllabi/{id:int}"), Produces<ApiResponse<int>>()]
        public Task<IActionResult<int>> UpdateSyllabus([FromRoute] int id, [NotNull, FromBody] CurriculumSyllabusDto request)
        {
            request.Id = id;
            return AnswerAsync(() => adminService.Value.SaveSyllabusAsync(request, User.UserId()));
        }

        [HttpDelete("syllabi/{id:int}"), Produces<ApiResponse<bool>>()]
        public Task<IActionResult<bool>> DeleteSyllabus([FromRoute] int id) => AnswerAsync(() => adminService.Value.DeleteSyllabusAsync(id));

        [HttpGet("syllabi/{id:int}/papers"), Produces<ApiResponse<IReadOnlyList<CurriculumPaperDto>>>()]
        public Task<IActionResult<IReadOnlyList<CurriculumPaperDto>>> GetPapers([FromRoute] int id) => AnswerAsync(() => adminService.Value.GetPapersAsync(id));

        /// <summary>Replaces the syllabus's papers (an item with <c>id</c> 0 is added, one left out is removed).</summary>
        [HttpPut("syllabi/{id:int}/papers"), Produces<ApiResponse<bool>>()]
        public Task<IActionResult<bool>> SavePapers([FromRoute] int id, [NotNull, FromBody] IReadOnlyList<CurriculumPaperDto> request) =>
            AnswerAsync(() => adminService.Value.SavePapersAsync(id, request));

        [HttpGet("document-types"), Produces<ApiResponse<IReadOnlyList<CurriculumDocumentTypeDto>>>()]
        public Task<IActionResult<IReadOnlyList<CurriculumDocumentTypeDto>>> GetDocumentTypes() => AnswerAsync(() => adminService.Value.GetDocumentTypesAsync());

        [HttpPost("document-types"), Produces<ApiResponse<int>>()]
        public Task<IActionResult<int>> CreateDocumentType([NotNull, FromBody] CurriculumDocumentTypeDto request)
        {
            request.Id = 0;
            return AnswerAsync(() => adminService.Value.SaveDocumentTypeAsync(request));
        }

        [HttpPut("document-types/{id:int}"), Produces<ApiResponse<int>>()]
        public Task<IActionResult<int>> UpdateDocumentType([FromRoute] int id, [NotNull, FromBody] CurriculumDocumentTypeDto request)
        {
            request.Id = id;
            return AnswerAsync(() => adminService.Value.SaveDocumentTypeAsync(request));
        }

        [HttpDelete("document-types/{id:int}"), Produces<ApiResponse<bool>>()]
        public Task<IActionResult<bool>> DeleteDocumentType([FromRoute] int id) => AnswerAsync(() => adminService.Value.DeleteDocumentTypeAsync(id));

        #endregion

        #region Topics, links, SEO

        /// <summary>A syllabus's topics, units first then their chapters.</summary>
        [HttpGet("syllabi/{id:int}/topics"), Produces<ApiResponse<IReadOnlyList<CurriculumTopicDto>>>()]
        public Task<IActionResult<IReadOnlyList<CurriculumTopicDto>>> GetTopics([FromRoute] int id) => AnswerAsync(() => adminService.Value.GetTopicsAsync(id));

        [HttpPost("topics"), Produces<ApiResponse<int>>()]
        public Task<IActionResult<int>> CreateTopic([NotNull, FromBody] CurriculumTopicDto request)
        {
            request.Id = 0;
            return AnswerAsync(() => adminService.Value.SaveTopicAsync(request, User.UserId()));
        }

        [HttpPut("topics/{id:int}"), Produces<ApiResponse<int>>()]
        public Task<IActionResult<int>> UpdateTopic([FromRoute] int id, [NotNull, FromBody] CurriculumTopicDto request)
        {
            request.Id = id;
            return AnswerAsync(() => adminService.Value.SaveTopicAsync(request, User.UserId()));
        }

        [HttpDelete("topics/{id:int}"), Produces<ApiResponse<bool>>()]
        public Task<IActionResult<bool>> DeleteTopic([FromRoute] int id) => AnswerAsync(() => adminService.Value.DeleteTopicAsync(id));

        [HttpGet("topics/{id:int}/links"), Produces<ApiResponse<IReadOnlyList<CurriculumTopicLinkDto>>>()]
        public Task<IActionResult<IReadOnlyList<CurriculumTopicLinkDto>>> GetTopicLinks([FromRoute] int id) => AnswerAsync(() => adminService.Value.GetTopicLinksAsync(id));

        /// <summary>Links a GAMA topic (<c>masterTopicId</c>) and another board's topic, exact or partial; answers its id.</summary>
        [HttpPost("topic-links"), Produces<ApiResponse<int>>()]
        public Task<IActionResult<int>> AddTopicLink([NotNull, FromBody] CurriculumTopicLinkAddDto request)
        {
            request.UserId = User.UserId();
            return AnswerAsync(() => adminService.Value.AddTopicLinkAsync(request));
        }

        [HttpDelete("topic-links/{id:int}"), Produces<ApiResponse<bool>>()]
        public Task<IActionResult<bool>> DeleteTopicLink([FromRoute] int id) => AnswerAsync(() => adminService.Value.DeleteTopicLinkAsync(id));

        /// <summary>The default SEO templates (no <c>boardId</c>) or a board's own.</summary>
        [HttpGet("seo-templates"), Produces<ApiResponse<IReadOnlyList<CurriculumSeoTemplateDto>>>()]
        public Task<IActionResult<IReadOnlyList<CurriculumSeoTemplateDto>>> GetSeoTemplates([FromQuery] int? boardId) =>
            AnswerAsync(() => adminService.Value.GetSeoTemplatesAsync(boardId));

        [HttpPost("seo-templates"), Produces<ApiResponse<int>>()]
        public Task<IActionResult<int>> CreateSeoTemplate([NotNull, FromBody] CurriculumSeoTemplateDto request)
        {
            request.Id = 0;
            return AnswerAsync(() => adminService.Value.SaveSeoTemplateAsync(request, User.UserId()));
        }

        [HttpPut("seo-templates/{id:int}"), Produces<ApiResponse<int>>()]
        public Task<IActionResult<int>> UpdateSeoTemplate([FromRoute] int id, [NotNull, FromBody] CurriculumSeoTemplateDto request)
        {
            request.Id = id;
            return AnswerAsync(() => adminService.Value.SaveSeoTemplateAsync(request, User.UserId()));
        }

        /// <summary>Removes a board's own template; its pages go back to the default.</summary>
        [HttpDelete("seo-templates/{id:int}"), Produces<ApiResponse<bool>>()]
        public Task<IActionResult<bool>> DeleteSeoTemplate([FromRoute] int id) => AnswerAsync(() => adminService.Value.DeleteSeoTemplateAsync(id));

        /// <summary>A page's title and description written with sample <c>values</c> (the template screen's preview).</summary>
        [HttpPost("seo-preview"), Produces<ApiResponse<CurriculumSeoResultDto>>()]
        public Task<IActionResult<CurriculumSeoResultDto>> PreviewSeo([FromQuery] int? boardId, [FromQuery] CurriculumSeoPage? page,
            [NotNull, FromBody] Dictionary<string, string?> values) =>
            AnswerAsync(() => curriculumService.Value.GetSeoAsync(boardId, page ?? CurriculumSeoPage.PastPaperList, values));

        #endregion

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
