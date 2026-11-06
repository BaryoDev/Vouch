namespace Vouch;

/// <summary>How many times something must occur. Build one with <see cref="Exactly"/>, <see cref="AtLeast"/>, <see cref="AtMost"/>, <see cref="MoreThan"/> or <see cref="LessThan"/>.</summary>
public readonly struct OccurrenceConstraint
{
    private readonly Kind _kind;

    internal OccurrenceConstraint(Kind kind, int times)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(times);
        _kind = kind;
        Times = times;
    }

    internal enum Kind
    {
        Exactly,
        AtLeast,
        AtMost,
        MoreThan,
        LessThan,
    }

    public int Times { get; }

    public bool IsSatisfiedBy(int actual) => _kind switch
    {
        Kind.Exactly => actual == Times,
        Kind.AtLeast => actual >= Times,
        Kind.AtMost => actual <= Times,
        Kind.MoreThan => actual > Times,
        Kind.LessThan => actual < Times,
        _ => false,
    };

    internal string Describe()
    {
        var prefix = _kind switch
        {
            Kind.Exactly => "exactly",
            Kind.AtLeast => "at least",
            Kind.AtMost => "at most",
            Kind.MoreThan => "more than",
            _ => "less than",
        };
        return $"{prefix} {Count(Times)}";
    }

    internal static string Count(int n) => n == 1 ? "1 time" : $"{n} times";
}

public static class Exactly
{
    public static OccurrenceConstraint Once() => Times(1);

    public static OccurrenceConstraint Twice() => Times(2);

    public static OccurrenceConstraint Times(int times) => new(OccurrenceConstraint.Kind.Exactly, times);
}

public static class AtLeast
{
    public static OccurrenceConstraint Once() => Times(1);

    public static OccurrenceConstraint Twice() => Times(2);

    public static OccurrenceConstraint Times(int times) => new(OccurrenceConstraint.Kind.AtLeast, times);
}

public static class AtMost
{
    public static OccurrenceConstraint Once() => Times(1);

    public static OccurrenceConstraint Twice() => Times(2);

    public static OccurrenceConstraint Times(int times) => new(OccurrenceConstraint.Kind.AtMost, times);
}

public static class MoreThan
{
    public static OccurrenceConstraint Once() => Times(1);

    public static OccurrenceConstraint Twice() => Times(2);

    public static OccurrenceConstraint Times(int times) => new(OccurrenceConstraint.Kind.MoreThan, times);
}

public static class LessThan
{
    public static OccurrenceConstraint Once() => Times(1);

    public static OccurrenceConstraint Twice() => Times(2);

    public static OccurrenceConstraint Times(int times) => new(OccurrenceConstraint.Kind.LessThan, times);
}
