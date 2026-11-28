namespace GamaEdtech.Data.Dto.Curriculum
{
    using System.Collections.Generic;

    using GamaEdtech.Domain.Enumeration;

    /// <summary>One page kind's title and description templates (see <see cref="CurriculumTemplate"/>).</summary>
    public sealed record CurriculumSeoText(CurriculumSeoPage Page, string Title, string Description);

    /// <summary>
    /// The SEO templates every board starts with: the website's own texts (useSearchMetadata, the paper, tutorial and exam
    /// pages, 2026-11-28) written as templates, so moving them to the backend changes no page. The migration that adds the
    /// curriculum tables stores these as the default rows (no board); an admin edits them from there.
    /// <para>The values the callers fill: <c>board</c>, <c>qualification</c>, <c>subject</c>, <c>classification</c>,
    /// <c>series</c>, <c>year</c>, <c>title</c> on every page; on a paper also <c>code</c> (the board's paper code),
    /// <c>syllabusCode</c>, <c>paper</c>, <c>paperVariant</c>, <c>seriesCode</c>, <c>yy</c> and <c>files</c> (its file
    /// list); on an exam <c>kind</c> ("Past Paper"), <c>format</c>, <c>facts</c> and <c>cta</c>; on the landing pages
    /// <c>qualifications</c>, <c>yearFrom</c>, <c>yearTo</c> and <c>city</c>.</para>
    /// </summary>
    public static class CurriculumSeoDefaults
    {
        private const string Filters = "{board}[ {qualification}][ {subject}][ {classification}][ {series}][ {year}]";
        private const string PaperPrep = "Includes mark scheme for exam preparation.";
        private const string StudyPrep = "Useful for study, practice, and exam preparation.";

        public static readonly IReadOnlyList<CurriculumSeoText> All =
        [
            new(CurriculumSeoPage.PastPaperList,
                $"{Filters} Past Papers || Educational Resources | K12 Education Papers and Materials",
                $"Download list of {Filters} Past Papers. {PaperPrep} || Enhance your learning with GamaTrain's extensive collection of online documents and texts, carefully curated to enrich your academic journey."),
            new(CurriculumSeoPage.StudyMaterialList,
                $"{Filters} Study Materials || Educational Resources | K12 Education Papers and Materials",
                $"Download list of {Filters}. {StudyPrep} || Enhance your learning with GamaTrain's extensive collection of online documents and texts, carefully curated to enrich your academic journey."),
            new(CurriculumSeoPage.ExamList,
                $"{Filters} Exams || Online Exams, Free Exams for Improving Education",
                $"Download list of {Filters} Online test. {StudyPrep} || Hone your skills and assess your knowledge with GamaTrain's online exams, designed to enhance your exam preparation and boost your confidence."),
            new(CurriculumSeoPage.TutorialList,
                $"{Filters} Revision notes || Master Concepts, Enhance Learning: GamaTrain's Online Tutorials",
                $"Download list of {Filters} Textbook. {StudyPrep} || Complement your studies with GamaTrain's comprehensive online tutorials, providing step-by-step guidance and practice opportunities to refine your understanding."),
            new(CurriculumSeoPage.PastPaper,
                "[{board} ][{qualification} ]{title} past paper",
                "Download [{board} ][{qualification} ]{title} past paper with mark scheme (MS). Access a full collection of past papers for study, revision, and exam practice."),
            new(CurriculumSeoPage.StudyMaterial,
                "[{board} ][{qualification} ]{title}",
                "Download {title} – {qualification}, {board} curriculum. Ideal for quick revision, practice, and exam prep. || Download {title}. Ideal for quick revision, practice, and exam prep."),
            new(CurriculumSeoPage.Exam,
                "{subject}[ {kind}] – {format}",
                "{subject}[ {kind}][: {facts}]. {cta} PDF, Word and PowerPoint too."),
            new(CurriculumSeoPage.Tutorial,
                "[{board} ][{qualification} ]{title}",
                "Learn {title} with step-by-step explanations and examples from the {qualification} {board} curriculum. || Learn {title} with step-by-step explanations and examples."),

            // The landing pages (phase 4) are new: written for them, short enough for search results, with the city
            // when the visitor's is known so the school lists rank locally.
            new(CurriculumSeoPage.BoardPage,
                "{board} Past Papers, Exams and Revision Notes",
                "Free {board} past papers with mark schemes, online exams and revision notes[ for {qualifications}]. Find {board} schools[ in {city}]."),
            new(CurriculumSeoPage.QualificationPage,
                "{board} {qualification} Past Papers and Revision Notes",
                "{board} {qualification} past papers with mark schemes, online exams and revision notes for every subject. Find schools offering {qualification}[ in {city}]."),
            new(CurriculumSeoPage.SyllabusPage,
                "{board} {qualification} {subject}[ {syllabusCode}] Past Papers",
                "{board} {qualification} {subject}[ {syllabusCode}] past papers with mark schemes[ {yearFrom}–{yearTo}], revision notes by topic and online exams. Find schools teaching it[ in {city}]."),
        ];

        /// <summary>Cambridge's own past paper texts (the website's useCambridgeSeo), attached to the Cambridge board when its
        /// data is copied in (phase 2). The first description lists the paper's files, the second is for a paper with none.</summary>
        public static readonly CurriculumSeoText CambridgePastPaper = new(CurriculumSeoPage.PastPaper,
            "[{board} ][{qualification} ]{title} past paper {code} || [{board} ][{qualification} ]{title} past paper",
            "Download {code} Cambridge {qualification} {subject} {syllabusCode}/{paperVariant} Paper {paper} {series} {year} past paper. In this page you can find {files} pdfs available for your upcoming exam series preparation. "
            + "|| Download Cambridge {qualification} {subject} {syllabusCode}/{paperVariant} {series} {year} question paper(QP) with mark scheme (MS) pdf. Access paper {paper}, {code} question paper pdf with answers for your upcoming exam series preparation. "
            + "|| Download [{board} ][{qualification} ]{title} past paper with mark scheme (MS). Access a full collection of past papers for study, revision, and exam practice.");

        /// <summary>Cambridge's paper code (0580/42/M/J/23), the board's <c>CodeTemplate</c> once copied in.</summary>
        public const string CambridgeCode = "{syllabusCode}/{paperVariant}/{seriesCode}/{yy}";

        /// <summary>GAMA's tutorials are titled by their own title only (the website's <c>isGamaBoard</c>).</summary>
        public static readonly CurriculumSeoText GamaTutorial = new(CurriculumSeoPage.Tutorial,
            "{title}",
            "Learn {title} with step-by-step explanations and examples from the {qualification} {board} curriculum. || Learn {title} with step-by-step explanations and examples.");
    }
}
