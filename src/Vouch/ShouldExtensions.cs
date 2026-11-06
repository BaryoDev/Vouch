using System.Runtime.CompilerServices;

namespace Vouch;

public static class ShouldExtensions
{
    public static ObjectAssertions Should(this object? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static StringAssertions Should(this string? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static ValueAssertions<T> Should<T>(this T subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        where T : struct
        => new(subject, expression);

    public static NullableValueAssertions<T> Should<T>(this T? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        where T : struct
        => new(subject, expression);

    public static BooleanAssertions Should(this bool subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableBooleanAssertions Should(this bool? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NumericAssertions<int> Should(this int subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableNumericAssertions<int> Should(this int? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NumericAssertions<long> Should(this long subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableNumericAssertions<long> Should(this long? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NumericAssertions<short> Should(this short subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableNumericAssertions<short> Should(this short? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NumericAssertions<byte> Should(this byte subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableNumericAssertions<byte> Should(this byte? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NumericAssertions<decimal> Should(this decimal subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableNumericAssertions<decimal> Should(this decimal? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NumericAssertions<double> Should(this double subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableNumericAssertions<double> Should(this double? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NumericAssertions<Half> Should(this Half subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableNumericAssertions<Half> Should(this Half? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NumericAssertions<float> Should(this float subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableNumericAssertions<float> Should(this float? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static DateTimeAssertions Should(this DateTime subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableDateTimeAssertions Should(this DateTime? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static DateTimeOffsetAssertions Should(this DateTimeOffset subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableDateTimeOffsetAssertions Should(this DateTimeOffset? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static DateOnlyAssertions Should(this DateOnly subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableDateOnlyAssertions Should(this DateOnly? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NumericAssertions<char> Should(this char subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableNumericAssertions<char> Should(this char? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NumericAssertions<sbyte> Should(this sbyte subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableNumericAssertions<sbyte> Should(this sbyte? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NumericAssertions<ushort> Should(this ushort subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableNumericAssertions<ushort> Should(this ushort? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NumericAssertions<uint> Should(this uint subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableNumericAssertions<uint> Should(this uint? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NumericAssertions<ulong> Should(this ulong subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableNumericAssertions<ulong> Should(this ulong? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static GuidAssertions Should(this Guid subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableGuidAssertions Should(this Guid? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static TimeOnlyAssertions Should(this TimeOnly subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static NullableTimeOnlyAssertions Should(this TimeOnly? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static GenericCollectionAssertions<T> Should<T>(this IEnumerable<T>? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static DictionaryAssertions<TKey, TValue> Should<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>>? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        where TKey : notnull
        => new(subject, expression);

    public static ActionAssertions Should(this Action? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static AsyncFunctionAssertions Should(this Func<Task>? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static AsyncFunctionAssertions<T> Should<T>(this Func<Task<T>>? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static TaskAssertions Should(this Task? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static TaskAssertions<T> Should<T>(this Task<T>? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static StreamAssertions Should(this Stream? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static ValueTaskFunctionAssertions Should(this Func<ValueTask>? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static ValueTaskFunctionAssertions<T> Should<T>(this Func<ValueTask<T>>? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);

    public static FunctionAssertions<T> Should<T>(this Func<T>? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);
}
