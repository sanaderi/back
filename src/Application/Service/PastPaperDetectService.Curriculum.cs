namespace GamaEdtech.Application.Service
{
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using GamaEdtech.Common.DataAccess.UnitOfWork;
    using GamaEdtech.Data.Dto.PastPaper;
    using GamaEdtech.Domain.Entity.Curriculum;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Reading a cover with the curriculum model (phase 6, "detection as data"): any board, not only Cambridge. The board by
    /// its name or short name on the page; the printed code by the board's code template with its own syllabus codes
    /// (<c>{syllabusCode}/{paperVariant}</c> reads <c>9700/11</c>, <c>{syllabusCode}/{paper}</c> reads <c>031/2</c>); the
    /// session by the board's session names followed by a year; the paper and variant as the board's own. The answer is in
    /// the lookup ids the add form saves. Used first once <c>Migration:Routes:Curriculum.Website</c> is Native; the Cambridge
    /// patterns stay as the fallback. See docs/business/curriculum.md, "Paper detection".
    /// </summary>
    public sealed partial class PastPaperDetectService
    {
        private static readonly TimeSpan MatchTimeout = TimeSpan.FromSeconds(1);

        // Where the printed code ends in a template: the session's part, or anything after a space.
        private static readonly string[] CodeStops = ["{seriesCode}", "{yy}", "{year}", "{series}", " "];

        /// <summary>One board as the reader needs it.</summary>
        internal sealed record CoverBoard(int LegacyId, string Name, string? ShortName, string? CodeTemplate,
            IReadOnlyList<CoverSyllabus> Syllabi, IReadOnlyList<CoverSession> BoardSessions, IReadOnlyList<(string Code, int LegacyId, string Label)> Variants);

        internal sealed record CoverSyllabus(int LegacyId, string Name, string Code, int QualificationLegacyId, string QualificationName,
            IReadOnlyList<(string ShortLabel, int LegacyId, string Name)> Papers);

        internal sealed record CoverSession(string Name, string? SeoName, int Month);

        /// <summary>The page read with the boards' own data, or null when no board with a template and a known code fits.</summary>
        internal static PastPaperDetectionDto? ReadWithCurriculum(string text, IReadOnlyList<CoverBoard> boards, bool isMarkScheme)
        {
            ArgumentNullException.ThrowIfNull(text);
            ArgumentNullException.ThrowIfNull(boards);
            foreach (var board in boards.Where(t => t.CodeTemplate is not null && Mentions(text, t)).OrderByDescending(t => t.Name.Length))
            {
                if (CoverCode(board) is not { } pattern || pattern.Match(text) is not { Success: true } code)
                {
                    continue;
                }

                var syllabus = board.Syllabi.FirstOrDefault(t => string.Equals(t.Code, code.Groups["code"].Value, StringComparison.OrdinalIgnoreCase));
                if (syllabus is null)
                {
                    continue;
                }

                // {paperVariant}: the paper's digit then the variant's ("42"); "0Y" or one digit is that paper with no variant.
                string? paper = null;
                string? variant = null;
                if (code.Groups["pv"].Success)
                {
                    var component = code.Groups["pv"].Value;
                    (paper, variant) = component.Length == 1 || component[0] == '0' ? (component[^1].ToString(), null) : (component[0].ToString(), component[1..]);
                }
                else
                {
                    paper = code.Groups["paper"].Success ? code.Groups["paper"].Value.TrimStart('0') : null;
                    variant = code.Groups["variant"].Success ? code.Groups["variant"].Value : null;
                }

                PastPaperDetectionDto detection = new()
                {
                    Source = "curriculum",
                    IsMarkScheme = isMarkScheme,
                    Board = Item(board.LegacyId, board.Name),
                    Grade = Item(syllabus.QualificationLegacyId, syllabus.QualificationName),
                    Subject = Item(syllabus.LegacyId, $"{syllabus.Name} ({syllabus.Code})"),
                    Code = code.Value,
                };

                if (paper is not null && syllabus.Papers.FirstOrDefault(t => t.ShortLabel == paper) is { LegacyId: > 0 } component2)
                {
                    detection.Classification = Item(component2.LegacyId, component2.Name);
                }

                if (variant is not null && board.Variants.FirstOrDefault(t => t.Code == variant) is { LegacyId: > 0 } option)
                {
                    detection.Variant = Item(option.LegacyId, option.Label);
                }

                foreach (var session in board.BoardSessions)
                {
                    var words = string.Join('|', new[] { session.Name, session.SeoName }.Where(t => !string.IsNullOrWhiteSpace(t)).Select(Words));
                    if (Regex.Match(text, $"\\b(?:{words})\\s+(\\d{{4}})\\b", RegexOptions.IgnoreCase, MatchTimeout) is { Success: true } found
                        && int.TryParse(found.Groups[1].Value, CultureInfo.InvariantCulture, out var year) && year is >= 1990 and <= 2100)
                    {
                        (detection.Year, detection.Month) = (year, session.Month);
                        break;
                    }
                }

                var sessionWords = board.BoardSessions.FirstOrDefault(t => t.Month == detection.Month) is { } s ? s.SeoName ?? s.Name : null;
                detection.Title = string.Join(' ', new[] { syllabus.Name, detection.Code, sessionWords, detection.Year?.ToString(CultureInfo.InvariantCulture) }
                    .Where(t => !string.IsNullOrWhiteSpace(t)));
                return detection;
            }

            return null;
        }

        /// <summary>The regex of the code a board prints on its covers: its template up to the session part, with its own
        /// syllabus codes in place of <c>{syllabusCode}</c>.</summary>
        internal static Regex? CoverCode(CoverBoard board)
        {
            ArgumentNullException.ThrowIfNull(board);
            var template = board.CodeTemplate ?? string.Empty;
            var start = template.IndexOf("{syllabusCode}", StringComparison.Ordinal);
            if (start < 0 || board.Syllabi.Count == 0)
            {
                return null;
            }

            // The printed code stops where the session or anything after a space begins.
            var rest = template[start..];
            var stop = CodeStops.Select(t => rest.IndexOf(t, StringComparison.Ordinal)).Where(t => t > 0).DefaultIfEmpty(rest.Length).Min();
            var printed = rest[..stop].TrimEnd('/', '-', '.', ' ');

            var codes = string.Join('|', board.Syllabi.Select(t => Regex.Escape(t.Code)).Distinct().OrderByDescending(t => t.Length));
            StringBuilder pattern = new("(?<![A-Za-z0-9])");
            foreach (var part in TemplatePart().Matches(printed).Select(t => t.Value))
            {
                _ = pattern.Append(part switch
                {
                    "{syllabusCode}" => $"(?<code>{codes})",
                    "{paperVariant}" => "(?<pv>\\d{1,2})",
                    "{paper}" => "(?<paper>\\d{1,2})",
                    "{variant}" => "(?<variant>\\d{1,2})",
                    _ when part.StartsWith('{') => "\\S+?",
                    _ => Regex.Escape(part),
                });
            }

            _ = pattern.Append("(?![0-9])");
            return new Regex(pattern.ToString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, MatchTimeout);
        }

        /// <summary>The boards that have a code template, with what reading their covers needs.</summary>
        internal static async Task<List<CoverBoard>> LoadCoverBoardsAsync(IUnitOfWork uow)
        {
            var boards = await uow.GetRepository<CurriculumBoard, int>().GetManyQueryable(t => t.LegacyId != null && t.CodeTemplate != null).ToListAsync();
            List<int> ids = [.. boards.Select(t => t.Id)];
            var levels = await uow.GetRepository<CurriculumQualification, int>().GetManyQueryable(t => ids.Contains(t.BoardId) && t.LegacyId != null).ToListAsync();
            List<int> levelIds = [.. levels.Select(t => t.Id)];
            var syllabi = await uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable(t => levelIds.Contains(t.QualificationId) && t.LegacyId != null && t.Code != null).ToListAsync();
            List<int> syllabusIds = [.. syllabi.Select(t => t.Id)];
            var papers = await uow.GetRepository<CurriculumPaper, int>().GetManyQueryable(t => syllabusIds.Contains(t.SyllabusId)).ToListAsync();
            var sessions = await uow.GetRepository<CurriculumSeriesType, int>().GetManyQueryable(t => ids.Contains(t.BoardId)).ToListAsync();
            var variants = await (from option in uow.GetRepository<CurriculumVariantOption, int>().GetManyQueryable(t => t.LegacyId != null && t.Code != null)
                                  join kind in uow.GetRepository<CurriculumVariantKind, int>().GetManyQueryable(t => ids.Contains(t.BoardId)) on option.VariantKindId equals kind.Id
                                  select new { kind.BoardId, option.Code, option.LegacyId, option.Label }).ToListAsync();

            return [.. boards.Select(board => new CoverBoard(
                board.LegacyId!.Value,
                board.Name!,
                board.ShortName,
                board.CodeTemplate,
                [.. syllabi.Where(s => levels.Exists(l => l.Id == s.QualificationId && l.BoardId == board.Id)).Select(s =>
                {
                    var level = levels.Find(l => l.Id == s.QualificationId)!;
                    return new CoverSyllabus(s.LegacyId!.Value, s.Name!, s.Code!, level.LegacyId!.Value, level.Name!,
                        [.. papers.Where(p => p.SyllabusId == s.Id).Select(p => (p.ShortLabel!, p.LegacyId ?? 0, p.Name!))]);
                })],
                [.. sessions.Where(s => s.BoardId == board.Id).Select(s => new CoverSession(s.Name!, s.SeoName,
                    int.TryParse((s.Months ?? string.Empty).Split(',')[0], CultureInfo.InvariantCulture, out var month) ? month : 0)).Where(s => s.Month > 0).DistinctBy(s => (s.Name, s.Month))],
                [.. variants.Where(v => v.BoardId == board.Id).Select(v => (v.Code!, v.LegacyId!.Value, v.Label!))]))];
        }

        private static bool Mentions(string text, CoverBoard board) =>
            new[] { board.Name, board.ShortName }.Where(t => !string.IsNullOrWhiteSpace(t))
                .Any(t => Regex.IsMatch(text, $"(?<![A-Za-z]){Regex.Escape(t!)}(?![A-Za-z])", RegexOptions.IgnoreCase, MatchTimeout));

        // "May/Jun" or "May June" reads "May/June", "May June" or "May - June" on a cover.
        private static string Words(string? name) => string.Join("[\\s/\\-]*", (name ?? string.Empty).Split([' ', '/', '-'], StringSplitOptions.RemoveEmptyEntries).Select(Regex.Escape));

        [GeneratedRegex(@"\{[A-Za-z]+\}|[^{]+")]
        private static partial Regex TemplatePart();
    }
}
