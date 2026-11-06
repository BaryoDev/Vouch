namespace Vouch;

public readonly struct DateTimeWithin
{
    private readonly DateTimeAssertions _parent;
    private readonly string? _expression;
    private readonly TimeSpan _span;

    internal DateTimeWithin(DateTimeAssertions parent, string? expression, TimeSpan span)
    {
        _parent = parent;
        _expression = expression;
        _span = span;
    }

    public AndConstraint<DateTimeAssertions> Before(DateTime target)
    {
        if (!DatePart.InWindow(target - _parent.Subject, _span))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be within {Formatter.Format(_span)} before {Formatter.Format(target)}, but found {Formatter.Format(_parent.Subject)}.");
        }

        return new(_parent);
    }

    public AndConstraint<DateTimeAssertions> After(DateTime target)
    {
        if (!DatePart.InWindow(_parent.Subject - target, _span))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be within {Formatter.Format(_span)} after {Formatter.Format(target)}, but found {Formatter.Format(_parent.Subject)}.");
        }

        return new(_parent);
    }
}

public readonly struct NullableDateTimeWithin
{
    private readonly NullableDateTimeAssertions _parent;
    private readonly string? _expression;
    private readonly TimeSpan _span;

    internal NullableDateTimeWithin(NullableDateTimeAssertions parent, string? expression, TimeSpan span)
    {
        _parent = parent;
        _expression = expression;
        _span = span;
    }

    public AndConstraint<NullableDateTimeAssertions> Before(DateTime target)
    {
        if (_parent.Subject is not { } actual || !DatePart.InWindow(target - actual, _span))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be within {Formatter.Format(_span)} before {Formatter.Format(target)}, but found {Formatter.Format(_parent.Subject)}.");
        }

        return new(_parent);
    }

    public AndConstraint<NullableDateTimeAssertions> After(DateTime target)
    {
        if (_parent.Subject is not { } actual || !DatePart.InWindow(actual - target, _span))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be within {Formatter.Format(_span)} after {Formatter.Format(target)}, but found {Formatter.Format(_parent.Subject)}.");
        }

        return new(_parent);
    }
}

public readonly struct DateTimeOffsetWithin
{
    private readonly DateTimeOffsetAssertions _parent;
    private readonly string? _expression;
    private readonly TimeSpan _span;

    internal DateTimeOffsetWithin(DateTimeOffsetAssertions parent, string? expression, TimeSpan span)
    {
        _parent = parent;
        _expression = expression;
        _span = span;
    }

    public AndConstraint<DateTimeOffsetAssertions> Before(DateTimeOffset target)
    {
        if (!DatePart.InWindow(target - _parent.Subject, _span))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be within {Formatter.Format(_span)} before {Formatter.Format(target)}, but found {Formatter.Format(_parent.Subject)}.");
        }

        return new(_parent);
    }

    public AndConstraint<DateTimeOffsetAssertions> After(DateTimeOffset target)
    {
        if (!DatePart.InWindow(_parent.Subject - target, _span))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be within {Formatter.Format(_span)} after {Formatter.Format(target)}, but found {Formatter.Format(_parent.Subject)}.");
        }

        return new(_parent);
    }
}

public readonly struct NullableDateTimeOffsetWithin
{
    private readonly NullableDateTimeOffsetAssertions _parent;
    private readonly string? _expression;
    private readonly TimeSpan _span;

    internal NullableDateTimeOffsetWithin(NullableDateTimeOffsetAssertions parent, string? expression, TimeSpan span)
    {
        _parent = parent;
        _expression = expression;
        _span = span;
    }

    public AndConstraint<NullableDateTimeOffsetAssertions> Before(DateTimeOffset target)
    {
        if (_parent.Subject is not { } actual || !DatePart.InWindow(target - actual, _span))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be within {Formatter.Format(_span)} before {Formatter.Format(target)}, but found {Formatter.Format(_parent.Subject)}.");
        }

        return new(_parent);
    }

    public AndConstraint<NullableDateTimeOffsetAssertions> After(DateTimeOffset target)
    {
        if (_parent.Subject is not { } actual || !DatePart.InWindow(actual - target, _span))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be within {Formatter.Format(_span)} after {Formatter.Format(target)}, but found {Formatter.Format(_parent.Subject)}.");
        }

        return new(_parent);
    }
}

internal static class DatePart
{
    public static void Have(string? expression, string part, int expected, int? actual)
    {
        if (actual != expected)
        {
            Fail.With($"Expected {Fail.Name(expression)} to have {part} {expected}, but found {(actual.HasValue ? actual.Value.ToString() : "null")}.");
        }
    }

    public static void NotHave(string? expression, string part, int unexpected, int? actual)
    {
        if (actual == unexpected)
        {
            Fail.With($"Expected {Fail.Name(expression)} not to have {part} {unexpected}, but it did.");
        }
    }

    public static bool InWindow(TimeSpan difference, TimeSpan span) => difference >= TimeSpan.Zero && difference <= span;
}
