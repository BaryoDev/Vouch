namespace Vouch;

public readonly struct NullableValueAssertions<T> where T : struct
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal NullableValueAssertions(T? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public T? Subject { get; }

    public AndConstraint<NullableValueAssertions<T>> Be(T expected)
    {
        if (Subject is not { } actual || !EqualityComparer<T>.Default.Equals(actual, expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableValueAssertions<T>> NotBe(T unexpected)
    {
        if (Subject is { } actual && EqualityComparer<T>.Default.Equals(actual, unexpected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be {Formatter.Format(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<NullableValueAssertions<T>> BeOneOf(params T[] validValues)
    {
        if (Subject is { } actual)
        {
            var comparer = EqualityComparer<T>.Default;
            foreach (var value in validValues)
            {
                if (comparer.Equals(actual, value))
                {
                    return new(this);
                }
            }
        }

        Fail.With($"Expected {Fail.Name(_expression)} to be one of {Formatter.FormatList(validValues)}, but found {Formatter.Format(Subject)}.");
        return new(this);
    }

    public AndConstraint<NullableValueAssertions<T>> BeNull()
    {
        if (Subject.HasValue)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be null, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndWhichConstraint<NullableValueAssertions<T>, T> NotBeNull()
    {
        if (Subject is not { } value)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be null, but it was.");
            return new(this, default);
        }

        return new(this, value);
    }

    public AndConstraint<NullableValueAssertions<T>> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => CheckEquivalent(expectation, EquivalencyOptions.Default);

    public AndConstraint<NullableValueAssertions<T>> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
        => CheckEquivalent(expectation, Equivalency.Options(configure));

    private AndConstraint<NullableValueAssertions<T>> CheckEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        Equivalency.Require(Subject, expectation, typeof(TExpectation), options, Fail.Name(_expression));
        return new(this);
    }

    public AndConstraint<NullableValueAssertions<T>> Be(T expected, string because)
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

    public AndConstraint<NullableValueAssertions<T>> NotBe(T unexpected, string because)
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

    public AndConstraint<NullableValueAssertions<T>> Be(T? expected)
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

    public AndConstraint<NullableValueAssertions<T>> Be(T? expected, string because)
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
}
