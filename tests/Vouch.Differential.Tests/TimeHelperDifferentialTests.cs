using System.Globalization;
using Xunit;
using AaDate = global::AwesomeAssertions.Extensions.FluentDateTimeExtensions;
using AaSpan = global::AwesomeAssertions.Extensions.FluentTimeSpanExtensions;
using VDate = global::Vouch.FluentDateTimeExtensions;
using VSpan = global::Vouch.FluentTimeSpanExtensions;

namespace Differential;

// Every call runs through AwesomeAssertions and Vouch.Time. The outcome is either a value or an
// exception type (not its message). TimeSpan compares by ticks, DateTime by ticks and kind,
// DateTimeOffset by clock ticks and offset, because the built-in equality hides those parts.
public class TimeHelperDifferentialTests
{
    private static readonly int[] Ints = [0, 1, -1, 5, int.MaxValue, int.MinValue];
    private static readonly long[] Longs = [0, 1, -1, 99, 150, -150, long.MaxValue, long.MinValue];

    private static readonly long[] NanosecondLongs = [0, 1, -1, 49, 50, 51, 99, 149, 150, 151, 250, 350, 450, 550, -50, -150, -250, -350, 1050, 1150, 2500, 2147483647, -2147483648, 9007199254740993, 4611686018427387903, 4811602766166610294, -1095215029522092345, 186831210636689527, long.MaxValue, long.MinValue];
    private static readonly double[] Doubles = [0, 1.5, -1.5, 0.0001, 1e-9, double.NaN, double.PositiveInfinity, double.NegativeInfinity, double.MaxValue, double.MinValue, -0.0];
    private static readonly long[] SpanTicks = [0, TimeSpan.TicksPerSecond * 30, long.MaxValue, long.MinValue, -1, 1, 15_000, -15_000, TimeSpan.TicksPerDay, 1234567, -1234567, 3, 7, -7, 10, 11, 1000000001, 987654321];
    private static readonly int[] Days = [0, 1, 28, 29, 30, 31, 32];
    private static readonly int[] Years = [0, 1, 2024, 2025, 9999, 10000];
    private static readonly DateTimeKind[] Kinds = [DateTimeKind.Unspecified, DateTimeKind.Utc, DateTimeKind.Local];

    private static readonly long[] BaseTicks =
    [
        new DateTime(2026, 3, 1, 12, 0, 0).Ticks,
        DateTime.MinValue.Ticks,
        DateTime.MaxValue.Ticks,
        new DateTime(2026, 3, 1, 12, 0, 0).Ticks + 1234567,
    ];

    // Clock times that are valid with every offset used below.
    private static readonly long[] OffsetBaseTicks =
    [
        new DateTime(2026, 3, 1, 12, 0, 0).Ticks,
        new DateTime(1, 1, 1, 12, 0, 0).Ticks,
        new DateTime(9999, 12, 31, 12, 0, 0).Ticks,
        new DateTime(2026, 3, 1, 12, 0, 0).Ticks + 1234567,
    ];

    private static readonly long[] WithOffsetTicks =
    [
        .. OffsetBaseTicks,
        DateTime.MinValue.Ticks,
        DateTime.MinValue.Ticks + 1,
        DateTime.MaxValue.Ticks,
        DateTime.MaxValue.Ticks - 1,
        14 * TimeSpan.TicksPerHour,
        14 * TimeSpan.TicksPerHour - 10_000,
        new DateTime(9999, 12, 31, 10, 0, 0).Ticks,
        new DateTime(9999, 12, 31, 9, 59, 59).Ticks,
    ];

    private static readonly long[] OffsetsForAt = [0, 2 * TimeSpan.TicksPerHour, -5 * TimeSpan.TicksPerHour];

    private static readonly long[] OffsetsForWithOffset =
    [
        0,
        2 * TimeSpan.TicksPerHour,
        14 * TimeSpan.TicksPerHour,
        15 * TimeSpan.TicksPerHour,
        -5 * TimeSpan.TicksPerHour,
        -14 * TimeSpan.TicksPerHour,
        TimeSpan.TicksPerSecond * 30,
        14 * TimeSpan.TicksPerHour + TimeSpan.TicksPerMinute,
        1,
    ];

