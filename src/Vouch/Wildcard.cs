namespace Vouch;

internal static class Wildcard
{
    public const string EmptyPatternMessage = "Cannot match string against an empty string. Provide a wildcard pattern or use the BeEmpty method.";

    public static void RequirePattern(string? pattern, string paramName)
    {
        ArgumentNullException.ThrowIfNull(pattern, paramName);
        if (pattern.Length == 0)
        {
            throw new ArgumentException(EmptyPatternMessage, paramName);
        }
    }

    public static bool MatchesString(string text, string pattern, bool ignoreCase = false)
        => IsMatch(text, pattern, ignoreCase) || (text.EndsWith('\n') && IsMatch(text[..^1], pattern, ignoreCase));

    public static bool MatchesMessage(string message, string pattern)
        => IsMatch(WithoutNewlines(message), WithoutNewlines(pattern), ignoreCase: true);

    private static string WithoutNewlines(string value)
        => value.AsSpan().IndexOfAny('\r', '\n') < 0 ? value : value.Replace("\r", string.Empty).Replace("\n", string.Empty);

    public static bool IsMatch(string text, string pattern, bool ignoreCase = false)
    {
        var t = 0;
        var p = 0;
        var starPattern = -1;
        var starText = 0;

        while (t < text.Length)
        {
            if (p < pattern.Length && pattern[p] == '*')
            {
                starPattern = p++;
                starText = t;
            }
            else if (p < pattern.Length && (pattern[p] == '?' || (pattern[p] == text[t] || (ignoreCase && char.ToUpperInvariant(pattern[p]) == char.ToUpperInvariant(text[t])))))
            {
                p++;
                t++;
            }
            else if (starPattern >= 0)
            {
                p = starPattern + 1;
                t = ++starText;
            }
            else
            {
                return false;
            }
        }

        while (p < pattern.Length && pattern[p] == '*')
        {
            p++;
        }

        return p == pattern.Length;
    }
}
