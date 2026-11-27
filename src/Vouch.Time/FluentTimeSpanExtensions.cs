namespace Vouch;

public static class FluentTimeSpanExtensions
{
    public static TimeSpan Ticks(this int ticks) => TimeSpan.FromTicks(ticks);

    public static TimeSpan Ticks(this long ticks) => TimeSpan.FromTicks(ticks);

    public static TimeSpan Microseconds(this int microseconds) =>
        TimeSpan.FromTicks(microseconds * TimeSpan.TicksPerMicrosecond);

    public static TimeSpan Microseconds(this long microseconds) =>
        TimeSpan.FromTicks(microseconds * TimeSpan.TicksPerMicrosecond);

    public static int Microseconds(this TimeSpan self) => self.Microseconds;

    public static TimeSpan Nanoseconds(this int nanoseconds) => TimeSpan.FromTicks(NanosecondsToTicks(nanoseconds));

    public static TimeSpan Nanoseconds(this long nanoseconds) => TimeSpan.FromTicks(NanosecondsToTicks(nanoseconds));

    public static int Nanoseconds(this TimeSpan self) => self.Nanoseconds;

    public static double TotalMicroseconds(this TimeSpan self) => self.Ticks * 0.1;

    public static double TotalNanoseconds(this TimeSpan self) => self.TotalNanoseconds;

    // The casts keep the double overloads of the BCL on both targets. On net10.0 an int
    // argument would otherwise bind to the new long overloads, which throw other exceptions.
    public static TimeSpan Milliseconds(this int milliseconds) => TimeSpan.FromMilliseconds((double)milliseconds);

    public static TimeSpan Milliseconds(this double milliseconds) => TimeSpan.FromMilliseconds(milliseconds);

    public static TimeSpan Seconds(this int seconds) => TimeSpan.FromSeconds((double)seconds);

    public static TimeSpan Seconds(this double seconds) => TimeSpan.FromSeconds(seconds);

    public static TimeSpan Seconds(this int seconds, TimeSpan offset) => TimeSpan.FromSeconds((double)seconds) + offset;

    public static TimeSpan Minutes(this int minutes) => TimeSpan.FromMinutes((double)minutes);

    public static TimeSpan Minutes(this double minutes) => TimeSpan.FromMinutes(minutes);

    public static TimeSpan Minutes(this int minutes, TimeSpan offset) => TimeSpan.FromMinutes((double)minutes) + offset;

    public static TimeSpan Hours(this int hours) => TimeSpan.FromHours((double)hours);

    public static TimeSpan Hours(this double hours) => TimeSpan.FromHours(hours);

    public static TimeSpan Hours(this int hours, TimeSpan offset) => TimeSpan.FromHours((double)hours) + offset;

    public static TimeSpan Days(this int days) => TimeSpan.FromDays((double)days);

    public static TimeSpan Days(this double days) => TimeSpan.FromDays(days);

    public static TimeSpan Days(this int days, TimeSpan offset) => TimeSpan.FromDays((double)days) + offset;

    public static TimeSpan And(this TimeSpan sourceTime, TimeSpan offset) => sourceTime + offset;

    // Multiplies by 0.01 in double and rounds half to even, the same way FluentAssertions does, so long.MaxValue ends on ...760.
    internal static long NanosecondsToTicks(long nanoseconds) => (long)Math.Round(nanoseconds * 0.01);
}