    private static readonly int[][] AtParts =
    [
        [10, 30, 0, 0, 0, 0],
        [23, 59, 59, 999, 999, 900],
        [24, 0, 0, 0, 0, 0],
        [-1, 0, 0, 0, 0, 0],
        [0, 0, 0, 0, 0, 150],
        [0, 0, 0, 0, 0, 1000],
        [0, 0, 0, 0, 5, 0],
        [0, 0, 1, 2, 3, 4],
        [0, 60, 0, 0, 0, 0],
        [0, 0, 0, 0, 0, -150],
        [0, 0, 60, 0, 0, 0],
        [0, 0, -1, 0, 0, 0],
        [0, 0, 0, 1000, 0, 0],
        [0, 0, 0, -1, 0, 0],
        [0, 0, 0, 0, 1000, 0],
        [0, 0, 0, 0, -1, 0],
        [0, 0, 0, 0, 999, 0],
        [0, 0, 0, 0, 0, 999],
        [0, 0, 0, 0, 0, -1],
        [0, 0, 0, 0, 0, 50],
        [0, 0, 0, 0, 0, 250],
        [0, 0, 0, 0, 0, 350],
        [0, 0, 0, 0, 0, 949],
        [0, 0, 0, 0, 0, 950],
        [23, 59, 59, 999, 999, 999],
        [23, 60, 0, 0, 0, 0],
        [25, 0, 0, 0, 0, 0],
    ];

    private static readonly long[] AddNanosecondValues = [0, 49, 50, 51, 99, -99, 100, 149, 150, -150, 151, 250, -250, 350, -350, 450, 550, 1050, 1150, 4811602766166610294, -1095215029522092345, 186831210636689527, long.MaxValue, long.MinValue];

    public static IEnumerable<object[]> IntRows() => Ints.Select(i => new object[] { i });

    public static IEnumerable<object[]> LongRows() => Longs.Select(i => new object[] { i });

    public static IEnumerable<object[]> NanosecondRows() => NanosecondLongs.Select(i => new object[] { i });

    public static IEnumerable<object[]> NanosecondIntRows() =>
        NanosecondLongs.Where(v => v >= int.MinValue && v <= int.MaxValue).Select(v => new object[] { (int)v });

    public static IEnumerable<object[]> DoubleRows() => Doubles.Select(d => new object[] { BitConverter.DoubleToInt64Bits(d) });

    public static IEnumerable<object[]> SpanRows() => SpanTicks.Select(t => new object[] { t });

    public static IEnumerable<object[]> IntOffsetRows() =>
        from i in Ints from t in SpanTicks select new object[] { i, t };

    public static IEnumerable<object[]> SpanPairRows() =>
        from a in SpanTicks from b in SpanTicks select new object[] { a, b };

    public static IEnumerable<object[]> MonthRows() =>
        from month in Enumerable.Range(1, 12)
        from day in Days
        from year in Years
        select new object[] { month, day, year };

    public static IEnumerable<object[]> KindRows() =>
        from kind in Kinds from ticks in BaseTicks select new object[] { kind, ticks };

    public static IEnumerable<object[]> AtPartsRows() =>
        from kind in Kinds
        from ticks in BaseTicks
        from parts in AtParts
        from count in new[] { 2, 3, 4, 5, 6 }
        select new object[] { kind, ticks, count, parts[0], parts[1], parts[2], parts[3], parts[4], parts[5] };

    public static IEnumerable<object[]> AtOffsetPartsRows() =>
        from offset in OffsetsForAt
        from ticks in OffsetBaseTicks
        from parts in AtParts
        from count in new[] { 2, 3, 4, 5, 6 }
        select new object[] { offset, ticks, count, parts[0], parts[1], parts[2], parts[3], parts[4], parts[5] };

    public static IEnumerable<object[]> AtSpanRows() =>
        from kind in Kinds
        from ticks in BaseTicks
        from span in new long[] { 0, new TimeSpan(10, 30, 0).Ticks, TimeSpan.TicksPerDay, -1, TimeSpan.MaxValue.Ticks, TimeSpan.MinValue.Ticks }
        select new object[] { kind, ticks, span };

