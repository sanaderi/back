namespace GamaEdtech.Data.Dto.Curriculum
{
    using System.Collections.Generic;

    /// <summary>
    /// A public answer of the curriculum model. <see cref="Native"/> is <c>Migration:Routes:Curriculum.Website</c> being
    /// Native: the website uses <see cref="Data"/> only then and keeps its own code otherwise, so the backend's route is the
    /// one switch for both (Shadow sends the data with false, for checking).
    /// </summary>
    public sealed class CurriculumAnswerDto<T>
    {
        public bool Native { get; set; }
        public T? Data { get; set; }
    }

    /// <summary>A board, level, subject or topic as the website lists it. <see cref="LegacyId"/> is the id today's
    /// search and forms take.</summary>
    public sealed class CurriculumPublicItemDto
    {
        public int Id { get; set; }
        public int? LegacyId { get; set; }
        public string? Slug { get; set; }
        public string? Name { get; set; }
        public string? ShortName { get; set; }
        public string? Code { get; set; }
        public string? Logo { get; set; }
        public string? CountryCode { get; set; }
        public int? ParentId { get; set; }
        public bool IsUnit { get; set; }
        public bool IsMaster { get; set; }
        public bool IsDefault { get; set; }
        public int ListOrder { get; set; }
    }

    /// <summary>One option of a filter: <see cref="Value"/> is what the search's <c>param</c> takes, with how many papers
    /// it has (options with none are left out).</summary>
    public sealed class CurriculumPublicOptionDto
    {
        public required string Value { get; set; }
        public required string Label { get; set; }
        public int Count { get; set; }
    }

    /// <summary>One filter of a board's panel, with the search parameter it sets.</summary>
    public sealed class CurriculumPublicFilterDto
    {
        public required string Key { get; set; }
        public required string Param { get; set; }
        public required string Label { get; set; }
        public required string Style { get; set; }
        public IReadOnlyList<CurriculumPublicOptionDto> Options { get; set; } = [];
    }

    /// <summary>A school teaching a board, for the board pages' "schools near you".</summary>
    public sealed class CurriculumPublicSchoolDto
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public int? CityId { get; set; }
        public string? City { get; set; }
    }

    /// <summary>A board, qualification or syllabus landing page: where it is, what is under it, its SEO texts, its paper
    /// count and schools.</summary>
    public sealed class CurriculumLandingDto
    {
        public required CurriculumPublicItemDto Board { get; set; }
        public CurriculumPublicItemDto? Level { get; set; }
        public CurriculumPublicItemDto? Subject { get; set; }

        /// <summary>The levels of a board, the subjects of a level, the topics of a subject.</summary>
        public IReadOnlyList<CurriculumPublicItemDto> Children { get; set; } = [];
        public int PaperCount { get; set; }
        public CurriculumSeoResultDto? Seo { get; set; }
        public IReadOnlyList<CurriculumPublicSchoolDto> Schools { get; set; } = [];
    }
}
