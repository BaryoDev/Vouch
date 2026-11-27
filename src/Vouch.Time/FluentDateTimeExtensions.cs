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

    public static DateTime At(this DateTime date, int hours, int minutes, int seconds = 0, int milliseconds = 0, int microseconds = 0, int nanoseconds = 0) =>
        date.Date + TimeOfDay(hours, minutes, seconds, milliseconds, microseconds, nanoseconds);

    public static DateTimeOffset At(this DateTimeOffset date, int hours, int minutes, int seconds = 0, int milliseconds = 0, int microseconds = 0, int nanoseconds = 0) =>
        new(date.Date + TimeOfDay(hours, minutes, seconds, milliseconds, microseconds, nanoseconds), date.Offset);

    public static DateTime AsUtc(this DateTime dateTime) => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

    public static DateTime AsLocal(this DateTime dateTime) => DateTime.SpecifyKind(dateTime, DateTimeKind.Local);

    public static DateTime Before(this TimeSpan timeDifference, DateTime sourceDateTime) => sourceDateTime - timeDifference;

    public static DateTime After(this TimeSpan timeDifference, DateTime sourceDateTime) => sourceDateTime + timeDifference;

    public static int Microsecond(this DateTime self) => self.Microsecond;

    public static int Microsecond(this DateTimeOffset self) => self.Microsecond;

    public static int Nanosecond(this DateTime self) => self.Nanosecond;

    public static int Nanosecond(this DateTimeOffset self) => self.Nanosecond;

    public static DateTime AddNanoseconds(this DateTime self, long nanoseconds) => self.AddTicks(nanoseconds / TimeSpan.NanosecondsPerTick);

    public static DateTimeOffset AddNanoseconds(this DateTimeOffset self, long nanoseconds) => self.AddTicks(nanoseconds / TimeSpan.NanosecondsPerTick);

    public static DateTimeOffset WithOffset(this DateTime self, TimeSpan offset) => new(self, offset);

    private static TimeSpan TimeOfDay(int hours, int minutes, int seconds, int milliseconds, int microseconds, int nanoseconds) =>
        new TimeSpan(0, hours, minutes, seconds, milliseconds)
        + TimeSpan.FromTicks(microseconds * TimeSpan.TicksPerMicrosecond)
        + TimeSpan.FromTicks(nanoseconds / TimeSpan.NanosecondsPerTick);
}