    public static IEnumerable<object[]> BeforeAfterRows() =>
        from kind in Kinds
        from ticks in BaseTicks
        from span in SpanTicks
        select new object[] { kind, ticks, span };

    public static IEnumerable<object[]> AddNanosecondRows() =>
        from kind in Kinds
        from ticks in BaseTicks
        from ns in AddNanosecondValues
        select new object[] { kind, ticks, ns };

    public static IEnumerable<object[]> AddNanosecondOffsetRows() =>
        from offset in OffsetsForAt
        from ticks in OffsetBaseTicks
        from ns in AddNanosecondValues
        select new object[] { offset, ticks, ns };

    public static IEnumerable<object[]> WithOffsetRows() =>
        from kind in Kinds
        from ticks in WithOffsetTicks
        from offset in OffsetsForWithOffset
        select new object[] { kind, ticks, offset };

    public static IEnumerable<object[]> OffsetReaderRows() =>
        from offset in OffsetsForAt
        from ticks in OffsetBaseTicks
        select new object[] { offset, ticks };

    [Theory, MemberData(nameof(IntRows))]
    public void Ticks_of_an_int(int v) => Agree(() => AaSpan.Ticks(v), () => VSpan.Ticks(v));

    [Theory, MemberData(nameof(LongRows))]
    public void Ticks_of_a_long(long v) => Agree(() => AaSpan.Ticks(v), () => VSpan.Ticks(v));

    [Theory, MemberData(nameof(IntRows))]
    public void Ticks_with_a_named_argument(int v) => Agree(() => AaSpan.Ticks(ticks: v), () => VSpan.Ticks(ticks: v));

    [Theory, MemberData(nameof(IntRows))]
    public void Microseconds_of_an_int(int v) => Agree(() => AaSpan.Microseconds(v), () => VSpan.Microseconds(v));

    [Theory, MemberData(nameof(LongRows))]
    public void Microseconds_of_a_long(long v) => Agree(() => AaSpan.Microseconds(v), () => VSpan.Microseconds(v));

    [Theory, MemberData(nameof(SpanRows))]
    public void Microseconds_of_a_span(long ticks) => Agree(() => AaSpan.Microseconds(new TimeSpan(ticks)), () => VSpan.Microseconds(new TimeSpan(ticks)));

    [Theory, MemberData(nameof(IntRows))]
    public void Nanoseconds_of_an_int(int v) => Agree(() => AaSpan.Nanoseconds(v), () => VSpan.Nanoseconds(v));

    [Theory, MemberData(nameof(NanosecondIntRows))]
    public void Nanoseconds_of_an_int_near_the_half_tick(int v) => Agree(() => AaSpan.Nanoseconds(v), () => VSpan.Nanoseconds(v));

    [Theory, MemberData(nameof(NanosecondRows))]
    public void Nanoseconds_of_a_long(long v) => Agree(() => AaSpan.Nanoseconds(v), () => VSpan.Nanoseconds(v));

    [Theory, MemberData(nameof(SpanRows))]
    public void Nanoseconds_of_a_span(long ticks) => Agree(() => AaSpan.Nanoseconds(new TimeSpan(ticks)), () => VSpan.Nanoseconds(new TimeSpan(ticks)));

    [Theory, MemberData(nameof(SpanRows))]
    public void TotalMicroseconds_of_a_span(long ticks) => Agree(() => AaSpan.TotalMicroseconds(new TimeSpan(ticks)), () => VSpan.TotalMicroseconds(new TimeSpan(ticks)));

    [Theory, MemberData(nameof(SpanRows))]
    public void TotalNanoseconds_of_a_span(long ticks) => Agree(() => AaSpan.TotalNanoseconds(new TimeSpan(ticks)), () => VSpan.TotalNanoseconds(new TimeSpan(ticks)));

