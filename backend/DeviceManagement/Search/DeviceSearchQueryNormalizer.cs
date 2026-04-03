using System.Text;
using System.Text.RegularExpressions;

namespace DeviceManagement.Search;

/// <summary>
/// Normalizes user search input for MongoDB <c>$text</c>: case-insensitive matching is handled by the text index;
/// this trims, collapses whitespace, and turns punctuation into separators so tokens align with indexed words.
/// </summary>
public static class DeviceSearchQueryNormalizer
{
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    /// <summary>Returns empty string if input has no searchable tokens after normalization.</summary>
    public static string Normalize(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return string.Empty;

        var sb = new StringBuilder(query.Length);
        foreach (var c in query)
        {
            if (char.IsLetterOrDigit(c))
                sb.Append(c);
            else if (char.IsWhiteSpace(c) || c is '-' or '_')
                sb.Append(' ');
            else
                sb.Append(' ');
        }

        return WhitespaceRegex.Replace(sb.ToString().Trim(), " ");
    }
}
