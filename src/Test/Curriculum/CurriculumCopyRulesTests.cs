namespace GamaEdtech.Test.Curriculum
{
    using GamaEdtech.Data.Dto.Curriculum;

    using Xunit;

    // How the copy reads gama-api's lookup lists: codes in titles, parents, the flat topic list, sessions, slugs.
    public sealed class CurriculumCopyRulesTests
    {
        [Theory]
        [InlineData("Mathematics (0580)", null, "Mathematics", "0580")]
        [InlineData(" Physics (9702) ", "{\"code\":\"\"}", "Physics", "9702")]
        [InlineData("Biology 1", "{\"code\":\"\"}", "Biology 1", null)]
        [InlineData("Mathematics A (4MA1)", "{\"code\":\"4MA1\"}", "Mathematics A", "4MA1")]
        [InlineData("English (as a second language)", null, "English (as a second language)", null)]
        [InlineData("Chemistry", "not json", "Chemistry", null)]
        public void ASubjectsCodeComesFromItsMetadataOrTitle(string title, string? metadata, string name, string? code) =>
            Assert.Equal((name, code), CurriculumCopyRules.Syllabus(title, metadata));

        [Fact]
        public void ParentsAreReadAsGamaApiWritesThem()
        {
            Assert.Equal([6627, 6657], CurriculumCopyRules.Parents("6627+6657", false));
            Assert.Equal([22], CurriculumCopyRules.Parents("0,22", true));
            Assert.Equal([22, 23], CurriculumCopyRules.Parents("0,22+0,23+0,22", true));
            Assert.Empty(CurriculumCopyRules.Parents("", false));
            Assert.Empty(CurriculumCopyRules.Parents("x+0", false));
        }

        [Fact]
        public void ChaptersBelongToTheUnitBeforeThem()
        {
            var units = CurriculumCopyRules.TopicUnits(
            [
                new(5, 3, false),
                new(1, 1, false),
                new(2, 2, true),
                new(4, 5, true),
                new(3, 4, false),
                new(6, 6, false),
            ]);

            // Topic 1 comes before any unit, so it stays at the top, as do the units themselves.
            Assert.Null(units[1]);
            Assert.Null(units[2]);
            Assert.Equal(2, units[5]);
            Assert.Equal(2, units[3]);
            Assert.Null(units[4]);
            Assert.Equal(4, units[6]);
        }

        [Fact]
        public void ALevelsSessionsAreTheWebsitesWithCambridgesCodes()
        {
            var igcse = CurriculumCopyRules.Sessions(22, true, []);
            Assert.Equal(["March", "May/Jun", "Oct/Nov"], igcse.Select(t => t.Name));
            Assert.Equal(("M/J", "s", "May June"), (igcse[1].Code, igcse[1].FileCode, igcse[1].SeoName));

            // A level the website had no list for gets one session per month its papers were sat in.
            var other = CurriculumCopyRules.Sessions(99999, false, [11, 6, 6, 0]);
            Assert.Equal(["June", "November"], other.Select(t => t.Name));
            Assert.All(other, t => Assert.Null(t.Code));
        }

        [Fact]
        public void SlugsAreReadableAndUnique()
        {
            Assert.Equal("as-a-level", CurriculumCopyRules.Slug(" AS & A Level", "x"));
            Assert.Equal("9-1-igcse", CurriculumCopyRules.Slug("9–1 IGCSE", "x"));
            Assert.Equal("francais", CurriculumCopyRules.Slug("Français", "x"));
            Assert.Equal("board-1", CurriculumCopyRules.Slug("کنکور", "board-1"));

            HashSet<string> taken = ["igcse"];
            Assert.Equal("igcse-2", CurriculumCopyRules.UniqueSlug("igcse", taken));
            Assert.Equal("igcse-3", CurriculumCopyRules.UniqueSlug("igcse", taken));
        }

        [Fact]
        public void PapersAndFilesGetTheWebsitesShortCodes()
        {
            Assert.Equal("4", CurriculumCopyRules.PaperShortLabel("Paper 4"));
            Assert.Equal("Multiple Choice", CurriculumCopyRules.PaperShortLabel(" Multiple Choice"));
            Assert.Equal(("in", "IN"), CurriculumCopyRules.DocumentCode("Insert"));
            Assert.Equal(("ci", "CI"), CurriculumCopyRules.DocumentCode("Confidental Instructions"));
            Assert.Equal(("solved_paper", null), CurriculumCopyRules.DocumentCode("Solved Paper"));
            Assert.Equal(("doc_3d_model", null), CurriculumCopyRules.DocumentCode("3D model"));
        }
    }
}