    [Theory, MemberData(nameof(SpanRows))]
    public void Span_readers_with_a_named_argument(long ticks)
    {
        Agree(() => AaSpan.Microseconds(self: new TimeSpan(ticks)), () => VSpan.Microseconds(self: new TimeSpan(ticks)));
        Agree(() => AaSpan.Nanoseconds(self: new TimeSpan(ticks)), () => VSpan.Nanoseconds(self: new TimeSpan(ticks)));
        Agree(() => AaSpan.TotalMicroseconds(self: new TimeSpan(ticks)), () => VSpan.TotalMicroseconds(self: new TimeSpan(ticks)));
        Agree(() => AaSpan.TotalNanoseconds(self: new TimeSpan(ticks)), () => VSpan.TotalNanoseconds(self: new TimeSpan(ticks)));
    }

    [Theory, MemberData(nameof(IntRows))]
    public void Milliseconds_of_an_int(int v) => Agree(() => AaSpan.Milliseconds(v), () => VSpan.Milliseconds(v));

    [Theory, MemberData(nameof(DoubleRows))]
    public void Milliseconds_of_a_double(long bits)
    {
        var v = BitConverter.Int64BitsToDouble(bits);
        Agree(() => AaSpan.Milliseconds(v), () => VSpan.Milliseconds(v));
    }

    [Theory, MemberData(nameof(IntRows))]
    public void Seconds_of_an_int(int v) => Agree(() => AaSpan.Seconds(v), () => VSpan.Seconds(v));

    [Theory, MemberData(nameof(DoubleRows))]
    public void Seconds_of_a_double(long bits)
    {
        var v = BitConverter.Int64BitsToDouble(bits);
        Agree(() => AaSpan.Seconds(v), () => VSpan.Seconds(v));
    }

    [Theory, MemberData(nameof(IntOffsetRows))]
    public void Seconds_with_an_offset(int v, long offset) =>
        Agree(() => AaSpan.Seconds(v, new TimeSpan(offset)), () => VSpan.Seconds(v, new TimeSpan(offset)));

    [Theory, MemberData(nameof(IntRows))]
    public void Minutes_of_an_int(int v) => Agree(() => AaSpan.Minutes(v), () => VSpan.Minutes(v));

    [Theory, MemberData(nameof(DoubleRows))]
    public void Minutes_of_a_double(long bits)
    {
        var v = BitConverter.Int64BitsToDouble(bits);
        Agree(() => AaSpan.Minutes(v), () => VSpan.Minutes(v));
    }

    [Theory, MemberData(nameof(IntOffsetRows))]
    public void Minutes_with_an_offset(int v, long offset) =>
        Agree(() => AaSpan.Minutes(v, new TimeSpan(offset)), () => VSpan.Minutes(v, new TimeSpan(offset)));

    [Theory, MemberData(nameof(IntRows))]
    public void Hours_of_an_int(int v) => Agree(() => AaSpan.Hours(v), () => VSpan.Hours(v));

    [Theory, MemberData(nameof(DoubleRows))]
    public void Hours_of_a_double(long bits)
    {
        var v = BitConverter.Int64BitsToDouble(bits);
        Agree(() => AaSpan.Hours(v), () => VSpan.Hours(v));
    }

    [Theory, MemberData(nameof(IntOffsetRows))]
    public void Hours_with_an_offset(int v, long offset) =>
        Agree(() => AaSpan.Hours(v, new TimeSpan(offset)), () => VSpan.Hours(v, new TimeSpan(offset)));

    [Theory, MemberData(nameof(IntRows))]
    public void Days_of_an_int(int v) => Agree(() => AaSpan.Days(v), () => VSpan.Days(v));

    [Theory, MemberData(nameof(DoubleRows))]
    public void Days_of_a_double(long bits)
    {
        var v = BitConverter.Int64BitsToDouble(bits);
        Agree(() => AaSpan.Days(v), () => VSpan.Days(v));
    }

    [Theory, MemberData(nameof(IntOffsetRows))]
    public void Days_with_an_offset(int v, long offset) =>
        Agree(() => AaSpan.Days(v, new TimeSpan(offset)), () => VSpan.Days(v, new TimeSpan(offset)));

