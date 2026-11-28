namespace GamaEdtech.Test.Curriculum
{
    using GamaEdtech.Data.Dto.Curriculum;
    using GamaEdtech.Domain.Enumeration;

    using Xunit;

    // Lists follow links own -> GAMA -> other boards, one hop through GAMA, exact links only, in the board's order.
    public sealed class CurriculumLinkRulesTests
    {
        private const int Gama = 9;
        private const int Cambridge = 1;
        private const int Edexcel = 2;
        private const int Aqa = 3;
        private const int Ib = 4;
        private const int Ocr = 5;

        // Cambridge C1 = GAMA G1 = Edexcel E1 = AQA A1 (exact), IB I1 partial; Edexcel E1 is also linked to GAMA G3, which
        // OCR X1 is linked to: one hop from Cambridge never reaches X1.
        private static readonly CurriculumLinkEdge[] Links =
        [
            new(101, 11, Cambridge, CurriculumLinkKind.Exact),
            new(101, 21, Edexcel, CurriculumLinkKind.Exact),
            new(101, 31, Aqa, CurriculumLinkKind.Exact),
            new(101, 41, Ib, CurriculumLinkKind.Partial),
            new(103, 21, Edexcel, CurriculumLinkKind.Exact),
            new(103, 51, Ocr, CurriculumLinkKind.Exact),
        ];

        private static readonly Dictionary<int, int> ListOrder = new() { [Cambridge] = 1, [Edexcel] = 2, [Aqa] = 3, [Ib] = 4, [Ocr] = 5, [Gama] = 0 };

        [Fact]
        public void PastPapersAreTheBoardsOwnByDefaultAndEveryOtherTabTakesAll()
        {
            Assert.Equal(CurriculumLinkScope.Own, CurriculumLinkRules.DefaultScope(CurriculumContentTab.PastPapers));
            Assert.All(CurriculumLinkRules.Tabs.Where(t => t != CurriculumContentTab.PastPapers),
                t => Assert.Equal(CurriculumLinkScope.All, CurriculumLinkRules.DefaultScope(t)));
        }

        [Fact]
        public void ATutorialListIsOwnThenGamaThenTheOtherBoardsInTheBoardsOrder()
        {
            var result = CurriculumLinkRules.Resolve(Cambridge, false, [11], Links, CurriculumLinkScope.All, [Aqa], ListOrder);

            Assert.Equal([11], result.Own);
            Assert.Equal([101], result.Master);

            // AQA first (the board's own order), then Edexcel by list order; OCR is two hops away and never comes.
            Assert.Equal([Aqa, Edexcel], result.OtherBoards.Select(t => t.BoardId));
            Assert.Equal([31], result.OtherBoards[0].TopicIds);
            Assert.DoesNotContain(result.OtherBoards, t => t.BoardId == Ocr);

            // IB's partial match is only related.
            Assert.Equal([41], result.Related);
        }

        [Fact]
        public void PastPapersStayTheBoardsOwnUnlessTheBoardTurnsThemOn()
        {
            var own = CurriculumLinkRules.Resolve(Cambridge, false, [11], Links, CurriculumLinkScope.Own, [], ListOrder);
            Assert.Equal([11], own.Own);
            Assert.Empty(own.Master);
            Assert.Empty(own.OtherBoards);

            var gamaToo = CurriculumLinkRules.Resolve(Cambridge, false, [11], Links, CurriculumLinkScope.OwnAndMaster, [], ListOrder);
            Assert.Equal([101], gamaToo.Master);
            Assert.Empty(gamaToo.OtherBoards);
        }

        [Fact]
        public void OnGamaItselfTheLinkedBoardsFollowItsOwnTopics()
        {
            var result = CurriculumLinkRules.Resolve(Gama, true, [101], Links, CurriculumLinkScope.All, [], ListOrder);
            Assert.Equal([101], result.Own);
            Assert.Empty(result.Master);
            Assert.Equal([Cambridge, Edexcel, Aqa], result.OtherBoards.Select(t => t.BoardId));
            Assert.Equal([41], result.Related);
        }

        [Fact]
        public void APartialLinkToGamaMakesEverythingBehindItRelatedOnly()
        {
            CurriculumLinkEdge[] links = [new(102, 12, Cambridge, CurriculumLinkKind.Partial), new(102, 22, Edexcel, CurriculumLinkKind.Exact)];
            var result = CurriculumLinkRules.Resolve(Cambridge, false, [12], links, CurriculumLinkScope.All, [], ListOrder);
            Assert.Empty(result.Master);
            Assert.Empty(result.OtherBoards);
            Assert.Equal([22, 102], result.Related);
        }
    }
}
