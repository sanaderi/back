namespace GamaEdtech.Test.Curriculum
{
    using GamaEdtech.Data.Dto.Curriculum;
    using GamaEdtech.Domain.Enumeration;

    using Xunit;

    // The SEO templates write what the website wrote before they moved to the backend (useSearchMetadata, useCambridgeSeo,
    // the exam and tutorial pages), and the template syntax behaves: optional parts, alternatives, no gaps.
    public sealed class CurriculumTemplateTests
    {
        private static CurriculumSeoText Default(CurriculumSeoPage page) => CurriculumSeoDefaults.All.Single(t => t.Page == page);

        [Fact]
        public void AListTitleNamesTheChosenFiltersAndFallsBackWithoutABoard()
        {
            var template = Default(CurriculumSeoPage.PastPaperList);
            Assert.Equal("Cambridge IGCSE Mathematics (0580) 2024 Past Papers", CurriculumTemplate.Render(template.Title, new Dictionary<string, string?>
            {
                ["board"] = "Cambridge",
                ["qualification"] = "IGCSE",
                ["subject"] = "Mathematics (0580)",
                ["year"] = "2024",
            }));
            Assert.Equal("Download list of Cambridge IGCSE Past Papers. Includes mark scheme for exam preparation.", CurriculumTemplate.Render(template.Description,
                new Dictionary<string, string?> { ["board"] = "Cambridge", ["qualification"] = "IGCSE" }));

            Assert.Equal("Educational Resources | K12 Education Papers and Materials", CurriculumTemplate.Render(template.Title, new Dictionary<string, string?>()));
        }

        [Fact]
        public void CambridgePaperTextsMatchTheWebsite()
        {
            Dictionary<string, string?> values = new()
            {
                ["board"] = "Cambridge",
                ["qualification"] = "IGCSE",
                ["title"] = "Mathematics (0580) Paper 4",
                ["subject"] = "Mathematics",
                ["syllabusCode"] = "0580",
                ["paper"] = "4",
                ["paperVariant"] = "42",
                ["series"] = "May June",
                ["seriesCode"] = "M/J",
                ["year"] = "2023",
                ["yy"] = "23",
            };
            values["code"] = CurriculumTemplate.Render(CurriculumSeoDefaults.CambridgeCode, values);
            Assert.Equal("0580/42/M/J/23", values["code"]);

            var paper = CurriculumSeoDefaults.CambridgePastPaper;
            Assert.Equal("Cambridge IGCSE Mathematics (0580) Paper 4 past paper 0580/42/M/J/23", CurriculumTemplate.Render(paper.Title, values));
            Assert.Equal(
                "Download Cambridge IGCSE Mathematics 0580/42 May June 2023 question paper(QP) with mark scheme (MS) pdf. Access paper 4, 0580/42/M/J/23 question paper pdf with answers for your upcoming exam series preparation.",
                CurriculumTemplate.Render(paper.Description, values));

            values["files"] = "question paper (QP) 0580_s23_qp_42, mark scheme (MS) 0580_s23_ms_42";
            Assert.Equal(
                "Download 0580/42/M/J/23 Cambridge IGCSE Mathematics 0580/42 Paper 4 May June 2023 past paper. In this page you can find question paper (QP) 0580_s23_qp_42, mark scheme (MS) 0580_s23_ms_42 pdfs available for your upcoming exam series preparation.",
                CurriculumTemplate.Render(paper.Description, values));
        }

        [Fact]
        public void ExamTextsMatchTheWebsite()
        {
            var exam = Default(CurriculumSeoPage.Exam);
            Dictionary<string, string?> values = new()
            {
                ["subject"] = "Biology 0610/12 May June 2024",
                ["kind"] = "Past Paper",
                ["format"] = "Free Online Test with Answers",
                ["facts"] = "40 questions, 45 min",
                ["cta"] = "Take it free online for your score, rank and the correct answers.",
            };
            Assert.Equal("Biology 0610/12 May June 2024 Past Paper – Free Online Test with Answers", CurriculumTemplate.Render(exam.Title, values));
            Assert.Equal(
                "Biology 0610/12 May June 2024 Past Paper: 40 questions, 45 min. Take it free online for your score, rank and the correct answers. PDF, Word and PowerPoint too.",
                CurriculumTemplate.Render(exam.Description, values));

            // A practice quiz with no facts drops the kind and the colon.
            values["kind"] = null;
            values["facts"] = null;
            Assert.Equal("Biology 0610/12 May June 2024. Take it free online for your score, rank and the correct answers. PDF, Word and PowerPoint too.",
                CurriculumTemplate.Render(exam.Description, values));
        }

        [Fact]
        public void GamaTutorialsUseTheirOwnTitle()
        {
            Dictionary<string, string?> values = new() { ["board"] = "GAMA", ["qualification"] = "Grade 10", ["title"] = "Differentiation" };
            Assert.Equal("Differentiation", CurriculumTemplate.Render(CurriculumSeoDefaults.GamaTutorial.Title, values));
            Assert.Equal("GAMA Grade 10 Differentiation", CurriculumTemplate.Render(Default(CurriculumSeoPage.Tutorial).Title, values));
            Assert.Equal("Learn Differentiation with step-by-step explanations and examples from the Grade 10 GAMA curriculum.",
                CurriculumTemplate.Render(Default(CurriculumSeoPage.Tutorial).Description, values));
        }

        [Fact]
        public void TheSyntaxDropsEmptyPartsAndUsesTheFirstCompleteAlternative()
        {
            Dictionary<string, string?> values = new() { ["a"] = "A", ["empty"] = " " };
            Assert.Equal("A", CurriculumTemplate.Render("{a}[ {empty}]", values));
            Assert.Equal("A.", CurriculumTemplate.Render("{a} [{empty}] .", values));
            Assert.Equal("fallback", CurriculumTemplate.Render("{empty} x || {missing} || fallback", values));
            Assert.Null(CurriculumTemplate.Render("{missing}", values));
            Assert.Equal("[A", CurriculumTemplate.Render("[{a}", values));
            Assert.Equal(new HashSet<string> { "a", "b" }, CurriculumTemplate.Placeholders("{a} [{b}] || {a}"));
        }

        [Fact]
        public void ARepeatedPhraseIsSaidOnce()
        {
            Dictionary<string, string?> values = new() { ["board"] = "Cambridge", ["qualification"] = "IGCSE", ["title"] = "Cambridge IGCSE Physics 0625" };
            Assert.Equal("Cambridge IGCSE Physics 0625 past paper", CurriculumTemplate.Render(Default(CurriculumSeoPage.PastPaper).Title, values));
            Assert.Equal("Paper 1 paper", CurriculumTemplate.Render("{a} {a} paper", new Dictionary<string, string?> { ["a"] = "Paper 1" }));
        }

        [Fact]
        public void TheLandingPagesFitSearchResults()
        {
            Dictionary<string, string?> values = new()
            {
                ["board"] = "Cambridge",
                ["qualification"] = "IGCSE",
                ["subject"] = "Mathematics",
                ["syllabusCode"] = "0580",
                ["yearFrom"] = "2013",
                ["yearTo"] = "2025",
                ["qualifications"] = "IGCSE, O Level and AS & A Level",
                ["city"] = "Lahore",
            };
            foreach (var page in new[] { CurriculumSeoPage.BoardPage, CurriculumSeoPage.QualificationPage, CurriculumSeoPage.SyllabusPage })
            {
                var text = Default(page);
                Assert.Empty(CurriculumTemplate.Warnings(CurriculumTemplate.Render(text.Title, values), CurriculumTemplate.Render(text.Description, values)));
            }

            Assert.Equal(["titleLong", "descriptionShort"], CurriculumTemplate.Warnings(new string('t', 61), "short"));
        }

        [Fact]
        public void ALongDescriptionIsCutAtAWord()
        {
            var cut = CurriculumTemplate.Cut(string.Join(' ', Enumerable.Repeat("word,", 60)), 30);
            Assert.Equal("word, word, word, word…", cut);
            Assert.True(cut.Length <= 30);
            Assert.Equal("short", CurriculumTemplate.Cut("short", 30));
        }
    }
}
