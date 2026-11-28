namespace GamaEdtech.Test.Curriculum
{
    using GamaEdtech.Application.Interface;
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

    using static GamaEdtech.Common.Core.Constants;

    using TestApplicationDBContext = GamaEdtech.Test.Application.ExamAnalysisServiceTests.TestApplicationDBContext;

    // The copy fills the model from the lookup lists without guessing, can run again with the same result, and stops
    // once the curriculum is edited here.
    public sealed class CurriculumCopyServiceTests : IDisposable
    {
        private readonly ServiceProvider serviceProvider;
        private readonly TestApplicationDBContext context;

        public CurriculumCopyServiceTests()
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
        public async Task TheCopyFillsTheModelAndTheCheckFindsEverything()
        {
            var result = await Service().CopyAsync();
            Assert.Equal(OperationResult.Succeeded, result.OperationResult);

            var boards = context.Set<CurriculumBoard>().ToList();
            var cambridge = boards.Single(t => t.LegacyId == 6627);
            var gama = boards.Single(t => t.LegacyId == 7131);
            Assert.Equal(("cambridge", "CIE", true, false), (cambridge.Slug, cambridge.ShortName, cambridge.IsDefault, cambridge.IsMaster));
            Assert.True(gama.IsMaster);
            Assert.Equal(CurriculumSeoDefaults.CambridgeCode, cambridge.CodeTemplate);

            var physics = context.Set<CurriculumSyllabus>().Single(t => t.LegacyId == 2195);
            Assert.Equal(("Physics", "0625"), (physics.Name, physics.Code));
            Assert.Equal("Physics", context.Set<CurriculumSubject>().Single(t => t.Id == physics.SubjectId).Name);

            // The flat topic list becomes units and chapters.
            var topics = context.Set<CurriculumTopic>().ToDictionary(t => t.LegacyId!.Value);
            Assert.Null(topics[10].ParentTopicId);
            Assert.Equal(topics[10].Id, topics[11].ParentTopicId);
            Assert.Equal(topics[13].Id, topics[14].ParentTopicId);

            // Paper 4 only where papers use it; Worksheets is a resource type; the variant without "nv".
            var paper = context.Set<CurriculumPaper>().Single();
            Assert.Equal((physics.Id, "4"), (paper.SyllabusId, paper.ShortLabel));
            Assert.Equal("Worksheets", context.Set<CurriculumResourceType>().Single().Name);
            Assert.Equal("2", context.Set<CurriculumVariantOption>().Single().Label);
            Assert.Contains(context.Set<CurriculumDocumentType>(), t => t.Code == "in" && t.LegacyId == 6351);

            // The website's sessions, filters and Cambridge's SEO texts become the boards' settings.
            Assert.Equal(["March", "May/Jun", "Oct/Nov"], context.Set<CurriculumSeriesType>().Where(t => t.BoardId == cambridge.Id).OrderBy(t => t.ListOrder).Select(t => t.Name));
            Assert.Contains(context.Set<CurriculumBoardFilter>(), t => t.BoardId == cambridge.Id && t.Key == "variant.variant");
            Assert.Contains(context.Set<CurriculumSeoTemplate>(), t => t.BoardId == cambridge.Id && t.Page == CurriculumSeoPage.PastPaper);

            // Only the GAMA - Cambridge link is a link; the rest is for review.
            var link = context.Set<CurriculumTopicLink>().Single();
            Assert.Equal((topics[21].Id, topics[11].Id, 1), (link.MasterTopicId, link.TopicId, link.LegacyRelationId));
            var issues = context.Set<CurriculumCopyIssue>().ToList();
            Assert.Contains(issues, t => t.Kind == CurriculumCopyService.Links && t.SourceId == 2);
            Assert.Contains(issues, t => t.Kind == CurriculumCopyService.Links && t.SourceId == 3);
            Assert.Contains(issues, t => t.Kind == CurriculumCopyService.Qualifications && t.SourceId == 99);

            Assert.Equal(cambridge.Id, context.Set<SchoolQualification>().Single().BoardId);

            var check = (await Service().CheckAsync()).Data!;
            Assert.All(check.Lists.Where(t => t.Kind != CurriculumCopyService.Links), t => Assert.Equal(0, t.Missing));
            var papers = check.Content.Single(t => t.Content == "pastPapers");
            Assert.Equal((2, 2), (papers.Total, papers.Placed));
        }

        [Fact]
        public async Task ASecondRunChangesNothingAndMirrorsRemovals()
        {
            _ = await Service().CopyAsync();
            var counts = (context.Set<CurriculumBoard>().Count(), context.Set<CurriculumTopic>().Count(), context.Set<CurriculumSeriesType>().Count(), context.Set<CurriculumBoardFilter>().Count());

            _ = context.Set<LegacyTaxonomyRelation>().Remove(context.Set<LegacyTaxonomyRelation>().Single(t => t.Id == 1));
            _ = await context.SaveChangesAsync();
            var again = (await Service().CopyAsync()).Data!;

            Assert.Equal(counts, (context.Set<CurriculumBoard>().Count(), context.Set<CurriculumTopic>().Count(), context.Set<CurriculumSeriesType>().Count(), context.Set<CurriculumBoardFilter>().Count()));
            Assert.DoesNotContain(again.Counts, t => t.Kind != CurriculumCopyService.Links && (t.Added > 0 || t.Updated > 0));
            Assert.Equal(1, again.Counts.Single(t => t.Kind == CurriculumCopyService.Links).Removed);
            Assert.Empty(context.Set<CurriculumTopicLink>());
        }

        [Fact]
        public async Task TheCopyStopsOnceTheCurriculumIsEditedHere()
        {
            var refused = await Service(MigrationRoute.Native).CopyAsync();
            Assert.Equal("notNative", refused.Errors?.Single().Reference);
            Assert.Empty(context.Set<CurriculumBoard>());
        }

        private CurriculumCopyService Service(MigrationRoute? writes = null) => new(new(serviceProvider.GetRequiredService<IUnitOfWorkProvider>),
            new(serviceProvider.GetRequiredService<IHttpContextAccessor>), new(() => NullLogger<CurriculumCopyService>.Instance),
            new(() => new PassThroughMigrationRouter() { WriteRoute = writes ?? MigrationRoute.Legacy }), new(() => new NoJobs()));

        internal sealed class NoJobs : IJobScheduler
        {
            public void Schedule<TService>(System.Linq.Expressions.Expression<Func<TService, Task>> call, TimeSpan delay)
                where TService : notnull
            {
            }
        }
    }
}
