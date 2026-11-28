namespace GamaEdtech.Application.Service
{
    using System.Globalization;
    using System.Linq;

    using GamaEdtech.Application.Interface;
    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.UnitOfWork;
    using GamaEdtech.Common.Service;
    using GamaEdtech.Data.Dto.Curriculum;
    using GamaEdtech.Data.Dto.Taxonomy;
    using GamaEdtech.Domain.Entity;
    using GamaEdtech.Domain.Entity.Curriculum;
    using GamaEdtech.Domain.Enumeration;

    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using static GamaEdtech.Common.Core.Constants;

    public sealed class CurriculumCopyService(Lazy<IUnitOfWorkProvider> unitOfWorkProvider, Lazy<IHttpContextAccessor> httpContextAccessor,
        Lazy<ILogger<CurriculumCopyService>> logger, Lazy<IMigrationRouter> migrationRouter, Lazy<IJobScheduler> jobScheduler)
        : ServiceBase<CurriculumCopyService>(unitOfWorkProvider, httpContextAccessor, logger), ICurriculumCopyService
    {
        internal const string Boards = "board";
        internal const string Qualifications = "qualification";
        internal const string Subjects = "subject";
        internal const string Syllabi = "syllabus";
        internal const string Topics = "topic";
        internal const string Papers = "paper";
        internal const string ResourceTypes = "resourceType";
        internal const string Variants = "variant";
        internal const string DocumentTypes = "documentType";
        internal const string Series = "series";
        internal const string Filters = "filter";
        internal const string Seo = "seo";
        internal const string Links = "link";
        internal const string Schools = "school";

        private readonly record struct Item(int Id, short Kind, string? Title, int ListOrder, string? Parent, string? Metadata, bool IsSeason, bool? IsPaper, bool IsDeleted, string? Picture);

        public async Task<ResultData<CurriculumCopyResultDto>> CopyAsync()
        {
            try
            {
                if (migrationRouter.Value.GetWriteRoute(MigrationDomain.Curriculum) == MigrationRoute.Native)
                {
                    return new(OperationResult.NotValid)
                    {
                        Errors = [new() { Message = "The curriculum is edited here since Migration:WriteRoutes:Curriculum is Native; the copy no longer runs.", Reference = "notNative" }],
                    };
                }

                Run run = new(UnitOfWorkProvider.Value.CreateUnitOfWork(), DateTimeOffset.UtcNow);
                await run.CopyAsync();
                return new(OperationResult.Succeeded) { Data = run.Result() };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message }] };
            }
        }

        public Task<ResultData<bool>> StartCopyAsync()
        {
            if (migrationRouter.Value.GetWriteRoute(MigrationDomain.Curriculum) == MigrationRoute.Native)
            {
                return Task.FromResult(new ResultData<bool>(OperationResult.NotValid)
                {
                    Errors = [new() { Message = "The copy no longer runs: Migration:WriteRoutes:Curriculum is Native.", Reference = "notNative" }],
                });
            }

            jobScheduler.Value.Schedule<ICurriculumCopyService>(t => t.CopyAsync(), TimeSpan.Zero);
            return Task.FromResult(new ResultData<bool>(OperationResult.Succeeded) { Data = true });
        }

        public async Task<ResultData<IReadOnlyList<CurriculumCopyIssueDto>>> GetIssuesAsync(string? kind)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var query = uow.GetRepository<CurriculumCopyIssue>().GetManyQueryable();
                if (!string.IsNullOrEmpty(kind))
                {
                    query = query.Where(t => t.Kind == kind);
                }

                return new(OperationResult.Succeeded)
                {
                    Data = await query.OrderBy(t => t.Kind).ThenBy(t => t.SourceId).Take(2000).Select(t => new CurriculumCopyIssueDto
                    {
                        Id = t.Id,
                        Kind = t.Kind,
                        SourceId = t.SourceId,
                        Message = t.Message,
                        CreationDate = t.CreationDate,
                    }).ToListAsync(),
                };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message }] };
            }
        }

        public async Task<ResultData<CurriculumCheckDto>> CheckAsync()
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var items = await uow.GetRepository<LegacyTaxonomyItem, int>().GetManyQueryable(t => !t.IsDeleted)
                    .Select(t => new { t.Id, t.Kind, t.IsPaper }).ToListAsync();
                HashSet<int> boards = [.. await uow.GetRepository<CurriculumBoard, int>().GetManyQueryable(t => t.LegacyId != null).Select(t => t.LegacyId!.Value).ToListAsync()];
                HashSet<int> qualifications = [.. await uow.GetRepository<CurriculumQualification, int>().GetManyQueryable(t => t.LegacyId != null).Select(t => t.LegacyId!.Value).ToListAsync()];
                HashSet<int> syllabi = [.. await uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable(t => t.LegacyId != null).Select(t => t.LegacyId!.Value).ToListAsync()];
                HashSet<int> topics = [.. await uow.GetRepository<CurriculumTopic, int>().GetManyQueryable(t => t.LegacyId != null).Select(t => t.LegacyId!.Value).ToListAsync()];
                HashSet<int> documents = [.. await uow.GetRepository<CurriculumDocumentType, int>().GetManyQueryable(t => t.LegacyId != null).Select(t => t.LegacyId!.Value).ToListAsync()];
                HashSet<int> resources = [.. await uow.GetRepository<CurriculumResourceType, int>().GetManyQueryable(t => t.LegacyId != null).Select(t => t.LegacyId!.Value).ToListAsync()];
                var paperRows = await uow.GetRepository<CurriculumPaper, int>().GetManyQueryable(t => t.LegacyId != null).Select(t => new { t.SyllabusId, t.LegacyId }).ToListAsync();
                var syllabusLegacy = await uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable(t => t.LegacyId != null).Select(t => new { t.Id, t.LegacyId }).ToDictionaryAsync(t => t.Id, t => t.LegacyId!.Value);
                HashSet<(int, int)> papers = [.. paperRows.Where(t => syllabusLegacy.ContainsKey(t.SyllabusId)).Select(t => (syllabusLegacy[t.SyllabusId], t.LegacyId!.Value))];
                var variantRows = await (from option in uow.GetRepository<CurriculumVariantOption, int>().GetManyQueryable(t => t.LegacyId != null)
                                         join kind in uow.GetRepository<CurriculumVariantKind, int>().GetManyQueryable() on option.VariantKindId equals kind.Id
                                         join board in uow.GetRepository<CurriculumBoard, int>().GetManyQueryable(t => t.LegacyId != null) on kind.BoardId equals board.Id
                                         select new { Board = board.LegacyId!.Value, Variant = option.LegacyId!.Value }).ToListAsync();
                HashSet<(int, int)> variants = [.. variantRows.Select(t => (t.Board, t.Variant))];
                HashSet<int> noVariant = [.. await uow.GetRepository<LegacyTaxonomyItem, int>().GetManyQueryable(t => t.Kind == LegacyTaxonomyKinds.Variant && t.Title == CurriculumCopyRules.NoVariant).Select(t => t.Id).ToListAsync()];

                var relationIds = await uow.GetRepository<LegacyTaxonomyRelation, int>().GetManyQueryable().Select(t => t.Id).ToListAsync();
                HashSet<int> copiedRelations = [.. await uow.GetRepository<CurriculumTopicLink, int>().GetManyQueryable(t => t.LegacyRelationId != null).Select(t => t.LegacyRelationId!.Value).ToListAsync()];
                var schoolBoards = await uow.GetRepository<SchoolBoard>().GetManyQueryable().Select(t => t.Id).ToListAsync();
                HashSet<long> copiedSchools = [.. await uow.GetRepository<SchoolQualification>().GetManyQueryable(t => t.LegacySchoolBoardId != null).Select(t => t.LegacySchoolBoardId!.Value).ToListAsync()];

                CurriculumCheckRowDto Row(string kind, IEnumerable<int> source, ISet<int> copied)
                {
                    var list = source.ToList();
                    var done = list.Count(copied.Contains);
                    return new() { Kind = kind, Source = list.Count, Copied = done, Missing = list.Count - done };
                }

                List<CurriculumCheckRowDto> lists =
                [
                    Row(Boards, items.Where(t => t.Kind == LegacyTaxonomyKinds.Board).Select(t => t.Id), boards),
                    Row(Qualifications, items.Where(t => t.Kind == LegacyTaxonomyKinds.Grade).Select(t => t.Id), qualifications),
                    Row(Syllabi, items.Where(t => t.Kind == LegacyTaxonomyKinds.Subject).Select(t => t.Id), syllabi),
                    Row(Topics, items.Where(t => t.Kind == LegacyTaxonomyKinds.Topic).Select(t => t.Id), topics),
                    Row(DocumentTypes, items.Where(t => t.Kind == LegacyTaxonomyKinds.ExtraFileType).Select(t => t.Id), documents),
                    Row(ResourceTypes, items.Where(t => t.Kind == LegacyTaxonomyKinds.Classification && t.IsPaper != true).Select(t => t.Id), resources),
                    Row(Links, relationIds, copiedRelations),
                    new() { Kind = Schools, Source = schoolBoards.Count, Copied = schoolBoards.Count(copiedSchools.Contains), Missing = schoolBoards.Count(t => !copiedSchools.Contains(t)) },
                ];

                // Content: grouped by the ids it points at, so each combination is checked once.
                var paperGroups = await uow.GetRepository<PastPaper>().GetManyQueryable(t => !t.IsDeleted)
                    .GroupBy(t => new { t.CoreBoardId, t.CoreGradeId, t.CoreSubjectId, t.CorePaperTypeId, t.Variant, t.CoreTopicIds })
                    .Select(t => new { t.Key, Count = t.Count() }).ToListAsync();
                var tutorialGroups = await uow.GetRepository<Tutorial>().GetManyQueryable()
                    .GroupBy(t => new { t.CoreBoardId, t.CoreGradeId, t.CoreSubjectId, t.CoreTopicIds })
                    .Select(t => new { t.Key, Count = t.Count() }).ToListAsync();
                var examGroups = await uow.GetRepository<Exam>().GetManyQueryable(t => !t.IsDeleted)
                    .GroupBy(t => new { t.CoreBoardId, t.CoreGradeId, t.CoreSubjectId, t.CoreTopicIds })
                    .Select(t => new { t.Key, Count = t.Count() }).ToListAsync();

                bool Missing(int? id, ISet<int> copied) => id is > 0 && !copied.Contains(id.Value);
                bool TopicsMissing(string? ids) => CurriculumCopyRules.Parents(ids?.Replace(',', '+'), false).Any(t => !topics.Contains(t));

                CurriculumCoverageDto Coverage<T>(string content, IEnumerable<T> groups, Func<T, int> count, Func<T, IEnumerable<string>> failures)
                {
                    var total = 0;
                    var placed = 0;
                    Dictionary<string, int> unplaced = new(StringComparer.Ordinal);
                    foreach (var group in groups)
                    {
                        var n = count(group);
                        total += n;
                        var failed = failures(group).ToList();
                        if (failed.Count == 0)
                        {
                            placed += n;
                        }

                        foreach (var part in failed)
                        {
                            unplaced[part] = unplaced.GetValueOrDefault(part) + n;
                        }
                    }

                    return new() { Content = content, Total = total, Placed = placed, Unplaced = unplaced };
                }

                IEnumerable<string> Common(int? board, int? grade, int? subject, string? topicIds)
                {
                    if (Missing(board, boards))
                    {
                        yield return Boards;
                    }

                    if (Missing(grade, qualifications))
                    {
                        yield return Qualifications;
                    }

                    if (Missing(subject, syllabi))
                    {
                        yield return Syllabi;
                    }

                    if (TopicsMissing(topicIds))
                    {
                        yield return Topics;
                    }
                }

                List<CurriculumCoverageDto> content =
                [
                    Coverage("pastPapers", paperGroups, t => t.Count, t =>
                    {
                        var parts = Common(t.Key.CoreBoardId, t.Key.CoreGradeId, t.Key.CoreSubjectId, t.Key.CoreTopicIds).ToList();
                        if (t.Key.CorePaperTypeId is int type && !papers.Contains((t.Key.CoreSubjectId, type)) && !resources.Contains(type))
                        {
                            parts.Add(Papers);
                        }

                        if (t.Key.Variant is short variant && variant > 0 && !noVariant.Contains(variant) && !variants.Contains((t.Key.CoreBoardId, variant)))
                        {
                            parts.Add(Variants);
                        }

                        return parts;
                    }),
                    Coverage("tutorials", tutorialGroups, t => t.Count, t => Common(t.Key.CoreBoardId, t.Key.CoreGradeId, t.Key.CoreSubjectId, t.Key.CoreTopicIds)),
                    Coverage("exams", examGroups, t => t.Count, t => Common(t.Key.CoreBoardId, t.Key.CoreGradeId, t.Key.CoreSubjectId, t.Key.CoreTopicIds)),
                ];

                return new(OperationResult.Succeeded)
                {
                    Data = new() { Lists = lists, Content = content, Issues = await uow.GetRepository<CurriculumCopyIssue>().CountAsync() },
                };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message }] };
            }
        }

        /// <summary>One run of the copy: the source read once, each list upserted in order, counts and issues collected.</summary>
        private sealed class Run(IUnitOfWork uow, DateTimeOffset now)
        {
            private readonly Dictionary<string, CurriculumCopyCountDto> counts = new(StringComparer.Ordinal);
            private readonly List<CurriculumCopyIssue> issues = [];
            private List<Item> items = [];
            private Dictionary<int, Item> byId = [];

            // Old id -> new id of what is copied so far.
            private Dictionary<int, int> boards = [];
            private Dictionary<int, int> qualifications = [];
            private Dictionary<int, int> syllabi = [];
            private Dictionary<int, int> topics = [];
            private int? masterBoardId;

            public async Task CopyAsync()
            {
                items = await uow.GetRepository<LegacyTaxonomyItem, int>().GetManyQueryable()
                    .Select(t => new Item(t.Id, t.Kind, t.Title, t.ListOrder, t.Parent, t.Metadata, t.IsSeason == true, t.IsPaper, t.IsDeleted, t.Picture)).ToListAsync();
                byId = items.ToDictionary(t => t.Id);

                await CopyBoardsAsync();
                await CopyQualificationsAsync();
                var subjects = await CopySubjectsAsync();
                await CopySyllabiAsync(subjects);
                await CopyTopicsAsync();
                await CopyClassificationsAsync();
                await CopyVariantsAsync();
                await CopyDocumentTypesAsync();
                await SeedSeriesAsync();
                await SeedFiltersAsync();
                await SeedSeoAsync();
                await CopyLinksAsync();
                await CopySchoolsAsync();
                await SaveIssuesAsync();
            }

            public CurriculumCopyResultDto Result() => new() { Counts = [.. counts.Values], Issues = issues.Count, FinishedAt = DateTimeOffset.UtcNow };

            private CurriculumCopyCountDto Count(string kind)
            {
                if (!counts.TryGetValue(kind, out var count))
                {
                    counts[kind] = count = new() { Kind = kind };
                }

                return count;
            }

            private void Issue(string kind, long? sourceId, string message) =>
                issues.Add(new() { Kind = kind, SourceId = sourceId, Message = message.Length > 500 ? message[..500] : message, CreationDate = now });

            private static string Name(string? title, int max, string fallback)
            {
                var name = string.IsNullOrWhiteSpace(title) ? fallback : title.Trim();
                return name.Length > max ? name[..max] : name;
            }

            private IEnumerable<Item> Of(short kind) => items.Where(t => t.Kind == kind).OrderBy(t => t.ListOrder).ThenBy(t => t.Id);

            private async Task CopyBoardsAsync()
            {
                var repository = uow.GetRepository<CurriculumBoard, int>();
                var rows = await repository.GetManyQueryable(tracking: true).ToListAsync();
                HashSet<string> slugs = [.. rows.Select(t => t.Slug!)];
                var hasMaster = rows.Exists(t => t.IsMaster);
                var hasDefault = rows.Exists(t => t.IsDefault);
                foreach (var item in Of(LegacyTaxonomyKinds.Board))
                {
                    var name = Name(item.Title, 200, $"Board {item.Id}");
                    var row = rows.Find(t => t.LegacyId == item.Id);
                    if (row is null)
                    {
                        if (item.IsDeleted)
                        {
                            continue;
                        }

                        row = new()
                        {
                            LegacyId = item.Id,
                            Slug = CurriculumCopyRules.UniqueSlug(CurriculumCopyRules.Slug(name, $"board-{item.Id}"), slugs),
                            Name = name,
                            ShortName = CurriculumCopyRules.ShortNames.GetValueOrDefault(item.Id),
                            IsEnabled = true,
                            IsMaster = item.Id == CurriculumCopyRules.GamaBoardId && !hasMaster,
                            IsDefault = item.Id == CurriculumCopyRules.CambridgeBoardId && !hasDefault,
                            CodeTemplate = item.Id == CurriculumCopyRules.CambridgeBoardId ? CurriculumSeoDefaults.CambridgeCode : null,
                            ListOrder = item.ListOrder,
                            CreationDate = now,
                        };
                        repository.Add(row);
                        rows.Add(row);
                        Count(Boards).Added++;
                    }
                    else if (row.Name != name || row.ListOrder != item.ListOrder)
                    {
                        (row.Name, row.ListOrder, row.LastModifyDate) = (name, item.ListOrder, now);
                        Count(Boards).Updated++;
                    }
                }

                _ = await uow.SaveChangesAsync();
                boards = rows.Where(t => t.LegacyId != null).ToDictionary(t => t.LegacyId!.Value, t => t.Id);
                masterBoardId = rows.Find(t => t.IsMaster)?.Id;
            }

            private async Task CopyQualificationsAsync()
            {
                var repository = uow.GetRepository<CurriculumQualification, int>();
                var rows = await repository.GetManyQueryable(tracking: true).ToListAsync();
                var slugs = rows.GroupBy(t => t.BoardId).ToDictionary(t => t.Key, t => t.Select(r => r.Slug!).ToHashSet());
                foreach (var item in Of(LegacyTaxonomyKinds.Grade))
                {
                    var parents = CurriculumCopyRules.Parents(item.Parent, false).Where(boards.ContainsKey).ToList();
                    if (parents.Count == 0)
                    {
                        if (!item.IsDeleted)
                        {
                            Issue(Qualifications, item.Id, $"\"{item.Title?.Trim()}\" is under no copied board (parent \"{item.Parent}\").");
                        }

                        continue;
                    }

                    if (parents.Count > 1)
                    {
                        Issue(Qualifications, item.Id, $"\"{item.Title?.Trim()}\" is under several boards ({string.Join(", ", parents)}); placed under {parents[0]}.");
                    }

                    var boardId = boards[parents[0]];
                    var name = Name(item.Title, 200, $"Level {item.Id}");
                    var row = rows.Find(t => t.LegacyId == item.Id);
                    if (row is null)
                    {
                        if (item.IsDeleted)
                        {
                            continue;
                        }

                        if (!slugs.TryGetValue(boardId, out var taken))
                        {
                            slugs[boardId] = taken = [];
                        }

                        row = new()
                        {
                            LegacyId = item.Id,
                            BoardId = boardId,
                            Slug = CurriculumCopyRules.UniqueSlug(CurriculumCopyRules.Slug(name, $"level-{item.Id}"), taken),
                            Name = name,
                            ListOrder = item.ListOrder,
                            CreationDate = now,
                        };
                        repository.Add(row);
                        rows.Add(row);
                        Count(Qualifications).Added++;
                    }
                    else if (row.Name != name || row.ListOrder != item.ListOrder || row.BoardId != boardId)
                    {
                        (row.Name, row.ListOrder, row.BoardId, row.LastModifyDate) = (name, item.ListOrder, boardId, now);
                        Count(Qualifications).Updated++;
                    }
                }

                _ = await uow.SaveChangesAsync();
                qualifications = rows.Where(t => t.LegacyId != null).ToDictionary(t => t.LegacyId!.Value, t => t.Id);
            }

            // The subjects across boards: the named groups of the correspondence list, by name.
            private async Task<Dictionary<string, int>> CopySubjectsAsync()
            {
                var repository = uow.GetRepository<CurriculumSubject, int>();
                var rows = await repository.GetManyQueryable(tracking: true).ToListAsync();
                var order = 0;
                foreach (var (name, _) in GamaSubjects.Groups)
                {
                    var slug = CurriculumCopyRules.Slug(name, "subject");
                    if (!rows.Exists(t => t.Slug == slug))
                    {
                        CurriculumSubject row = new() { Slug = slug, Name = name, ListOrder = order };
                        repository.Add(row);
                        rows.Add(row);
                        Count(Subjects).Added++;
                    }

                    order++;
                }

                _ = await uow.SaveChangesAsync();
                return GamaSubjects.Groups.ToDictionary(t => t.Name, t => rows.Find(r => r.Slug == CurriculumCopyRules.Slug(t.Name, "subject"))!.Id);
            }

            private async Task CopySyllabiAsync(Dictionary<string, int> subjects)
            {
                var repository = uow.GetRepository<CurriculumSyllabus, int>();
                var rows = await repository.GetManyQueryable(tracking: true).ToListAsync();
                var slugs = rows.GroupBy(t => t.QualificationId).ToDictionary(t => t.Key, t => t.Select(r => r.Slug!).ToHashSet());
                foreach (var item in Of(LegacyTaxonomyKinds.Subject))
                {
                    var parents = CurriculumCopyRules.Parents(item.Parent, true).Where(qualifications.ContainsKey).ToList();
                    if (parents.Count == 0)
                    {
                        if (!item.IsDeleted)
                        {
                            Issue(Syllabi, item.Id, $"\"{item.Title?.Trim()}\" is under no copied level (parent \"{item.Parent}\").");
                        }

                        continue;
                    }

                    if (parents.Count > 1)
                    {
                        Issue(Syllabi, item.Id, $"\"{item.Title?.Trim()}\" is under several levels ({string.Join(", ", parents)}); placed under {parents[0]}.");
                    }

                    var qualificationId = qualifications[parents[0]];
                    var (title, code) = CurriculumCopyRules.Syllabus(item.Title, item.Metadata);
                    int? subjectId = GamaSubjects.GroupOf(item.Id) is { } group ? subjects[group] : null;
                    var name = Name(title, 200, $"Subject {item.Id}");
                    var row = rows.Find(t => t.LegacyId == item.Id);
                    if (row is null)
                    {
                        if (item.IsDeleted)
                        {
                            continue;
                        }

                        if (!slugs.TryGetValue(qualificationId, out var taken))
                        {
                            slugs[qualificationId] = taken = [];
                        }

                        row = new()
                        {
                            LegacyId = item.Id,
                            QualificationId = qualificationId,
                            Slug = CurriculumCopyRules.UniqueSlug(CurriculumCopyRules.Slug(code is null ? name : $"{code} {name}", $"subject-{item.Id}"), taken),
                            CreationDate = now,
                        };
                        repository.Add(row);
                        rows.Add(row);
                        Count(Syllabi).Added++;
                    }
                    else if (row.Name == name && row.Code == code && row.ListOrder == item.ListOrder && row.QualificationId == qualificationId && row.Picture == item.Picture
                        && (row.SubjectId is not null || subjectId is null))
                    {
                        continue;
                    }
                    else
                    {
                        row.LastModifyDate = now;
                        Count(Syllabi).Updated++;
                    }

                    row.QualificationId = qualificationId;
                    row.Name = name;
                    row.Code = code;
                    row.ListOrder = item.ListOrder;
                    row.Picture = item.Picture;
                    row.SubjectId ??= subjectId;
                }

                _ = await uow.SaveChangesAsync();
                syllabi = rows.Where(t => t.LegacyId != null).ToDictionary(t => t.LegacyId!.Value, t => t.Id);
            }

            private async Task CopyTopicsAsync()
            {
                var repository = uow.GetRepository<CurriculumTopic, int>();
                var rows = (await repository.GetManyQueryable(t => t.LegacyId != null, tracking: true).ToListAsync()).ToDictionary(t => t.LegacyId!.Value);
                Dictionary<int, int> syllabusOf = [];
                foreach (var item in Of(LegacyTaxonomyKinds.Topic))
                {
                    var parent = CurriculumCopyRules.Parents(item.Parent, false).FirstOrDefault(syllabi.ContainsKey);
                    if (parent == 0)
                    {
                        if (!item.IsDeleted)
                        {
                            Issue(Topics, item.Id, $"\"{item.Title?.Trim()}\" is under no copied subject (parent \"{item.Parent}\").");
                        }

                        continue;
                    }

                    syllabusOf[item.Id] = parent;
                    var name = Name(item.Title, 500, $"Topic {item.Id}");
                    if (!rows.TryGetValue(item.Id, out var row))
                    {
                        if (item.IsDeleted)
                        {
                            continue;
                        }

                        row = new() { LegacyId = item.Id, CreationDate = now };
                        repository.Add(row);
                        rows[item.Id] = row;
                        Count(Topics).Added++;
                    }
                    else if (row.Name != name || row.ListOrder != item.ListOrder || row.IsUnit != item.IsSeason || row.SyllabusId != syllabi[parent])
                    {
                        row.LastModifyDate = now;
                        Count(Topics).Updated++;
                    }

                    (row.SyllabusId, row.Name, row.ListOrder, row.IsUnit) = (syllabi[parent], name, item.ListOrder, item.IsSeason);
                }

                _ = await uow.SaveChangesAsync();

                // Units and chapters, rebuilt from each subject's list order.
                foreach (var subject in syllabusOf.GroupBy(t => t.Value))
                {
                    var units = CurriculumCopyRules.TopicUnits(subject.Select(t => new CurriculumCopyTopic(t.Key, byId[t.Key].ListOrder, byId[t.Key].IsSeason)));
                    foreach (var (id, unit) in units)
                    {
                        if (rows.TryGetValue(id, out var row))
                        {
                            row.ParentTopicId = unit is int u && rows.TryGetValue(u, out var unitRow) ? unitRow.Id : null;
                        }
                    }
                }

                _ = await uow.SaveChangesAsync();
                topics = rows.ToDictionary(t => t.Key, t => t.Value.Id);
            }

            // Classifications: papers become each syllabus's papers where real papers use them; the rest, resource types.
            private async Task CopyClassificationsAsync()
            {
                var resourceRepository = uow.GetRepository<CurriculumResourceType, int>();
                var resourceRows = await resourceRepository.GetManyQueryable(tracking: true).ToListAsync();
                HashSet<string> slugs = [.. resourceRows.Select(t => t.Slug!)];
                foreach (var item in Of(LegacyTaxonomyKinds.Classification).Where(t => t.IsPaper != true))
                {
                    var name = Name(item.Title, 200, $"Resource {item.Id}");
                    var row = resourceRows.Find(t => t.LegacyId == item.Id);
                    if (row is null)
                    {
                        if (item.IsDeleted)
                        {
                            continue;
                        }

                        row = new() { LegacyId = item.Id, Slug = CurriculumCopyRules.UniqueSlug(CurriculumCopyRules.Slug(name, $"resource-{item.Id}"), slugs) };
                        resourceRepository.Add(row);
                        resourceRows.Add(row);
                        Count(ResourceTypes).Added++;
                    }
                    else if (row.Name == name && row.ListOrder == item.ListOrder)
                    {
                        continue;
                    }
                    else
                    {
                        Count(ResourceTypes).Updated++;
                    }

                    (row.Name, row.ListOrder) = (name, item.ListOrder);
                }

                var used = await uow.GetRepository<PastPaper>().GetManyQueryable(t => t.CorePaperTypeId != null)
                    .Select(t => new { t.CoreSubjectId, Type = t.CorePaperTypeId!.Value }).Distinct().ToListAsync();
                var paperRepository = uow.GetRepository<CurriculumPaper, int>();
                var paperRows = await paperRepository.GetManyQueryable(t => t.LegacyId != null, tracking: true).ToListAsync();
                foreach (var pair in used.OrderBy(t => t.CoreSubjectId).ThenBy(t => t.Type))
                {
                    if (!byId.TryGetValue(pair.Type, out var type) || type.Kind != LegacyTaxonomyKinds.Classification)
                    {
                        Issue(Papers, pair.Type, $"Papers of subject {pair.CoreSubjectId} have the classification {pair.Type}, which is not a copied classification.");
                        continue;
                    }

                    if (type.IsPaper != true || !syllabi.TryGetValue(pair.CoreSubjectId, out var syllabusId))
                    {
                        continue;
                    }

                    var name = Name(type.Title, 100, $"Paper {type.Id}");
                    var label = CurriculumCopyRules.PaperShortLabel(name);
                    var row = paperRows.Find(t => t.SyllabusId == syllabusId && t.LegacyId == type.Id);
                    if (row is null)
                    {
                        row = new() { LegacyId = type.Id, SyllabusId = syllabusId };
                        paperRepository.Add(row);
                        paperRows.Add(row);
                        Count(Papers).Added++;
                    }
                    else if (row.Name == name && row.ShortLabel == label && row.ListOrder == type.ListOrder)
                    {
                        continue;
                    }
                    else
                    {
                        Count(Papers).Updated++;
                    }

                    (row.Name, row.ShortLabel, row.ListOrder) = (name, label, type.ListOrder);
                }

                _ = await uow.SaveChangesAsync();
            }

            // Variants: each board gets a "variant" kind with the values its papers use ("nv" is no variant).
            private async Task CopyVariantsAsync()
            {
                var used = await uow.GetRepository<PastPaper>().GetManyQueryable(t => t.Variant != null && t.Variant > 0)
                    .Select(t => new { t.CoreBoardId, Variant = (int)t.Variant!.Value }).Distinct().ToListAsync();
                var kindRepository = uow.GetRepository<CurriculumVariantKind, int>();
                var optionRepository = uow.GetRepository<CurriculumVariantOption, int>();
                var kinds = await kindRepository.GetManyQueryable(t => t.Key == CurriculumCopyRules.VariantKindKey, tracking: true).ToListAsync();
                List<int> kindIds = [.. kinds.Select(t => t.Id)];
                var options = await optionRepository.GetManyQueryable(t => kindIds.Contains(t.VariantKindId), tracking: true).ToListAsync();

                foreach (var board in used.GroupBy(t => t.CoreBoardId))
                {
                    if (!boards.TryGetValue(board.Key, out var boardId))
                    {
                        continue;
                    }

                    var values = board.Select(t => t.Variant).Where(t => !(byId.TryGetValue(t, out var item) && item.Title?.Trim() == CurriculumCopyRules.NoVariant)).ToList();
                    foreach (var unknown in values.Where(t => !byId.TryGetValue(t, out var item) || item.Kind != LegacyTaxonomyKinds.Variant))
                    {
                        Issue(Variants, unknown, $"Papers of board {board.Key} have the variant {unknown}, which is not a copied variant.");
                    }

                    var known = values.Where(t => byId.TryGetValue(t, out var item) && item.Kind == LegacyTaxonomyKinds.Variant).Select(t => byId[t]).OrderBy(t => t.ListOrder).ToList();
                    if (known.Count == 0)
                    {
                        continue;
                    }

                    var kind = kinds.Find(t => t.BoardId == boardId);
                    if (kind is null)
                    {
                        kind = new() { BoardId = boardId, Key = CurriculumCopyRules.VariantKindKey, Label = "Variant" };
                        kindRepository.Add(kind);
                        kinds.Add(kind);
                        _ = await uow.SaveChangesAsync();
                    }

                    foreach (var item in known)
                    {
                        var label = Name(item.Title, 50, item.Id.ToString(CultureInfo.InvariantCulture));
                        var row = options.Find(t => t.VariantKindId == kind.Id && t.LegacyId == item.Id);
                        if (row is null)
                        {
                            row = new() { VariantKindId = kind.Id, LegacyId = item.Id };
                            optionRepository.Add(row);
                            options.Add(row);
                            Count(Variants).Added++;
                        }
                        else if (row.Label == label && row.ListOrder == item.ListOrder)
                        {
                            continue;
                        }
                        else
                        {
                            Count(Variants).Updated++;
                        }

                        (row.Label, row.Code, row.ListOrder) = (label, label.Length > 20 ? label[..20] : label, item.ListOrder);
                    }
                }

                _ = await uow.SaveChangesAsync();
            }

            private async Task CopyDocumentTypesAsync()
            {
                var repository = uow.GetRepository<CurriculumDocumentType, int>();
                var rows = await repository.GetManyQueryable(tracking: true).ToListAsync();
                HashSet<string> codes = [.. rows.Select(t => t.Code!)];

                // The question paper and mark scheme are columns on gama-api, not extra file types.
                foreach (var (code, name, label, order) in new[] { ("qp", "Question paper", "QP", -2), ("ms", "Mark scheme", "MS", -1) })
                {
                    if (codes.Add(code))
                    {
                        CurriculumDocumentType row = new() { Code = code, Name = name, ShortLabel = label, ListOrder = order };
                        repository.Add(row);
                        rows.Add(row);
                        Count(DocumentTypes).Added++;
                    }
                }

                foreach (var item in Of(LegacyTaxonomyKinds.ExtraFileType))
                {
                    var name = Name(item.Title, 100, $"File {item.Id}");
                    var row = rows.Find(t => t.LegacyId == item.Id);
                    if (row is null)
                    {
                        if (item.IsDeleted)
                        {
                            continue;
                        }

                        var (code, label) = CurriculumCopyRules.DocumentCode(name);
                        var unique = code;
                        var i = 2;
                        while (!codes.Add(unique))
                        {
                            unique = $"{(code.Length > 17 ? code[..17] : code)}_{i++}";
                        }

                        row = new() { LegacyId = item.Id, Code = unique, ShortLabel = label };
                        repository.Add(row);
                        rows.Add(row);
                        Count(DocumentTypes).Added++;
                    }
                    else if (row.Name == name && row.ListOrder == item.ListOrder)
                    {
                        continue;
                    }
                    else
                    {
                        Count(DocumentTypes).Updated++;
                    }

                    (row.Name, row.ListOrder) = (name, item.ListOrder);
                }

                _ = await uow.SaveChangesAsync();
            }

            // Profile, set once: a level's sessions (the website's list, else the months its papers were sat in).
            private async Task SeedSeriesAsync()
            {
                var repository = uow.GetRepository<CurriculumSeriesType, int>();
                HashSet<int?> done = [.. await repository.GetManyQueryable().Select(t => t.QualificationId).Distinct().ToListAsync()];
                var months = await uow.GetRepository<PastPaper>().GetManyQueryable(t => t.EduMonth != null)
                    .Select(t => new { t.CoreGradeId, Month = t.EduMonth!.Value }).Distinct().ToListAsync();
                var levels = await uow.GetRepository<CurriculumQualification, int>().GetManyQueryable(t => t.LegacyId != null).Select(t => new { t.Id, t.BoardId, t.LegacyId }).ToListAsync();
                var cambridge = boards.GetValueOrDefault(CurriculumCopyRules.CambridgeBoardId);
                foreach (var level in levels.Where(t => !done.Contains(t.Id)))
                {
                    var sessions = CurriculumCopyRules.Sessions(level.LegacyId!.Value, level.BoardId == cambridge,
                        months.Where(t => t.CoreGradeId == level.LegacyId).Select(t => t.Month));
                    for (var i = 0; i < sessions.Count; i++)
                    {
                        var session = sessions[i];
                        repository.Add(new()
                        {
                            BoardId = level.BoardId,
                            QualificationId = level.Id,
                            Name = session.Name,
                            Code = session.Code,
                            FileCode = session.FileCode,
                            SeoName = session.SeoName,
                            Months = session.Month.ToString(CultureInfo.InvariantCulture),
                            ListOrder = i,
                        });
                        Count(Series).Added++;
                    }
                }

                _ = await uow.SaveChangesAsync();
            }

            // Profile, set once: the filters the website's search panel showed (Board is the board itself).
            private async Task SeedFiltersAsync()
            {
                var repository = uow.GetRepository<CurriculumBoardFilter, int>();
                HashSet<int> done = [.. await repository.GetManyQueryable().Select(t => t.BoardId).Distinct().ToListAsync()];
                HashSet<int> withVariants = [.. await uow.GetRepository<CurriculumVariantKind, int>().GetManyQueryable(t => t.Key == CurriculumCopyRules.VariantKindKey).Select(t => t.BoardId).ToListAsync()];
                foreach (var boardId in boards.Values.Where(t => !done.Contains(t)))
                {
                    List<(string Key, string Label, CurriculumFilterStyle Style)> filters =
                    [
                        (CurriculumFilterKeys.Level, "Level", CurriculumFilterStyle.Select),
                        (CurriculumFilterKeys.Subject, "Subject", CurriculumFilterStyle.Select),
                        (CurriculumFilterKeys.Year, "Year", CurriculumFilterStyle.Select),
                        (CurriculumFilterKeys.Series, "Session", CurriculumFilterStyle.Chips),
                        (CurriculumFilterKeys.Paper, "Paper", CurriculumFilterStyle.Chips),
                    ];
                    if (withVariants.Contains(boardId))
                    {
                        filters.Add(($"{CurriculumFilterKeys.Variant}.{CurriculumCopyRules.VariantKindKey}", "Variant", CurriculumFilterStyle.Chips));
                    }

                    for (var i = 0; i < filters.Count; i++)
                    {
                        repository.Add(new() { BoardId = boardId, Key = filters[i].Key, Label = filters[i].Label, Style = filters[i].Style, ListOrder = i, IsEnabled = true });
                        Count(Filters).Added++;
                    }
                }

                _ = await uow.SaveChangesAsync();
            }

            // Profile, set once: Cambridge's paper texts and GAMA's tutorial title, which the website wrote for them alone.
            private async Task SeedSeoAsync()
            {
                var repository = uow.GetRepository<CurriculumSeoTemplate, int>();
                foreach (var (legacyId, text) in new[] { (CurriculumCopyRules.CambridgeBoardId, CurriculumSeoDefaults.CambridgePastPaper), (CurriculumCopyRules.GamaBoardId, CurriculumSeoDefaults.GamaTutorial) })
                {
                    if (boards.TryGetValue(legacyId, out var boardId) && !await repository.AnyAsync(t => t.BoardId == boardId && t.Page == text.Page))
                    {
                        repository.Add(new() { BoardId = boardId, Page = text.Page, TitleTemplate = text.Title, DescriptionTemplate = text.Description, LastModifyDate = now });
                        Count(Seo).Added++;
                    }
                }

                _ = await uow.SaveChangesAsync();
            }

            // The links between topics: a GAMA topic and another board's become an exact link; anything else is reviewed.
            private async Task CopyLinksAsync()
            {
                var relations = await uow.GetRepository<LegacyTaxonomyRelation, int>().GetManyQueryable().Select(t => new { t.Id, t.FromId, t.ToId }).ToListAsync();
                var repository = uow.GetRepository<CurriculumTopicLink, int>();
                var rows = await repository.GetManyQueryable(tracking: true).ToListAsync();
                var boardOfTopic = await (from topic in uow.GetRepository<CurriculumTopic, int>().GetManyQueryable(t => t.LegacyId != null)
                                          join syllabus in uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable() on topic.SyllabusId equals syllabus.Id
                                          join qualification in uow.GetRepository<CurriculumQualification, int>().GetManyQueryable() on syllabus.QualificationId equals qualification.Id
                                          select new { topic.Id, qualification.BoardId }).ToDictionaryAsync(t => t.Id, t => t.BoardId);

                HashSet<int> seen = [];
                foreach (var relation in relations)
                {
                    _ = seen.Add(relation.Id);
                    if (!topics.TryGetValue(relation.FromId, out var from) || !topics.TryGetValue(relation.ToId, out var to))
                    {
                        Issue(Links, relation.Id, $"Link {relation.FromId} - {relation.ToId} is not between two copied topics.");
                        continue;
                    }

                    var fromMaster = boardOfTopic.GetValueOrDefault(from) == masterBoardId;
                    var toMaster = boardOfTopic.GetValueOrDefault(to) == masterBoardId;
                    if (fromMaster == toMaster)
                    {
                        Issue(Links, relation.Id, fromMaster
                            ? $"Link {relation.FromId} - {relation.ToId} joins two GAMA topics."
                            : $"Link {relation.FromId} - {relation.ToId} joins two topics of other boards; links go through GAMA only.");
                        continue;
                    }

                    var (master, other) = fromMaster ? (from, to) : (to, from);
                    if (rows.Exists(t => t.LegacyRelationId == relation.Id) || rows.Exists(t => t.MasterTopicId == master && t.TopicId == other))
                    {
                        continue;
                    }

                    CurriculumTopicLink row = new() { MasterTopicId = master, TopicId = other, Kind = CurriculumLinkKind.Exact, LegacyRelationId = relation.Id, CreationDate = now };
                    repository.Add(row);
                    rows.Add(row);
                    Count(Links).Added++;
                }

                // The copied links mirror the source while it is the one edited: a link removed there goes here too.
                foreach (var gone in rows.Where(t => t.LegacyRelationId is int id && !seen.Contains(id)).ToList())
                {
                    repository.Remove(gone);
                    Count(Links).Removed++;
                }

                _ = await uow.SaveChangesAsync();
            }

            // Schools: each SchoolBoards row, through the native board's code (the lookup list's board id).
            private async Task CopySchoolsAsync()
            {
                var source = await (from schoolBoard in uow.GetRepository<SchoolBoard>().GetManyQueryable()
                                    join board in uow.GetRepository<Board, int>().GetManyQueryable() on schoolBoard.BoardId equals board.Id
                                    select new { schoolBoard.Id, schoolBoard.SchoolId, board.Code, schoolBoard.CreationDate }).ToListAsync();
                var repository = uow.GetRepository<SchoolQualification>();
                var rows = await repository.GetManyQueryable(tracking: true).ToListAsync();
                HashSet<long> seen = [];
                foreach (var item in source)
                {
                    _ = seen.Add(item.Id);
                    if (item.Code is not int code || !boards.TryGetValue(code, out var boardId))
                    {
                        Issue(Schools, item.Id, $"School {item.SchoolId}'s board (code {item.Code}) has no curriculum board.");
                        continue;
                    }

                    var row = rows.Find(t => t.LegacySchoolBoardId == item.Id);
                    if (row is null)
                    {
                        if (rows.Exists(t => t.SchoolId == item.SchoolId && t.BoardId == boardId && t.QualificationId == null))
                        {
                            continue;
                        }

                        row = new() { SchoolId = item.SchoolId, BoardId = boardId, LegacySchoolBoardId = item.Id, CreationDate = item.CreationDate };
                        repository.Add(row);
                        rows.Add(row);
                        Count(Schools).Added++;
                    }
                    else if (row.BoardId != boardId)
                    {
                        row.BoardId = boardId;
                        Count(Schools).Updated++;
                    }
                }

                foreach (var gone in rows.Where(t => t.LegacySchoolBoardId is long id && !seen.Contains(id)).ToList())
                {
                    repository.Remove(gone);
                    Count(Schools).Removed++;
                }

                _ = await uow.SaveChangesAsync();
            }

            private async Task SaveIssuesAsync()
            {
                var repository = uow.GetRepository<CurriculumCopyIssue>();
                foreach (var old in await repository.GetManyQueryable(tracking: true).ToListAsync())
                {
                    repository.Remove(old);
                }

                repository.AddRange(issues);
                _ = await uow.SaveChangesAsync();
            }
        }
    }
}