    [Theory, MemberData(nameof(IntOffsetRows))]
    public void Unit_helpers_with_named_arguments(int v, long offset)
    {
        var o = new TimeSpan(offset);
        Agree(() => AaSpan.Seconds(seconds: v, offset: o), () => VSpan.Seconds(seconds: v, offset: o));
        Agree(() => AaSpan.Minutes(minutes: v, offset: o), () => VSpan.Minutes(minutes: v, offset: o));
        Agree(() => AaSpan.Hours(hours: v, offset: o), () => VSpan.Hours(hours: v, offset: o));
        Agree(() => AaSpan.Days(days: v, offset: o), () => VSpan.Days(days: v, offset: o));
        Agree(() => AaSpan.Seconds(seconds: v), () => VSpan.Seconds(seconds: v));
        Agree(() => AaSpan.Minutes(minutes: v), () => VSpan.Minutes(minutes: v));
        Agree(() => AaSpan.Hours(hours: v), () => VSpan.Hours(hours: v));
        Agree(() => AaSpan.Days(days: v), () => VSpan.Days(days: v));
        Agree(() => AaSpan.Milliseconds(milliseconds: v), () => VSpan.Milliseconds(milliseconds: v));
        Agree(() => AaSpan.Microseconds(microseconds: v), () => VSpan.Microseconds(microseconds: v));
        Agree(() => AaSpan.Nanoseconds(nanoseconds: v), () => VSpan.Nanoseconds(nanoseconds: v));
    }

    [Theory, MemberData(nameof(DoubleRows))]
    public void Double_helpers_with_named_arguments(long bits)
    {
        var v = BitConverter.Int64BitsToDouble(bits);
        Agree(() => AaSpan.Seconds(seconds: v), () => VSpan.Seconds(seconds: v));
        Agree(() => AaSpan.Minutes(minutes: v), () => VSpan.Minutes(minutes: v));
        Agree(() => AaSpan.Hours(hours: v), () => VSpan.Hours(hours: v));
        Agree(() => AaSpan.Days(days: v), () => VSpan.Days(days: v));
        Agree(() => AaSpan.Milliseconds(milliseconds: v), () => VSpan.Milliseconds(milliseconds: v));
    }

    [Theory, MemberData(nameof(SpanPairRows))]
    public void And_of_two_spans(long a, long b) =>
        Agree(() => AaSpan.And(new TimeSpan(a), new TimeSpan(b)), () => VSpan.And(new TimeSpan(a), new TimeSpan(b)));

    [Theory, MemberData(nameof(SpanPairRows))]
    public void And_with_named_arguments(long a, long b) =>
        Agree(() => AaSpan.And(sourceTime: new TimeSpan(a), offset: new TimeSpan(b)), () => VSpan.And(sourceTime: new TimeSpan(a), offset: new TimeSpan(b)));

    [Theory, MemberData(nameof(MonthRows))]
    public void Month_helpers(int month, int day, int year) =>
        Agree(() => AaMonth(month, day, year), () => VMonth(month, day, year));

    [Theory, MemberData(nameof(MonthRows))]
    public void Month_helpers_with_named_arguments(int month, int day, int year) =>
        Agree(() => AaMonthNamed(month, day, year), () => VMonthNamed(month, day, year));

    [Theory, MemberData(nameof(AtPartsRows))]
    public void At_on_a_date_time_with_parts(DateTimeKind kind, long ticks, int count, int h, int m, int s, int ms, int us, int ns)
    {
        var d = new DateTime(ticks, kind);
        AgreeWithParamName(() => AtAa(d, count, h, m, s, ms, us, ns), () => AtV(d, count, h, m, s, ms, us, ns));
    }

    [Theory, MemberData(nameof(AtOffsetPartsRows))]
    public void At_on_an_offset_with_parts(long offset, long ticks, int count, int h, int m, int s, int ms, int us, int ns)
    {
        var d = new DateTimeOffset(new DateTime(ticks), new TimeSpan(offset));
        AgreeWithParamName(() => AtAa(d, count, h, m, s, ms, us, ns), () => AtV(d, count, h, m, s, ms, us, ns));
    }

