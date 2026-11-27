namespace Vouch;

public static class FluentDateTimeExtensions
{
    public static DateTime January(this int day, int year) => new(year, 1, day);

    public static DateTime February(this int day, int year) => new(year, 2, day);

    public static DateTime March(this int day, int year) => new(year, 3, day);

    public static DateTime April(this int day, int year) => new(year, 4, day);

    public static DateTime May(this int day, int year) => new(year, 5, day);

    public static DateTime June(this int day, int year) => new(year, 6, day);

    public static DateTime July(this int day, int year) => new(year, 7, day);

    public static DateTime August(this int day, int year) => new(year, 8, day);

    public static DateTime September(this int day, int year) => new(year, 9, day);

    public static DateTime October(this int day, int year) => new(year, 10, day);

    public static DateTime November(this int day, int year) => new(year, 11, day);

    public static DateTime December(this int day, int year) => new(year, 12, day);

    public static DateTime At(this DateTime date, TimeSpan time) => date.Date + time;

    // The BCL constructor checks hours, minutes, seconds, milliseconds and the offset range, in an
    // order that changed in .NET 10, so it decides which part is reported. The rest is added after.
    public static DateTime At(this DateTime date, int hours, int minutes, int seconds = 0, int milliseconds = 0, int microseconds = 0, int nanoseconds = 0)
    {
        var rest = BelowMillisecond(microseconds, nanoseconds);
        try
        {
            return new DateTime(date.Year, date.Month, date.Day, hours, minutes, seconds, milliseconds, date.Kind) + rest;
        }
        catch (ArgumentOutOfRangeException ex) when (ex.ParamName is null or "millisecond")
        {
            throw OutOfRange(ex.ParamName, hours);
        }
    }

    public static DateTimeOffset At(this DateTimeOffset date, int hours, int minutes, int seconds = 0, int milliseconds = 0, int microseconds = 0, int nanoseconds = 0)
    {
        var rest = BelowMillisecond(microseconds, nanoseconds);
        try
        {
            return new DateTimeOffset(date.Year, date.Month, date.Day, hours, minutes, seconds, milliseconds, date.Offset) + rest;
        }
        catch (ArgumentOutOfRangeException ex) when (ex.ParamName is null or "millisecond")
        {
            throw OutOfRange(ex.ParamName, hours);
        }
    }

    public static DateTime AsUtc(this DateTime dateTime) => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

    public static DateTime AsLocal(this DateTime dateTime) => DateTime.SpecifyKind(dateTime, DateTimeKind.Local);

    public static DateTime Before(this TimeSpan timeDifference, DateTime sourceDateTime) => sourceDateTime - timeDifference;

    public static DateTime After(this TimeSpan timeDifference, DateTime sourceDateTime) => sourceDateTime + timeDifference;

    public static int Microsecond(this DateTime self) => self.Microsecond;

    public static int Microsecond(this DateTimeOffset self) => self.Microsecond;

    public static int Nanosecond(this DateTime self) => self.Nanosecond;

    public static int Nanosecond(this DateTimeOffset self) => self.Nanosecond;

    public static DateTime AddNanoseconds(this DateTime self, long nanoseconds) => self + TimeSpan.FromTicks(FluentTimeSpanExtensions.NanosecondsToTicks(nanoseconds));

    public static DateTimeOffset AddNanoseconds(this DateTimeOffset self, long nanoseconds) => self + TimeSpan.FromTicks(FluentTimeSpanExtensions.NanosecondsToTicks(nanoseconds));

    public static DateTimeOffset WithOffset(this DateTime self, TimeSpan offset) => new(self.Ticks, offset);

    private static TimeSpan BelowMillisecond(int microseconds, int nanoseconds)
    {
        Check(microseconds, 999, nameof(microseconds));
        Check(nanoseconds, 999, nameof(nanoseconds));

        return TimeSpan.FromTicks((microseconds * TimeSpan.TicksPerMicrosecond) + FluentTimeSpanExtensions.NanosecondsToTicks(nanoseconds));
    }

    // name is null for hours, minutes or seconds, the same as the BCL.
    private static ArgumentOutOfRangeException OutOfRange(string? name, int hours) =>
        new(name, $"Must be from 0 to {(name is not null ? 999 : hours is < 0 or > 23 ? 23 : 59)}.");

    private static void Check(int value, int max, string name)
    {
        if (value < 0 || value > max)
        {
            throw new ArgumentOutOfRangeException(name, $"Must be from 0 to {max}.");
        }
    }
}
