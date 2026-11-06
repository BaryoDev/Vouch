namespace Vouch;

public readonly struct DateOnlyAssertions
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal DateOnlyAssertions(DateOnly subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public DateOnly Subject { get; }

    public AndConstraint<DateOnlyAssertions> Be(DateOnly expected)
    {
        if (Subject != expected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<DateOnlyAssertions> NotBe(DateOnly unexpected)
    {
        if (Subject == unexpected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be {Formatter.Format(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<DateOnlyAssertions> BeAfter(DateOnly expected)
    {
        if (!(Subject > expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be after {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<DateOnlyAssertions> BeBefore(DateOnly expected)
    {
        if (!(Subject < expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be before {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<DateOnlyAssertions> BeOnOrAfter(DateOnly expected)
    {
        if (!(Subject >= expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be on or after {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<DateOnlyAssertions> BeOnOrBefore(DateOnly expected)
    {
        if (!(Subject <= expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be on or before {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<DateOnlyAssertions> HaveYear(int expected) { DatePart.Have(_expression, "year", expected, Subject.Year); return new(this); }

    public AndConstraint<DateOnlyAssertions> NotHaveYear(int unexpected) { DatePart.NotHave(_expression, "year", unexpected, Subject.Year); return new(this); }

    public AndConstraint<DateOnlyAssertions> HaveMonth(int expected) { DatePart.Have(_expression, "month", expected, Subject.Month); return new(this); }

    public AndConstraint<DateOnlyAssertions> NotHaveMonth(int unexpected) { DatePart.NotHave(_expression, "month", unexpected, Subject.Month); return new(this); }

    public AndConstraint<DateOnlyAssertions> HaveDay(int expected) { DatePart.Have(_expression, "day", expected, Subject.Day); return new(this); }

    public AndConstraint<DateOnlyAssertions> NotHaveDay(int unexpected) { DatePart.NotHave(_expression, "day", unexpected, Subject.Day); return new(this); }

    public AndConstraint<DateOnlyAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => CheckEquivalent(expectation, EquivalencyOptions.Default);

    public AndConstraint<DateOnlyAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
        => CheckEquivalent(expectation, Equivalency.Options(configure));

    private AndConstraint<DateOnlyAssertions> CheckEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        Equivalency.Require(Subject, expectation, typeof(TExpectation), options, Fail.Name(_expression));
        return new(this);
    }
}

public readonly struct NullableDateOnlyAssertions
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal NullableDateOnlyAssertions(DateOnly? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public DateOnly? Subject { get; }

    public AndConstraint<NullableDateOnlyAssertions> Be(DateOnly expected)
    {
        if (Subject is not { } actual || actual != expected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateOnlyAssertions> NotBe(DateOnly unexpected)
    {
        if (Subject is { } actual && actual == unexpected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be {Formatter.Format(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateOnlyAssertions> BeAfter(DateOnly expected)
    {
        if (Subject is not { } actual || !(actual > expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be after {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateOnlyAssertions> BeBefore(DateOnly expected)
    {
        if (Subject is not { } actual || !(actual < expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be before {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateOnlyAssertions> BeOnOrAfter(DateOnly expected)
    {
        if (Subject is not { } actual || !(actual >= expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be on or after {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateOnlyAssertions> BeOnOrBefore(DateOnly expected)
    {
        if (Subject is not { } actual || !(actual <= expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be on or before {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableDateOnlyAssertions> HaveYear(int expected) { DatePart.Have(_expression, "year", expected, Subject?.Year); return new(this); }

    public AndConstraint<NullableDateOnlyAssertions> NotHaveYear(int unexpected) { DatePart.NotHave(_expression, "year", unexpected, Subject?.Year); return new(this); }

    public AndConstraint<NullableDateOnlyAssertions> HaveMonth(int expected) { DatePart.Have(_expression, "month", expected, Subject?.Month); return new(this); }

    public AndConstraint<NullableDateOnlyAssertions> NotHaveMonth(int unexpected) { DatePart.NotHave(_expression, "month", unexpected, Subject?.Month); return new(this); }

    public AndConstraint<NullableDateOnlyAssertions> HaveDay(int expected) { DatePart.Have(_expression, "day", expected, Subject?.Day); return new(this); }

    public AndConstraint<NullableDateOnlyAssertions> NotHaveDay(int unexpected) { DatePart.NotHave(_expression, "day", unexpected, Subject?.Day); return new(this); }

    public AndConstraint<NullableDateOnlyAssertions> BeNull()
    {
        if (Subject.HasValue)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be null, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndWhichConstraint<NullableDateOnlyAssertions, DateOnly> NotBeNull()
    {
        if (Subject is not { } value)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be null, but it was.");
            return new(this, default);
        }

        return new(this, value);
    }

    public AndConstraint<NullableDateOnlyAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => CheckEquivalent(expectation, EquivalencyOptions.Default);

    public AndConstraint<NullableDateOnlyAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
        => CheckEquivalent(expectation, Equivalency.Options(configure));

    private AndConstraint<NullableDateOnlyAssertions> CheckEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        Equivalency.Require(Subject, expectation, typeof(TExpectation), options, Fail.Name(_expression));
        return new(this);
    }
}
