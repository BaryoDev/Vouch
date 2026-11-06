namespace Vouch;

public readonly struct ValueAssertions<T> where T : struct
{
    private readonly string? _expression;

    internal ValueAssertions(T subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public T Subject { get; }

    internal string Name => Fail.Name(_expression);

    public AndConstraint<ValueAssertions<T>> Be(T expected)
    {
        if (!EqualityComparer<T>.Default.Equals(Subject, expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<ValueAssertions<T>> NotBe(T unexpected)
    {
        if (EqualityComparer<T>.Default.Equals(Subject, unexpected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be {Formatter.Format(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<ValueAssertions<T>> BeOneOf(params T[] validValues)
    {
        var comparer = EqualityComparer<T>.Default;
        foreach (var value in validValues)
        {
            if (comparer.Equals(Subject, value))
            {
                return new(this);
            }
        }

        Fail.With($"Expected {Fail.Name(_expression)} to be one of {Formatter.FormatList(validValues)}, but found {Formatter.Format(Subject)}.");
        return new(this);
    }

    public AndConstraint<ValueAssertions<T>> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => CheckEquivalent(expectation, EquivalencyOptions.Default);

    public AndConstraint<ValueAssertions<T>> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
        => CheckEquivalent(expectation, Equivalency.Options(configure));

    private AndConstraint<ValueAssertions<T>> CheckEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        Equivalency.Require(Subject, expectation, typeof(TExpectation), options, Fail.Name(_expression));
        return new(this);
    }

    public AndConstraint<ValueAssertions<T>> Be(T expected, string because)
    {
        try
        {
            return Be(expected);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<ValueAssertions<T>> NotBe(T unexpected, string because)
    {
        try
        {
            return NotBe(unexpected);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }
}
