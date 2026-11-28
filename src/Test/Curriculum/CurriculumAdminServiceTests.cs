namespace GamaEdtech.Test.Curriculum
{
    using GamaEdtech.Application.Service;
    using GamaEdtech.Common.DataAccess.Context;
    using GamaEdtech.Common.DataAccess.Repositories;
    using GamaEdtech.Common.DataAccess.UnitOfWork;
    using GamaEdtech.Data.Dto.Curriculum;
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

    // The admin keeps the model consistent: one GAMA, links only through it, copied rows untouched while the copy writes
    // them, tab scopes stored only when they differ from the default; and the read side answers from what was saved.
    public sealed class CurriculumAdminServiceTests : IDisposable
    {
        private const long AdminId = 7;
        private readonly ServiceProvider serviceProvider;
        private readonly TestApplicationDBContext context;

        public CurriculumAdminServiceTests()
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
        }

        public void Dispose()
        {
            context.Dispose();
            serviceProvider.Dispose();
        }

        [Fact]
        public async Task ThereIsOneGamaAndLinksOnlyGoThroughIt()
        {
            var admin = Admin();
            var gama = (await admin.SaveBoardAsync(new() { Slug = "gama", Name = "GAMA", IsMaster = true }, AdminId)).Data;
            var cambridge = (await admin.SaveBoardAsync(new() { Slug = "cambridge", Name = "Cambridge" }, AdminId)).Data;
            var edexcel = (await admin.SaveBoardAsync(new() { Slug = "edexcel", Name = "Edexcel" }, AdminId)).Data;
            Assert.Equal(OperationResult.NotValid, (await admin.SaveBoardAsync(new() { Slug = "other", Name = "Other", IsMaster = true }, AdminId)).OperationResult);
            Assert.Equal(OperationResult.Duplicate, (await admin.SaveBoardAsync(new() { Slug = "gama", Name = "Again" }, AdminId)).OperationResult);
            Assert.Equal(OperationResult.NotValid, (await admin.SaveBoardAsync(new() { Slug = "Bad Slug", Name = "Bad" }, AdminId)).OperationResult);

            var g = await TopicAsync(admin, gama, "Differentiation");
            var c = await TopicAsync(admin, cambridge, "Differentiation");
            var e = await TopicAsync(admin, edexcel, "Differentiation");

            Assert.Equal(OperationResult.NotValid, (await admin.AddTopicLinkAsync(new() { MasterTopicId = c, TopicId = e })).OperationResult);
            Assert.Equal(OperationResult.NotValid, (await admin.AddTopicLinkAsync(new() { MasterTopicId = c, TopicId = g })).OperationResult);
            Assert.Equal(OperationResult.Succeeded, (await admin.AddTopicLinkAsync(new() { MasterTopicId = g, TopicId = c })).OperationResult);
            Assert.Equal(OperationResult.Succeeded, (await admin.AddTopicLinkAsync(new() { MasterTopicId = g, TopicId = e })).OperationResult);
            Assert.Equal(OperationResult.Duplicate, (await admin.AddTopicLinkAsync(new() { MasterTopicId = g, TopicId = e })).OperationResult);

            // Cambridge's tutorials list: own, GAMA's, Edexcel's. Its past papers: its own only.
            var reader = Reader();
            var tutorials = (await reader.GetLinkedTopicsAsync(cambridge, c, CurriculumContentTab.Tutorials)).Data!;
            Assert.Equal([CurriculumLinkSource.Own, CurriculumLinkSource.Master, CurriculumLinkSource.OtherBoard], tutorials.Groups.Select(t => t.Source));
            Assert.Equal(e, tutorials.Groups[2].Topics.Single().Id);
            var papers = (await reader.GetLinkedTopicsAsync(cambridge, c, CurriculumContentTab.PastPapers)).Data!;
            Assert.Equal(CurriculumLinkScope.Own, papers.Scope);
            _ = Assert.Single(papers.Groups);
        }

        [Fact]
        public async Task ACopiedRowIsLeftToTheCopyUntilWritesSwitch()
        {
            _ = context.Set<CurriculumBoard>().Add(new() { Slug = "cambridge", Name = "Cambridge", LegacyId = 6627, CreationDate = DateTimeOffset.UtcNow });
            _ = await context.SaveChangesAsync();
            var id = context.Set<CurriculumBoard>().Single().Id;
            CurriculumBoardDto edit = new() { Id = id, Slug = "cambridge", Name = "Cambridge International" };

            var refused = await Admin().SaveBoardAsync(edit, AdminId);
            Assert.Equal("notNative", refused.Errors?.Single().Reference);

            // Its profile is not copied, so it can be set up before the switch.
            Assert.True((await Admin().SaveBoardFiltersAsync(id, [new() { Key = CurriculumFilterKeys.Paper, Label = "Paper", Style = CurriculumFilterStyle.Chips, IsEnabled = true }])).Data);

            Assert.Equal(id, (await Admin(MigrationRoute.Native).SaveBoardAsync(edit, AdminId)).Data);
            Assert.Equal(OperationResult.NotValid, (await Admin(MigrationRoute.Native).DeleteBoardAsync(id)).OperationResult);
        }

        [Fact]
        public async Task ATabBackAtItsDefaultIsNotStored()
        {
            var admin = Admin();
            var board = (await admin.SaveBoardAsync(new() { Slug = "ib", Name = "IB" }, AdminId)).Data;
            var other = (await admin.SaveBoardAsync(new() { Slug = "aqa", Name = "AQA" }, AdminId)).Data;

            Assert.True((await admin.SaveBoardLinksAsync(board, new()
            {
                Tabs = [new() { Tab = CurriculumContentTab.PastPapers, Scope = CurriculumLinkScope.All }, new() { Tab = CurriculumContentTab.Exams, Scope = CurriculumLinkScope.All }],
                LinkOrder = [other],
            })).Data);
            _ = Assert.Single(context.Set<CurriculumBoardTab>());

            var detail = (await admin.GetBoardAsync(board)).Data!;
            Assert.Equal(CurriculumLinkScope.All, detail.Tabs.Single(t => t.Tab == CurriculumContentTab.PastPapers).Scope);
            Assert.True(detail.Tabs.Single(t => t.Tab == CurriculumContentTab.Exams).IsDefault);
            Assert.Equal([other], detail.LinkOrder);
            Assert.Equal(OperationResult.NotValid, (await admin.SaveBoardLinksAsync(board, new() { LinkOrder = [board] })).OperationResult);
        }

        [Fact]
        public async Task ABoardsSeoTemplateWinsOverTheDefault()
        {
            var admin = Admin();
            var board = (await admin.SaveBoardAsync(new() { Slug = "necta", Name = "NECTA" }, AdminId)).Data;
            _ = await admin.SaveSeoTemplateAsync(new() { Page = CurriculumSeoPage.TutorialList, TitleTemplate = "{board} notes || Notes", DescriptionTemplate = "About {board}." }, AdminId);
            var own = (await admin.SaveSeoTemplateAsync(new() { BoardId = board, Page = CurriculumSeoPage.TutorialList, TitleTemplate = "{board} revision", DescriptionTemplate = "Revise {board}." }, AdminId)).Data;

            Dictionary<string, string?> values = new() { ["board"] = "NECTA" };
            Assert.Equal("NECTA revision", (await Reader().GetSeoAsync(board, CurriculumSeoPage.TutorialList, values)).Data!.Title);
            Assert.Equal("NECTA notes", (await Reader().GetSeoAsync(null, CurriculumSeoPage.TutorialList, values)).Data!.Title);

            Assert.True((await admin.DeleteSeoTemplateAsync(own)).Data);
            Assert.Equal("NECTA notes", (await Reader().GetSeoAsync(board, CurriculumSeoPage.TutorialList, values)).Data!.Title);
            var global = context.Set<CurriculumSeoTemplate>().Single().Id;
            Assert.Equal(OperationResult.NotValid, (await admin.DeleteSeoTemplateAsync(global)).OperationResult);
        }

        private CurriculumAdminService Admin(MigrationRoute? writes = null) => new(new(serviceProvider.GetRequiredService<IUnitOfWorkProvider>),
            new(serviceProvider.GetRequiredService<IHttpContextAccessor>), new(() => NullLogger<CurriculumAdminService>.Instance),
            new(() => new PassThroughMigrationRouter() { WriteRoute = writes ?? MigrationRoute.Legacy }));

        private CurriculumService Reader() => new(new(serviceProvider.GetRequiredService<IUnitOfWorkProvider>),
            new(serviceProvider.GetRequiredService<IHttpContextAccessor>), new(() => NullLogger<CurriculumService>.Instance));

        private static async Task<int> TopicAsync(CurriculumAdminService admin, int boardId, string name)
        {
            var qualification = (await admin.SaveQualificationAsync(new() { BoardId = boardId, Slug = "q", Name = "Q" }, AdminId)).Data;
            var syllabus = (await admin.SaveSyllabusAsync(new() { QualificationId = qualification, Slug = "maths", Name = "Maths" }, AdminId)).Data;
            return (await admin.SaveTopicAsync(new() { SyllabusId = syllabus, Name = name }, AdminId)).Data;
        }
    }
}
