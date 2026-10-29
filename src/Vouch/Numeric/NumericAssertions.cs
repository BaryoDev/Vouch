using System.Numerics;

namespace Vouch;

public readonly struct NumericAssertions<T> where T : struct, INumber<T>
{
    private readonly string? _expression;

    internal NumericAssertions(T subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public T Subject { get; }

    internal string Name => Fail.Name(_expression);

    public AndConstraint<NumericAssertions<T>> Be(T expected)
    {
        if (!Subject.Equals(expected))
        {
            Fail.With($"Expected {Name} to be {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NumericAssertions<T>> NotBe(T unexpected)
    {
        if (Subject.Equals(unexpected))
        {
            Fail.With($"Expected {Name} not to be {Formatter.Format(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<NumericAssertions<T>> BeGreaterThan(T expected)
    {
        if (!(Subject > expected))
        {
            Fail.With($"Expected {Name} to be greater than {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NumericAssertions<T>> BeGreaterThanOrEqualTo(T expected)
    {
        if (!(Subject >= expected))
        {
            Fail.With($"Expected {Name} to be greater than or equal to {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NumericAssertions<T>> BeLessThan(T expected)
    {
        if (!(Subject < expected))
        {
            Fail.With($"Expected {Name} to be less than {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NumericAssertions<T>> BeLessThanOrEqualTo(T expected)
    {
        if (!(Subject <= expected))
        {
            Fail.With($"Expected {Name} to be less than or equal to {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NumericAssertions<T>> BePositive()
    {
        if (!(Subject > T.Zero))
        {
            Fail.With($"Expected {Name} to be positive, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NumericAssertions<T>> BeInRange(T min, T max)
    {
        if (!(Subject >= min && Subject <= max))
        {
            Fail.With($"Expected {Name} to be between {Formatter.Format(min)} and {Formatter.Format(max)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NumericAssertions<T>> BeOneOf(params T[] validValues)
    {
        foreach (var value in validValues)
        {
            if (Subject.Equals(value))
            {
                return new(this);
            }
        }

        Fail.With($"Expected {Name} to be one of {Formatter.FormatList(validValues)}, but found {Formatter.Format(Subject)}.");
        return new(this);
    }

    public AndConstraint<NumericAssertions<T>> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => CheckEquivalent(expectation, EquivalencyOptions.Default);

    public AndConstraint<NumericAssertions<T>> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions, EquivalencyOptions> configure)
        => CheckEquivalent(expectation, Equivalency.Options(configure));

    private AndConstraint<NumericAssertions<T>> CheckEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        Equivalency.Require(Subject, expectation, typeof(TExpectation), options, Fail.Name(_expression));
        return new(this);
    }

    public AndConstraint<NumericAssertions<T>> Be(T expected, string because)
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

    public AndConstraint<NumericAssertions<T>> NotBe(T unexpected, string because)
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

    public AndConstraint<NumericAssertions<T>> BeGreaterThan(T expected, string because)
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

    public AndConstraint<NumericAssertions<T>> BeGreaterThanOrEqualTo(T expected, string because)
    {
        try
        {
            return BeGreaterThanOrEqualTo(expected);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<NumericAssertions<T>> BeLessThan(T expected, string because)
    {
        try
        {
            return BeLessThan(expected);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<NumericAssertions<T>> BeLessThanOrEqualTo(T expected, string because)
    {
        try
        {
            return BeLessThanOrEqualTo(expected);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<NumericAssertions<T>> BePositive(string because)
    {
        try
        {
            return BePositive();
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }

    public AndConstraint<NumericAssertions<T>> BeInRange(T min, T max, string because)
    {
        try
        {
            return BeInRange(min, max);
        }
        catch (VouchException failure)
        {
            throw Reason.Add(failure, because);
        }
    }
}

public static class NumericApproximationExtensions
{
    public static AndConstraint<NumericAssertions<decimal>> BeApproximately(this NumericAssertions<decimal> assertions, decimal expected, decimal precision)
        => Approximate(assertions, expected, precision);

    public static AndConstraint<NumericAssertions<double>> BeApproximately(this NumericAssertions<double> assertions, double expected, double precision)
        => Approximate(assertions, expected, precision);

    public static AndConstraint<NumericAssertions<float>> BeApproximately(this NumericAssertions<float> assertions, float expected, float precision)
        => Approximate(assertions, expected, precision);

    private static AndConstraint<NumericAssertions<T>> Approximate<T>(NumericAssertions<T> assertions, T expected, T precision)
        where T : struct, INumber<T>
    {
        if (!NumericDifference.IsWithin(assertions.Subject, expected, precision, out var detail))
        {
            Fail.With($"Expected {assertions.Name} to approximate {Formatter.Format(expected)} +/- {Formatter.Format(precision)}, but found {Formatter.Format(assertions.Subject)} {detail}.");
        }

        return new(assertions);
    }
}

internal static class NumericDifference
{
    public static bool IsWithin<T>(T actual, T expected, T precision, out string detail)
        where T : struct, INumber<T>
    {
        detail = string.Empty;
        if (actual == expected)
        {
            return true;
        }

        T difference;
        try
        {
            difference = T.Abs(actual - expected);
        }
        catch (OverflowException)
        {
            detail = "(difference is out of range)";
            return false;
        }

        if (difference <= precision)
        {
            return true;
        }

        detail = $"(off by {Formatter.Format(difference)})";
        return false;
    }
}
