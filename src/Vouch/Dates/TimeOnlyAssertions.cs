namespace Vouch;

public readonly struct TimeOnlyAssertions
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal TimeOnlyAssertions(TimeOnly subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public TimeOnly Subject { get; }

    public AndConstraint<TimeOnlyAssertions> Be(TimeOnly expected)
    {
        if (Subject != expected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<TimeOnlyAssertions> NotBe(TimeOnly unexpected)
    {
        if (Subject == unexpected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be {Formatter.Format(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<TimeOnlyAssertions> BeAfter(TimeOnly expected)
    {
        if (!(Subject > expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be after {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<TimeOnlyAssertions> BeBefore(TimeOnly expected)
    {
        if (!(Subject < expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be before {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<TimeOnlyAssertions> BeOnOrAfter(TimeOnly expected)
    {
        if (!(Subject >= expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be on or after {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<TimeOnlyAssertions> BeOnOrBefore(TimeOnly expected)
    {
        if (!(Subject <= expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be on or before {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<TimeOnlyAssertions> BeOneOf(params TimeOnly[] validValues)
    {
        foreach (var value in validValues)
        {
            if (Subject == value)
            {
                return new(this);
            }
        }

        Fail.With($"Expected {Fail.Name(_expression)} to be one of {Formatter.FormatList(validValues)}, but found {Formatter.Format(Subject)}.");
        return new(this);
    }

    public AndConstraint<TimeOnlyAssertions> BeGreaterThan(TimeOnly expected)
    {
        if (!(Subject > expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be greater than {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<TimeOnlyAssertions> BeGreaterThanOrEqualTo(TimeOnly expected)
    {
        if (!(Subject >= expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be greater than or equal to {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<TimeOnlyAssertions> BeLessThan(TimeOnly expected)
    {
        if (!(Subject < expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be less than {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<TimeOnlyAssertions> BeLessThanOrEqualTo(TimeOnly expected)
    {
        if (!(Subject <= expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be less than or equal to {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<TimeOnlyAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => CheckEquivalent(expectation, EquivalencyOptions.Default);

    public AndConstraint<TimeOnlyAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
        => CheckEquivalent(expectation, Equivalency.Options(configure));

    private AndConstraint<TimeOnlyAssertions> CheckEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        Equivalency.Require(Subject, expectation, typeof(TExpectation), options, Fail.Name(_expression));
        return new(this);
    }

    public AndConstraint<TimeOnlyAssertions> Be(TimeOnly expected, string because)
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

    public AndConstraint<TimeOnlyAssertions> NotBe(TimeOnly unexpected, string because)
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
}

public readonly struct NullableTimeOnlyAssertions
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal NullableTimeOnlyAssertions(TimeOnly? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public TimeOnly? Subject { get; }

    public AndConstraint<NullableTimeOnlyAssertions> Be(TimeOnly expected)
    {
        if (Subject is not { } actual || actual != expected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableTimeOnlyAssertions> NotBe(TimeOnly unexpected)
    {
        if (Subject is { } actual && actual == unexpected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be {Formatter.Format(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<NullableTimeOnlyAssertions> BeAfter(TimeOnly expected)
    {
        if (Subject is not { } actual || !(actual > expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be after {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableTimeOnlyAssertions> BeBefore(TimeOnly expected)
    {
        if (Subject is not { } actual || !(actual < expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be before {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableTimeOnlyAssertions> BeOnOrAfter(TimeOnly expected)
    {
        if (Subject is not { } actual || !(actual >= expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be on or after {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableTimeOnlyAssertions> BeOnOrBefore(TimeOnly expected)
    {
        if (Subject is not { } actual || !(actual <= expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be on or before {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableTimeOnlyAssertions> BeNull()
    {
        if (Subject.HasValue)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be null, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    /// <summary>Same as <see cref="NotBeNull"/>: the subject must have a value, which is returned as Which.</summary>
    public AndWhichConstraint<NullableTimeOnlyAssertions, TimeOnly> HaveValue()
    {
        if (Subject is not { } value)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to have a value, but found null.");
            return new(this, default);
        }

        return new(this, value);
    }

    /// <summary>Same as <see cref="BeNull"/>: the subject must be null.</summary>
    public AndConstraint<NullableTimeOnlyAssertions> NotHaveValue()
    {
        if (Subject.HasValue)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to have a value, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndWhichConstraint<NullableTimeOnlyAssertions, TimeOnly> NotBeNull()
    {
        if (Subject is not { } value)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be null, but it was.");
            return new(this, default);
        }

        return new(this, value);
    }

    public AndConstraint<NullableTimeOnlyAssertions> BeOneOf(params TimeOnly[] validValues)
    {
        if (Subject is { } actual)
        {
            foreach (var value in validValues)
            {
                if (actual == value)
                {
                    return new(this);
                }
            }
        }

        Fail.With($"Expected {Fail.Name(_expression)} to be one of {Formatter.FormatList(validValues)}, but found {Formatter.Format(Subject)}.");
        return new(this);
    }

    public AndConstraint<NullableTimeOnlyAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => CheckEquivalent(expectation, EquivalencyOptions.Default);

    public AndConstraint<NullableTimeOnlyAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
        => CheckEquivalent(expectation, Equivalency.Options(configure));

    private AndConstraint<NullableTimeOnlyAssertions> CheckEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        Equivalency.Require(Subject, expectation, typeof(TExpectation), options, Fail.Name(_expression));
        return new(this);
    }

    public AndConstraint<NullableTimeOnlyAssertions> Be(TimeOnly? expected)
    {
        if (expected is { } value)
        {
            return Be(value);
        }

        if (Subject is not null)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be null, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableTimeOnlyAssertions> Be(TimeOnly? expected, string because)
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

    public AndConstraint<NullableTimeOnlyAssertions> Be(TimeOnly expected, string because)
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

    public AndConstraint<NullableTimeOnlyAssertions> NotBe(TimeOnly unexpected, string because)
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
}
