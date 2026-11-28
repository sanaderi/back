namespace GamaEdtech.Test.Curriculum
{
    using GamaEdtech.Data.Dto.Taxonomy;
    using GamaEdtech.Domain.Entity;
    using GamaEdtech.Domain.Enumeration;

    using TestApplicationDBContext = GamaEdtech.Test.Application.ExamAnalysisServiceTests.TestApplicationDBContext;

    // A small copy of the lookup lists: Cambridge IGCSE Physics (0625) with two units, GAMA Physics, a GAMA - Cambridge
    // topic link, two papers (Paper 4 variant 2 in May/June 2023 with an insert, and a worksheet) and a school.
    internal static class CurriculumTestData
    {
        internal static void Seed(TestApplicationDBContext context)
        {
            var now = DateTimeOffset.UtcNow;
            LegacyTaxonomyItem Item(int id, short kind, string title, int order, string parent = "", bool? season = null, bool? isPaper = null) =>
                new() { Id = id, Kind = kind, Title = title, ListOrder = order, Parent = parent, IsSeason = season, IsPaper = isPaper, SyncDate = now };

            context.Set<LegacyTaxonomyItem>().AddRange(
                Item(6627, LegacyTaxonomyKinds.Board, "Cambridge", 0),
                Item(7131, LegacyTaxonomyKinds.Board, "GAMA", 5),
                Item(22, LegacyTaxonomyKinds.Grade, "IGCSE", 2, "6627"),
                Item(7132, LegacyTaxonomyKinds.Grade, "High School", 1, "7131"),
                Item(99, LegacyTaxonomyKinds.Grade, "Shared", 9, "6627+7131"),
                Item(2195, LegacyTaxonomyKinds.Subject, "Physics (0625)", 1, "0,22"),
                Item(7133, LegacyTaxonomyKinds.Subject, "Physics", 1, "0,7132"),
                Item(10, LegacyTaxonomyKinds.Topic, "Unit 1", 1, "2195", season: true),
                Item(11, LegacyTaxonomyKinds.Topic, " Chapter 1", 2, "2195", season: false),
                Item(12, LegacyTaxonomyKinds.Topic, " Chapter 2", 3, "2195", season: false),
                Item(13, LegacyTaxonomyKinds.Topic, "Unit 2", 4, "2195", season: true),
                Item(14, LegacyTaxonomyKinds.Topic, " Chapter 3", 5, "2195", season: false),
                Item(20, LegacyTaxonomyKinds.Topic, "Mechanics", 1, "7133", season: true),
                Item(21, LegacyTaxonomyKinds.Topic, "Motion", 2, "7133", season: false),
                Item(6920, LegacyTaxonomyKinds.Classification, "Paper 4", 4, "6627", isPaper: true),
                Item(310, LegacyTaxonomyKinds.Classification, "Worksheets", 0, "6627+7131", isPaper: false),
                Item(7813, LegacyTaxonomyKinds.Variant, "nv", 1),
                Item(7815, LegacyTaxonomyKinds.Variant, "2", 3),
                Item(6351, LegacyTaxonomyKinds.ExtraFileType, "Insert", 1));
            context.Set<LegacyTaxonomyRelation>().AddRange(
                new LegacyTaxonomyRelation { Id = 1, FromId = 21, ToId = 11, SyncDate = now },
                new LegacyTaxonomyRelation { Id = 2, FromId = 11, ToId = 12, SyncDate = now },
                new LegacyTaxonomyRelation { Id = 3, FromId = 2195, ToId = 7133, SyncDate = now });
            context.Set<PastPaper>().AddRange(
                new PastPaper { Id = 1, Title = "Physics (0625) Paper 4", CoreBoardId = 6627, CoreGradeId = 22, CoreSubjectId = 2195, CorePaperTypeId = 6920, Variant = 7815, CoreTopicIds = "10+11", EduMonth = 6, EduYear = 2023, IsPaper = true, Status = PastPaperStatus.Confirmed, CreationDate = now, SyncDate = now, Files = [new() { Kind = PastPaperFileKind.Question, Path = "q.pdf", Extension = "pdf" }, new() { Kind = PastPaperFileKind.Answer, Path = "a.pdf", Extension = "pdf" }, new() { Kind = PastPaperFileKind.Extra, Path = "i.pdf", CoreTypeId = 6351, Extension = "pdf" }] },
                new PastPaper { Id = 2, Title = "Worksheet", CoreBoardId = 6627, CoreGradeId = 22, CoreSubjectId = 2195, CorePaperTypeId = 310, Variant = 7813, CreationDate = now, SyncDate = now });
            _ = context.Set<School>().Add(new School { Id = 5, Name = "Al-Noor", CreationDate = now, CreationUserId = 1 });
            _ = context.Set<Board>().Add(new Board { Id = 3, Title = "Cambridge", Code = 6627, CreationDate = now, CreationUserId = 1 });
            _ = context.Set<SchoolBoard>().Add(new SchoolBoard { Id = 8, SchoolId = 5, BoardId = 3, CreationDate = now, CreationUserId = 1 });
            _ = context.SaveChanges();
        }
    }
}
