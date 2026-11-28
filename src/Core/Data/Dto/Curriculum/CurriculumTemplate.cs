namespace GamaEdtech.Data.Dto.Curriculum
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Fills the curriculum's text templates: a board's paper code (<c>{code}/{paperVariant}/{seriesCode}/{yy}</c>) and the
    /// SEO titles and descriptions (<c>CurriculumSeoTemplates</c>). The one place they are written, so the website, the
    /// sitemaps and the MCP say the same thing.
    /// <list type="bullet">
    /// <item><c>{name}</c> is a value; outside brackets it is required.</item>
    /// <item><c>[...]</c> is an optional part, dropped when any value in it is empty.</item>
    /// <item><c>a || b</c> are alternatives: the first whose required values are all there is used, so the last one can be
    /// plain text with no values (the page's fallback when no board is chosen).</item>
    /// </list>
    /// Spaces are collapsed and a space before <c>. , ; :</c> is removed, so an empty value never leaves a gap.
    /// </summary>
    public static partial class CurriculumTemplate
    {
        /// <summary>A meta description longer than this is cut at a word (what search results show).</summary>
        public const int DescriptionLimit = 155;

        private const string Alternative = "||";

        /// <summary>Search results show about this many characters of a title.</summary>
        public const int TitleLimit = 60;

        /// <summary>A description shorter than this says too little to be shown as written.</summary>
        public const int DescriptionMinimum = 70;

        /// <summary>A description longer than this is cut in search results.</summary>
        public const int DescriptionMaximum = 160;

        /// <summary>The template filled with <paramref name="values"/>, or null when no alternative has all its values.</summary>
        public static string? Render(string? template, IReadOnlyDictionary<string, string?> values)
        {
            ArgumentNullException.ThrowIfNull(values);
            if (string.IsNullOrWhiteSpace(template))
            {
                return null;
            }

            foreach (var alternative in template.Split(Alternative))
            {
                if (RenderOne(alternative, values) is { Length: > 0 } text)
                {
                    return text;
                }
            }

            return null;
        }

        /// <summary><see cref="Render"/>, cut to <paramref name="limit"/> characters at a word with an ellipsis.</summary>
        public static string? RenderDescription(string? template, IReadOnlyDictionary<string, string?> values, int limit = DescriptionLimit) =>
            Render(template, values) is { } text ? Cut(text, limit) : null;

        /// <summary>The website's cut: at the last space before the limit, trailing punctuation dropped, then "…".</summary>
        public static string Cut(string text, int limit)
        {
            ArgumentNullException.ThrowIfNull(text);
            if (text.Length <= limit)
            {
                return text;
            }

            var head = text[..(limit - 1)];
            return TrailingPunctuation().Replace(head[..Math.Max(head.LastIndexOf(' '), 0)], string.Empty) + "…";
        }

        /// <summary>What search results would do to a page's title and description, for the admin's preview.</summary>
        public static IReadOnlyList<string> Warnings(string? title, string? description)
        {
            List<string> warnings = [];
            if (title is null || description is null)
            {
                warnings.Add("missing");
            }

            if (title?.Length > TitleLimit)
            {
                warnings.Add("titleLong");
            }

            if (description?.Length > DescriptionMaximum)
            {
                warnings.Add("descriptionLong");
            }
            else if (description?.Length < DescriptionMinimum)
            {
                warnings.Add("descriptionShort");
            }

            return warnings;
        }

        /// <summary>The names of the values a template uses, for the admin's check and help text.</summary>
        public static IReadOnlySet<string> Placeholders(string? template)
        {
            HashSet<string> names = new(StringComparer.Ordinal);
            foreach (Match match in Placeholder().Matches(template ?? string.Empty))
            {
                _ = names.Add(match.Groups[1].Value);
            }

            return names;
        }

        private static string? RenderOne(string template, IReadOnlyDictionary<string, string?> values)
        {
            StringBuilder text = new();
            var index = 0;
            while (index < template.Length)
            {
                var open = template.IndexOf('[', index);
                if (open < 0)
                {
                    if (!Fill(template[index..], values, text))
                    {
                        return null;
                    }

                    break;
                }

                if (!Fill(template[index..open], values, text))
                {
                    return null;
                }

                var close = template.IndexOf(']', open + 1);
                if (close < 0)
                {
                    // An unclosed bracket is text, as written.
                    if (!Fill(template[open..], values, text))
                    {
                        return null;
                    }

                    break;
                }

                StringBuilder part = new();
                if (Fill(template[(open + 1)..close], values, part))
                {
                    _ = text.Append(part);
                }

                index = close + 1;
            }

            return Tidy(text.ToString());
        }

        // Appends the text with its values; false when one of them is empty.
        private static bool Fill(string text, IReadOnlyDictionary<string, string?> values, StringBuilder into)
        {
            var missing = false;
            var filled = Placeholder().Replace(text, match =>
            {
                var value = values.TryGetValue(match.Groups[1].Value, out var found) ? found?.Trim() : null;
                if (string.IsNullOrEmpty(value))
                {
                    missing = true;
                    return string.Empty;
                }

                return value;
            });
            if (missing)
            {
                return false;
            }

            _ = into.Append(filled);
            return true;
        }

        private static string? Tidy(string text)
        {
            var tidy = SpaceBeforePunctuation().Replace(Spaces().Replace(text, " "), "$1").Trim();
            return tidy.Length == 0 ? null : WithoutRepeats(tidy);
        }

        // "Cambridge Cambridge IGCSE" (a value that already starts with the one before it) reads as spam to search
        // engines: a phrase of up to four words said twice in a row is said once, whatever its case.
        private static string WithoutRepeats(string text)
        {
            List<string> words = [.. text.Split(' ')];
            for (var size = 4; size >= 1; size--)
            {
                for (var i = 0; i + (2 * size) <= words.Count; i++)
                {
                    while (i + (2 * size) <= words.Count && Enumerable.Range(0, size).All(k => string.Equals(words[i + k], words[i + size + k], StringComparison.OrdinalIgnoreCase)))
                    {
                        words.RemoveRange(i + size, size);
                    }
                }
            }

            return string.Join(' ', words);
        }

        [GeneratedRegex(@"\{([A-Za-z][A-Za-z0-9]*)\}")]
        private static partial Regex Placeholder();

        [GeneratedRegex(@"\s+")]
        private static partial Regex Spaces();

        [GeneratedRegex(@"\s+([.,;:])")]
        private static partial Regex SpaceBeforePunctuation();

        [GeneratedRegex(@"[\s,.;:–-]+$")]
        private static partial Regex TrailingPunctuation();
    }
}