    [Theory, MemberData(nameof(KindRows))]
    public void At_with_named_arguments(DateTimeKind kind, long ticks)
    {
        var d = new DateTime(ticks, kind);
        Agree(
            () => AaDate.At(date: d, hours: 1, minutes: 2, seconds: 3, milliseconds: 4, microseconds: 5, nanoseconds: 600),
            () => VDate.At(date: d, hours: 1, minutes: 2, seconds: 3, milliseconds: 4, microseconds: 5, nanoseconds: 600));
        Agree(() => AaDate.At(d, 10, 30, nanoseconds: 150), () => VDate.At(d, 10, 30, nanoseconds: 150));
        Agree(() => AaDate.At(d, 10, 30, microseconds: 5), () => VDate.At(d, 10, 30, microseconds: 5));
        Agree(() => AaDate.At(d, 10, 30, milliseconds: 5), () => VDate.At(d, 10, 30, milliseconds: 5));
        Agree(() => AaDate.At(d, 10, 30, seconds: 5), () => VDate.At(d, 10, 30, seconds: 5));
        Agree(() => AaDate.At(d, minutes: 30, hours: 10), () => VDate.At(d, minutes: 30, hours: 10));
    }

    [Theory, MemberData(nameof(OffsetReaderRows))]
    public void At_on_an_offset_with_named_arguments(long offset, long ticks)
    {
        var d = new DateTimeOffset(new DateTime(ticks), new TimeSpan(offset));
        Agree(
            () => AaDate.At(date: d, hours: 1, minutes: 2, seconds: 3, milliseconds: 4, microseconds: 5, nanoseconds: 600),
            () => VDate.At(date: d, hours: 1, minutes: 2, seconds: 3, milliseconds: 4, microseconds: 5, nanoseconds: 600));
        Agree(() => AaDate.At(d, 10, 30, nanoseconds: 150), () => VDate.At(d, 10, 30, nanoseconds: 150));
        Agree(() => AaDate.At(d, 10, 30, microseconds: 5), () => VDate.At(d, 10, 30, microseconds: 5));
        Agree(() => AaDate.At(d, minutes: 30, hours: 10), () => VDate.At(d, minutes: 30, hours: 10));
    }

    [Theory, MemberData(nameof(AtSpanRows))]
    public void At_a_time_span(DateTimeKind kind, long ticks, long span)
    {
        var d = new DateTime(ticks, kind);
        Agree(() => AaDate.At(d, new TimeSpan(span)), () => VDate.At(d, new TimeSpan(span)));
        Agree(() => AaDate.At(date: d, time: new TimeSpan(span)), () => VDate.At(date: d, time: new TimeSpan(span)));
    }

    [Theory, MemberData(nameof(KindRows))]
    public void AsUtc_and_AsLocal(DateTimeKind kind, long ticks)
    {
        var d = new DateTime(ticks, kind);
        Agree(() => AaDate.AsUtc(d), () => VDate.AsUtc(d));
        Agree(() => AaDate.AsLocal(d), () => VDate.AsLocal(d));
        Agree(() => AaDate.AsUtc(dateTime: d), () => VDate.AsUtc(dateTime: d));
        Agree(() => AaDate.AsLocal(dateTime: d), () => VDate.AsLocal(dateTime: d));
    }

    [Theory, MemberData(nameof(BeforeAfterRows))]
    public void Before_and_After(DateTimeKind kind, long ticks, long span)
    {
        var d = new DateTime(ticks, kind);
        var t = new TimeSpan(span);
        Agree(() => AaDate.Before(t, d), () => VDate.Before(t, d));
        Agree(() => AaDate.After(t, d), () => VDate.After(t, d));
        Agree(() => AaDate.Before(timeDifference: t, sourceDateTime: d), () => VDate.Before(timeDifference: t, sourceDateTime: d));
        Agree(() => AaDate.After(timeDifference: t, sourceDateTime: d), () => VDate.After(timeDifference: t, sourceDateTime: d));
    }

