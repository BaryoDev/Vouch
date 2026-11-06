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

    public AndConstraint<DateTimeAssertions> BeSameDateAs(DateTime expected)
    {
        if (Subject.Date != expected.Date)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be the same date as {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<DateTimeAssertions> NotBeSameDateAs(DateTime unexpected)
    {
        if (Subject.Date == unexpected.Date)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be the same date as {Formatter.Format(unexpected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<DateTimeAssertions> HaveYear(int expected) { DatePart.Have(_expression, "year", expected, Subject.Year); return new(this); }

    public AndConstraint<DateTimeAssertions> NotHaveYear(int unexpected) { DatePart.NotHave(_expression, "year", unexpected, Subject.Year); return new(this); }

    public AndConstraint<DateTimeAssertions> HaveMonth(int expected) { DatePart.Have(_expression, "month", expected, Subject.Month); return new(this); }

    public AndConstraint<DateTimeAssertions> NotHaveMonth(int unexpected) { DatePart.NotHave(_expression, "month", unexpected, Subject.Month); return new(this); }

    public AndConstraint<DateTimeAssertions> HaveDay(int expected) { DatePart.Have(_expression, "day", expected, Subject.Day); return new(this); }

    public AndConstraint<DateTimeAssertions> NotHaveDay(int unexpected) { DatePart.NotHave(_expression, "day", unexpected, Subject.Day); return new(this); }

    public AndConstraint<DateTimeAssertions> HaveHour(int expected) { DatePart.Have(_expression, "hour", expected, Subject.Hour); return new(this); }

    public AndConstraint<DateTimeAssertions> NotHaveHour(int unexpected) { DatePart.NotHave(_expression, "hour", unexpected, Subject.Hour); return new(this); }

    public AndConstraint<DateTimeAssertions> HaveMinute(int expected) { DatePart.Have(_expression, "minute", expected, Subject.Minute); return new(this); }

    public AndConstraint<DateTimeAssertions> NotHaveMinute(int unexpected) { DatePart.NotHave(_expression, "minute", unexpected, Subject.Minute); return new(this); }

    public AndConstraint<DateTimeAssertions> HaveSecond(int expected) { DatePart.Have(_expression, "second", expected, Subject.Second); return new(this); }

    public AndConstraint<DateTimeAssertions> NotHaveSecond(int unexpected) { DatePart.NotHave(_expression, "second", unexpected, Subject.Second); return new(this); }

    public DateTimeWithin BeWithin(TimeSpan timeSpan) => new(this, _expression, timeSpan);

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

    public AndConstraint<NullableDateTimeAssertions> BeSameDateAs(DateTime expected)
    {
        if (Subject is not { } actual || actual.Date != expected.Date)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be the same date as {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateTimeAssertions> NotBeSameDateAs(DateTime unexpected)
    {
        if (Subject is { } actual && actual.Date == unexpected.Date)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be the same date as {Formatter.Format(unexpected)}, but found {Formatter.Format(actual)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateTimeAssertions> HaveYear(int expected) { DatePart.Have(_expression, "year", expected, Subject?.Year); return new(this); }

    public AndConstraint<NullableDateTimeAssertions> NotHaveYear(int unexpected) { DatePart.NotHave(_expression, "year", unexpected, Subject?.Year); return new(this); }

    public AndConstraint<NullableDateTimeAssertions> HaveMonth(int expected) { DatePart.Have(_expression, "month", expected, Subject?.Month); return new(this); }

    public AndConstraint<NullableDateTimeAssertions> NotHaveMonth(int unexpected) { DatePart.NotHave(_expression, "month", unexpected, Subject?.Month); return new(this); }

    public AndConstraint<NullableDateTimeAssertions> HaveDay(int expected) { DatePart.Have(_expression, "day", expected, Subject?.Day); return new(this); }

    public AndConstraint<NullableDateTimeAssertions> NotHaveDay(int unexpected) { DatePart.NotHave(_expression, "day", unexpected, Subject?.Day); return new(this); }

    public AndConstraint<NullableDateTimeAssertions> HaveHour(int expected) { DatePart.Have(_expression, "hour", expected, Subject?.Hour); return new(this); }

    public AndConstraint<NullableDateTimeAssertions> NotHaveHour(int unexpected) { DatePart.NotHave(_expression, "hour", unexpected, Subject?.Hour); return new(this); }

    public AndConstraint<NullableDateTimeAssertions> HaveMinute(int expected) { DatePart.Have(_expression, "minute", expected, Subject?.Minute); return new(this); }

    public AndConstraint<NullableDateTimeAssertions> NotHaveMinute(int unexpected) { DatePart.NotHave(_expression, "minute", unexpected, Subject?.Minute); return new(this); }

    public AndConstraint<NullableDateTimeAssertions> HaveSecond(int expected) { DatePart.Have(_expression, "second", expected, Subject?.Second); return new(this); }

    public AndConstraint<NullableDateTimeAssertions> NotHaveSecond(int unexpected) { DatePart.NotHave(_expression, "second", unexpected, Subject?.Second); return new(this); }

    public NullableDateTimeWithin BeWithin(TimeSpan timeSpan) => new(this, _expression, timeSpan);

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
