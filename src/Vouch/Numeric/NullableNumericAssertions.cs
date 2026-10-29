using System.Numerics;

namespace Vouch;

public readonly struct NullableNumericAssertions<T> where T : struct, INumber<T>
{
    private readonly string? _expression;

    internal NullableNumericAssertions(T? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public T? Subject { get; }

    internal string Name => Fail.Name(_expression);

    public AndConstraint<NullableNumericAssertions<T>> Be(T expected)
    {
        if (Subject is not { } actual || !actual.Equals(expected))
        {
            Fail.With($"Expected {Name} to be {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableNumericAssertions<T>> NotBe(T unexpected)
    {
        if (Subject is { } actual && actual.Equals(unexpected))
        {
            Fail.With($"Expected {Name} not to be {Formatter.Format(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<NullableNumericAssertions<T>> BeGreaterThan(T expected)
    {
        if (Subject is not { } actual || !(actual > expected))
        {
            Fail.With($"Expected {Name} to be greater than {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableNumericAssertions<T>> BeGreaterThanOrEqualTo(T expected)
    {
        if (Subject is not { } actual || !(actual >= expected))
        {
            Fail.With($"Expected {Name} to be greater than or equal to {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableNumericAssertions<T>> BeLessThan(T expected)
    {
        if (Subject is not { } actual || !(actual < expected))
        {
            Fail.With($"Expected {Name} to be less than {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableNumericAssertions<T>> BeLessThanOrEqualTo(T expected)
    {
        if (Subject is not { } actual || !(actual <= expected))
        {
            Fail.With($"Expected {Name} to be less than or equal to {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableNumericAssertions<T>> BePositive()
    {
        if (Subject is not { } actual || !(actual > T.Zero))
        {
            Fail.With($"Expected {Name} to be positive, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableNumericAssertions<T>> BeInRange(T min, T max)
    {
        if (Subject is not { } actual || !(actual >= min && actual <= max))
        {
            Fail.With($"Expected {Name} to be between {Formatter.Format(min)} and {Formatter.Format(max)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableNumericAssertions<T>> BeOneOf(params T[] validValues)
    {
        if (Subject is { } actual)
        {
            foreach (var value in validValues)
            {
                if (actual.Equals(value))
                {
                    return new(this);
                }
            }
        }

        Fail.With($"Expected {Name} to be one of {Formatter.FormatList(validValues)}, but found {Formatter.Format(Subject)}.");
        return new(this);
    }

    public AndConstraint<NullableNumericAssertions<T>> BeNull()
    {
        if (Subject.HasValue)
        {
            Fail.With($"Expected {Name} to be null, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndWhichConstraint<NullableNumericAssertions<T>, T> NotBeNull()
    {
        if (Subject is not { } value)
        {
            Fail.With($"Expected {Name} not to be null, but it was.");
            return default;
        }

        return new(this, value);
    }

    public AndConstraint<NullableNumericAssertions<T>> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => CheckEquivalent(expectation, EquivalencyOptions.Default);

    public AndConstraint<NullableNumericAssertions<T>> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions, EquivalencyOptions> configure)
        => CheckEquivalent(expectation, Equivalency.Options(configure));

    private AndConstraint<NullableNumericAssertions<T>> CheckEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        Equivalency.Require(Subject, expectation, typeof(TExpectation), options, Fail.Name(_expression));
        return new(this);
    }

    public AndConstraint<NullableNumericAssertions<T>> Be(T expected, string because)
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

    public AndConstraint<NullableNumericAssertions<T>> BeGreaterThan(T expected, string because)
    {
        try
        {
            return BeGreaterThan(expected);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }
}

public static class NullableNumericApproximationExtensions
{
    public static AndConstraint<NullableNumericAssertions<decimal>> BeApproximately(this NullableNumericAssertions<decimal> assertions, decimal expected, decimal precision)
        => Approximate(assertions, expected, precision);

    public static AndConstraint<NullableNumericAssertions<double>> BeApproximately(this NullableNumericAssertions<double> assertions, double expected, double precision)
        => Approximate(assertions, expected, precision);

    public static AndConstraint<NullableNumericAssertions<float>> BeApproximately(this NullableNumericAssertions<float> assertions, float expected, float precision)
        => Approximate(assertions, expected, precision);

    private static AndConstraint<NullableNumericAssertions<T>> Approximate<T>(NullableNumericAssertions<T> assertions, T expected, T precision)
        where T : struct, INumber<T>
    {
        if (assertions.Subject is not { } actual)
        {
            Fail.With($"Expected {assertions.Name} to approximate {Formatter.Format(expected)} +/- {Formatter.Format(precision)}, but found null.");
            return new(assertions);
        }

        if (!NumericDifference.IsWithin(actual, expected, precision, out var detail))
        {
            Fail.With($"Expected {assertions.Name} to approximate {Formatter.Format(expected)} +/- {Formatter.Format(precision)}, but found {Formatter.Format(actual)} {detail}.");
        }

        return new(assertions);
    }
}
