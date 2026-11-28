namespace GamaEdtech.Domain.Enumeration
{
    using GamaEdtech.Common.Data.Enumeration;
    using GamaEdtech.Common.DataAnnotation;

    /// <summary>
    /// A part of gama-api (the legacy backend) that moves to this backend on its own, in the order of the members. Each one
    /// has its own <see cref="MigrationRoute"/>, so a domain is switched without touching the others. See
    /// <c>docs/architecture/legacy-migration.md</c>.
    /// </summary>
    public sealed class MigrationDomain : Enumeration<MigrationDomain, byte>
    {
        /// <summary>Boards, grades, subjects, topics and the other lookup lists (gama-api's <c>types</c>).</summary>
        [Display]
        public static readonly MigrationDomain Types = new(nameof(Types), 0);

        [Display]
        public static readonly MigrationDomain PastPapers = new(nameof(PastPapers), 1);

        [Display]
        public static readonly MigrationDomain Tutorials = new(nameof(Tutorials), 2);

        [Display]
        public static readonly MigrationDomain Exams = new(nameof(Exams), 3);

        [Display]
        public static readonly MigrationDomain Finance = new(nameof(Finance), 4);

        /// <summary>Users and sign-in. The last one to move.</summary>
        [Display]
        public static readonly MigrationDomain Identity = new(nameof(Identity), 5);

        /// <summary>
        /// Not a move from gama-api but the same kind of switch: from the copied lookup lists (<c>LegacyTaxonomyItems</c>,
        /// the Legacy side) to the curriculum model (<c>Curriculum*</c> tables, the Native side), reads first (Shadow, then
        /// Native), writes after. See docs/business/curriculum.md.
        /// </summary>
        [Display]
        public static readonly MigrationDomain Curriculum = new(nameof(Curriculum), 6);

        public MigrationDomain()
        {
        }

        public MigrationDomain(string name, byte value) : base(name, value)
        {
        }
    }
}
