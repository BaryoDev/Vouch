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
        var actual = ToInt128(assertions.Subject);
        if (actual != value)
        {
            Fail.With($"Expected {assertions.Name} to have value {Formatter.Format(value)}, but found {FormatUnderlying<T>(actual)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<ValueAssertions<T>> HaveValue<T>(this ValueAssertions<T> assertions, ulong value)
        where T : struct, Enum
    {
        var actual = ToInt128(assertions.Subject);
        if (actual != value)
        {
            Fail.With($"Expected {assertions.Name} to have value {Formatter.Format(value)}, but found {FormatUnderlying<T>(actual)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<ValueAssertions<T>> HaveSameNameAs<T, TOther>(this ValueAssertions<T> assertions, TOther other)
        where T : struct, Enum
        where TOther : struct, Enum
    {
        var actualName = Enum.GetName(assertions.Subject);
        var expectedName = Enum.GetName(other);
        if (actualName is null)
        {
            Fail.With($"Expected {assertions.Name} to have the same name as {Formatter.Format(other)}, but {Formatter.Format(assertions.Subject)} has no name (it is not defined in {Formatter.Format(typeof(T))}).");
        }
        else if (expectedName is null)
        {
            Fail.With($"Expected {assertions.Name} to have the same name as {Formatter.Format(other)}, but {Formatter.Format(other)} has no name (it is not defined in {Formatter.Format(typeof(TOther))}).");
        }
        else if (!string.Equals(actualName, expectedName, StringComparison.Ordinal))
        {
            Fail.With($"Expected {assertions.Name} to have the same name as {Formatter.Format(other)}, but found {Formatter.Format(assertions.Subject)}.");
        }

        return new(assertions);
    }

    public static AndConstraint<ValueAssertions<T>> HaveSameValueAs<T, TOther>(this ValueAssertions<T> assertions, TOther other)
        where T : struct, Enum
        where TOther : struct, Enum
    {
        if (ToInt128(assertions.Subject) != ToInt128(other))
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

    private static Int128 ToInt128<T>(T value) where T : struct, Enum
    {
        var type = Enum.GetUnderlyingType(typeof(T));
        if (type == typeof(int)) return Unsafe.As<T, int>(ref value);
        if (type == typeof(uint)) return Unsafe.As<T, uint>(ref value);
        if (type == typeof(long)) return Unsafe.As<T, long>(ref value);
        if (type == typeof(ulong)) return Unsafe.As<T, ulong>(ref value);
        if (type == typeof(byte)) return Unsafe.As<T, byte>(ref value);
        if (type == typeof(sbyte)) return Unsafe.As<T, sbyte>(ref value);
        if (type == typeof(short)) return Unsafe.As<T, short>(ref value);
        return Unsafe.As<T, ushort>(ref value);
    }

    private static string FormatUnderlying<T>(Int128 value) where T : struct, Enum
        => Enum.GetUnderlyingType(typeof(T)) == typeof(ulong)
            ? Formatter.Format((ulong)value)
            : Formatter.Format((long)value);
}