    [Theory, MemberData(nameof(KindRows))]
    public void Microsecond_and_Nanosecond_of_a_date_time(DateTimeKind kind, long ticks)
    {
        var d = new DateTime(ticks, kind);
        Agree(() => AaDate.Microsecond(d), () => VDate.Microsecond(d));
        Agree(() => AaDate.Nanosecond(d), () => VDate.Nanosecond(d));
        Agree(() => AaDate.Microsecond(self: d), () => VDate.Microsecond(self: d));
        Agree(() => AaDate.Nanosecond(self: d), () => VDate.Nanosecond(self: d));
    }

    [Theory, MemberData(nameof(OffsetReaderRows))]
    public void Microsecond_and_Nanosecond_of_an_offset(long offset, long ticks)
    {
        var d = new DateTimeOffset(new DateTime(ticks), new TimeSpan(offset));
        Agree(() => AaDate.Microsecond(d), () => VDate.Microsecond(d));
        Agree(() => AaDate.Nanosecond(d), () => VDate.Nanosecond(d));
        Agree(() => AaDate.Microsecond(self: d), () => VDate.Microsecond(self: d));
        Agree(() => AaDate.Nanosecond(self: d), () => VDate.Nanosecond(self: d));
    }

    [Theory, MemberData(nameof(AddNanosecondRows))]
    public void AddNanoseconds_on_a_date_time(DateTimeKind kind, long ticks, long ns)
    {
        var d = new DateTime(ticks, kind);
        AgreeWithParamName(() => AaDate.AddNanoseconds(d, ns), () => VDate.AddNanoseconds(d, ns));
        AgreeWithParamName(() => AaDate.AddNanoseconds(self: d, nanoseconds: ns), () => VDate.AddNanoseconds(self: d, nanoseconds: ns));
    }

    [Theory, MemberData(nameof(AddNanosecondOffsetRows))]
    public void AddNanoseconds_on_an_offset(long offset, long ticks, long ns)
    {
        var d = new DateTimeOffset(new DateTime(ticks), new TimeSpan(offset));
        AgreeWithParamName(() => AaDate.AddNanoseconds(d, ns), () => VDate.AddNanoseconds(d, ns));
        AgreeWithParamName(() => AaDate.AddNanoseconds(self: d, nanoseconds: ns), () => VDate.AddNanoseconds(self: d, nanoseconds: ns));
    }

    [Theory, MemberData(nameof(WithOffsetRows))]
    public void WithOffset(DateTimeKind kind, long ticks, long offset)
    {
        var d = new DateTime(ticks, kind);
        var o = new TimeSpan(offset);
        Agree(() => AaDate.WithOffset(d, o), () => VDate.WithOffset(d, o));
        Agree(() => AaDate.WithOffset(self: d, offset: o), () => VDate.WithOffset(self: d, offset: o));
    }

    private static void Agree(Func<object?> aa, Func<object?> vouch) => Assert.Equal(Run(aa), Run(vouch));

    // Same as Agree, and an ArgumentException must also carry the same ParamName.
    private static void AgreeWithParamName(Func<object?> aa, Func<object?> vouch) => Assert.Equal(Run(aa, true), Run(vouch, true));

    private static string Run(Func<object?> call, bool withParamName = false)
    {
        try
        {
            return Show(call());
        }
        catch (Exception ex)
        {
            var name = withParamName && ex is ArgumentException arg ? " param=" + (arg.ParamName ?? "(null)") : "";
            return "throws " + ex.GetType().FullName + name;
        }
    }

    private static string Show(object? value) => value switch
    {
        TimeSpan t => $"TimeSpan {t.Ticks}",
        DateTime d => $"DateTime {d.Ticks} {d.Kind}",
        DateTimeOffset o => $"DateTimeOffset {o.DateTime.Ticks} {o.Offset.Ticks}",
        double x => x.Equals(0.0) ? "double 0" : "double " + x.ToString("R", CultureInfo.InvariantCulture),
        null => "null",
        _ => value.GetType().Name + " " + Convert.ToString(value, CultureInfo.InvariantCulture),
    };

