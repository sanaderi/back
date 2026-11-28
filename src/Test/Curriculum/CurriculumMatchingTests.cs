namespace GamaEdtech.Test.Curriculum
{
    using GamaEdtech.Application.Interface;
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

    using Fake = GamaEdtech.Test.ExamImport.McpSignOutTests.Fake;
    using TestApplicationDBContext = GamaEdtech.Test.Application.ExamAnalysisServiceTests.TestApplicationDBContext;

    // Topic matching on the curriculum model: approve exact or partial, the model's link follows at once, the review's
    // subject filter works, and the subjects compared come from the subjects across boards.
    public sealed class CurriculumMatchingTests : IDisposable
    {
        private const int GamaChapter = 21;
        private const int CambridgeUnit = 13;
        private readonly ServiceProvider serviceProvider;
        private readonly TestApplicationDBContext context;

        public CurriculumMatchingTests()
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
        public async Task APartialApprovalIsOnlyRelatedAndAnExactOneLinksTheLists()
        {
            await CopyAsync();
            _ = context.Set<TopicMatch>().Add(new TopicMatch
            {
                Id = 1,
                GamaTopicId = GamaChapter,
                GamaSubjectId = 7133,
                ExternalTopicId = CambridgeUnit,
                ExternalSubjectId = 2195,
                Confidence = 92,
                CreationDate = DateTimeOffset.UtcNow,
                SyncDate = DateTimeOffset.UtcNow,
            });
            _ = await context.SaveChangesAsync();
            var admin = Admin();

            // The review's subject filter (either side's subject).
            Assert.Equal([1L], (await admin.GetListAsync(new() { SubjectId = 2195 })).Data!.Items.Select(t => t.Id));
            Assert.Empty((await admin.GetListAsync(new() { SubjectId = 9999 })).Data!.Items);

            Assert.True((await admin.ApproveAsync(1, 7, CurriculumLinkKind.Partial)).Data);
            var gama = context.Set<CurriculumTopic>().Single(t => t.LegacyId == GamaChapter).Id;
            var cambridge = context.Set<CurriculumTopic>().Single(t => t.LegacyId == CambridgeUnit).Id;
            Assert.Equal(CurriculumLinkKind.Partial, context.Set<CurriculumTopicLink>().Single(t => t.MasterTopicId == gama && t.TopicId == cambridge).Kind);
            Assert.DoesNotContain(context.Set<LegacyTaxonomyRelation>(), t => t.FromId == GamaChapter && t.ToId == CambridgeUnit);
            Assert.Equal(CurriculumLinkKind.Partial, (await admin.GetListAsync(new() { TopicId = GamaChapter })).Data!.Items.Single().Kind);

            // Taken back, then approved exact: the link follows each time.
            Assert.True((await admin.UnapproveAsync(1)).Data);
            Assert.DoesNotContain(context.Set<CurriculumTopicLink>(), t => t.MasterTopicId == gama && t.TopicId == cambridge);
            Assert.True((await admin.ApproveAsync(1, 7, CurriculumLinkKind.Exact)).Data);
            Assert.Equal(CurriculumLinkKind.Exact, context.Set<CurriculumTopicLink>().Single(t => t.MasterTopicId == gama && t.TopicId == cambridge).Kind);
            Assert.Contains(context.Set<LegacyTaxonomyRelation>(), t => t.FromId == GamaChapter && t.ToId == CambridgeUnit);
        }

        [Fact]
        public async Task TheSubjectsComparedComeFromTheSubjectsAcrossBoards()
        {
            await CopyAsync();
            var uow = serviceProvider.GetRequiredService<IUnitOfWorkProvider>().CreateUnitOfWork();
            var (cambridgeCandidates, cambridgeIsGama) = await CurriculumMatching.CandidatesAsync(uow, 2195);
            Assert.Equal([7133], cambridgeCandidates);
            Assert.False(cambridgeIsGama);
            var (gamaCandidates, gamaIsGama) = await CurriculumMatching.CandidatesAsync(uow, 7133);
            Assert.Equal([2195], gamaCandidates);
            Assert.True(gamaIsGama);
            Assert.Empty((await CurriculumMatching.CandidatesAsync(uow, 999999)).Subjects);
        }

        private TopicMatchAdminService Admin() => new(new(serviceProvider.GetRequiredService<IUnitOfWorkProvider>), new(serviceProvider.GetRequiredService<IHttpContextAccessor>),
            new(() => NullLogger<TopicMatchAdminService>.Instance), new(() => new PassThroughMigrationRouter { WriteRoute = MigrationRoute.Native }),
            new(() => Fake.Create<IAiService>([])));

        private async Task CopyAsync()
        {
            var copied = await new CurriculumCopyService(new(serviceProvider.GetRequiredService<IUnitOfWorkProvider>), new(serviceProvider.GetRequiredService<IHttpContextAccessor>),
                new(() => NullLogger<CurriculumCopyService>.Instance), new(() => new PassThroughMigrationRouter()), new(() => new CurriculumCopyServiceTests.NoJobs())).CopyAsync();
            Assert.NotNull(copied.Data);
        }
    }
}
