namespace GamaEdtech.Test.Curriculum
{
    using GamaEdtech.Application.Service;
    using GamaEdtech.Common.DataAccess.Context;
    using GamaEdtech.Common.DataAccess.Repositories;
    using GamaEdtech.Common.DataAccess.UnitOfWork;
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

    // Content gets real curriculum keys from its lookup ids, again after any change, and the landing pages reach the
    // sitemap only once the website reads the model.
    public sealed class CurriculumContentServiceTests : IDisposable
    {
        private readonly ServiceProvider serviceProvider;
        private readonly TestApplicationDBContext context;

        public CurriculumContentServiceTests()
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
        public async Task ContentGetsItsCurriculumKeysAndAgainAfterAChange()
        {
            await CopyAsync();
            var service = Service(MigrationRoute.Legacy);
            Assert.True((await service.LinkAsync()).Data >= 2);

            var paper = context.Set<PastPaper>().Single(t => t.Id == 1);
            var syllabus = context.Set<CurriculumSyllabus>().Single(t => t.LegacyId == 2195);
            Assert.Equal(syllabus.Id, paper.CurriculumSyllabusId);
            Assert.Equal(context.Set<CurriculumPaper>().Single().Id, paper.CurriculumPaperId);
            Assert.Equal(context.Set<CurriculumVariantOption>().Single().Id, paper.CurriculumVariantOptionId);
            Assert.Equal("May/Jun", context.Set<CurriculumSeriesType>().Single(t => t.Id == paper.CurriculumSeriesTypeId).Name);
            var topics = context.Set<CurriculumTopic>().Where(t => t.LegacyId == 10 || t.LegacyId == 11).Select(t => t.Id).Order().ToList();
            Assert.Equal(topics, context.Set<CurriculumContentTopic>().Where(t => t.ContentKind == CurriculumContentKind.PastPaper && t.ContentId == 1).Select(t => t.TopicId).Order());

            // Nothing new: nothing to do. A change: placed again.
            Assert.Equal(0, (await service.LinkAsync()).Data);
            paper.CoreTopicIds = "12";
            paper.LastModifyDate = DateTimeOffset.UtcNow.AddMinutes(1);
            _ = await context.SaveChangesAsync();
            Assert.Equal(1, (await service.LinkAsync()).Data);
            Assert.Equal([context.Set<CurriculumTopic>().Single(t => t.LegacyId == 12).Id], context.Set<CurriculumContentTopic>().Where(t => t.ContentId == 1).Select(t => t.TopicId));
        }

        [Fact]
        public async Task LandingPagesReachTheSitemapOnlyOnceTheWebsiteReadsTheModel()
        {
            await CopyAsync();
            var uow = serviceProvider.GetRequiredService<IUnitOfWorkProvider>().CreateUnitOfWork();
            Assert.Empty(new CurriculumSiteMapHandler(new(() => new PassThroughMigrationRouter())).GetSiteMapData(uow).ToList());

            var native = new CurriculumSiteMapHandler(new(() => new PassThroughMigrationRouter { Routes = new Dictionary<string, MigrationRoute> { [nameof(MigrationDomain.Curriculum)] = MigrationRoute.Native } }));
            var paths = native.GetSiteMapData(uow).Select(t => t.Path1).ToList();
            Assert.Contains("cambridge", paths);
            Assert.Contains("cambridge/igcse", paths);
            Assert.Contains("cambridge/igcse/0625-physics", paths);
            Assert.Equal(paths.Count, native.GetSiteMapData(uow).Select(t => t.Id).Distinct().Count());
        }

        [Fact]
        public void TheStatusSaysWhichSwitchIsOn()
        {
            var status = new CurriculumContentService(new(serviceProvider.GetRequiredService<IUnitOfWorkProvider>), new(serviceProvider.GetRequiredService<IHttpContextAccessor>),
                new(() => NullLogger<CurriculumContentService>.Instance), new(() => new PassThroughMigrationRouter { WriteRoute = MigrationRoute.Native })).GetStatus();
            Assert.Equal((false, true), (status.ReadNative, status.WriteNative));
        }

        private CurriculumContentService Service(MigrationRoute route) => new(new(serviceProvider.GetRequiredService<IUnitOfWorkProvider>),
            new(serviceProvider.GetRequiredService<IHttpContextAccessor>), new(() => NullLogger<CurriculumContentService>.Instance),
            new(() => new PassThroughMigrationRouter { Route = route }));

        private async Task CopyAsync()
        {
            var copied = await new CurriculumCopyService(new(serviceProvider.GetRequiredService<IUnitOfWorkProvider>), new(serviceProvider.GetRequiredService<IHttpContextAccessor>),
                new(() => NullLogger<CurriculumCopyService>.Instance), new(() => new PassThroughMigrationRouter()), new(() => new CurriculumCopyServiceTests.NoJobs())).CopyAsync();
            Assert.NotNull(copied.Data);
        }
    }
}
