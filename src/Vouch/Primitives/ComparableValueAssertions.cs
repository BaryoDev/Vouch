namespace Vouch;

public static class ComparableValueAssertions
{
    public static AndConstraint<ValueAssertions<T>> BeGreaterThan<T>(this ValueAssertions<T> assertions, T expected)
        where T : struct, IComparable<T>
    {
        if (IsNaN(assertions.Subject) || IsNaN(expected) || assertions.Subject.CompareTo(expected) <= 0)
        {
            Fail.With($"Expected {assertions.Name} to be greater than {Formatter.Format(expected)}, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<ValueAssertions<T>> BeGreaterThanOrEqualTo<T>(this ValueAssertions<T> assertions, T expected)
        where T : struct, IComparable<T>
    {
        if (IsNaN(assertions.Subject) || IsNaN(expected) || assertions.Subject.CompareTo(expected) < 0)
        {
            Fail.With($"Expected {assertions.Name} to be greater than or equal to {Formatter.Format(expected)}, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<ValueAssertions<T>> BeLessThan<T>(this ValueAssertions<T> assertions, T expected)
        where T : struct, IComparable<T>
    {
        if (IsNaN(assertions.Subject) || IsNaN(expected) || assertions.Subject.CompareTo(expected) >= 0)
        {
            Fail.With($"Expected {assertions.Name} to be less than {Formatter.Format(expected)}, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<ValueAssertions<T>> BeLessThanOrEqualTo<T>(this ValueAssertions<T> assertions, T expected)
        where T : struct, IComparable<T>
    {
        if (IsNaN(assertions.Subject) || IsNaN(expected) || assertions.Subject.CompareTo(expected) > 0)
        {
            Fail.With($"Expected {assertions.Name} to be less than or equal to {Formatter.Format(expected)}, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    private static bool IsNaN<T>(T value) =>
        value is Half h && Half.IsNaN(h)
        || value is float f && float.IsNaN(f)
        || value is double d && double.IsNaN(d)
        || value is System.Runtime.InteropServices.NFloat n && System.Runtime.InteropServices.NFloat.IsNaN(n);

    public static AndConstraint<ValueAssertions<Guid>> NotBeEmpty(this ValueAssertions<Guid> assertions)
    {
        if (assertions.Subject == Guid.Empty)
        {
            Fail.With($"Expected {assertions.Name} not to be empty, but it was.");
        }

        return new(assertions);
    }
}