    private static DateTime AaMonth(int month, int day, int year) => month switch
    {
        1 => AaDate.January(day, year),
        2 => AaDate.February(day, year),
        3 => AaDate.March(day, year),
        4 => AaDate.April(day, year),
        5 => AaDate.May(day, year),
        6 => AaDate.June(day, year),
        7 => AaDate.July(day, year),
        8 => AaDate.August(day, year),
        9 => AaDate.September(day, year),
        10 => AaDate.October(day, year),
        11 => AaDate.November(day, year),
        _ => AaDate.December(day, year),
    };

    private static DateTime VMonth(int month, int day, int year) => month switch
    {
        1 => VDate.January(day, year),
        2 => VDate.February(day, year),
        3 => VDate.March(day, year),
        4 => VDate.April(day, year),
        5 => VDate.May(day, year),
        6 => VDate.June(day, year),
        7 => VDate.July(day, year),
        8 => VDate.August(day, year),
        9 => VDate.September(day, year),
        10 => VDate.October(day, year),
        11 => VDate.November(day, year),
        _ => VDate.December(day, year),
    };

    private static DateTime AaMonthNamed(int month, int day, int year) => month switch
    {
        1 => AaDate.January(day: day, year: year),
        2 => AaDate.February(day: day, year: year),
        3 => AaDate.March(day: day, year: year),
        4 => AaDate.April(day: day, year: year),
        5 => AaDate.May(day: day, year: year),
        6 => AaDate.June(day: day, year: year),
        7 => AaDate.July(day: day, year: year),
        8 => AaDate.August(day: day, year: year),
        9 => AaDate.September(day: day, year: year),
        10 => AaDate.October(day: day, year: year),
        11 => AaDate.November(day: day, year: year),
        _ => AaDate.December(day: day, year: year),
    };

    private static DateTime VMonthNamed(int month, int day, int year) => month switch
    {
        1 => VDate.January(day: day, year: year),
        2 => VDate.February(day: day, year: year),
        3 => VDate.March(day: day, year: year),
        4 => VDate.April(day: day, year: year),
        5 => VDate.May(day: day, year: year),
        6 => VDate.June(day: day, year: year),
        7 => VDate.July(day: day, year: year),
        8 => VDate.August(day: day, year: year),
        9 => VDate.September(day: day, year: year),
        10 => VDate.October(day: day, year: year),
        11 => VDate.November(day: day, year: year),
        _ => VDate.December(day: day, year: year),
    };

    // count is how many of the six int arguments are passed, so the omitted ones use the defaults.
    private static object AtAa(DateTime d, int count, int h, int m, int s, int ms, int us, int ns) => count switch
    {
        2 => AaDate.At(d, h, m),
        3 => AaDate.At(d, h, m, s),
        4 => AaDate.At(d, h, m, s, ms),
        5 => AaDate.At(d, h, m, s, ms, us),
        _ => AaDate.At(d, h, m, s, ms, us, ns),
    };

    private static object AtV(DateTime d, int count, int h, int m, int s, int ms, int us, int ns) => count switch
    {
        2 => VDate.At(d, h, m),
        3 => VDate.At(d, h, m, s),
        4 => VDate.At(d, h, m, s, ms),
        5 => VDate.At(d, h, m, s, ms, us),
        _ => VDate.At(d, h, m, s, ms, us, ns),
    };

    private static object AtAa(DateTimeOffset d, int count, int h, int m, int s, int ms, int us, int ns) => count switch
    {
        2 => AaDate.At(d, h, m),
        3 => AaDate.At(d, h, m, s),
        4 => AaDate.At(d, h, m, s, ms),
        5 => AaDate.At(d, h, m, s, ms, us),
        _ => AaDate.At(d, h, m, s, ms, us, ns),
    };

    private static object AtV(DateTimeOffset d, int count, int h, int m, int s, int ms, int us, int ns) => count switch
    {
        2 => VDate.At(d, h, m),
        3 => VDate.At(d, h, m, s),
        4 => VDate.At(d, h, m, s, ms),
        5 => VDate.At(d, h, m, s, ms, us),
        _ => VDate.At(d, h, m, s, ms, us, ns),
    };
}
