namespace Vouch;

public readonly struct DateTimeAssertions
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal DateTimeAssertions(DateTime subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public DateTime Subject { get; }

    public AndConstraint<DateTimeAssertions> Be(DateTime expected)
    {
        if (Subject != expected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<DateTimeAssertions> NotBe(DateTime unexpected)
    {
        if (Subject == unexpected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be {Formatter.Format(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<DateTimeAssertions> BeAfter(DateTime expected)
    {
        if (!(Subject > expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be after {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<DateTimeAssertions> BeBefore(DateTime expected)
    {
        if (!(Subject < expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be before {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<DateTimeAssertions> BeOnOrAfter(DateTime expected)
    {
        if (!(Subject >= expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be on or after {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<DateTimeAssertions> BeOnOrBefore(DateTime expected)
    {
        if (!(Subject <= expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be on or before {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<DateTimeAssertions> BeCloseTo(DateTime expected, TimeSpan precision)
    {
        var difference = (Subject - expected).Duration();
        if (difference > precision)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be within {Formatter.Format(precision)} of {Formatter.Format(expected)}, but found {Formatter.Format(Subject)} (off by {Formatter.Format(difference)}).");
        }

        return new(this);
    }

    public AndConstraint<DateTimeAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => CheckEquivalent(expectation, EquivalencyOptions.Default);

    public AndConstraint<DateTimeAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
        => CheckEquivalent(expectation, Equivalency.Options(configure));

    private AndConstraint<DateTimeAssertions> CheckEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        Equivalency.Require(Subject, expectation, typeof(TExpectation), options, Fail.Name(_expression));
        return new(this);
    }
}

public readonly struct NullableDateTimeAssertions
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal NullableDateTimeAssertions(DateTime? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public DateTime? Subject { get; }

    public AndConstraint<NullableDateTimeAssertions> Be(DateTime expected)
    {
        if (Subject is not { } actual || actual != expected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateTimeAssertions> NotBe(DateTime unexpected)
    {
        if (Subject is { } actual && actual == unexpected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be {Formatter.Format(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateTimeAssertions> BeAfter(DateTime expected)
    {
        if (Subject is not { } actual || !(actual > expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be after {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateTimeAssertions> BeBefore(DateTime expected)
    {
        if (Subject is not { } actual || !(actual < expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be before {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateTimeAssertions> BeOnOrAfter(DateTime expected)
    {
        if (Subject is not { } actual || !(actual >= expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be on or after {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateTimeAssertions> BeOnOrBefore(DateTime expected)
    {
        if (Subject is not { } actual || !(actual <= expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be on or before {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateTimeAssertions> BeCloseTo(DateTime expected, TimeSpan precision)
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

    public AndConstraint<NullableDateTimeAssertions> BeNull()
    {
        if (Subject.HasValue)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be null, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndWhichConstraint<NullableDateTimeAssertions, DateTime> NotBeNull()
    {
        if (Subject is not { } value)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be null, but it was.");
            return new(this, default);
        }

        return new(this, value);
    }

    public AndConstraint<NullableDateTimeAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => CheckEquivalent(expectation, EquivalencyOptions.Default);

    public AndConstraint<NullableDateTimeAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
        => CheckEquivalent(expectation, Equivalency.Options(configure));

    private AndConstraint<NullableDateTimeAssertions> CheckEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        Equivalency.Require(Subject, expectation, typeof(TExpectation), options, Fail.Name(_expression));
        return new(this);
    }
}
