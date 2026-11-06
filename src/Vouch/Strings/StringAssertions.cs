using System.Globalization;
using System.Text.RegularExpressions;

namespace Vouch;

public readonly struct StringAssertions
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal StringAssertions(string? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public string? Subject { get; }

    private string Name => Fail.Name(_expression);

    public AndConstraint<StringAssertions> Be(string? expected)
    {
        if (!string.Equals(Subject, expected, StringComparison.Ordinal))
        {
            var message = $"Expected {Name} to be {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.";
            if (Subject is not null && expected is not null)
            {
                message += "\n  " + StringDiff.Describe(Subject, expected);
            }

            Fail.With(message);
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> NotBe(string? unexpected)
    {
        if (string.Equals(Subject, unexpected, StringComparison.Ordinal))
        {
            Fail.With($"Expected {Name} not to be {Formatter.Format(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> BeEmpty()
    {
        if (Subject is null || Subject.Length != 0)
        {
            Fail.With($"Expected {Name} to be empty, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> NotBeEmpty()
    {
        if (Subject is null)
        {
            Fail.With($"Expected {Name} not to be empty, but found null.");
            return new(this);
        }

        if (Subject.Length == 0)
        {
            Fail.With($"Expected {Name} not to be empty, but it was.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> BeNull()
    {
        if (Subject is not null)
        {
            Fail.With($"Expected {Name} to be null, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> NotBeNull()
    {
        if (Subject is null)
        {
            Fail.With($"Expected {Name} not to be null, but it was.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> BeNullOrEmpty()
    {
        if (!string.IsNullOrEmpty(Subject))
        {
            Fail.With($"Expected {Name} to be null or empty, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> NotBeNullOrEmpty()
    {
        if (string.IsNullOrEmpty(Subject))
        {
            Fail.With($"Expected {Name} not to be null or empty, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> NotBeNullOrWhiteSpace()
    {
        if (string.IsNullOrWhiteSpace(Subject))
        {
            Fail.With($"Expected {Name} not to be null or white space, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> StartWith(string expected)
    {
        ArgumentNullException.ThrowIfNull(expected);

        if (Subject is null || !Subject.StartsWith(expected, StringComparison.Ordinal))
        {
            Fail.With($"Expected {Name} to start with {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> NotStartWith(string unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected);

        if (Subject is null || Subject.StartsWith(unexpected, StringComparison.Ordinal))
        {
            Fail.With($"Expected {Name} not to start with {Formatter.Format(unexpected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> EndWith(string expected)
    {
        ArgumentNullException.ThrowIfNull(expected);

        if (Subject is null || !Subject.EndsWith(expected, StringComparison.Ordinal))
        {
            Fail.With($"Expected {Name} to end with {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> Contain(string expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        if (expected.Length == 0)
        {
            throw new ArgumentException("Expected value cannot be empty.", nameof(expected));
        }

        if (Subject is null || !Subject.Contains(expected, StringComparison.Ordinal))
        {
            Fail.With($"Expected {Name} to contain {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> NotContain(string unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected);
        if (unexpected.Length == 0)
        {
            throw new ArgumentException("Unexpected value cannot be empty.", nameof(unexpected));
        }

        if (Subject is null || Subject.Contains(unexpected, StringComparison.Ordinal))
        {
            Fail.With($"Expected {Name} not to contain {Formatter.Format(unexpected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> Match(string wildcardPattern)
    {
        Wildcard.RequirePattern(wildcardPattern, nameof(wildcardPattern));

        if (Subject is null || !Wildcard.MatchesString(Subject, wildcardPattern))
        {
            Fail.With($"Expected {Name} to match {Formatter.Format(wildcardPattern)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> MatchRegex(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return RegexCore(pattern, null, RegexOptions.None, null, negate: false);
    }

    public AndConstraint<StringAssertions> MatchRegex(string pattern, RegexOptions options)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return RegexCore(pattern, null, options, null, negate: false);
    }

    public AndConstraint<StringAssertions> MatchRegex(Regex regex)
    {
        ArgumentNullException.ThrowIfNull(regex);
        return RegexCore(null, regex, RegexOptions.None, null, negate: false);
    }

    public AndConstraint<StringAssertions> MatchRegex(string pattern, OccurrenceConstraint occurrence)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return RegexCore(pattern, null, RegexOptions.None, occurrence, negate: false);
    }

    public AndConstraint<StringAssertions> MatchRegex(Regex regex, OccurrenceConstraint occurrence)
    {
        ArgumentNullException.ThrowIfNull(regex);
        return RegexCore(null, regex, RegexOptions.None, occurrence, negate: false);
    }

    public AndConstraint<StringAssertions> NotMatchRegex(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return RegexCore(pattern, null, RegexOptions.None, null, negate: true);
    }

    public AndConstraint<StringAssertions> NotMatchRegex(string pattern, RegexOptions options)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return RegexCore(pattern, null, options, null, negate: true);
    }

    public AndConstraint<StringAssertions> NotMatchRegex(Regex regex)
    {
        ArgumentNullException.ThrowIfNull(regex);
        return RegexCore(null, regex, RegexOptions.None, null, negate: true);
    }

    // A Regex without a timeout could hang the test run, so one is applied to it.
    private AndConstraint<StringAssertions> RegexCore(string? pattern, Regex? regex, RegexOptions options, OccurrenceConstraint? occurrence, bool negate)
    {
        if (Subject is null)
        {
            Fail.With($"Expected {Name} {(negate ? "not to match" : "to match")} regex {Formatter.Format(pattern ?? regex!.ToString())}{Suffix(occurrence)}, but found null.");
            return new(this);
        }

        var count = 0;
        try
        {
            if (regex is not null && regex.MatchTimeout == Regex.InfiniteMatchTimeout)
            {
                regex = new Regex(regex.ToString(), regex.Options, RegexTimeout);
            }

            if (occurrence is null)
            {
                var matched = regex is not null
                    ? regex.IsMatch(Subject)
                    : Regex.IsMatch(Subject, pattern!, options, RegexTimeout);
                count = matched ? 1 : 0;
            }
            else
            {
                count = regex is not null
                    ? regex.Count(Subject)
                    : Regex.Count(Subject, pattern!, options, RegexTimeout);
            }
        }
        catch (RegexMatchTimeoutException ex)
        {
            Fail.With($"Expected {Name} {(negate ? "not to match" : "to match")} regex {Formatter.Format(pattern ?? regex!.ToString())}{Suffix(occurrence)}, but the match timed out after {Seconds(ex.MatchTimeout)}.");
            return new(this);
        }
        catch (ArgumentException ex)
        {
            Fail.With($"Expected {Name} {(negate ? "not to match" : "to match")} regex {Formatter.Format(pattern ?? regex!.ToString())}{Suffix(occurrence)}, but the pattern is not a valid regular expression: {ex.Message.TrimEnd('.')}.");
            return new(this);
        }

        if (occurrence is { } required)
        {
            if (!required.IsSatisfiedBy(count))
            {
                Fail.With($"Expected {Name} to match regex {Formatter.Format(pattern ?? regex!.ToString())} {required.Describe()}, but it matched {OccurrenceConstraint.Count(count)} in {Formatter.Format(Subject)}.");
            }
        }
        else if (negate == (count > 0))
        {
            Fail.With(negate
                ? $"Expected {Name} not to match regex {Formatter.Format(pattern ?? regex!.ToString())}, but found {Formatter.Format(Subject)}."
                : $"Expected {Name} to match regex {Formatter.Format(pattern ?? regex!.ToString())}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    private static string Suffix(OccurrenceConstraint? occurrence) => occurrence is { } o ? " " + o.Describe() : string.Empty;

    private static string Seconds(TimeSpan timeout)
    {
        var seconds = timeout.TotalSeconds;
        return seconds == 1 ? "1 second" : $"{seconds.ToString(CultureInfo.InvariantCulture)} seconds";
    }

    public AndConstraint<StringAssertions> Contain(string expected, OccurrenceConstraint occurrence)
    {
        ArgumentNullException.ThrowIfNull(expected);
        if (expected.Length == 0)
        {
            throw new ArgumentException("Expected value cannot be empty.", nameof(expected));
        }

        if (Subject is null)
        {
            Fail.With($"Expected {Name} to contain {Formatter.Format(expected)} {occurrence.Describe()}, but found null.");
            return new(this);
        }

        var count = 0;
        var rest = Subject.AsSpan();
        int index;
        while ((index = rest.IndexOf(expected.AsSpan(), StringComparison.Ordinal)) >= 0)
        {
            count++;
            rest = rest[(index + expected.Length)..];
        }

        if (!occurrence.IsSatisfiedBy(count))
        {
            Fail.With($"Expected {Name} to contain {Formatter.Format(expected)} {occurrence.Describe()}, but found it {OccurrenceConstraint.Count(count)} in {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> StartWithEquivalentOf(string expected)
    {
        ArgumentNullException.ThrowIfNull(expected);

        if (Subject is null || !Subject.StartsWith(expected, StringComparison.OrdinalIgnoreCase))
        {
            Fail.With($"Expected {Name} to start with {Formatter.Format(expected)} (ignoring case), but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> NotStartWithEquivalentOf(string unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected);

        if (Subject is null || Subject.StartsWith(unexpected, StringComparison.OrdinalIgnoreCase))
        {
            Fail.With($"Expected {Name} not to start with {Formatter.Format(unexpected)} (ignoring case), but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> EndWithEquivalentOf(string expected)
    {
        ArgumentNullException.ThrowIfNull(expected);

        if (Subject is null || !Subject.EndsWith(expected, StringComparison.OrdinalIgnoreCase))
        {
            Fail.With($"Expected {Name} to end with {Formatter.Format(expected)} (ignoring case), but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> NotEndWithEquivalentOf(string unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected);

        if (Subject is null || Subject.EndsWith(unexpected, StringComparison.OrdinalIgnoreCase))
        {
            Fail.With($"Expected {Name} not to end with {Formatter.Format(unexpected)} (ignoring case), but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> MatchEquivalentOf(string wildcardPattern)
    {
        Wildcard.RequirePattern(wildcardPattern, nameof(wildcardPattern));

        if (Subject is null || !Wildcard.MatchesString(Subject, wildcardPattern, ignoreCase: true))
        {
            Fail.With($"Expected {Name} to match {Formatter.Format(wildcardPattern)} (ignoring case), but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> NotMatchEquivalentOf(string wildcardPattern)
    {
        Wildcard.RequirePattern(wildcardPattern, nameof(wildcardPattern));

        if (Subject is null || Wildcard.MatchesString(Subject, wildcardPattern, ignoreCase: true))
        {
            Fail.With($"Expected {Name} not to match {Formatter.Format(wildcardPattern)} (ignoring case), but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> NotMatch(string wildcardPattern)
    {
        Wildcard.RequirePattern(wildcardPattern, nameof(wildcardPattern));

        if (Subject is null || Wildcard.MatchesString(Subject, wildcardPattern))
        {
            Fail.With($"Expected {Name} not to match {Formatter.Format(wildcardPattern)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    // Upper and lower case are judged character by character, so a string with no letters is both.
    public AndConstraint<StringAssertions> BeUpperCased()
    {
        if (Subject is null || !IsCased(Subject, upper: true))
        {
            Fail.With($"Expected {Name} to be upper-cased, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> NotBeUpperCased()
    {
        if (Subject is null || IsCased(Subject, upper: true))
        {
            Fail.With($"Expected {Name} not to be upper-cased, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> BeLowerCased()
    {
        if (Subject is null || !IsCased(Subject, upper: false))
        {
            Fail.With($"Expected {Name} to be lower-cased, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> NotBeLowerCased()
    {
        if (Subject is null || IsCased(Subject, upper: false))
        {
            Fail.With($"Expected {Name} not to be lower-cased, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    private static bool IsCased(string value, bool upper)
    {
        foreach (var c in value)
        {
            if ((upper ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c)) != c)
            {
                return false;
            }
        }

        return true;
    }

    public AndConstraint<StringAssertions> BeNullOrWhiteSpace()
    {
        if (!string.IsNullOrWhiteSpace(Subject))
        {
            Fail.With($"Expected {Name} to be null or white space, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndWhichConstraint<StringAssertions, T> BeParsableInto<T>() where T : IParsable<T>
    {
        if (!T.TryParse(Subject, CultureInfo.InvariantCulture, out var value))
        {
            Fail.With($"Expected {Name} to be parsable into {Formatter.TypeName(typeof(T))}, but found {Formatter.Format(Subject)}.");
            return new(this, default!);
        }

        return new(this, value);
    }

    public AndConstraint<StringAssertions> HaveLength(int expected)
    {
        if (Subject is null)
        {
            Fail.With($"Expected {Name} to have length {expected}, but found null.");
            return new(this);
        }

        if (Subject.Length != expected)
        {
            Fail.With($"Expected {Name} to have length {expected}, but found {Formatter.Format(Subject)} with length {Subject.Length}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> BeOneOf(params string?[] validValues)
    {
        ArgumentNullException.ThrowIfNull(validValues);

        foreach (var value in validValues)
        {
            if (string.Equals(Subject, value, StringComparison.Ordinal))
            {
                return new(this);
            }
        }

        Fail.With($"Expected {Name} to be one of {Formatter.FormatList(validValues)}, but found {Formatter.Format(Subject)}.");
        return new(this);
    }

    public AndConstraint<StringAssertions> BeEquivalentTo(string? expected)
    {
        if (!string.Equals(Subject, expected, StringComparison.OrdinalIgnoreCase))
        {
            Fail.With($"Expected {Name} to be equivalent to {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> Be(string? expected, string because)
    {
        Reason.Set(because);
        try
        {
            return Be(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> NotBe(string? unexpected, string because)
    {
        Reason.Set(because);
        try
        {
            return NotBe(unexpected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> BeEmpty(string because)
    {
        Reason.Set(because);
        try
        {
            return BeEmpty();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> NotBeEmpty(string because)
    {
        Reason.Set(because);
        try
        {
            return NotBeEmpty();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> BeNull(string because)
    {
        Reason.Set(because);
        try
        {
            return BeNull();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> NotBeNull(string because)
    {
        Reason.Set(because);
        try
        {
            return NotBeNull();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> BeNullOrEmpty(string because)
    {
        Reason.Set(because);
        try
        {
            return BeNullOrEmpty();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> NotBeNullOrEmpty(string because)
    {
        Reason.Set(because);
        try
        {
            return NotBeNullOrEmpty();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> NotBeNullOrWhiteSpace(string because)
    {
        Reason.Set(because);
        try
        {
            return NotBeNullOrWhiteSpace();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> StartWith(string expected, string because)
    {
        Reason.Set(because);
        try
        {
            return StartWith(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> NotStartWith(string unexpected, string because)
    {
        Reason.Set(because);
        try
        {
            return NotStartWith(unexpected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> EndWith(string expected, string because)
    {
        Reason.Set(because);
        try
        {
            return EndWith(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> Contain(string expected, string because)
    {
        Reason.Set(because);
        try
        {
            return Contain(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> NotContain(string unexpected, string because)
    {
        Reason.Set(because);
        try
        {
            return NotContain(unexpected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> Match(string wildcardPattern, string because)
    {
        Reason.Set(because);
        try
        {
            return Match(wildcardPattern);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> MatchRegex(string pattern, string because)
    {
        Reason.Set(because);
        try
        {
            return MatchRegex(pattern);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> HaveLength(int expected, string because)
    {
        Reason.Set(because);
        try
        {
            return HaveLength(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> BeEquivalentTo(string? expected, string because)
    {
        Reason.Set(because);
        try
        {
            return BeEquivalentTo(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> BeOneOf(IEnumerable<string?> validValues, string because)
    {
        ArgumentNullException.ThrowIfNull(validValues);
        var values = validValues.ToArray();
        Reason.Set(because);
        try
        {
            return BeOneOf(values);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> NotContainEquivalentOf(string unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected);

        if (Subject is null || Subject.Contains(unexpected, StringComparison.OrdinalIgnoreCase))
        {
            Fail.With($"Expected {Name} not to contain {Formatter.Format(unexpected)} (ignoring case), but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> NotContainEquivalentOf(string unexpected, string because)
    {
        Reason.Set(because);
        try
        {
            return NotContainEquivalentOf(unexpected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> MatchRegex(string pattern, RegexOptions options, string because)
    {
        Reason.Set(because);
        try
        {
            return MatchRegex(pattern, options);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> MatchRegex(Regex regex, string because)
    {
        Reason.Set(because);
        try
        {
            return MatchRegex(regex);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> MatchRegex(string pattern, OccurrenceConstraint occurrence, string because)
    {
        Reason.Set(because);
        try
        {
            return MatchRegex(pattern, occurrence);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> MatchRegex(Regex regex, OccurrenceConstraint occurrence, string because)
    {
        Reason.Set(because);
        try
        {
            return MatchRegex(regex, occurrence);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> NotMatchRegex(string pattern, string because)
    {
        Reason.Set(because);
        try
        {
            return NotMatchRegex(pattern);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> NotMatchRegex(string pattern, RegexOptions options, string because)
    {
        Reason.Set(because);
        try
        {
            return NotMatchRegex(pattern, options);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> NotMatchRegex(Regex regex, string because)
    {
        Reason.Set(because);
        try
        {
            return NotMatchRegex(regex);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> Contain(string expected, OccurrenceConstraint occurrence, string because)
    {
        Reason.Set(because);
        try
        {
            return Contain(expected, occurrence);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> StartWithEquivalentOf(string expected, string because)
    {
        Reason.Set(because);
        try
        {
            return StartWithEquivalentOf(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> NotStartWithEquivalentOf(string unexpected, string because)
    {
        Reason.Set(because);
        try
        {
            return NotStartWithEquivalentOf(unexpected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> EndWithEquivalentOf(string expected, string because)
    {
        Reason.Set(because);
        try
        {
            return EndWithEquivalentOf(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> NotEndWithEquivalentOf(string unexpected, string because)
    {
        Reason.Set(because);
        try
        {
            return NotEndWithEquivalentOf(unexpected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> MatchEquivalentOf(string wildcardPattern, string because)
    {
        Reason.Set(because);
        try
        {
            return MatchEquivalentOf(wildcardPattern);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> NotMatchEquivalentOf(string wildcardPattern, string because)
    {
        Reason.Set(because);
        try
        {
            return NotMatchEquivalentOf(wildcardPattern);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> NotMatch(string wildcardPattern, string because)
    {
        Reason.Set(because);
        try
        {
            return NotMatch(wildcardPattern);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> BeUpperCased(string because)
    {
        Reason.Set(because);
        try
        {
            return BeUpperCased();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> NotBeUpperCased(string because)
    {
        Reason.Set(because);
        try
        {
            return NotBeUpperCased();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> BeLowerCased(string because)
    {
        Reason.Set(because);
        try
        {
            return BeLowerCased();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> NotBeLowerCased(string because)
    {
        Reason.Set(because);
        try
        {
            return NotBeLowerCased();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StringAssertions> BeNullOrWhiteSpace(string because)
    {
        Reason.Set(because);
        try
        {
            return BeNullOrWhiteSpace();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndWhichConstraint<StringAssertions, T> BeParsableInto<T>(string because) where T : IParsable<T>
    {
        Reason.Set(because);
        try
        {
            return BeParsableInto<T>();
        }
        finally
        {
            Reason.Clear();
        }
    }
}
