using System.Runtime.CompilerServices;

namespace Vouch;

public static class EnumAssertionsExtensions
{
    public static AndConstraint<ValueAssertions<T>> BeDefined<T>(this ValueAssertions<T> assertions)
        where T : struct, Enum
    {
        if (!Enum.IsDefined(assertions.Subject))
        {
            Fail.With($"Expected {assertions.Name} to be defined in {Formatter.Format(typeof(T))}, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<ValueAssertions<T>> NotBeDefined<T>(this ValueAssertions<T> assertions)
        where T : struct, Enum
    {
        if (Enum.IsDefined(assertions.Subject))
        {
            Fail.With($"Expected {assertions.Name} not to be defined in {Formatter.Format(typeof(T))}, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<ValueAssertions<T>> HaveFlag<T>(this ValueAssertions<T> assertions, T flag)
        where T : struct, Enum
    {
        var bits = Bits(flag);
        if ((Bits(assertions.Subject) & bits) != bits)
        {
            Fail.With($"Expected {assertions.Name} to have flag {Formatter.Format(flag)}, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<ValueAssertions<T>> NotHaveFlag<T>(this ValueAssertions<T> assertions, T flag)
        where T : struct, Enum
    {
        var bits = Bits(flag);
        if ((Bits(assertions.Subject) & bits) == bits)
        {
            Fail.With($"Expected {assertions.Name} not to have flag {Formatter.Format(flag)}, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<ValueAssertions<T>> HaveValue<T>(this ValueAssertions<T> assertions, long value)
        where T : struct, Enum
    {
        if (ToInt64(assertions.Subject) != value)
        {
            Fail.With($"Expected {assertions.Name} to have value {Formatter.Format(value)}, but found {Formatter.Format(ToInt64(assertions.Subject))}.");
        }

        return new(assertions);
    }

    public static AndConstraint<ValueAssertions<T>> HaveSameNameAs<T, TOther>(this ValueAssertions<T> assertions, TOther other)
        where T : struct, Enum
        where TOther : struct, Enum
    {
        if (assertions.Subject.ToString() != other.ToString())
        {
            Fail.With($"Expected {assertions.Name} to have the same name as {Formatter.Format(other)}, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<ValueAssertions<T>> HaveSameValueAs<T, TOther>(this ValueAssertions<T> assertions, TOther other)
        where T : struct, Enum
        where TOther : struct, Enum
    {
        if (ToInt64(assertions.Subject) != ToInt64(other))
        {
            Fail.With($"Expected {assertions.Name} to have the same value as {Formatter.Format(other)}, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    private static ulong Bits<T>(T value) where T : struct, Enum => Unsafe.SizeOf<T>() switch
    {
        1 => Unsafe.As<T, byte>(ref value),
        2 => Unsafe.As<T, ushort>(ref value),
        4 => Unsafe.As<T, uint>(ref value),
        _ => Unsafe.As<T, ulong>(ref value),
    };

    private static long ToInt64<T>(T value) where T : struct, Enum =>
        Enum.GetUnderlyingType(typeof(T)) == typeof(ulong)
            ? unchecked((long)Convert.ToUInt64(value))
            : Convert.ToInt64(value);
}
