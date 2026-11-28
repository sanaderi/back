namespace GamaEdtech.Domain.Entity.Curriculum
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Entities;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.DataAnnotation.Schema;
    using GamaEdtech.Common.Localization;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// An exam board (Cambridge, IB, NECTA, GAMA...): its names, country, logo and the settings that make its filter panel,
    /// paper codes and links (its profile: <see cref="CurriculumBoardFilter"/>, <see cref="CurriculumBoardTab"/>,
    /// <see cref="CurriculumBoardLinkOrder"/>, <see cref="CurriculumSeoTemplate"/>). Names in other languages are
    /// <c>ContentLocalizations</c> with the content type <c>CurriculumBoard</c>. See docs/business/curriculum.md.
    /// </summary>
    [Table(nameof(CurriculumBoard))]
    public class CurriculumBoard : IEntity<CurriculumBoard, int>, IContentLocalizeable
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column(nameof(Id), DataType.Int)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        /// <summary>The copied lookup item's id (<c>LegacyTaxonomyItems</c>, a board), so old links such as <c>?board=6627</c> keep working; null for a board added here.</summary>
        [Column(nameof(LegacyId), DataType.Int)]
        public int? LegacyId { get; set; }

        /// <summary>In the URLs (<c>/boards/cambridge</c>): lower case, digits and dashes.</summary>
        [Column(nameof(Slug), DataType.String)]
        [StringLength(100)]
        [Required]
        public string? Slug { get; set; }

        [Column(nameof(Name), DataType.UnicodeString)]
        [StringLength(200)]
        [Required]
        public string? Name { get; set; }

        /// <summary>Shown in the filter panel's logo box (CIE, IB).</summary>
        [Column(nameof(ShortName), DataType.UnicodeString)]
        [StringLength(50)]
        public string? ShortName { get; set; }

        /// <summary>ISO 3166-1 alpha-2, or null for an international board.</summary>
        [Column(nameof(CountryCode), DataType.String)]
        [StringLength(2)]
        public string? CountryCode { get; set; }

        /// <summary>A regional or member board's parent (BISE Lahore under Pakistan's boards); it starts from the parent's settings.</summary>
        [Column(nameof(ParentBoardId), DataType.Int)]
        public int? ParentBoardId { get; set; }

        [Column(nameof(Logo), DataType.String)]
        [StringLength(300)]
        public string? Logo { get; set; }

        [Column(nameof(Website), DataType.String)]
        [StringLength(300)]
        public string? Website { get; set; }

        /// <summary>GAMA: the board every other board's topics link to. One board at most.</summary>
        [Column(nameof(IsMaster), DataType.Boolean)]
        [Required]
        public bool IsMaster { get; set; }

        /// <summary>Selected when a visitor has chosen none. One board at most.</summary>
        [Column(nameof(IsDefault), DataType.Boolean)]
        [Required]
        public bool IsDefault { get; set; }

        /// <summary>Shown to visitors. A new board stays hidden until it is turned on.</summary>
        [Column(nameof(IsEnabled), DataType.Boolean)]
        [Required]
        public bool IsEnabled { get; set; }

        [Column(nameof(ListOrder), DataType.Int)]
        [Required]
        public int ListOrder { get; set; }

        /// <summary>How a paper's code is shown, e.g. <c>{code}/{paper}{variant}/{seriesCode}/{yy}</c> (see <c>CurriculumTemplate</c>).</summary>
        [Column(nameof(CodeTemplate), DataType.String)]
        [StringLength(200)]
        public string? CodeTemplate { get; set; }

        /// <summary>The language its papers are in unless a qualification or paper says otherwise (<c>Languages</c>).</summary>
        [Column(nameof(LanguageId), DataType.Int)]
        public int? LanguageId { get; set; }

        [Column(nameof(CreationDate), DataType.DateTimeOffset)]
        [Required]
        public DateTimeOffset CreationDate { get; set; }

        [Column(nameof(CreationUserId), DataType.Long)]
        public long? CreationUserId { get; set; }

        [Column(nameof(LastModifyDate), DataType.DateTimeOffset)]
        public DateTimeOffset? LastModifyDate { get; set; }

        [Column(nameof(LastModifyUserId), DataType.Long)]
        public long? LastModifyUserId { get; set; }

        public void Configure([NotNull] EntityTypeBuilder<CurriculumBoard> builder)
        {
            _ = builder.HasIndex(t => t.Slug).IsUnique(true);
            _ = builder.HasIndex(t => t.LegacyId).IsUnique(true).HasFilter("[LegacyId] IS NOT NULL");
            _ = builder.HasIndex(t => t.IsMaster).IsUnique(true).HasFilter("[IsMaster] = 1");
            _ = builder.HasIndex(t => t.IsDefault).IsUnique(true).HasFilter("[IsDefault] = 1");
            _ = builder.HasOne<CurriculumBoard>().WithMany().HasForeignKey(t => t.ParentBoardId).OnDelete(DeleteBehavior.NoAction);
            _ = builder.HasOne<Language>().WithMany().HasForeignKey(t => t.LanguageId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
