namespace Vouch;

public readonly struct DateTimeOffsetAssertions
{
    private readonly string? _expression;

    internal DateTimeOffsetAssertions(DateTimeOffset subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public DateTimeOffset Subject { get; }

    public AndConstraint<DateTimeOffsetAssertions> Be(DateTimeOffset expected)
    {
        if (Subject != expected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<DateTimeOffsetAssertions> NotBe(DateTimeOffset unexpected)
    {
        if (Subject == unexpected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be {Formatter.Format(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<DateTimeOffsetAssertions> BeAfter(DateTimeOffset expected)
    {
        if (!(Subject > expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be after {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<DateTimeOffsetAssertions> BeBefore(DateTimeOffset expected)
    {
        if (!(Subject < expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be before {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<DateTimeOffsetAssertions> BeOnOrAfter(DateTimeOffset expected)
    {
        if (!(Subject >= expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be on or after {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<DateTimeOffsetAssertions> BeOnOrBefore(DateTimeOffset expected)
    {
        if (!(Subject <= expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be on or before {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<DateTimeOffsetAssertions> BeCloseTo(DateTimeOffset expected, TimeSpan precision)
    {
        var difference = (Subject - expected).Duration();
        if (difference > precision)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be within {Formatter.Format(precision)} of {Formatter.Format(expected)}, but found {Formatter.Format(Subject)} (off by {Formatter.Format(difference)}).");
        }

        return new(this);
    }

    public AndConstraint<DateTimeOffsetAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => CheckEquivalent(expectation, EquivalencyOptions.Default);

    public AndConstraint<DateTimeOffsetAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
        => CheckEquivalent(expectation, Equivalency.Options(configure));

    private AndConstraint<DateTimeOffsetAssertions> CheckEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        Equivalency.Require(Subject, expectation, typeof(TExpectation), options, Fail.Name(_expression));
        return new(this);
    }
}

public readonly struct NullableDateTimeOffsetAssertions
{
    private readonly string? _expression;

    internal NullableDateTimeOffsetAssertions(DateTimeOffset? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public DateTimeOffset? Subject { get; }

    public AndConstraint<NullableDateTimeOffsetAssertions> Be(DateTimeOffset expected)
    {
        if (Subject is not { } actual || actual != expected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateTimeOffsetAssertions> NotBe(DateTimeOffset unexpected)
    {
        if (Subject is { } actual && actual == unexpected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be {Formatter.Format(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateTimeOffsetAssertions> BeAfter(DateTimeOffset expected)
    {
        if (Subject is not { } actual || !(actual > expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be after {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateTimeOffsetAssertions> BeBefore(DateTimeOffset expected)
    {
        if (Subject is not { } actual || !(actual < expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be before {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateTimeOffsetAssertions> BeOnOrAfter(DateTimeOffset expected)
    {
        if (Subject is not { } actual || !(actual >= expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be on or after {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateTimeOffsetAssertions> BeOnOrBefore(DateTimeOffset expected)
    {
        if (Subject is not { } actual || !(actual <= expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be on or before {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateTimeOffsetAssertions> BeCloseTo(DateTimeOffset expected, TimeSpan precision)
    {
        if (Subject is not { } actual)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be within {Formatter.Format(precision)} of {Formatter.Format(expected)}, but found null.");
            return new(this);
        }

        var difference = (actual - expected).Duration();
        if (difference > precision)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be within {Formatter.Format(precision)} of {Formatter.Format(expected)}, but found {Formatter.Format(actual)} (off by {Formatter.Format(difference)}).");
        }

        return new(this);
    }

    public AndConstraint<NullableDateTimeOffsetAssertions> BeNull()
    {
        if (Subject.HasValue)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be null, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndWhichConstraint<NullableDateTimeOffsetAssertions, DateTimeOffset> NotBeNull()
    {
        if (Subject is not { } value)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be null, but it was.");
            return default;
        }

        return new(this, value);
    }

    public AndConstraint<NullableDateTimeOffsetAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => CheckEquivalent(expectation, EquivalencyOptions.Default);

    public AndConstraint<NullableDateTimeOffsetAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
        => CheckEquivalent(expectation, Equivalency.Options(configure));

    private AndConstraint<NullableDateTimeOffsetAssertions> CheckEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        Equivalency.Require(Subject, expectation, typeof(TExpectation), options, Fail.Name(_expression));
        return new(this);
    }
}
