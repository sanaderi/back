namespace GamaEdtech.Test.Curriculum
{
    using GamaEdtech.Application.Service;
    using GamaEdtech.Common.DataAccess.Context;
    using GamaEdtech.Common.DataAccess.Repositories;
    using GamaEdtech.Common.DataAccess.UnitOfWork;
    using GamaEdtech.Data.Dto.Curriculum;
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

    // What the website reads from the model after the copy: the filter panel with counts, the paper's SEO texts worked out
    // from the model, the landing page, the linked lists' plan, and the native flag that is the one switch.
    public sealed class CurriculumPublicServiceTests : IDisposable
    {
        private readonly ServiceProvider serviceProvider;
        private readonly TestApplicationDBContext context;

        public CurriculumPublicServiceTests()
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
        public async Task TheFilterPanelHasTheBoardsFiltersWithLiveCountsAndSearchParameters()
        {
            await CopyAsync();
            var answer = (await Service(MigrationRoute.Native).GetFiltersAsync(6627, 22, 2195, isPaper: true)).Data!;
            Assert.True(answer.Native);
            var panel = answer.Data!;
            Assert.Equal(["level", "subject", "year", "series", "paper", "variant.variant"], panel.Select(t => t.Key));

            var series = panel.Single(t => t.Key == "series");
            Assert.Equal(("edu_month", "Chips"), (series.Param, series.Style));
            Assert.Equal([("6", "May/Jun", 1)], series.Options.Select(t => (t.Value, t.Label, t.Count)));
            Assert.Equal([("6920", "4")], panel.Single(t => t.Key == "paper").Options.Select(t => (t.Value, t.Label)));
            Assert.Equal([("7815", "2")], panel.Single(t => t.Key == "variant.variant").Options.Select(t => (t.Value, t.Label)));
            Assert.Equal("variant", panel.Single(t => t.Key == "variant.variant").Param);

            // Until the route is Native the website keeps its own panel; the data still comes, for checking.
            Assert.False((await Service(MigrationRoute.Shadow).GetFiltersAsync(6627, 22, 2195, isPaper: true)).Data!.Native);
        }

        [Fact]
        public async Task APapersSeoTextsAreWorkedOutFromTheModel()
        {
            await CopyAsync();
            var seo = (await Service(MigrationRoute.Native).GetPaperSeoAsync(1)).Data!.Data!;
            Assert.Equal("Cambridge IGCSE Physics (0625) Paper 4 past paper 0625/42/M/J/23", seo.Title);
            Assert.Equal(
                "Download 0625/42/M/J/23 Cambridge IGCSE Physics 0625/42 Paper 4 May June 2023 past paper. In this page you can find question paper (QP) 0625_s23_qp_42, mark scheme (MS) 0625_s23_ms_42, insert 0625_s23_in_42 pdfs available for your upcoming exam series preparation.",
                seo.Description);
        }

        [Fact]
        public async Task ALandingPageHasItsChildrenSchoolsAndTexts()
        {
            await CopyAsync();
            var landing = (await Service(MigrationRoute.Native).GetLandingAsync("cambridge", "igcse", null)).Data!.Data!;
            Assert.Equal(("Cambridge", "IGCSE"), (landing.Board.Name, landing.Level!.Name));
            Assert.Equal(["Physics"], landing.Children.Select(t => t.Name));
            Assert.Equal(1, landing.PaperCount);
            Assert.Equal("Al-Noor", landing.Schools.Single().Name);
            Assert.Equal("Cambridge IGCSE Past Papers and Revision Notes", landing.Seo!.Title);
            Assert.Equal(Common.Core.Constants.OperationResult.NotFound, (await Service(MigrationRoute.Native).GetLandingAsync("cambridge", "nope", null)).OperationResult);
        }

        [Fact]
        public async Task ListsFollowTheBoardsTabScopeThroughGama()
        {
            await CopyAsync();
            var uow = serviceProvider.GetRequiredService<IUnitOfWorkProvider>().CreateUnitOfWork();
            var tutorials = await CurriculumLinking.PlanAsync(uow, CurriculumContentTab.Tutorials, null, null, null, 11);
            Assert.Equal([(CurriculumLinkSource.Master, "21")], tutorials!.Groups.SelectMany(t => t.TopicKeys.Select(k => (t.LinkSource, k))));

            // Past papers are the board's own by default.
            var papers = await CurriculumLinking.PlanAsync(uow, CurriculumContentTab.PastPapers, null, null, null, 11);
            Assert.Empty(papers!.Groups);

            // The lesson tree puts GAMA's tutorials under Cambridge's chapter.
            _ = context.Set<Tutorial>().Add(new Tutorial { Id = 900, Title = "Motion explained", CoreTopicIds = "21", Status = TutorialStatus.Confirmed, CreationDate = DateTimeOffset.UtcNow });
            _ = await context.SaveChangesAsync();
            var tree = (await Service(MigrationRoute.Native).GetLessonTreeAsync(2195)).Data!.Data!;
            var chapter = tree["list"]![0]!["chapters"]![0]!;
            Assert.Equal("900", (string?)chapter["tutorials"]![0]!["id"]);
        }

        [Fact]
        public async Task TheFormsListsAnswerByTodaysIds()
        {
            await CopyAsync();
            var levels = (await Service(MigrationRoute.Native).GetTypeListAsync("base", 6627)).Data!.Data!;
            // "Shared" is under two boards on gama-api; the copy put it under the first, Cambridge.
            Assert.Equal([(22, "IGCSE"), (99, "Shared")], levels.Select(t => (t.LegacyId!.Value, t.Name)));
            var topics = (await Service(MigrationRoute.Native).GetTypeListAsync("topic", 2195)).Data!.Data!;
            Assert.Equal([10, 11, 12, 13, 14], topics.Select(t => t.LegacyId!.Value));
            Assert.True(topics[0].IsUnit);
        }

        [Fact]
        public void RanksPutTheBoardsOwnFirstThenEachGroup()
        {
            System.Linq.Expressions.Expression<Func<int, bool>> own = t => t < 10;
            var rank = CurriculumLinking.Rank(own, [t => t >= 10 && t < 20, t => t >= 20]).Compile();
            Assert.Equal([0, 1, 2, 0], RankSamples.Select(rank));
            var any = CurriculumLinking.Any(own, [t => t == 50]).Compile();
            Assert.Equal([true, true, false], AnySamples.Select(any));
        }

        private static readonly int[] RankSamples = [5, 15, 25, 9];
        private static readonly int[] AnySamples = [1, 50, 30];

        // The copy, then the default templates the migration seeds.
        private async Task CopyAsync()
        {
            var copied = await new CurriculumCopyService(new(serviceProvider.GetRequiredService<IUnitOfWorkProvider>), new(serviceProvider.GetRequiredService<IHttpContextAccessor>),
                new(() => NullLogger<CurriculumCopyService>.Instance), new(() => new PassThroughMigrationRouter()), new(() => new CurriculumCopyServiceTests.NoJobs())).CopyAsync();
            Assert.NotNull(copied.Data);
            foreach (var text in CurriculumSeoDefaults.All)
            {
                _ = context.Set<CurriculumSeoTemplate>().Add(new() { Page = text.Page, TitleTemplate = text.Title, DescriptionTemplate = text.Description });
            }

            _ = await context.SaveChangesAsync();
        }

        private CurriculumPublicService Service(MigrationRoute route)
        {
            PassThroughMigrationRouter router = new() { Routes = new Dictionary<string, MigrationRoute> { [nameof(MigrationDomain.Curriculum)] = route } };
            CurriculumService reader = new(new(serviceProvider.GetRequiredService<IUnitOfWorkProvider>), new(serviceProvider.GetRequiredService<IHttpContextAccessor>),
                new(() => NullLogger<CurriculumService>.Instance));
            return new(new(serviceProvider.GetRequiredService<IUnitOfWorkProvider>), new(serviceProvider.GetRequiredService<IHttpContextAccessor>),
                new(() => NullLogger<CurriculumPublicService>.Instance), new(() => router), new(() => reader));
        }
    }
}
