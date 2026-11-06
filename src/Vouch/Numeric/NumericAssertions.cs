using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vouch;

public readonly struct NumericAssertions<T> where T : struct, INumber<T>
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

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

    public AndConstraint<NumericAssertions<T>> BeNegative()
    {
        if (!(Subject < T.Zero))
        {
            Fail.With($"Expected {Name} to be negative, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NumericAssertions<T>> BeCloseTo(T expected, T delta)
    {
        if (!NumericDifference.IsWithin(Subject, expected, delta, out var difference, out var outOfRange))
        {
            Fail.With($"Expected {Name} to be within {Formatter.Format(delta)} of {Formatter.Format(expected)}, but found {Formatter.Format(Subject)} {NumericDifference.Detail(difference, outOfRange)}.");
        }

        return new(this);
    }

    public AndConstraint<NumericAssertions<T>> NotBeCloseTo(T unexpected, T delta)
    {
        if (NumericDifference.IsWithin(Subject, unexpected, delta, out _, out _))
        {
            Fail.With($"Expected {Name} not to be within {Formatter.Format(delta)} of {Formatter.Format(unexpected)}, but found {Formatter.Format(Subject)}.");
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

    public AndConstraint<NumericAssertions<T>> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
        => CheckEquivalent(expectation, Equivalency.Options(configure));

    private AndConstraint<NumericAssertions<T>> CheckEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        Equivalency.Require(Subject, expectation, typeof(TExpectation), options, Fail.Name(_expression));
        return new(this);
    }

    public AndConstraint<NumericAssertions<T>> Be(T expected, string because)
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

    public AndConstraint<NumericAssertions<T>> NotBe(T unexpected, string because)
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

    public AndConstraint<NumericAssertions<T>> BeGreaterThan(T expected, string because)
    {
        Reason.Set(because);
        try
        {
            return BeGreaterThan(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<NumericAssertions<T>> BeGreaterThanOrEqualTo(T expected, string because)
    {
        Reason.Set(because);
        try
        {
            return BeGreaterThanOrEqualTo(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<NumericAssertions<T>> BeLessThan(T expected, string because)
    {
        Reason.Set(because);
        try
        {
            return BeLessThan(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<NumericAssertions<T>> BeLessThanOrEqualTo(T expected, string because)
    {
        Reason.Set(because);
        try
        {
            return BeLessThanOrEqualTo(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<NumericAssertions<T>> BePositive(string because)
    {
        Reason.Set(because);
        try
        {
            return BePositive();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<NumericAssertions<T>> BeNegative(string because)
    {
        Reason.Set(because);
        try
        {
            return BeNegative();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<NumericAssertions<T>> BeCloseTo(T expected, T delta, string because)
    {
        Reason.Set(because);
        try
        {
            return BeCloseTo(expected, delta);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<NumericAssertions<T>> NotBeCloseTo(T unexpected, T delta, string because)
    {
        Reason.Set(because);
        try
        {
            return NotBeCloseTo(unexpected, delta);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<NumericAssertions<T>> BeInRange(T min, T max, string because)
    {
        Reason.Set(because);
        try
        {
            return BeInRange(min, max);
        }
        finally
        {
            Reason.Clear();
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
        if (!NumericDifference.IsWithin(assertions.Subject, expected, precision, out var difference, out var outOfRange))
        {
            Fail.With($"Expected {assertions.Name} to approximate {Formatter.Format(expected)} +/- {Formatter.Format(precision)}, but found {Formatter.Format(assertions.Subject)} {NumericDifference.Detail(difference, outOfRange)}.");
        }

        return new(assertions);
    }
}

public static class NumericFloatingPointExtensions
{
    public static AndConstraint<NumericAssertions<double>> BeNaN(this NumericAssertions<double> assertions)
    {
        if (!double.IsNaN(assertions.Subject))
        {
            Fail.With($"Expected {assertions.Name} to be NaN, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<NumericAssertions<double>> NotBeNaN(this NumericAssertions<double> assertions)
    {
        if (double.IsNaN(assertions.Subject))
        {
            Fail.With($"Expected {assertions.Name} not to be NaN, but it was.");
        }

        return new(assertions);
    }

    public static AndConstraint<NumericAssertions<double>> BeFinite(this NumericAssertions<double> assertions)
    {
        if (!double.IsFinite(assertions.Subject))
        {
            Fail.With($"Expected {assertions.Name} to be finite, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<NumericAssertions<double>> NotBeFinite(this NumericAssertions<double> assertions)
    {
        if (double.IsFinite(assertions.Subject))
        {
            Fail.With($"Expected {assertions.Name} not to be finite, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<NumericAssertions<Half>> BeNaN(this NumericAssertions<Half> assertions)
    {
        if (!Half.IsNaN(assertions.Subject))
        {
            Fail.With($"Expected {assertions.Name} to be NaN, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<NumericAssertions<Half>> NotBeNaN(this NumericAssertions<Half> assertions)
    {
        if (Half.IsNaN(assertions.Subject))
        {
            Fail.With($"Expected {assertions.Name} not to be NaN, but it was.");
        }

        return new(assertions);
    }

    public static AndConstraint<NumericAssertions<Half>> BeFinite(this NumericAssertions<Half> assertions)
    {
        if (!Half.IsFinite(assertions.Subject))
        {
            Fail.With($"Expected {assertions.Name} to be finite, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<NumericAssertions<Half>> NotBeFinite(this NumericAssertions<Half> assertions)
    {
        if (Half.IsFinite(assertions.Subject))
        {
            Fail.With($"Expected {assertions.Name} not to be finite, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<NumericAssertions<float>> BeNaN(this NumericAssertions<float> assertions)
    {
        if (!float.IsNaN(assertions.Subject))
        {
            Fail.With($"Expected {assertions.Name} to be NaN, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<NumericAssertions<float>> NotBeNaN(this NumericAssertions<float> assertions)
    {
        if (float.IsNaN(assertions.Subject))
        {
            Fail.With($"Expected {assertions.Name} not to be NaN, but it was.");
        }

        return new(assertions);
    }

    public static AndConstraint<NumericAssertions<float>> BeFinite(this NumericAssertions<float> assertions)
    {
        if (!float.IsFinite(assertions.Subject))
        {
            Fail.With($"Expected {assertions.Name} to be finite, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<NumericAssertions<float>> NotBeFinite(this NumericAssertions<float> assertions)
    {
        if (float.IsFinite(assertions.Subject))
        {
            Fail.With($"Expected {assertions.Name} not to be finite, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }
}

internal static class NumericDifference
{
    public static void ValidateDelta<T>(T delta)
        where T : struct, INumber<T>
    {
        if (T.IsNaN(delta) || delta < T.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delta), "The delta must not be NaN or negative.");
        }
    }

    // True when |actual - expected| <= precision. When the difference cannot be represented, outOfRange is
    // set and the result is false. Allocates nothing: the detail text is built by Detail on the failure path.
    public static bool IsWithin<T>(T actual, T expected, T precision, out T difference, out bool outOfRange)
        where T : struct, INumber<T>
    {
        ValidateDelta(precision);
        difference = T.Zero;
        outOfRange = false;
        if (actual == expected)
        {
            return true;
        }

        if (T.IsNaN(actual) || T.IsNaN(expected))
        {
            // NaN is close to nothing.
            difference = T.IsNaN(actual) ? actual : expected;
            return false;
        }

        // Subtract the smaller from the larger so unsigned types cannot wrap.
        T larger = actual >= expected ? actual : expected;
        T smaller = actual >= expected ? expected : actual;

        if (typeof(T) == typeof(decimal))
        {
            // decimal throws on overflow, so check first: larger - smaller > MaxValue <=> larger > MaxValue + smaller.
            var l = Unsafe.As<T, decimal>(ref larger);
            var s = Unsafe.As<T, decimal>(ref smaller);
            if (s < 0m && l > decimal.MaxValue + s)
            {
                outOfRange = true;
                return false;
            }
        }

        difference = unchecked(larger - smaller);

        // A wrapped signed difference is negative; an overflowed floating-point difference is infinite.
        if (difference < T.Zero || !T.IsFinite(difference))
        {
            outOfRange = true;
            return false;
        }

        return difference <= precision;
    }

    public static string Detail<T>(T difference, bool outOfRange)
        where T : struct, INumber<T>
        => outOfRange ? "(difference is out of range)" : $"(off by {Formatter.Format(difference)})";
}
