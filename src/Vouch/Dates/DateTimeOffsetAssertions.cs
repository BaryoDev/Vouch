namespace Vouch;

public readonly struct DateTimeOffsetAssertions
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

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
        TimeSpanAssertionsExtensions.ThrowIfNegative(precision);
        var difference = (Subject - expected).Duration();
        if (difference > precision)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be within {Formatter.Format(precision)} of {Formatter.Format(expected)}, but found {Formatter.Format(Subject)} (off by {Formatter.Format(difference)}).");
        }

        return new(this);
    }

    /// <summary>
    /// Compares the calendar dates in each value's own offset (its local <c>Date</c>), as FluentAssertions does.
    /// The instants are not converted to a common offset, so 23:30+02:00 and 22:30+01:00 on the next day differ.
    /// </summary>
    public AndConstraint<DateTimeOffsetAssertions> BeSameDateAs(DateTimeOffset expected)
    {
        if (Subject.Date != expected.Date)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be the same date as {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<DateTimeOffsetAssertions> NotBeSameDateAs(DateTimeOffset unexpected)
    {
        if (Subject.Date == unexpected.Date)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be the same date as {Formatter.Format(unexpected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    /// <summary>
    /// HaveYear through HaveSecond (and their Not variants) check the offset's local fields, not UTC,
    /// as FluentAssertions does: 23:30+02:00 has hour 23 even though it is 21:30 UTC.
    /// </summary>
    public AndConstraint<DateTimeOffsetAssertions> HaveYear(int expected) { DatePart.Have(_expression, "year", expected, Subject.Year); return new(this); }

    public AndConstraint<DateTimeOffsetAssertions> NotHaveYear(int unexpected) { DatePart.NotHave(_expression, "year", unexpected, Subject.Year); return new(this); }

    public AndConstraint<DateTimeOffsetAssertions> HaveMonth(int expected) { DatePart.Have(_expression, "month", expected, Subject.Month); return new(this); }

    public AndConstraint<DateTimeOffsetAssertions> NotHaveMonth(int unexpected) { DatePart.NotHave(_expression, "month", unexpected, Subject.Month); return new(this); }

    public AndConstraint<DateTimeOffsetAssertions> HaveDay(int expected) { DatePart.Have(_expression, "day", expected, Subject.Day); return new(this); }

    public AndConstraint<DateTimeOffsetAssertions> NotHaveDay(int unexpected) { DatePart.NotHave(_expression, "day", unexpected, Subject.Day); return new(this); }

    public AndConstraint<DateTimeOffsetAssertions> HaveHour(int expected) { DatePart.Have(_expression, "hour", expected, Subject.Hour); return new(this); }

    public AndConstraint<DateTimeOffsetAssertions> NotHaveHour(int unexpected) { DatePart.NotHave(_expression, "hour", unexpected, Subject.Hour); return new(this); }

    public AndConstraint<DateTimeOffsetAssertions> HaveMinute(int expected) { DatePart.Have(_expression, "minute", expected, Subject.Minute); return new(this); }

    public AndConstraint<DateTimeOffsetAssertions> NotHaveMinute(int unexpected) { DatePart.NotHave(_expression, "minute", unexpected, Subject.Minute); return new(this); }

    public AndConstraint<DateTimeOffsetAssertions> HaveSecond(int expected) { DatePart.Have(_expression, "second", expected, Subject.Second); return new(this); }

    public AndConstraint<DateTimeOffsetAssertions> NotHaveSecond(int unexpected) { DatePart.NotHave(_expression, "second", unexpected, Subject.Second); return new(this); }

    /// <exception cref="ArgumentOutOfRangeException">The span is negative.</exception>
    public DateTimeOffsetWithin BeWithin(TimeSpan timeSpan) => new(this, _expression, DatePart.RequireNonNegative(timeSpan));

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

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

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
        TimeSpanAssertionsExtensions.ThrowIfNegative(precision);
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

    /// <summary>Same as <see cref="NotBeNull"/>: the subject must have a value, which is returned as Which.</summary>
    public AndWhichConstraint<NullableDateTimeOffsetAssertions, DateTimeOffset> HaveValue()
    {
        if (Subject is not { } value)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to have a value, but found null.");
            return new(this, default);
        }

        return new(this, value);
    }

    /// <summary>Same as <see cref="BeNull"/>: the subject must be null.</summary>
    public AndConstraint<NullableDateTimeOffsetAssertions> NotHaveValue()
    {
        if (Subject.HasValue)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to have a value, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndWhichConstraint<NullableDateTimeOffsetAssertions, DateTimeOffset> NotBeNull()
    {
        if (Subject is not { } value)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be null, but it was.");
            return new(this, default);
        }

        return new(this, value);
    }

    /// <summary>
    /// Compares the calendar dates in each value's own offset (its local <c>Date</c>), as FluentAssertions does.
    /// The instants are not converted to a common offset.
    /// </summary>
    public AndConstraint<NullableDateTimeOffsetAssertions> BeSameDateAs(DateTimeOffset expected)
    {
        if (Subject is not { } actual || actual.Date != expected.Date)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be the same date as {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateTimeOffsetAssertions> NotBeSameDateAs(DateTimeOffset unexpected)
    {
        if (Subject is { } actual && actual.Date == unexpected.Date)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be the same date as {Formatter.Format(unexpected)}, but found {Formatter.Format(actual)}.");
        }

        return new(this);
    }

    /// <summary>
    /// HaveYear through HaveSecond (and their Not variants) check the offset's local fields, not UTC,
    /// as FluentAssertions does.
    /// </summary>
    public AndConstraint<NullableDateTimeOffsetAssertions> HaveYear(int expected) { DatePart.Have(_expression, "year", expected, Subject?.Year); return new(this); }

    public AndConstraint<NullableDateTimeOffsetAssertions> NotHaveYear(int unexpected) { DatePart.NotHave(_expression, "year", unexpected, Subject?.Year); return new(this); }

    public AndConstraint<NullableDateTimeOffsetAssertions> HaveMonth(int expected) { DatePart.Have(_expression, "month", expected, Subject?.Month); return new(this); }

    public AndConstraint<NullableDateTimeOffsetAssertions> NotHaveMonth(int unexpected) { DatePart.NotHave(_expression, "month", unexpected, Subject?.Month); return new(this); }

    public AndConstraint<NullableDateTimeOffsetAssertions> HaveDay(int expected) { DatePart.Have(_expression, "day", expected, Subject?.Day); return new(this); }

    public AndConstraint<NullableDateTimeOffsetAssertions> NotHaveDay(int unexpected) { DatePart.NotHave(_expression, "day", unexpected, Subject?.Day); return new(this); }

    public AndConstraint<NullableDateTimeOffsetAssertions> HaveHour(int expected) { DatePart.Have(_expression, "hour", expected, Subject?.Hour); return new(this); }

    public AndConstraint<NullableDateTimeOffsetAssertions> NotHaveHour(int unexpected) { DatePart.NotHave(_expression, "hour", unexpected, Subject?.Hour); return new(this); }

    public AndConstraint<NullableDateTimeOffsetAssertions> HaveMinute(int expected) { DatePart.Have(_expression, "minute", expected, Subject?.Minute); return new(this); }

    public AndConstraint<NullableDateTimeOffsetAssertions> NotHaveMinute(int unexpected) { DatePart.NotHave(_expression, "minute", unexpected, Subject?.Minute); return new(this); }

    public AndConstraint<NullableDateTimeOffsetAssertions> HaveSecond(int expected) { DatePart.Have(_expression, "second", expected, Subject?.Second); return new(this); }

    public AndConstraint<NullableDateTimeOffsetAssertions> NotHaveSecond(int unexpected) { DatePart.NotHave(_expression, "second", unexpected, Subject?.Second); return new(this); }

    /// <exception cref="ArgumentOutOfRangeException">The span is negative.</exception>
    public NullableDateTimeOffsetWithin BeWithin(TimeSpan timeSpan) => new(this, _expression, DatePart.RequireNonNegative(timeSpan));

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
