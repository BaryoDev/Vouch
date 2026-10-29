using System.Text.RegularExpressions;

namespace Vouch;

public readonly struct StringAssertions
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    private readonly string? _expression;

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

        var matched = false;
        var timedOut = false;
        if (Subject is not null)
        {
            try
            {
                matched = Regex.IsMatch(Subject, pattern, RegexOptions.None, RegexTimeout);
            }
            catch (RegexMatchTimeoutException)
            {
                timedOut = true;
            }
            catch (ArgumentException ex)
            {
                Fail.With($"Expected {Name} to match regex {Formatter.Format(pattern)}, but the pattern is not a valid regular expression: {ex.Message.TrimEnd('.')}.");
            }
        }

        if (timedOut)
        {
            Fail.With($"Expected {Name} to match regex {Formatter.Format(pattern)}, but the match timed out after 1 second.");
        }

        if (!matched)
        {
            Fail.With($"Expected {Name} to match regex {Formatter.Format(pattern)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<StringAssertions> HaveLength(int expected)
    {
        if (Subject is null)
        {
            Fail.With($"Expected {Name} to have length {expected}, but found null.");
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
        try
        {
            return Be(expected);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<StringAssertions> NotBe(string? unexpected, string because)
    {
        try
        {
            return NotBe(unexpected);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<StringAssertions> BeEmpty(string because)
    {
        try
        {
            return BeEmpty();
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<StringAssertions> NotBeEmpty(string because)
    {
        try
        {
            return NotBeEmpty();
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<StringAssertions> BeNull(string because)
    {
        try
        {
            return BeNull();
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<StringAssertions> NotBeNull(string because)
    {
        try
        {
            return NotBeNull();
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<StringAssertions> BeNullOrEmpty(string because)
    {
        try
        {
            return BeNullOrEmpty();
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<StringAssertions> NotBeNullOrEmpty(string because)
    {
        try
        {
            return NotBeNullOrEmpty();
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<StringAssertions> NotBeNullOrWhiteSpace(string because)
    {
        try
        {
            return NotBeNullOrWhiteSpace();
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<StringAssertions> StartWith(string expected, string because)
    {
        try
        {
            return StartWith(expected);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<StringAssertions> NotStartWith(string unexpected, string because)
    {
        try
        {
            return NotStartWith(unexpected);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<StringAssertions> EndWith(string expected, string because)
    {
        try
        {
            return EndWith(expected);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<StringAssertions> Contain(string expected, string because)
    {
        try
        {
            return Contain(expected);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<StringAssertions> NotContain(string unexpected, string because)
    {
        try
        {
            return NotContain(unexpected);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<StringAssertions> Match(string wildcardPattern, string because)
    {
        try
        {
            return Match(wildcardPattern);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<StringAssertions> MatchRegex(string pattern, string because)
    {
        try
        {
            return MatchRegex(pattern);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<StringAssertions> HaveLength(int expected, string because)
    {
        try
        {
            return HaveLength(expected);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<StringAssertions> BeEquivalentTo(string? expected, string because)
    {
        try
        {
            return BeEquivalentTo(expected);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<StringAssertions> BeOneOf(IEnumerable<string?> validValues, string because)
    {
        ArgumentNullException.ThrowIfNull(validValues);
        var values = validValues.ToArray();
        try
        {
            return BeOneOf(values);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
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
        try
        {
            return NotContainEquivalentOf(unexpected);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }
}
