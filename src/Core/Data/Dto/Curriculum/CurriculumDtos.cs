namespace GamaEdtech.Data.Dto.Curriculum
{
    using System;
    using System.Collections.Generic;

    using GamaEdtech.Domain.Enumeration;

    /// <summary>The filters a board's panel can show (<c>CurriculumBoardFilters.Key</c>). A variant filter is
    /// <c>variant.{kind}</c>, one per variant kind.</summary>
    public static class CurriculumFilterKeys
    {
        public const string Level = "level";
        public const string Subject = "subject";
        public const string Year = "year";
        public const string Series = "series";
        public const string Paper = "paper";
        public const string Tier = "tier";
        public const string Variant = "variant";
        public const string Language = "language";

        public static readonly IReadOnlyList<string> All = [Level, Subject, Year, Series, Paper, Tier, Variant, Language];

        /// <summary>A known key, or <c>variant.{kind}</c>.</summary>
        public static bool IsKnown(string? key) => key is not null
            && (All.Contains(key) || (key.StartsWith(Variant + ".", StringComparison.Ordinal) && key.Length > Variant.Length + 1));
    }

    public sealed class CurriculumBoardDto
    {
        public int Id { get; set; }
        public int? LegacyId { get; set; }
        public string? Slug { get; set; }
        public string? Name { get; set; }
        public string? ShortName { get; set; }
        public string? CountryCode { get; set; }
        public int? ParentBoardId { get; set; }
        public string? Logo { get; set; }
        public string? Website { get; set; }
        public bool IsMaster { get; set; }
        public bool IsDefault { get; set; }
        public bool IsEnabled { get; set; }
        public int ListOrder { get; set; }
        public string? CodeTemplate { get; set; }
        public int? LanguageId { get; set; }
    }

    public sealed class CurriculumBoardFilterDto
    {
        public string? Key { get; set; }
        public string? Label { get; set; }
        public CurriculumFilterStyle? Style { get; set; }
        public int ListOrder { get; set; }
        public bool IsEnabled { get; set; }
    }

    /// <summary>A tab's scope; <see cref="IsDefault"/> when the board has not set it.</summary>
    public sealed class CurriculumBoardTabDto
    {
        public required CurriculumContentTab Tab { get; set; }
        public required CurriculumLinkScope Scope { get; set; }
        public bool IsDefault { get; set; }
    }

    public sealed class CurriculumSeriesTypeDto
    {
        public int Id { get; set; }
        public int? QualificationId { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? FileCode { get; set; }
        public string? Months { get; set; }
        public bool IsSupplementary { get; set; }
        public int ListOrder { get; set; }
    }

    public sealed class CurriculumVariantOptionDto
    {
        public int Id { get; set; }
        public int? LegacyId { get; set; }
        public string? Label { get; set; }
        public string? Code { get; set; }
        public int ListOrder { get; set; }
    }

    public sealed class CurriculumVariantKindDto
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public string? Label { get; set; }
        public int ListOrder { get; set; }
        public IReadOnlyList<CurriculumVariantOptionDto> Options { get; set; } = [];
    }

    public sealed class CurriculumSeoTemplateDto
    {
        public int Id { get; set; }

        /// <summary>Null for the default of every board.</summary>
        public int? BoardId { get; set; }
        public CurriculumSeoPage? Page { get; set; }
        public string? TitleTemplate { get; set; }
        public string? DescriptionTemplate { get; set; }
    }

    /// <summary>A board with its whole profile, as the admin's board screen shows it.</summary>
    public sealed class CurriculumBoardDetailDto
    {
        public required CurriculumBoardDto Board { get; set; }
        public IReadOnlyList<CurriculumBoardFilterDto> Filters { get; set; } = [];

        /// <summary>Every tab, with its default when the board has not set one.</summary>
        public IReadOnlyList<CurriculumBoardTabDto> Tabs { get; set; } = [];

        /// <summary>The other boards in the order their items come, after the board's own and GAMA's.</summary>
        public IReadOnlyList<int> LinkOrder { get; set; } = [];
        public IReadOnlyList<CurriculumSeriesTypeDto> SeriesTypes { get; set; } = [];
        public IReadOnlyList<CurriculumVariantKindDto> VariantKinds { get; set; } = [];

        /// <summary>The board's own SEO templates (the defaults apply to the pages it has none for).</summary>
        public IReadOnlyList<CurriculumSeoTemplateDto> SeoTemplates { get; set; } = [];
    }

    /// <summary>A board's tab scopes and its order of the other boards, saved together.</summary>
    public sealed class CurriculumBoardLinksDto
    {
        public IReadOnlyList<CurriculumBoardTabDto> Tabs { get; set; } = [];
        public IReadOnlyList<int> LinkOrder { get; set; } = [];
    }

    public sealed class CurriculumQualificationDto
    {
        public int Id { get; set; }
        public int? LegacyId { get; set; }
        public int BoardId { get; set; }
        public string? Slug { get; set; }
        public string? Name { get; set; }
        public string? ShortName { get; set; }
        public int? LanguageId { get; set; }
        public int ListOrder { get; set; }
    }

    public sealed class CurriculumSubjectDto
    {
        public int Id { get; set; }
        public string? Slug { get; set; }
        public string? Name { get; set; }
        public string? Picture { get; set; }
        public int ListOrder { get; set; }
    }

    public sealed class CurriculumSyllabusDto
    {
        public int Id { get; set; }
        public int? LegacyId { get; set; }
        public int QualificationId { get; set; }
        public int? SubjectId { get; set; }
        public string? Code { get; set; }
        public string? Slug { get; set; }
        public string? Name { get; set; }
        public short? YearFrom { get; set; }
        public short? YearTo { get; set; }
        public string? Picture { get; set; }
        public int ListOrder { get; set; }
    }

    public sealed class CurriculumPaperDto
    {
        public int Id { get; set; }
        public int? LegacyId { get; set; }
        public string? Name { get; set; }
        public string? ShortLabel { get; set; }
        public string? Tier { get; set; }
        public int ListOrder { get; set; }
    }

    public sealed class CurriculumDocumentTypeDto
    {
        public int Id { get; set; }
        public int? LegacyId { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? ShortLabel { get; set; }
        public string? Icon { get; set; }
        public int ListOrder { get; set; }
    }

    public sealed class CurriculumTopicDto
    {
        public int Id { get; set; }
        public int? LegacyId { get; set; }
        public int SyllabusId { get; set; }
        public int? ParentTopicId { get; set; }
        public string? Name { get; set; }
        public bool IsUnit { get; set; }
        public int ListOrder { get; set; }
    }

    /// <summary>A link as the admin sees it: both topics with their syllabus and board.</summary>
    public sealed class CurriculumTopicLinkDto
    {
        public int Id { get; set; }
        public int MasterTopicId { get; set; }
        public int TopicId { get; set; }
        public required CurriculumLinkKind Kind { get; set; }
        public string? MasterTopicName { get; set; }
        public string? TopicName { get; set; }
        public int TopicBoardId { get; set; }
        public string? TopicBoardName { get; set; }
        public string? TopicSyllabusName { get; set; }
        public int? LegacyRelationId { get; set; }
        public DateTimeOffset CreationDate { get; set; }
    }

    public sealed class CurriculumTopicLinkAddDto
    {
        public int MasterTopicId { get; set; }
        public int TopicId { get; set; }
        public CurriculumLinkKind? Kind { get; set; }
        public long? UserId { get; set; }
    }

    /// <summary>One option of a panel filter.</summary>
    public sealed class CurriculumFilterOptionDto
    {
        public required string Value { get; set; }
        public required string Label { get; set; }
    }

    /// <summary>One filter as the website's panel draws it.</summary>
    public sealed class CurriculumPanelFilterDto
    {
        public required string Key { get; set; }
        public required string Label { get; set; }
        public required CurriculumFilterStyle Style { get; set; }

        /// <summary>The options the model knows (levels, subjects, sessions, papers, variants). Years and counts come
        /// with the papers (phase 3).</summary>
        public IReadOnlyList<CurriculumFilterOptionDto> Options { get; set; } = [];
    }

    /// <summary>The topics a board's tab brings in for one of its topics, grouped and in order, with names.</summary>
    public sealed class CurriculumLinkedTopicsDto
    {
        public required CurriculumContentTab Tab { get; set; }
        public required CurriculumLinkScope Scope { get; set; }
        public IReadOnlyList<CurriculumLinkedGroupDto> Groups { get; set; } = [];
        public IReadOnlyList<CurriculumLinkedTopicDto> Related { get; set; } = [];
    }

    /// <summary>One group of a linked list: <c>own</c>, <c>gama</c> or <c>otherBoard</c>, which the website shows as is.</summary>
    public sealed class CurriculumLinkedGroupDto
    {
        public required string Source { get; set; }
        public int BoardId { get; set; }
        public string? BoardName { get; set; }
        public IReadOnlyList<CurriculumLinkedTopicDto> Topics { get; set; } = [];
    }

    public sealed class CurriculumLinkedTopicDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? LegacyId { get; set; }
    }

    /// <summary>A page's title and description written from its templates.</summary>
    public sealed class CurriculumSeoResultDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }

        /// <summary>What search results would cut or find thin (<c>titleLong</c>, <c>descriptionLong</c>,
        /// <c>descriptionShort</c>, <c>missing</c>), for the admin's preview.</summary>
        public IReadOnlyList<string> Warnings { get; set; } = [];
    }

    /// <summary>A link group's source, as the API sends it.</summary>
    public static class CurriculumLinkSource
    {
        public const string Own = "own";
        public const string Master = "gama";
        public const string OtherBoard = "otherBoard";
    }
}
