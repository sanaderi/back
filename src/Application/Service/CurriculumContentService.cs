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
    using GamaEdtech.Domain.Entity;
    using GamaEdtech.Domain.Entity.Curriculum;
    using GamaEdtech.Domain.Enumeration;

    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using static GamaEdtech.Common.Core.Constants;

    public sealed class CurriculumContentService(Lazy<IUnitOfWorkProvider> unitOfWorkProvider, Lazy<IHttpContextAccessor> httpContextAccessor,
        Lazy<ILogger<CurriculumContentService>> logger, Lazy<IMigrationRouter> migrationRouter)
        : ServiceBase<CurriculumContentService>(unitOfWorkProvider, httpContextAccessor, logger), ICurriculumContentService
    {
        private const int BatchSize = 1000;

        public CurriculumStatusDto GetStatus() => new()
        {
            ReadNative = migrationRouter.Value.GetRoute(MigrationDomain.Curriculum, MigrationFeatures.Website) == MigrationRoute.Native,
            WriteNative = migrationRouter.Value.GetWriteRoute(MigrationDomain.Curriculum) == MigrationRoute.Native,
        };

        public async Task<ResultData<int>> LinkAsync(int maxRows = 20000)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var maps = await Maps.LoadAsync(uow);
                var now = DateTimeOffset.UtcNow;
                var done = 0;

                // Past papers: board, level, subject, paper, variant, session and topics.
                var papers = uow.GetRepository<PastPaper>();
                long lastPastPaper = 0;
                while (done < maxRows)
                {
                    var batch = await papers.GetManyQueryable(t => t.Id > lastPastPaper && (t.CurriculumLinkDate == null || (t.LastModifyDate ?? t.CreationDate) > t.CurriculumLinkDate), tracking: true)
                        .OrderBy(t => t.Id).Take(BatchSize).ToListAsync();
                    if (batch.Count == 0)
                    {
                        break;
                    }

                    foreach (var paper in batch)
                    {
                        paper.CurriculumBoardId = maps.Boards.GetValueOrDefault(paper.CoreBoardId);
                        paper.CurriculumQualificationId = maps.Qualifications.GetValueOrDefault(paper.CoreGradeId);
                        paper.CurriculumSyllabusId = maps.Syllabi.GetValueOrDefault(paper.CoreSubjectId);
                        paper.CurriculumPaperId = paper.CurriculumSyllabusId is int syllabus && paper.CorePaperTypeId is int type ? maps.Papers.GetValueOrDefault((syllabus, type)) : null;
                        paper.CurriculumVariantOptionId = paper.CurriculumBoardId is int board && paper.Variant is short variant ? maps.Variants.GetValueOrDefault((board, variant)) : null;
                        paper.CurriculumSeriesTypeId = paper.CurriculumQualificationId is int level && paper.EduMonth is byte month ? maps.Series.GetValueOrDefault((level, month)) : null;
                        paper.CurriculumLinkDate = Later(now, paper.LastModifyDate ?? paper.CreationDate);
                    }

                    lastPastPaper = batch[^1].Id;
                    await LinkTopicsAsync(uow, CurriculumContentKind.PastPaper, batch.Select(t => (t.Id, t.CoreTopicIds)), maps);
                    _ = await uow.SaveChangesAsync();
                    done += batch.Count;
                }

                var tutorials = uow.GetRepository<Tutorial>();
                long lastTutorial = 0;
                while (done < maxRows * 2)
                {
                    var batch = await tutorials.GetManyQueryable(t => t.Id > lastTutorial && (t.CurriculumLinkDate == null || (t.LastModifyDate ?? t.CreationDate) > t.CurriculumLinkDate), tracking: true)
                        .OrderBy(t => t.Id).Take(BatchSize).ToListAsync();
                    if (batch.Count == 0)
                    {
                        break;
                    }

                    foreach (var tutorial in batch)
                    {
                        tutorial.CurriculumBoardId = tutorial.CoreBoardId is int b ? maps.Boards.GetValueOrDefault(b) : null;
                        tutorial.CurriculumQualificationId = tutorial.CoreGradeId is int g ? maps.Qualifications.GetValueOrDefault(g) : null;
                        tutorial.CurriculumSyllabusId = tutorial.CoreSubjectId is int s ? maps.Syllabi.GetValueOrDefault(s) : null;
                        tutorial.CurriculumLinkDate = Later(now, tutorial.LastModifyDate ?? tutorial.CreationDate);
                    }

                    lastTutorial = batch[^1].Id;
                    await LinkTopicsAsync(uow, CurriculumContentKind.Tutorial, batch.Select(t => (t.Id, t.CoreTopicIds)), maps);
                    _ = await uow.SaveChangesAsync();
                    done += batch.Count;
                }

                var exams = uow.GetRepository<Exam>();
                long lastExam = 0;
                while (done < maxRows * 3)
                {
                    var batch = await exams.GetManyQueryable(t => t.Id > lastExam && (t.CurriculumLinkDate == null || (t.LastModifyDate ?? t.CreationDate) > t.CurriculumLinkDate), tracking: true)
                        .OrderBy(t => t.Id).Take(BatchSize).ToListAsync();
                    if (batch.Count == 0)
                    {
                        break;
                    }

                    foreach (var exam in batch)
                    {
                        exam.CurriculumBoardId = exam.CoreBoardId is int b ? maps.Boards.GetValueOrDefault(b) : null;
                        exam.CurriculumQualificationId = exam.CoreGradeId is int g ? maps.Qualifications.GetValueOrDefault(g) : null;
                        exam.CurriculumSyllabusId = exam.CoreSubjectId is int s ? maps.Syllabi.GetValueOrDefault(s) : null;
                        exam.CurriculumLinkDate = Later(now, exam.LastModifyDate ?? exam.CreationDate);
                    }

                    lastExam = batch[^1].Id;
                    await LinkTopicsAsync(uow, CurriculumContentKind.Exam, batch.Select(t => (t.Id, t.CoreTopicIds)), maps);
                    _ = await uow.SaveChangesAsync();
                    done += batch.Count;
                }

                return new(OperationResult.Succeeded) { Data = done };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message }] };
            }
        }

        // A row changed "in the future" (another server's clock) is not taken again on every pass.
        private static DateTimeOffset Later(DateTimeOffset a, DateTimeOffset b) => a > b ? a : b;

        // Replaces the batch's topic rows with what their lookup topic ids mean in the model.
        private static async Task LinkTopicsAsync(IUnitOfWork uow, CurriculumContentKind kind, IEnumerable<(long Id, string? TopicIds)> rows, Maps maps)
        {
            var list = rows.ToList();
            List<long> ids = [.. list.Select(t => t.Id)];
            var repository = uow.GetRepository<CurriculumContentTopic>();
            foreach (var old in await repository.GetManyQueryable(t => t.ContentKind == kind && ids.Contains(t.ContentId), tracking: true).ToListAsync())
            {
                repository.Remove(old);
            }

            foreach (var (id, topicIds) in list)
            {
                foreach (var topic in CurriculumCopyRules.Parents(topicIds?.Replace(',', '+'), false).Select(t => maps.Topics.GetValueOrDefault(t)).Where(t => t > 0).Distinct())
                {
                    repository.Add(new() { ContentKind = kind, ContentId = id, TopicId = topic });
                }
            }
        }

        /// <summary>The lookup ids' places in the model, read once per run.</summary>
        private sealed record Maps(
            Dictionary<int, int> Boards,
            Dictionary<int, int> Qualifications,
            Dictionary<int, int> Syllabi,
            Dictionary<int, int> Topics,
            Dictionary<(int Syllabus, int Type), int> Papers,
            Dictionary<(int Board, int Variant), int> Variants,
            Dictionary<(int Level, byte Month), int> Series)
        {
            public static async Task<Maps> LoadAsync(IUnitOfWork uow)
            {
                var boards = (await uow.GetRepository<CurriculumBoard, int>().GetManyQueryable(t => t.LegacyId != null).Select(t => new { t.Id, t.LegacyId }).ToListAsync())
                    .ToDictionary(t => t.LegacyId!.Value, t => t.Id);
                var qualifications = (await uow.GetRepository<CurriculumQualification, int>().GetManyQueryable(t => t.LegacyId != null).Select(t => new { t.Id, t.LegacyId }).ToListAsync())
                    .ToDictionary(t => t.LegacyId!.Value, t => t.Id);
                var syllabi = (await uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable(t => t.LegacyId != null).Select(t => new { t.Id, t.LegacyId }).ToListAsync())
                    .ToDictionary(t => t.LegacyId!.Value, t => t.Id);
                var topics = (await uow.GetRepository<CurriculumTopic, int>().GetManyQueryable(t => t.LegacyId != null).Select(t => new { t.Id, t.LegacyId }).ToListAsync())
                    .ToDictionary(t => t.LegacyId!.Value, t => t.Id);
                var papers = (await uow.GetRepository<CurriculumPaper, int>().GetManyQueryable(t => t.LegacyId != null).Select(t => new { t.Id, t.SyllabusId, t.LegacyId }).ToListAsync())
                    .GroupBy(t => (t.SyllabusId, t.LegacyId!.Value)).ToDictionary(t => t.Key, t => t.First().Id);
                var variants = (await (from option in uow.GetRepository<CurriculumVariantOption, int>().GetManyQueryable(t => t.LegacyId != null)
                                       join kind in uow.GetRepository<CurriculumVariantKind, int>().GetManyQueryable() on option.VariantKindId equals kind.Id
                                       select new { option.Id, kind.BoardId, option.LegacyId }).ToListAsync())
                    .GroupBy(t => (t.BoardId, t.LegacyId!.Value)).ToDictionary(t => t.Key, t => t.First().Id);

                // A level's sessions by month: its own first, then the board's general ones.
                var levelBoards = await uow.GetRepository<CurriculumQualification, int>().GetManyQueryable().Select(t => new { t.Id, t.BoardId }).ToListAsync();
                var seriesRows = await uow.GetRepository<CurriculumSeriesType, int>().GetManyQueryable().Select(t => new { t.Id, t.BoardId, t.QualificationId, t.Months }).ToListAsync();
                Dictionary<(int, byte), int> series = [];
                foreach (var level in levelBoards)
                {
                    foreach (var row in seriesRows.Where(t => t.QualificationId == level.Id).Concat(seriesRows.Where(t => t.QualificationId == null && t.BoardId == level.BoardId)))
                    {
                        foreach (var month in (row.Months ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        {
                            if (byte.TryParse(month, CultureInfo.InvariantCulture, out var m))
                            {
                                _ = series.TryAdd((level.Id, m), row.Id);
                            }
                        }
                    }
                }

                return new(boards, qualifications, syllabi, topics, papers, variants, series);
            }
        }
    }
}
