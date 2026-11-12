namespace Vouch;

public static class TimeSpanAssertionsExtensions
{
    internal static void ThrowIfNegative(TimeSpan precision)
    {
        if (precision < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(precision), precision, "Precision must not be negative.");
        }
    }

    public static AndConstraint<ValueAssertions<TimeSpan>> BeCloseTo(this ValueAssertions<TimeSpan> assertions, TimeSpan expected, TimeSpan precision)
    {
        ThrowIfNegative(precision);

        var ticks = (Int128)assertions.Subject.Ticks - expected.Ticks;
        if (ticks < 0)
        {
            ticks = -ticks;
        }

        if (ticks > precision.Ticks)
        {
            Fail.With($"Expected {assertions.Name} to be within {Formatter.Format(precision)} of {Formatter.Format(expected)}, but found {Formatter.Format(assertions.Subject)} (off by {(ticks <= long.MaxValue ? Formatter.Format(TimeSpan.FromTicks((long)ticks)) : $"{ticks} ticks")}).");
        }

        return new(assertions);
    }

    public static AndConstraint<ValueAssertions<TimeSpan>> BePositive(this ValueAssertions<TimeSpan> assertions)
    {
        if (assertions.Subject <= TimeSpan.Zero)
        {
            Fail.With($"Expected {assertions.Name} to be positive, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<ValueAssertions<TimeSpan>> BeNegative(this ValueAssertions<TimeSpan> assertions)
    {
        if (assertions.Subject >= TimeSpan.Zero)
        {
            Fail.With($"Expected {assertions.Name} to be negative, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }
}
