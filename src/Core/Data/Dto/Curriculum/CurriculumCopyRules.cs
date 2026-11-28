namespace GamaEdtech.Data.Dto.Curriculum
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Text.RegularExpressions;

    /// <summary>One copied lookup topic, as the copy reads it.</summary>
    public readonly record struct CurriculumCopyTopic(int Id, int ListOrder, bool IsSeason);

    /// <summary>A session the website showed for a level, with Cambridge's codes when it has them.</summary>
    public sealed record CurriculumCopySession(byte Month, string Name, string? Code = null, string? FileCode = null, string? SeoName = null);

    /// <summary>
    /// How the curriculum copy (phase 2) reads the copied lookup lists, and the board details the website hard-coded until
    /// now (2026-11-28), written once here as the data they become. See docs/business/curriculum.md, "The copy".
    /// </summary>
    public static partial class CurriculumCopyRules
    {
        /// <summary>GAMA, the master board (the website's <c>isGamaBoard</c>, <c>'7131'</c>).</summary>
        public const int GamaBoardId = 7131;

        /// <summary>Cambridge, the default board (the website's <c>DEFAULT_BOARD_ID</c>).</summary>
        public const int CambridgeBoardId = 6627;

        /// <summary>The variant item meaning "no variant" (<c>nv</c>), not an option.</summary>
        public const string NoVariant = "nv";

        public const string VariantKindKey = "variant";

        /// <summary>The website's short names (<c>SEARCH_BOARD_ICON_BY_TITLE</c>), by board id.</summary>
        public static readonly IReadOnlyDictionary<int, string> ShortNames = new Dictionary<int, string>
        {
            [6627] = "CIE",
            [6657] = "Edexcel",
            [6658] = "AQA",
            [6659] = "OCR",
            [7131] = "GAMA",
            [8674] = "CXC",
            [9130] = "Nigeria",
        };

        // Cambridge's session codes (useCambridgeSeo) by month: code in a paper's code, letter in file names, words in titles.
        private static readonly Dictionary<byte, (string Code, string File, string Seo)> CambridgeCodes = new()
        {
            [3] = ("F/M", "m", "February March"),
            [4] = ("A/M", "s", "April May"),
            [5] = ("A/M", "s", "April May"),
            [6] = ("M/J", "s", "May June"),
            [10] = ("O/N", "w", "October November"),
            [11] = ("O/N", "w", "October November"),
        };

        // The sessions the website's search showed per level (SEARCH_MONTHS_BY_LEVEL), by level id.
        private static readonly Dictionary<int, (byte Month, string Name)[]> SessionsByLevel = new()
        {
            [22] = [(3, "March"), (6, "May/Jun"), (11, "Oct/Nov")],
            [23] = [(6, "May/Jun"), (11, "Oct/Nov")],
            [4161] = [(3, "March"), (6, "May/Jun"), (11, "Oct/Nov")],
            [6374] = [(6, "May/Jun"), (11, "Oct/Nov")],
            [6533] = [(6, "May/Jun"), (11, "Oct/Nov")],
            [6635] = [(3, "March"), (4, "April"), (5, "May"), (10, "October")],
            [6639] = [(3, "March"), (4, "April"), (5, "May"), (10, "October")],
            [6668] = [(1, "January"), (4, "April"), (6, "June"), (10, "October"), (11, "November")],
            [6669] = [(6, "May/Jun"), (11, "Oct/Nov")],
            [6670] = [(1, "January"), (4, "April"), (6, "June"), (10, "October"), (11, "November")],
            [6671] = [(6, "May/Jun"), (11, "Oct/Nov")],
            [6672] = [(6, "June"), (11, "November")],
            [6673] = [(6, "June")],
            [6674] = [(6, "June")],
            [6675] = [(6, "May/Jun"), (11, "Oct/Nov")],
            [6676] = [(6, "June"), (11, "November")],
            [6677] = [(6, "June"), (11, "Oct/Nov")],
            [6678] = [(6, "June"), (11, "Oct/Nov")],
        };

        // Extra file types' codes in file names (useCambridgeSeo's CAMBRIDGE_EXTRA_FILE_TYPES), by normalised title.
        private static readonly Dictionary<string, (string Code, string Label)> DocumentCodes = new(StringComparer.Ordinal)
        {
            ["insert"] = ("in", "IN"),
            ["audio file"] = ("sf", "SF"),
            ["confidential instructions"] = ("ci", "CI"),
            ["confidental instructions"] = ("ci", "CI"),
            ["teacher's notes"] = ("tn", "TN"),
            ["teacher notes"] = ("tn", "TN"),
            ["transcript"] = ("qr", "QR"),
            ["map file"] = ("i2", "I2"),
            ["pre released material"] = ("pm", "PM"),
        };

        private static readonly string[] MonthNames = [.. Enumerable.Range(1, 12).Select(t => CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(t))];

        /// <summary>
        /// The sessions of a level: the website's list for it (with Cambridge's codes on a Cambridge level), else one per
        /// month its papers were sat in, named by the month.
        /// </summary>
        public static IReadOnlyList<CurriculumCopySession> Sessions(int levelId, bool isCambridge, IEnumerable<byte> monthsUsed)
        {
            ArgumentNullException.ThrowIfNull(monthsUsed);
            IEnumerable<(byte Month, string Name)> list = SessionsByLevel.TryGetValue(levelId, out var known)
                ? known
                : monthsUsed.Where(t => t is >= 1 and <= 12).Distinct().Order().Select(t => (t, MonthNames[t - 1]));
            return [.. list.Select(t => isCambridge && CambridgeCodes.TryGetValue(t.Month, out var code)
                ? new CurriculumCopySession(t.Month, t.Name, code.Code, code.File, code.Seo)
                : new CurriculumCopySession(t.Month, t.Name))];
        }

        /// <summary>A URL slug: lower case letters and digits joined by dashes; <paramref name="fallback"/> when nothing is left
        /// (a title in another script).</summary>
        public static string Slug(string? title, string fallback)
        {
            ArgumentNullException.ThrowIfNull(fallback);
            var normalized = (title ?? string.Empty).Normalize(NormalizationForm.FormD);
            StringBuilder ascii = new();
            foreach (var c in normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
            {
                _ = ascii.Append(c);
            }

            var slug = NotSlug().Replace(ascii.ToString().ToLowerInvariant(), "-").Trim('-');
            if (slug.Length > 90)
            {
                slug = slug[..90].TrimEnd('-');
            }

            return slug.Length == 0 ? fallback : slug;
        }

        /// <summary>A slug not in <paramref name="taken"/> (adds -2, -3...), which it is then added to.</summary>
        public static string UniqueSlug(string slug, ISet<string> taken)
        {
            ArgumentNullException.ThrowIfNull(slug);
            ArgumentNullException.ThrowIfNull(taken);
            var candidate = slug;
            var i = 2;
            while (!taken.Add(candidate))
            {
                candidate = $"{slug}-{i++}";
            }

            return candidate;
        }

        /// <summary>A subject's name and board code: the metadata's <c>code</c>, else "(0580)" at the end of the title.</summary>
        public static (string Name, string? Code) Syllabus(string? title, string? metadata)
        {
            var name = (title ?? string.Empty).Trim();
            string? code = null;
            if (!string.IsNullOrWhiteSpace(metadata))
            {
                try
                {
                    using var json = JsonDocument.Parse(metadata);
                    if (json.RootElement.ValueKind == JsonValueKind.Object && json.RootElement.TryGetProperty("code", out var value)
                        && value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString()))
                    {
                        code = value.GetString()!.Trim();
                    }
                }
                catch (JsonException)
                {
                    // Metadata that isn't JSON says nothing about the code.
                }
            }

            var match = TrailingCode().Match(name);
            if (match.Success)
            {
                code ??= match.Groups[2].Value;
                if (code == match.Groups[2].Value)
                {
                    name = match.Groups[1].Value.Trim();
                }
            }

            return (name, code is { Length: <= 30 } ? code : null);
        }

        /// <summary>The ids in a parent field ("6627+6657", a subject's "0,22"): each part, or for a subject each grade.</summary>
        public static IReadOnlyList<int> Parents(string? parent, bool isSubject)
        {
            List<int> ids = [];
            foreach (var part in (parent ?? string.Empty).Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var id = isSubject ? part.Split(',').LastOrDefault() : part;
                if (int.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out var value) && value > 0 && !ids.Contains(value))
                {
                    ids.Add(value);
                }
            }

            return ids;
        }

        /// <summary>
        /// Each topic's unit: gama-api lists a subject's topics flat, a unit (<c>season</c>) followed by its chapters in list
        /// order. A unit, and a chapter before any unit, has none.
        /// </summary>
        public static IReadOnlyDictionary<int, int?> TopicUnits(IEnumerable<CurriculumCopyTopic> topics)
        {
            ArgumentNullException.ThrowIfNull(topics);
            Dictionary<int, int?> units = [];
            int? unit = null;
            foreach (var topic in topics.OrderBy(t => t.ListOrder).ThenBy(t => t.Id))
            {
                if (topic.IsSeason)
                {
                    unit = topic.Id;
                    units[topic.Id] = null;
                }
                else
                {
                    units[topic.Id] = unit;
                }
            }

            return units;
        }

        /// <summary>A paper's chip: the number of "Paper 4", else its whole title.</summary>
        public static string PaperShortLabel(string? title)
        {
            var name = (title ?? string.Empty).Trim();
            var match = PaperNumber().Match(name);
            var label = match.Success ? match.Groups[1].Value : name;
            return label.Length is > 0 and <= 20 ? label : name[..Math.Min(name.Length, 20)];
        }

        /// <summary>An extra file type's code and short label: the website's for the types it knew, else built from the
        /// title (lower case letters, digits and underscores, starting with a letter).</summary>
        public static (string Code, string? Label) DocumentCode(string? title)
        {
            var key = (title ?? string.Empty).Trim().ToLowerInvariant().Replace('’', '\'');
            if (DocumentCodes.TryGetValue(key, out var known))
            {
                return known;
            }

            var code = NotCode().Replace(key, "_").Trim('_');
            if (code.Length == 0 || !char.IsAsciiLetterLower(code[0]))
            {
                code = "doc_" + code;
            }

            return (code.Length > 20 ? code[..20].TrimEnd('_') : code, null);
        }

        [GeneratedRegex("[^a-z0-9]+")]
        private static partial Regex NotSlug();

        [GeneratedRegex("[^a-z0-9]+")]
        private static partial Regex NotCode();

        [GeneratedRegex(@"^(.*?)\s*\(([A-Za-z0-9][A-Za-z0-9/.\-]{1,29})\)$")]
        private static partial Regex TrailingCode();

        [GeneratedRegex(@"^paper\s+(\d+)\b", RegexOptions.IgnoreCase)]
        private static partial Regex PaperNumber();
    }
}
