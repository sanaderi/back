namespace GamaEdtech.Test.Curriculum
{
    using GamaEdtech.Application.Service;
    using GamaEdtech.Common.DataAccess.Context;
    using GamaEdtech.Common.DataAccess.Repositories;
    using GamaEdtech.Common.DataAccess.UnitOfWork;
    using GamaEdtech.Data.Dto.Taxonomy;
    using GamaEdtech.Domain.Entity;
    using GamaEdtech.Domain.Entity.Curriculum;
    using GamaEdtech.Domain.Enumeration;
    using GamaEdtech.Test.Application;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging.Abstractions;

    using Xunit;

    using TestApplicationDBContext = GamaEdtech.Test.Application.ExamAnalysisServiceTests.TestApplicationDBContext;

    // After the write switch, a board made in the admin gets lookup ids, so content, forms and lists use it like any
    // other; the switch waits for the lookup lists' own; a school's boards keep its curriculum boards in step.
    public sealed class CurriculumMirrorTests : IDisposable
    {
        private const long AdminId = 7;
        private readonly ServiceProvider serviceProvider;
        private readonly TestApplicationDBContext context;

        public CurriculumMirrorTests()
        {
            Environment.SetEnvironmentVariable("Test", "True");
            ServiceCollection services = new();
            _ = services.AddLogging();
            _ = services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["EnableAudit"] = bool.FalseString }).Build());
            _ = services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            _ = services.AddSingleton<TestApplicationDBContext>();
            _ = services.AddSingleton<IEntityContext>(provider => provider.GetRequiredService<TestApplicationDBContext>());
            _ = services.AddTransient(typeof(IRepository<,>), typeof(GenericEntityRepository<,>));
            _ = services.AddSingleton<IUnitOfWorkProvider, UnitOfWorkProvider>();
            serviceProvider = services.BuildServiceProvider();
            context = serviceProvider.GetRequiredService<TestApplicationDBContext>();
            CurriculumTestData.Seed(context);
        }

        public void Dispose()
        {
            context.Dispose();
            serviceProvider.Dispose();
        }

        [Fact]
        public async Task ABoardMadeAfterTheSwitchGetsLookupIdsContentCanUse()
        {
            var admin = Admin(MigrationRoute.Native, MigrationRoute.Native);
            var board = (await admin.SaveBoardAsync(new() { Slug = "necta", Name = "NECTA", IsEnabled = true }, AdminId)).Data;
            var level = (await admin.SaveQualificationAsync(new() { BoardId = board, Slug = "csee", Name = "CSEE (Form 4)" }, AdminId)).Data;
            var syllabus = (await admin.SaveSyllabusAsync(new() { QualificationId = level, Slug = "031-physics", Name = "Physics", Code = "031" }, AdminId)).Data;
            var chapterUnit = (await admin.SaveTopicAsync(new() { SyllabusId = syllabus, Name = "Mechanics", IsUnit = true }, AdminId)).Data;
            _ = await admin.SaveTopicAsync(new() { SyllabusId = syllabus, ParentTopicId = chapterUnit, Name = "Motion" }, AdminId);
            Assert.True((await admin.SavePapersAsync(syllabus, [new() { Name = "Paper 2", ShortLabel = "2" }])).Data);

            var items = context.Set<LegacyTaxonomyItem>().ToDictionary(t => t.Id);
            var boardRow = context.Set<CurriculumBoard>().Single(t => t.Id == board);
            var levelRow = context.Set<CurriculumQualification>().Single(t => t.Id == level);
            var syllabusRow = context.Set<CurriculumSyllabus>().Single(t => t.Id == syllabus);
            Assert.Equal((LegacyTaxonomyKinds.Board, "NECTA"), (items[boardRow.LegacyId!.Value].Kind, items[boardRow.LegacyId.Value].Title));
            Assert.Equal((LegacyTaxonomyKinds.Grade, boardRow.LegacyId.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)), (items[levelRow.LegacyId!.Value].Kind, items[levelRow.LegacyId.Value].Parent));
            Assert.Equal(("Physics (031)", $"0,{levelRow.LegacyId}"), (items[syllabusRow.LegacyId!.Value].Title, items[syllabusRow.LegacyId.Value].Parent));

            // The topics, flat in tree order with the unit as a season, as gama-api lists them.
            var topics = context.Set<CurriculumTopic>().Where(t => t.SyllabusId == syllabus).OrderBy(t => t.Id).ToList();
            Assert.True(items[topics[0].LegacyId!.Value].IsSeason);
            Assert.True(items[topics[0].LegacyId!.Value].ListOrder < items[topics[1].LegacyId!.Value].ListOrder);
            var paper = context.Set<CurriculumPaper>().Single(t => t.SyllabusId == syllabus);
            Assert.Equal((LegacyTaxonomyKinds.Classification, true), (items[paper.LegacyId!.Value].Kind, items[paper.LegacyId.Value].IsPaper));

            // A paper saved with those ids is placed in the model.
            _ = context.Set<PastPaper>().Add(new PastPaper
            {
                Id = 50,
                Title = "031/2 Physics 2022",
                CoreBoardId = boardRow.LegacyId.Value,
                CoreGradeId = levelRow.LegacyId.Value,
                CoreSubjectId = syllabusRow.LegacyId.Value,
                CorePaperTypeId = paper.LegacyId,
                CoreTopicIds = topics[1].LegacyId!.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
                CreationDate = DateTimeOffset.UtcNow,
                SyncDate = DateTimeOffset.UtcNow,
            });
            _ = await context.SaveChangesAsync();
            _ = await new CurriculumContentService(new(serviceProvider.GetRequiredService<IUnitOfWorkProvider>), new(serviceProvider.GetRequiredService<IHttpContextAccessor>),
                new(() => NullLogger<CurriculumContentService>.Instance), new(() => new PassThroughMigrationRouter())).LinkAsync();
            var placed = context.Set<PastPaper>().Single(t => t.Id == 50);
            Assert.Equal((board, syllabus, paper.Id), (placed.CurriculumBoardId, placed.CurriculumSyllabusId, placed.CurriculumPaperId));
            Assert.Equal(topics[1].Id, context.Set<CurriculumContentTopic>().Single(t => t.ContentId == 50).TopicId);
        }

        [Fact]
        public async Task TheCurriculumsWritesWaitForTheListsOwn()
        {
            var refused = await Admin(MigrationRoute.Native, MigrationRoute.Legacy).SaveBoardAsync(new() { Slug = "ib", Name = "IB" }, AdminId);
            Assert.Equal("typesNotNative", refused.Errors!.Single().Reference);

            // Before the switch nothing is mirrored.
            var before = (await Admin(MigrationRoute.Legacy, MigrationRoute.Legacy).SaveBoardAsync(new() { Slug = "ib", Name = "IB" }, AdminId)).Data;
            Assert.Null(context.Set<CurriculumBoard>().Single(t => t.Id == before).LegacyId);
        }

        [Fact]
        public async Task ASchoolsBoardsKeepItsCurriculumBoardsInStep()
        {
            _ = await new CurriculumCopyService(new(serviceProvider.GetRequiredService<IUnitOfWorkProvider>), new(serviceProvider.GetRequiredService<IHttpContextAccessor>),
                new(() => NullLogger<CurriculumCopyService>.Instance), new(() => new PassThroughMigrationRouter()), new(() => new CurriculumCopyServiceTests.NoJobs())).CopyAsync();
            var uow = serviceProvider.GetRequiredService<IUnitOfWorkProvider>().CreateUnitOfWork();
            _ = Assert.Single(context.Set<SchoolQualification>());

            // The school drops its only board: its curriculum board goes too.
            _ = context.Set<SchoolBoard>().Remove(context.Set<SchoolBoard>().Single());
            _ = await context.SaveChangesAsync();
            await CurriculumMirror.SyncSchoolAsync(uow, 5);
            Assert.Empty(context.Set<SchoolQualification>());

            // And picks it again.
            _ = context.Set<SchoolBoard>().Add(new SchoolBoard { Id = 9, SchoolId = 5, BoardId = 3, CreationDate = DateTimeOffset.UtcNow, CreationUserId = 1 });
            _ = await context.SaveChangesAsync();
            await CurriculumMirror.SyncSchoolAsync(uow, 5);
            Assert.Equal(9, context.Set<SchoolQualification>().Single().LegacySchoolBoardId);
        }

        private CurriculumAdminService Admin(MigrationRoute curriculumWrites, MigrationRoute typesWrites) => new(new(serviceProvider.GetRequiredService<IUnitOfWorkProvider>),
            new(serviceProvider.GetRequiredService<IHttpContextAccessor>), new(() => NullLogger<CurriculumAdminService>.Instance),
            new(() => new PassThroughMigrationRouter
            {
                WriteRoutes = new Dictionary<string, MigrationRoute> { [nameof(MigrationDomain.Curriculum)] = curriculumWrites, [nameof(MigrationDomain.Types)] = typesWrites },
            }));
    }
}
