namespace GamaEdtech.Test.Curriculum
{
    using GamaEdtech.Application.Service;
    using GamaEdtech.Common.DataAccess.Context;
    using GamaEdtech.Common.DataAccess.Repositories;
    using GamaEdtech.Common.DataAccess.UnitOfWork;
    using GamaEdtech.Test.Application;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging.Abstractions;

    using Xunit;

    using TestApplicationDBContext = GamaEdtech.Test.Application.ExamAnalysisServiceTests.TestApplicationDBContext;

    // Paper detection as data: a board's covers are read with its own template, codes, sessions and variants, so a board
    // entered in the admin is understood without code.
    public sealed class CurriculumDetectionTests
    {
        private static readonly PastPaperDetectService.CoverBoard Cambridge = new(6627, "Cambridge", "CIE", "{syllabusCode}/{paperVariant}/{seriesCode}/{yy}",
            [new(2195, "Physics", "0625", 22, "IGCSE", [("4", 6920, "Paper 4")])],
            [new("March", "February March", 3), new("May/Jun", "May June", 6), new("Oct/Nov", "October November", 11)],
            [("2", 7815, "2")]);

        private static readonly PastPaperDetectService.CoverBoard Necta = new(9001, "National Examinations Council of Tanzania", "NECTA", "{syllabusCode}/{paper} · {level} {year}",
            [new(9101, "Physics", "031", 9050, "CSEE (Form 4)", [("2", 9201, "Paper 2")])],
            [new("November", null, 11)],
            []);

        [Fact]
        public void ACambridgeCoverIsReadWithTheBoardsOwnData()
        {
            var found = PastPaperDetectService.ReadWithCurriculum(
                "Cambridge IGCSE™ PHYSICS 0625/42 Paper 4 Theory (Extended) May/June 2023 1 hour 15 minutes", [Cambridge, Necta], isMarkScheme: false)!;
            Assert.Equal(("curriculum", 6627, 22, 2195), (found.Source, found.Board!.Id, found.Grade!.Id, found.Subject!.Id));
            Assert.Equal((6920, 7815, 2023, 6), (found.Classification!.Id, found.Variant!.Id, found.Year, found.Month));
            Assert.Equal("0625/42", found.Code);
            Assert.Equal("Physics 0625/42 May June 2023", found.Title);
        }

        [Fact]
        public void ABoardEnteredOnlyAsDataIsReadToo()
        {
            var found = PastPaperDetectService.ReadWithCurriculum(
                "THE UNITED REPUBLIC OF TANZANIA NECTA CERTIFICATE OF SECONDARY EDUCATION EXAMINATION 031/2 PHYSICS 2 November 2022", [Cambridge, Necta], isMarkScheme: false)!;
            Assert.Equal((9001, 9101, 9201, 2022, 11), (found.Board!.Id, found.Subject!.Id, found.Classification!.Id, found.Year, found.Month));
            Assert.Null(found.Variant);
        }

        [Fact]
        public void APageOfNoKnownBoardOrCodeIsLeftToThePatternsAndTheAi()
        {
            Assert.Null(PastPaperDetectService.ReadWithCurriculum("Edexcel GCSE Mathematics 1MA1/1H June 2022", [Cambridge, Necta], false));
            Assert.Null(PastPaperDetectService.ReadWithCurriculum("Cambridge IGCSE Chemistry 0620/42 May/June 2023", [Cambridge], false));
            Assert.Null(PastPaperDetectService.CoverCode(Cambridge with { CodeTemplate = "{seriesCode}" }));
        }

        [Fact]
        public async Task TheReaderLoadsBoardsFromTheCopiedModel()
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
            using var provider = services.BuildServiceProvider();
            using var context = provider.GetRequiredService<TestApplicationDBContext>();
            CurriculumTestData.Seed(context);
            _ = await new CurriculumCopyService(new(provider.GetRequiredService<IUnitOfWorkProvider>), new(provider.GetRequiredService<IHttpContextAccessor>),
                new(() => NullLogger<CurriculumCopyService>.Instance), new(() => new PassThroughMigrationRouter()), new(() => new CurriculumCopyServiceTests.NoJobs())).CopyAsync();

            var boards = await PastPaperDetectService.LoadCoverBoardsAsync(provider.GetRequiredService<IUnitOfWorkProvider>().CreateUnitOfWork());
            var cambridge = Assert.Single(boards);
            var found = PastPaperDetectService.ReadWithCurriculum("Cambridge IGCSE PHYSICS 0625/42 Paper 4 May/June 2023", boards, false)!;
            Assert.Equal((cambridge.LegacyId, 2195, 6920, 7815, 6), (found.Board!.Id, found.Subject!.Id, found.Classification!.Id, found.Variant!.Id, found.Month));
        }
    }
}
