namespace GamaEdtech.Application.Service
{
    using System.Globalization;
    using System.Linq;
    using System.Text.Json.Nodes;

    using GamaEdtech.Application.Interface;
    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.UnitOfWork;
    using GamaEdtech.Common.Service;
    using GamaEdtech.Data.Dto.Curriculum;
    using GamaEdtech.Domain.Entity;
    using GamaEdtech.Domain.Entity.Curriculum;
    using GamaEdtech.Domain.Enumeration;

    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using static GamaEdtech.Common.Core.Constants;

    public sealed class CurriculumPublicService(Lazy<IUnitOfWorkProvider> unitOfWorkProvider, Lazy<IHttpContextAccessor> httpContextAccessor,
        Lazy<ILogger<CurriculumPublicService>> logger, Lazy<IMigrationRouter> migrationRouter, Lazy<ICurriculumService> curriculumService)
        : ServiceBase<CurriculumPublicService>(unitOfWorkProvider, httpContextAccessor, logger), ICurriculumPublicService
    {
        private const int TreeTutorialsPerTopic = 20;
        private const int LandingSchools = 12;
        private const int MaxSchoolPage = 100;

        // The search parameter each filter sets (today's search takes the lookup lists' ids).
        private static readonly Dictionary<string, string> Params = new(StringComparer.Ordinal)
        {
            [CurriculumFilterKeys.Level] = "base",
            [CurriculumFilterKeys.Subject] = "lesson",
            [CurriculumFilterKeys.Year] = "edu_year",
            [CurriculumFilterKeys.Series] = "edu_month",
            [CurriculumFilterKeys.Paper] = "test_type",
            [CurriculumFilterKeys.Tier] = "tier",
            [CurriculumFilterKeys.Language] = "language",
        };

        private bool Native => migrationRouter.Value.GetRoute(MigrationDomain.Curriculum, MigrationFeatures.Website) == MigrationRoute.Native;

        public Task<ResultData<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicItemDto>>>> GetBoardsAsync() => AnswerAsync<IReadOnlyList<CurriculumPublicItemDto>>(async uow =>
            await uow.GetRepository<CurriculumBoard, int>().GetManyQueryable(t => t.IsEnabled).OrderBy(t => t.ListOrder).ThenBy(t => t.Name)
                .Select(t => new CurriculumPublicItemDto
                {
                    Id = t.Id,
                    LegacyId = t.LegacyId,
                    Slug = t.Slug,
                    Name = t.Name,
                    ShortName = t.ShortName,
                    Logo = t.Logo,
                    CountryCode = t.CountryCode,
                    IsMaster = t.IsMaster,
                    IsDefault = t.IsDefault,
                    ListOrder = t.ListOrder,
                }).ToListAsync());

        public Task<ResultData<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicItemDto>>>> GetChildrenAsync(string kind, int parentId) => AnswerAsync<IReadOnlyList<CurriculumPublicItemDto>>(async uow => kind switch
        {
            "levels" => await uow.GetRepository<CurriculumQualification, int>().GetManyQueryable(t => t.BoardId == parentId).OrderBy(t => t.ListOrder).ThenBy(t => t.Name)
                .Select(t => new CurriculumPublicItemDto { Id = t.Id, LegacyId = t.LegacyId, Slug = t.Slug, Name = t.Name, ShortName = t.ShortName, ParentId = t.BoardId, ListOrder = t.ListOrder }).ToListAsync(),
            "subjects" => await uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable(t => t.QualificationId == parentId).OrderBy(t => t.ListOrder).ThenBy(t => t.Name)
                .Select(t => new CurriculumPublicItemDto { Id = t.Id, LegacyId = t.LegacyId, Slug = t.Slug, Name = t.Name, Code = t.Code, ParentId = t.QualificationId, ListOrder = t.ListOrder }).ToListAsync(),
            "topics" => await uow.GetRepository<CurriculumTopic, int>().GetManyQueryable(t => t.SyllabusId == parentId).OrderBy(t => t.ListOrder).ThenBy(t => t.Id)
                .Select(t => new CurriculumPublicItemDto { Id = t.Id, LegacyId = t.LegacyId, Name = t.Name, ParentId = t.ParentTopicId, IsUnit = t.IsUnit, ListOrder = t.ListOrder }).ToListAsync(),
            _ => [],
        });

        public Task<ResultData<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicItemDto>>>> GetTypeListAsync(string type, int parentLegacyId) =>
            AnswerAsync<IReadOnlyList<CurriculumPublicItemDto>>(async uow =>
            {
                switch (type)
                {
                    case "base":
                    {
                        var boardId = await uow.GetRepository<CurriculumBoard, int>().GetManyQueryable(t => t.LegacyId == parentLegacyId).Select(t => (int?)t.Id).FirstOrDefaultAsync();
                        return boardId is int id ? ((await GetChildrenAsync("levels", id)).Data?.Data ?? []).Where(t => t.LegacyId != null).ToList() : [];
                    }

                    case "lesson":
                    {
                        var levelId = await uow.GetRepository<CurriculumQualification, int>().GetManyQueryable(t => t.LegacyId == parentLegacyId).Select(t => (int?)t.Id).FirstOrDefaultAsync();
                        return levelId is int id ? ((await GetChildrenAsync("subjects", id)).Data?.Data ?? []).Where(t => t.LegacyId != null).ToList() : [];
                    }

                    case "topic":
                    {
                        var syllabusId = await uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable(t => t.LegacyId == parentLegacyId).Select(t => (int?)t.Id).FirstOrDefaultAsync();
                        return syllabusId is int id ? ((await GetChildrenAsync("topics", id)).Data?.Data ?? []).Where(t => t.LegacyId != null).ToList() : [];
                    }

                    default:
                        return [];
                }
            });

        public Task<ResultData<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicFilterDto>>>> GetFiltersAsync(int boardLegacyId, int? levelLegacyId, int? subjectLegacyId, bool isPaper) =>
            AnswerAsync<IReadOnlyList<CurriculumPublicFilterDto>>(async uow =>
            {
                var board = await uow.GetRepository<CurriculumBoard, int>().GetManyQueryable(t => t.LegacyId == boardLegacyId).Select(t => new { t.Id }).FirstOrDefaultAsync();
                if (board is null)
                {
                    return [];
                }

                var level = levelLegacyId is int l
                    ? await uow.GetRepository<CurriculumQualification, int>().GetManyQueryable(t => t.LegacyId == l).Select(t => new { t.Id }).FirstOrDefaultAsync()
                    : null;
                var syllabus = subjectLegacyId is int s
                    ? await uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable(t => t.LegacyId == s).Select(t => new { t.Id }).FirstOrDefaultAsync()
                    : null;

                // The papers under the choice so far: counts come from them, so empty options never show.
                var papers = uow.GetRepository<PastPaper>().GetManyQueryable(t => t.Status == PastPaperStatus.Confirmed && !t.IsDeleted && t.IsPaper == isPaper && t.CoreBoardId == boardLegacyId);
                var underLevel = levelLegacyId is int grade ? papers.Where(t => t.CoreGradeId == grade) : papers;
                var underSubject = subjectLegacyId is int subject ? underLevel.Where(t => t.CoreSubjectId == subject) : underLevel;

                var filters = await uow.GetRepository<CurriculumBoardFilter, int>().GetManyQueryable(t => t.BoardId == board.Id && t.IsEnabled).OrderBy(t => t.ListOrder).ToListAsync();
                List<CurriculumPublicFilterDto> panel = [];
                foreach (var filter in filters)
                {
                    var options = await OptionsAsync(uow, filter.Key!, board.Id, level?.Id, syllabus?.Id, papers, underLevel, underSubject);
                    if (options is null)
                    {
                        continue;
                    }

                    panel.Add(new()
                    {
                        Key = filter.Key!,
                        Param = filter.Key!.StartsWith(CurriculumFilterKeys.Variant + ".", StringComparison.Ordinal) ? "variant" : Params.GetValueOrDefault(filter.Key!, filter.Key!),
                        Label = filter.Label!,
                        Style = (filter.Style ?? CurriculumFilterStyle.Select).Name,
                        Options = options,
                    });
                }

                return panel;
            });

        public Task<ResultData<CurriculumAnswerDto<CurriculumSeoResultDto>>> GetSeoAsync(CurriculumSeoPage page, int? boardLegacyId, IReadOnlyDictionary<string, string?> values) =>
            AnswerAsync(async uow =>
            {
                var boardId = boardLegacyId is int legacy
                    ? await uow.GetRepository<CurriculumBoard, int>().GetManyQueryable(t => t.LegacyId == legacy).Select(t => (int?)t.Id).FirstOrDefaultAsync()
                    : null;
                return (await curriculumService.Value.GetSeoAsync(boardId, page, values)).Data;
            });

        public Task<ResultData<CurriculumAnswerDto<CurriculumSeoResultDto>>> GetPaperSeoAsync(long paperId) => AnswerAsync(async uow =>
        {
            var paper = await uow.GetRepository<PastPaper>().GetManyQueryable(t => t.Id == paperId && !t.IsDeleted, includes: t => t.Include(p => p.Files)).FirstOrDefaultAsync();
            if (paper is null)
            {
                return null;
            }

            var board = await uow.GetRepository<CurriculumBoard, int>().GetManyQueryable(t => t.LegacyId == paper.CoreBoardId).FirstOrDefaultAsync();
            var level = await uow.GetRepository<CurriculumQualification, int>().GetManyQueryable(t => t.LegacyId == paper.CoreGradeId).FirstOrDefaultAsync();
            var syllabus = await uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable(t => t.LegacyId == paper.CoreSubjectId).FirstOrDefaultAsync();
            var component = syllabus is null || paper.CorePaperTypeId is not int type
                ? null
                : await uow.GetRepository<CurriculumPaper, int>().GetManyQueryable(t => t.SyllabusId == syllabus.Id && t.LegacyId == type).FirstOrDefaultAsync();
            var variant = paper.Variant is short v && board is not null
                ? await (from option in uow.GetRepository<CurriculumVariantOption, int>().GetManyQueryable(t => t.LegacyId == v)
                         join kind in uow.GetRepository<CurriculumVariantKind, int>().GetManyQueryable(t => t.BoardId == board.Id) on option.VariantKindId equals kind.Id
                         select option.Code).FirstOrDefaultAsync()
                : null;
            var month = paper.EduMonth?.ToString(CultureInfo.InvariantCulture);
            var series = month is null || board is null
                ? null
                : (await uow.GetRepository<CurriculumSeriesType, int>().GetManyQueryable(t => t.BoardId == board.Id && (t.QualificationId == null || t.QualificationId == level!.Id))
                    .ToListAsync()).Where(t => (t.Months ?? string.Empty).Split(',').Contains(month)).OrderByDescending(t => t.QualificationId != null).FirstOrDefault();

            var year = paper.EduYear is short y and > 0 ? y.ToString(CultureInfo.InvariantCulture) : null;
            var yy = year is { Length: 4 } ? year[2..] : null;
            var paperLabel = component?.ShortLabel;
            string? paperVariant = null;
            if (paperLabel is not null)
            {
                paperVariant = int.TryParse(variant, CultureInfo.InvariantCulture, out var variantNumber) && int.TryParse(paperLabel, CultureInfo.InvariantCulture, out _)
                    ? $"{paperLabel}{variantNumber}"
                    : paperLabel.PadLeft(2, '0');
            }
            var syllabusCode = syllabus?.Code ?? paper.Code;

            Dictionary<string, string?> values = new(StringComparer.Ordinal)
            {
                ["board"] = board?.Name,
                ["qualification"] = level?.Name,
                ["subject"] = syllabus?.Name,
                ["syllabusCode"] = syllabusCode,
                ["title"] = paper.Title,
                ["paper"] = paperLabel,
                ["paperVariant"] = paperVariant,
                ["series"] = series?.SeoName ?? series?.Name,
                ["seriesCode"] = series?.Code,
                ["year"] = year,
                ["yy"] = yy,
            };
            values["code"] = CurriculumTemplate.Render(board?.CodeTemplate, values);

            // The file list, as the website wrote it: question paper and mark scheme (not a Word one), then each extra kind once.
            if (series?.FileCode is { } fileCode && syllabusCode is not null && yy is not null && paperVariant is not null)
            {
                var prefix = $"{syllabusCode}_{fileCode}{yy}";
                var files = paper.Files ?? [];
                List<string> list = [];
                if (files.Any(t => t.Kind == PastPaperFileKind.Question))
                {
                    list.Add($"question paper (QP) {prefix}_qp_{paperVariant}");
                }

                if (files.FirstOrDefault(t => t.Kind == PastPaperFileKind.Answer) is { } answer && answer.Extension?.ToLowerInvariant() is not ("doc" or "docx" or "word"))
                {
                    list.Add($"mark scheme (MS) {prefix}_ms_{paperVariant}");
                }

                List<int> extraTypes = [.. files.Where(t => t.Kind == PastPaperFileKind.Extra && t.CoreTypeId != null).Select(t => t.CoreTypeId!.Value).Distinct()];
                var documents = await uow.GetRepository<CurriculumDocumentType, int>().GetManyQueryable(t => t.LegacyId != null && extraTypes.Contains(t.LegacyId.Value)).ToListAsync();
                foreach (var code in extraTypes.Select(t => documents.Find(d => d.LegacyId == t)?.Code).OfType<string>().Distinct())
                {
                    if (CurriculumCopyRules.SeoFileLabels.TryGetValue(code, out var label))
                    {
                        list.Add($"{label} {prefix}_{code}_{paperVariant}");
                    }
                }

                values["files"] = list.Count > 0 ? string.Join(", ", list) : null;
            }

            return (await curriculumService.Value.GetSeoAsync(board?.Id, paper.IsPaper ? CurriculumSeoPage.PastPaper : CurriculumSeoPage.StudyMaterial, values)).Data;
        });

        public Task<ResultData<CurriculumAnswerDto<JsonObject>>> GetLessonTreeAsync(int subjectLegacyId) => AnswerAsync(async uow =>
        {
            var syllabus = await uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable(t => t.LegacyId == subjectLegacyId).Select(t => new { t.Id, t.QualificationId }).FirstOrDefaultAsync();
            if (syllabus is null)
            {
                return null;
            }

            var boardId = await uow.GetRepository<CurriculumQualification, int>().GetManyQueryable(t => t.Id == syllabus.QualificationId).Select(t => t.BoardId).FirstAsync();
            var topics = await uow.GetRepository<CurriculumTopic, int>().GetManyQueryable(t => t.SyllabusId == syllabus.Id).OrderBy(t => t.ListOrder).ThenBy(t => t.Id).ToListAsync();

            // Each topic's keys: its own lookup id, and the topics the tutorials tab brings in for it.
            Dictionary<int, HashSet<string>> keysOf = [];
            foreach (var topic in topics)
            {
                HashSet<string> keys = [];
                if (topic.LegacyId is int own)
                {
                    _ = keys.Add(own.ToString(CultureInfo.InvariantCulture));
                }

                keysOf[topic.Id] = keys;
            }

            var linked = await curriculumService.Value.GetLinkedTopicsForTreeAsync(boardId, [.. topics.Select(t => t.Id)]);
            foreach (var (topicId, keys) in linked)
            {
                keysOf[topicId].UnionWith(keys);
            }

            List<string> allKeys = [.. keysOf.Values.SelectMany(t => t).Distinct()];
            var tutorials = allKeys.Count == 0
                ? []
                : await uow.GetRepository<Tutorial>().GetManyQueryable(t => t.Status == TutorialStatus.Confirmed)
                    .Where(TopicMatching.TopicIn<Tutorial>(nameof(Tutorial.CoreTopicIds), allKeys))
                    .OrderByDescending(t => t.LastModifyDate).ThenByDescending(t => t.Id)
                    .Select(t => new { t.Id, t.Title, t.CoreTopicIds }).ToListAsync();

            JsonArray Tutorials(int topicId)
            {
                var keys = keysOf[topicId];
                return [.. tutorials.Where(t => (t.CoreTopicIds ?? string.Empty).Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Any(keys.Contains))
                    .Take(TreeTutorialsPerTopic)
                    .Select(t => (JsonNode)new JsonObject
                    {
                        ["id"] = t.Id.ToString(CultureInfo.InvariantCulture),
                        ["title"] = t.Title,
                        ["title_url"] = PastPaperOutput.TitleUrl(t.Title),
                    })];
            }

            JsonArray list = [];
            foreach (var unit in topics.Where(t => t.ParentTopicId is null))
            {
                JsonObject item = new() { ["id"] = (unit.LegacyId ?? unit.Id).ToString(CultureInfo.InvariantCulture), ["title"] = unit.Name, ["season"] = unit.IsUnit };
                var chapters = topics.Where(t => t.ParentTopicId == unit.Id).ToList();
                if (unit.IsUnit || chapters.Count > 0)
                {
                    JsonArray chapterList = [];
                    var count = 0;
                    foreach (var chapter in chapters)
                    {
                        var chapterTutorials = Tutorials(chapter.Id);
                        count += chapterTutorials.Count;
                        chapterList.Add(new JsonObject
                        {
                            ["id"] = (chapter.LegacyId ?? chapter.Id).ToString(CultureInfo.InvariantCulture),
                            ["title"] = chapter.Name,
                            ["season"] = false,
                            ["tutorials"] = chapterTutorials,
                        });
                    }

                    item["season"] = true;
                    item["chapters"] = chapterList;
                    item["tutorialsNum"] = count;
                }
                else
                {
                    item["tutorials"] = Tutorials(unit.Id);
                }

                list.Add(item);
            }

            return new JsonObject { ["list"] = list };
        });

        public Task<ResultData<CurriculumAnswerDto<IReadOnlyList<CurriculumPublicSchoolDto>>>> GetSchoolsAsync(int boardId, int? cityId, int page, int pageSize) =>
            AnswerAsync<IReadOnlyList<CurriculumPublicSchoolDto>>(async uow =>
                await SchoolsQuery(uow, boardId, null, cityId).Skip((Math.Max(page, 1) - 1) * Math.Clamp(pageSize, 1, MaxSchoolPage)).Take(Math.Clamp(pageSize, 1, MaxSchoolPage)).ToListAsync());

        public Task<ResultData<CurriculumAnswerDto<CurriculumLandingDto>>> GetLandingAsync(string boardSlug, string? levelSlug, string? subjectSlug) => AnswerAsync(async uow =>
        {
            var board = await uow.GetRepository<CurriculumBoard, int>().GetManyQueryable(t => t.Slug == boardSlug && t.IsEnabled).FirstOrDefaultAsync();
            if (board is null)
            {
                return null;
            }

            var level = levelSlug is null ? null : await uow.GetRepository<CurriculumQualification, int>().GetManyQueryable(t => t.BoardId == board.Id && t.Slug == levelSlug).FirstOrDefaultAsync();
            var subject = level is null || subjectSlug is null ? null : await uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable(t => t.QualificationId == level.Id && t.Slug == subjectSlug).FirstOrDefaultAsync();
            if ((levelSlug is not null && level is null) || (subjectSlug is not null && subject is null))
            {
                return null;
            }

            var papers = uow.GetRepository<PastPaper>().GetManyQueryable(t => t.Status == PastPaperStatus.Confirmed && !t.IsDeleted && t.CoreBoardId == board.LegacyId);
            if (level?.LegacyId is int grade)
            {
                papers = papers.Where(t => t.CoreGradeId == grade);
            }

            if (subject?.LegacyId is int lesson)
            {
                papers = papers.Where(t => t.CoreSubjectId == lesson);
            }

            var (childKind, parentId, page) = ("levels", board.Id, CurriculumSeoPage.BoardPage);
            if (subject is not null)
            {
                (childKind, parentId, page) = ("topics", subject.Id, CurriculumSeoPage.SyllabusPage);
            }
            else if (level is not null)
            {
                (childKind, parentId, page) = ("subjects", level.Id, CurriculumSeoPage.QualificationPage);
            }

            var children = (await GetChildrenAsync(childKind, parentId)).Data?.Data;
            var schools = await SchoolsQuery(uow, board.Id, level?.Id, null).Take(LandingSchools).ToListAsync();

            Dictionary<string, string?> values = new(StringComparer.Ordinal)
            {
                ["board"] = board.Name,
                ["qualification"] = level?.Name,
                ["subject"] = subject?.Name,
                ["syllabusCode"] = subject?.Code,
                ["yearFrom"] = subject?.YearFrom?.ToString(CultureInfo.InvariantCulture),
                ["yearTo"] = subject?.YearTo?.ToString(CultureInfo.InvariantCulture),
                ["qualifications"] = level is null && children is { Count: > 0 } ? JoinNames([.. children.Take(3).Select(t => t.Name!)]) : null,
                ["city"] = schools.Select(t => t.City).FirstOrDefault(t => t is not null),
            };

            return new CurriculumLandingDto
            {
                Board = new() { Id = board.Id, LegacyId = board.LegacyId, Slug = board.Slug, Name = board.Name, ShortName = board.ShortName, Logo = board.Logo, CountryCode = board.CountryCode, IsMaster = board.IsMaster },
                Level = level is null ? null : new() { Id = level.Id, LegacyId = level.LegacyId, Slug = level.Slug, Name = level.Name, ParentId = board.Id },
                Subject = subject is null ? null : new() { Id = subject.Id, LegacyId = subject.LegacyId, Slug = subject.Slug, Name = subject.Name, Code = subject.Code, ParentId = level!.Id },
                Children = children ?? [],
                PaperCount = await papers.CountAsync(),
                Seo = (await curriculumService.Value.GetSeoAsync(board.Id, page, values)).Data,
                Schools = schools,
            };
        });

        private static string JoinNames(IReadOnlyList<string> names) => names.Count switch
        {
            0 => string.Empty,
            1 => names[0],
            _ => $"{string.Join(", ", names.Take(names.Count - 1))} and {names[^1]}",
        };

        private static IQueryable<CurriculumPublicSchoolDto> SchoolsQuery(IUnitOfWork uow, int boardId, int? qualificationId, int? cityId) =>
            from link in uow.GetRepository<SchoolQualification>().GetManyQueryable(t => t.BoardId == boardId && (qualificationId == null || t.QualificationId == null || t.QualificationId == qualificationId))
            join school in uow.GetRepository<School>().GetManyQueryable(t => !t.IsDeleted && (cityId == null || t.CityId == cityId)) on link.SchoolId equals school.Id
            orderby school.Name
            select new CurriculumPublicSchoolDto { Id = school.Id, Name = school.Name, CityId = school.CityId, City = school.City != null ? school.City.Title : null };

        // A filter's options with their counts, or null when the filter can't show yet (subject before a level...).
        private static async Task<List<CurriculumPublicOptionDto>?> OptionsAsync(IUnitOfWork uow, string key, int boardId, int? levelId, int? syllabusId,
            IQueryable<PastPaper> board, IQueryable<PastPaper> underLevel, IQueryable<PastPaper> underSubject)
        {
            static string Text(long value) => value.ToString(CultureInfo.InvariantCulture);
            switch (key)
            {
                case CurriculumFilterKeys.Level:
                {
                    var counts = await board.GroupBy(t => t.CoreGradeId).Select(t => new { t.Key, Count = t.Count() }).ToDictionaryAsync(t => t.Key, t => t.Count);
                    var levels = await uow.GetRepository<CurriculumQualification, int>().GetManyQueryable(t => t.BoardId == boardId && t.LegacyId != null).OrderBy(t => t.ListOrder).ToListAsync();
                    return [.. levels.Where(t => counts.ContainsKey(t.LegacyId!.Value)).Select(t => new CurriculumPublicOptionDto { Value = Text(t.LegacyId!.Value), Label = t.Name!, Count = counts[t.LegacyId!.Value] })];
                }

                case CurriculumFilterKeys.Subject:
                {
                    if (levelId is null)
                    {
                        return null;
                    }

                    var counts = await underLevel.GroupBy(t => t.CoreSubjectId).Select(t => new { t.Key, Count = t.Count() }).ToDictionaryAsync(t => t.Key, t => t.Count);
                    var subjects = await uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable(t => t.QualificationId == levelId && t.LegacyId != null).OrderBy(t => t.ListOrder).ToListAsync();
                    return [.. subjects.Where(t => counts.ContainsKey(t.LegacyId!.Value)).Select(t => new CurriculumPublicOptionDto
                    {
                        Value = Text(t.LegacyId!.Value),
                        Label = t.Code is null ? t.Name! : $"{t.Name} ({t.Code})",
                        Count = counts[t.LegacyId!.Value],
                    })];
                }

                case CurriculumFilterKeys.Year:
                {
                    var counts = await underSubject.Where(t => t.EduYear > 0).GroupBy(t => t.EduYear!.Value).Select(t => new { t.Key, Count = t.Count() }).ToListAsync();
                    return [.. counts.OrderByDescending(t => t.Key).Select(t => new CurriculumPublicOptionDto { Value = Text(t.Key), Label = Text(t.Key), Count = t.Count })];
                }

                case CurriculumFilterKeys.Series:
                {
                    if (levelId is null)
                    {
                        return null;
                    }

                    var counts = await underSubject.Where(t => t.EduMonth > 0).GroupBy(t => t.EduMonth!.Value).Select(t => new { t.Key, Count = t.Count() }).ToDictionaryAsync(t => t.Key, t => t.Count);
                    var series = await uow.GetRepository<CurriculumSeriesType, int>().GetManyQueryable(t => t.BoardId == boardId && (t.QualificationId == null || t.QualificationId == levelId))
                        .OrderBy(t => t.ListOrder).ToListAsync();
                    List<CurriculumPublicOptionDto> options = [];
                    foreach (var item in series)
                    {
                        foreach (var month in (item.Months ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        {
                            if (byte.TryParse(month, CultureInfo.InvariantCulture, out var m) && counts.TryGetValue(m, out var count))
                            {
                                options.Add(new() { Value = month, Label = item.Name!, Count = count });
                            }
                        }
                    }

                    return options;
                }

                case CurriculumFilterKeys.Paper:
                {
                    if (syllabusId is null)
                    {
                        return null;
                    }

                    var counts = await underSubject.Where(t => t.CorePaperTypeId != null).GroupBy(t => t.CorePaperTypeId!.Value).Select(t => new { t.Key, Count = t.Count() }).ToDictionaryAsync(t => t.Key, t => t.Count);
                    var papers = await uow.GetRepository<CurriculumPaper, int>().GetManyQueryable(t => t.SyllabusId == syllabusId && t.LegacyId != null).OrderBy(t => t.ListOrder).ToListAsync();
                    return [.. papers.Where(t => counts.ContainsKey(t.LegacyId!.Value)).Select(t => new CurriculumPublicOptionDto { Value = Text(t.LegacyId!.Value), Label = t.ShortLabel!, Count = counts[t.LegacyId!.Value] })];
                }

                default:
                    if (!key.StartsWith(CurriculumFilterKeys.Variant + ".", StringComparison.Ordinal))
                    {
                        return null;
                    }

                    var kindKey = key[(CurriculumFilterKeys.Variant.Length + 1)..];
                    var variantCounts = await underSubject.Where(t => t.Variant != null).GroupBy(t => (int)t.Variant!.Value).Select(t => new { t.Key, Count = t.Count() }).ToDictionaryAsync(t => t.Key, t => t.Count);
                    var variantOptions = await (from option in uow.GetRepository<CurriculumVariantOption, int>().GetManyQueryable(t => t.LegacyId != null)
                                                join kind in uow.GetRepository<CurriculumVariantKind, int>().GetManyQueryable(t => t.BoardId == boardId && t.Key == kindKey) on option.VariantKindId equals kind.Id
                                                orderby option.ListOrder
                                                select option).ToListAsync();
                    return [.. variantOptions.Where(t => variantCounts.ContainsKey(t.LegacyId!.Value)).Select(t => new CurriculumPublicOptionDto { Value = Text(t.LegacyId!.Value), Label = t.Label!, Count = variantCounts[t.LegacyId!.Value] })];
            }
        }

        private async Task<ResultData<CurriculumAnswerDto<T>>> AnswerAsync<T>(Func<IUnitOfWork, Task<T?>> read)
            where T : class
        {
            try
            {
                var data = await read(UnitOfWorkProvider.Value.CreateUnitOfWork());
                return data is null
                    ? new(OperationResult.NotFound) { Errors = [new() { Message = "Not found.", Reference = "notFound" }] }
                    : new(OperationResult.Succeeded) { Data = new() { Native = Native, Data = data } };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message }] };
            }
        }
    }
}
