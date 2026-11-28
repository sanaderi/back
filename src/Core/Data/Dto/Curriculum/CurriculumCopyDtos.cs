namespace GamaEdtech.Data.Dto.Curriculum
{
    using System;
    using System.Collections.Generic;

    /// <summary>What one run of the copy did, by kind (board, qualification, syllabus, topic...).</summary>
    public sealed class CurriculumCopyResultDto
    {
        public IReadOnlyList<CurriculumCopyCountDto> Counts { get; set; } = [];
        public int Issues { get; set; }
        public DateTimeOffset FinishedAt { get; set; }
    }

    public sealed class CurriculumCopyCountDto
    {
        public required string Kind { get; set; }
        public int Added { get; set; }
        public int Updated { get; set; }
        public int Removed { get; set; }
    }

    /// <summary>
    /// The count check before any switch: each copied list against its source (both counted now), and how much of the
    /// content finds its place in the model. Nothing switches until every list's <c>Missing</c> is 0.
    /// </summary>
    public sealed class CurriculumCheckDto
    {
        public IReadOnlyList<CurriculumCheckRowDto> Lists { get; set; } = [];
        public IReadOnlyList<CurriculumCoverageDto> Content { get; set; } = [];
        public int Issues { get; set; }
    }

    /// <summary>One list: its source rows (not deleted), the rows copied from them, and the source rows not copied.</summary>
    public sealed class CurriculumCheckRowDto
    {
        public required string Kind { get; set; }
        public int Source { get; set; }
        public int Copied { get; set; }
        public int Missing { get; set; }
    }

    /// <summary>One kind of content (past papers, tutorials, exams): how many find their board, level, subject, paper or
    /// resource type, variant and topics in the model.</summary>
    public sealed class CurriculumCoverageDto
    {
        public required string Content { get; set; }
        public int Total { get; set; }
        public int Placed { get; set; }

        /// <summary>The parts that failed, with how many rows each left out.</summary>
        public IReadOnlyDictionary<string, int> Unplaced { get; set; } = new Dictionary<string, int>();
    }

    public sealed class CurriculumCopyIssueDto
    {
        public long Id { get; set; }
        public string? Kind { get; set; }
        public long? SourceId { get; set; }
        public string? Message { get; set; }
        public DateTimeOffset CreationDate { get; set; }
    }
}
