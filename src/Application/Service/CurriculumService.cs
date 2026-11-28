namespace GamaEdtech.Application.Service
{
    using System.Linq;

    using GamaEdtech.Application.Interface;
    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.UnitOfWork;
    using GamaEdtech.Common.Service;
    using GamaEdtech.Data.Dto.Curriculum;
    using GamaEdtech.Domain.Entity.Curriculum;
    using GamaEdtech.Domain.Enumeration;

    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using static GamaEdtech.Common.Core.Constants;

    public sealed class CurriculumService(Lazy<IUnitOfWorkProvider> unitOfWorkProvider, Lazy<IHttpContextAccessor> httpContextAccessor,
        Lazy<ILogger<CurriculumService>> logger)
        : ServiceBase<CurriculumService>(unitOfWorkProvider, httpContextAccessor, logger), ICurriculumService
    {
        public async Task<ResultData<IReadOnlyList<CurriculumPanelFilterDto>>> GetFilterPanelAsync(int boardId, int? qualificationId, int? syllabusId)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                if (!await uow.GetRepository<CurriculumBoard, int>().AnyAsync(t => t.Id == boardId))
                {
                    return NotFound<IReadOnlyList<CurriculumPanelFilterDto>>();
                }

                var filters = await uow.GetRepository<CurriculumBoardFilter, int>().GetManyQueryable(t => t.BoardId == boardId && t.IsEnabled)
                    .OrderBy(t => t.ListOrder).ToListAsync();
                var papers = syllabusId is int syllabus
                    ? await uow.GetRepository<CurriculumPaper, int>().GetManyQueryable(t => t.SyllabusId == syllabus).OrderBy(t => t.ListOrder).ToListAsync()
                    : [];

                List<CurriculumPanelFilterDto> panel = [];
                foreach (var filter in filters)
                {
                    var options = await OptionsAsync(uow, boardId, qualificationId, filter.Key!, papers);
                    panel.Add(new() { Key = filter.Key!, Label = filter.Label!, Style = filter.Style ?? CurriculumFilterStyle.Select, Options = options });
                }

                return new(OperationResult.Succeeded) { Data = panel };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message }] };
            }
        }

        public async Task<ResultData<CurriculumLinkedTopicsDto>> GetLinkedTopicsAsync(int boardId, int topicId, CurriculumContentTab tab)
        {
            ArgumentNullException.ThrowIfNull(tab);
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var boards = await uow.GetRepository<CurriculumBoard, int>().GetManyQueryable().Select(t => new { t.Id, t.Name, t.IsMaster, t.ListOrder }).ToListAsync();
                var board = boards.Find(t => t.Id == boardId);
                var place = (await CurriculumAdminService.TopicPlacesAsync(uow, [topicId])).GetValueOrDefault(topicId);
                if (board is null || place is null || place.BoardId != boardId)
                {
                    return NotFound<CurriculumLinkedTopicsDto>();
                }

                // A unit's list is its chapters' too.
                var topics = uow.GetRepository<CurriculumTopic, int>();
                List<int> own = [topicId, .. await topics.GetManyQueryable(t => t.ParentTopicId == topicId).OrderBy(t => t.ListOrder).Select(t => t.Id).ToListAsync()];

                var linkRepository = uow.GetRepository<CurriculumTopicLink, int>();
                var direct = await linkRepository.GetManyQueryable(t => own.Contains(t.TopicId) || own.Contains(t.MasterTopicId)).ToListAsync();
                List<int> masters = [.. direct.Select(t => t.MasterTopicId).Distinct()];
                var links = await linkRepository.GetManyQueryable(t => masters.Contains(t.MasterTopicId)).ToListAsync();
                var places = await CurriculumAdminService.TopicPlacesAsync(uow, [.. own, .. links.SelectMany(t => new[] { t.MasterTopicId, t.TopicId })]);

                var scope = (await uow.GetRepository<CurriculumBoardTab, int>().GetAsync(t => t.BoardId == boardId && t.Tab == tab, tracking: false))?.Scope
                    ?? CurriculumLinkRules.DefaultScope(tab);
                var order = await uow.GetRepository<CurriculumBoardLinkOrder, int>().GetManyQueryable(t => t.BoardId == boardId).OrderBy(t => t.ListOrder)
                    .Select(t => t.OtherBoardId).ToListAsync();

                var resolved = CurriculumLinkRules.Resolve(boardId, board.IsMaster, own,
                    links.Where(t => places.ContainsKey(t.TopicId)).Select(t => new CurriculumLinkEdge(t.MasterTopicId, t.TopicId, places[t.TopicId].BoardId, t.Kind)),
                    scope, order, boards.ToDictionary(t => t.Id, t => t.ListOrder));

                var legacy = await topics.GetManyQueryable(t => places.Keys.Contains(t.Id)).Select(t => new { t.Id, t.LegacyId }).ToDictionaryAsync(t => t.Id, t => t.LegacyId);
                List<CurriculumLinkedTopicDto> Named(IEnumerable<int> ids) => [.. ids.Select(id => new CurriculumLinkedTopicDto
                {
                    Id = id,
                    Name = places.GetValueOrDefault(id)?.TopicName,
                    LegacyId = legacy.GetValueOrDefault(id),
                })];

                var master = boards.Find(t => t.IsMaster);
                List<CurriculumLinkedGroupDto> groups = [new() { Source = CurriculumLinkSource.Own, BoardId = boardId, BoardName = board.Name, Topics = Named(resolved.Own) }];
                if (resolved.Master.Count > 0 && master is not null)
                {
                    groups.Add(new() { Source = CurriculumLinkSource.Master, BoardId = master.Id, BoardName = master.Name, Topics = Named(resolved.Master) });
                }

                groups.AddRange(resolved.OtherBoards.Select(t => new CurriculumLinkedGroupDto
                {
                    Source = CurriculumLinkSource.OtherBoard,
                    BoardId = t.BoardId,
                    BoardName = boards.Find(b => b.Id == t.BoardId)?.Name,
                    Topics = Named(t.TopicIds),
                }));

                return new(OperationResult.Succeeded) { Data = new() { Tab = tab, Scope = scope, Groups = groups, Related = Named(resolved.Related) } };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message }] };
            }
        }

        public async Task<ResultData<CurriculumSeoResultDto>> GetSeoAsync(int? boardId, CurriculumSeoPage page, IReadOnlyDictionary<string, string?> values)
        {
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(values);
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var templates = await uow.GetRepository<CurriculumSeoTemplate, int>()
                    .GetManyQueryable(t => t.Page == page && (t.BoardId == null || t.BoardId == boardId)).ToListAsync();
                var template = templates.Find(t => t.BoardId != null) ?? templates.Find(t => t.BoardId == null);
                if (template is null)
                {
                    return NotFound<CurriculumSeoResultDto>();
                }

                // Only the exam page's description is cut, as the website did; the paper pages keep their file codes.
                var description = page == CurriculumSeoPage.Exam
                    ? CurriculumTemplate.RenderDescription(template.DescriptionTemplate, values)
                    : CurriculumTemplate.Render(template.DescriptionTemplate, values);
                var title = CurriculumTemplate.Render(template.TitleTemplate, values);
                return new(OperationResult.Succeeded) { Data = new() { Title = title, Description = description, Warnings = CurriculumTemplate.Warnings(title, description) } };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message }] };
            }
        }

        private static async Task<List<CurriculumFilterOptionDto>> OptionsAsync(IUnitOfWork uow, int boardId, int? qualificationId, string key, List<CurriculumPaper> papers)
        {
            switch (key)
            {
                case CurriculumFilterKeys.Level:
                    return await uow.GetRepository<CurriculumQualification, int>().GetManyQueryable(t => t.BoardId == boardId)
                        .OrderBy(t => t.ListOrder).ThenBy(t => t.Name)
                        .Select(t => new CurriculumFilterOptionDto { Value = t.Slug!, Label = t.Name! }).ToListAsync();
                case CurriculumFilterKeys.Subject when qualificationId is int qualification:
                    return await uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable(t => t.QualificationId == qualification)
                        .OrderBy(t => t.ListOrder).ThenBy(t => t.Name)
                        .Select(t => new CurriculumFilterOptionDto { Value = t.Slug!, Label = t.Code == null ? t.Name! : t.Name + " (" + t.Code + ")" }).ToListAsync();
                case CurriculumFilterKeys.Series:
                    var series = await uow.GetRepository<CurriculumSeriesType, int>()
                        .GetManyQueryable(t => t.BoardId == boardId && (t.QualificationId == null || t.QualificationId == qualificationId))
                        .OrderBy(t => t.ListOrder).Select(t => new { t.Id, t.Name }).ToListAsync();
                    return [.. series.Select(t => new CurriculumFilterOptionDto { Value = t.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), Label = t.Name! })];
                case CurriculumFilterKeys.Paper:
                    return [.. papers.Select(t => new CurriculumFilterOptionDto { Value = t.ShortLabel!, Label = t.ShortLabel! }).DistinctBy(t => t.Value)];
                case CurriculumFilterKeys.Tier:
                    return [.. papers.Where(t => t.Tier != null).Select(t => new CurriculumFilterOptionDto { Value = t.Tier!, Label = t.Tier! }).DistinctBy(t => t.Value)];
                default:
                    // Years and languages come from the papers themselves, with their counts (phase 3).
                    return key.StartsWith(CurriculumFilterKeys.Variant + ".", StringComparison.Ordinal)
                        ? await VariantOptionsAsync(uow, boardId, key[(CurriculumFilterKeys.Variant.Length + 1)..])
                        : [];
            }
        }

        private static async Task<List<CurriculumFilterOptionDto>> VariantOptionsAsync(IUnitOfWork uow, int boardId, string kindKey)
        {
            var kind = await uow.GetRepository<CurriculumVariantKind, int>().GetAsync(t => t.BoardId == boardId && t.Key == kindKey, tracking: false);
            return kind is null
                ? []
                : await uow.GetRepository<CurriculumVariantOption, int>().GetManyQueryable(t => t.VariantKindId == kind.Id).OrderBy(t => t.ListOrder)
                    .Select(t => new CurriculumFilterOptionDto { Value = t.Code ?? t.Label!, Label = t.Label! }).ToListAsync();
        }

        private static ResultData<T> NotFound<T>() => new(OperationResult.NotFound) { Errors = [new() { Message = "Not found.", Reference = "notFound" }] };
    }
}
