using Xunit;

namespace Vouch.Tests;

public class DateAssertionsTests
{
    private static string Message(Action act) => Assert.Throws<VouchException>(act).Message;

    private static readonly DateTime Now = new(2026, 10, 19, 8, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime Yesterday = Now.AddDays(-1);

    [Fact] public void DateTime_Be_passes() { Now.Should().Be(Now); }
    [Fact] public void DateTime_Be_message() { var at = Yesterday; Assert.Equal("Expected at to be 2026-10-19T08:30:00.0000000Z, but found 2026-10-18T08:30:00.0000000Z.", Message(() => at.Should().Be(Now))); }
    [Fact] public void DateTime_NotBe_passes() { Now.Should().NotBe(Yesterday); }
    [Fact] public void DateTime_NotBe_message() { var at = Now; Assert.Equal("Expected at not to be 2026-10-19T08:30:00.0000000Z, but it was.", Message(() => at.Should().NotBe(Now))); }
    [Fact] public void DateTime_BeAfter_passes() { Now.Should().BeAfter(Yesterday); }
    [Fact] public void DateTime_BeAfter_fails_on_equal() { Assert.Throws<VouchException>(() => Now.Should().BeAfter(Now)); }
    [Fact] public void DateTime_BeAfter_message() { var at = Yesterday; Assert.Equal("Expected at to be after 2026-10-19T08:30:00.0000000Z, but found 2026-10-18T08:30:00.0000000Z.", Message(() => at.Should().BeAfter(Now))); }
    [Fact] public void DateTime_BeBefore_passes() { Yesterday.Should().BeBefore(Now); }
    [Fact] public void DateTime_BeBefore_message() { var at = Now; Assert.Equal("Expected at to be before 2026-10-18T08:30:00.0000000Z, but found 2026-10-19T08:30:00.0000000Z.", Message(() => at.Should().BeBefore(Yesterday))); }
    [Fact] public void DateTime_BeOnOrAfter_passes() { Now.Should().BeOnOrAfter(Now); }
    [Fact] public void DateTime_BeOnOrAfter_message() { var at = Yesterday; Assert.Equal("Expected at to be on or after 2026-10-19T08:30:00.0000000Z, but found 2026-10-18T08:30:00.0000000Z.", Message(() => at.Should().BeOnOrAfter(Now))); }
    [Fact] public void DateTime_BeOnOrBefore_passes() { Now.Should().BeOnOrBefore(Now); }
    [Fact] public void DateTime_BeOnOrBefore_message() { var at = Now; Assert.Equal("Expected at to be on or before 2026-10-18T08:30:00.0000000Z, but found 2026-10-19T08:30:00.0000000Z.", Message(() => at.Should().BeOnOrBefore(Yesterday))); }
    [Fact] public void DateTime_BeCloseTo_passes_on_boundary() { Now.AddSeconds(5).Should().BeCloseTo(Now, TimeSpan.FromSeconds(5)); }
    [Fact] public void DateTime_BeCloseTo_passes_before() { Now.AddSeconds(-5).Should().BeCloseTo(Now, TimeSpan.FromSeconds(5)); }
    [Fact] public void DateTime_BeCloseTo_message() { var at = Now.AddMinutes(1); Assert.Equal("Expected at to be within 00:00:05 of 2026-10-19T08:30:00.0000000Z, but found 2026-10-19T08:31:00.0000000Z (off by 00:01:00).", Message(() => at.Should().BeCloseTo(Now, TimeSpan.FromSeconds(5)))); }

    private static readonly DateTimeOffset Offset = new(2026, 10, 19, 8, 30, 0, TimeSpan.Zero);

    [Fact] public void DateTime_BeSameDateAs_passes() { Now.Should().BeSameDateAs(Now.AddHours(1)); }
    [Fact] public void DateTime_BeSameDateAs_message() { var at = Yesterday; Assert.Equal("Expected at to be the same date as 2026-10-19T08:30:00.0000000Z, but found 2026-10-18T08:30:00.0000000Z.", Message(() => at.Should().BeSameDateAs(Now))); }
    [Fact] public void DateTime_NotBeSameDateAs_passes() { Now.Should().NotBeSameDateAs(Yesterday); }
    [Fact] public void DateTime_NotBeSameDateAs_message() { var at = Now; Assert.Equal("Expected at not to be the same date as 2026-10-19T09:30:00.0000000Z, but found 2026-10-19T08:30:00.0000000Z.", Message(() => at.Should().NotBeSameDateAs(Now.AddHours(1)))); }

    [Fact] public void DateTime_HaveYear_passes() { Now.Should().HaveYear(2026).And.HaveMonth(10).And.HaveDay(19).And.HaveHour(8).And.HaveMinute(30).And.HaveSecond(0); }
    [Fact] public void DateTime_HaveYear_message() { var at = Now; Assert.Equal("Expected at to have year 2025, but found 2026.", Message(() => at.Should().HaveYear(2025))); }
    [Fact] public void DateTime_HaveMonth_message() { var at = Now; Assert.Equal("Expected at to have month 9, but found 10.", Message(() => at.Should().HaveMonth(9))); }
    [Fact] public void DateTime_HaveDay_message() { var at = Now; Assert.Equal("Expected at to have day 1, but found 19.", Message(() => at.Should().HaveDay(1))); }
    [Fact] public void DateTime_HaveHour_message() { var at = Now; Assert.Equal("Expected at to have hour 1, but found 8.", Message(() => at.Should().HaveHour(1))); }
    [Fact] public void DateTime_HaveMinute_message() { var at = Now; Assert.Equal("Expected at to have minute 1, but found 30.", Message(() => at.Should().HaveMinute(1))); }
    [Fact] public void DateTime_HaveSecond_message() { var at = Now; Assert.Equal("Expected at to have second 1, but found 0.", Message(() => at.Should().HaveSecond(1))); }
    [Fact] public void DateTime_NotHaveYear_passes() { Now.Should().NotHaveYear(2025).And.NotHaveMonth(9).And.NotHaveDay(1).And.NotHaveHour(1).And.NotHaveMinute(1).And.NotHaveSecond(1); }
    [Fact]
    public void DateTime_NotHave_messages()
    {
        var at = Now;
        Assert.Equal("Expected at not to have year 2026, but it did.", Message(() => at.Should().NotHaveYear(2026)));
        Assert.Equal("Expected at not to have month 10, but it did.", Message(() => at.Should().NotHaveMonth(10)));
        Assert.Equal("Expected at not to have day 19, but it did.", Message(() => at.Should().NotHaveDay(19)));
        Assert.Equal("Expected at not to have hour 8, but it did.", Message(() => at.Should().NotHaveHour(8)));
        Assert.Equal("Expected at not to have minute 30, but it did.", Message(() => at.Should().NotHaveMinute(30)));
        Assert.Equal("Expected at not to have second 0, but it did.", Message(() => at.Should().NotHaveSecond(0)));
    }

    [Fact] public void DateTime_BeWithin_Before_passes() { Now.AddMinutes(-3).Should().BeWithin(TimeSpan.FromMinutes(5)).Before(Now); Now.AddMinutes(-5).Should().BeWithin(TimeSpan.FromMinutes(5)).Before(Now); }
    [Fact] public void DateTime_BeWithin_Before_message() { var at = Now.AddMinutes(-10); Assert.Equal("Expected at to be within 00:05:00 before 2026-10-19T08:30:00.0000000Z, but found 2026-10-19T08:20:00.0000000Z.", Message(() => at.Should().BeWithin(TimeSpan.FromMinutes(5)).Before(Now))); }
    [Fact] public void DateTime_BeWithin_Before_fails_when_subject_is_after() { Assert.Throws<VouchException>(() => Now.AddMinutes(1).Should().BeWithin(TimeSpan.FromMinutes(5)).Before(Now)); }
    [Fact] public void DateTime_BeWithin_After_passes() { Now.AddMinutes(3).Should().BeWithin(TimeSpan.FromMinutes(5)).After(Now); Now.AddMinutes(5).Should().BeWithin(TimeSpan.FromMinutes(5)).After(Now); }
    [Fact] public void DateTime_BeWithin_After_message() { var at = Now.AddMinutes(10); Assert.Equal("Expected at to be within 00:05:00 after 2026-10-19T08:30:00.0000000Z, but found 2026-10-19T08:40:00.0000000Z.", Message(() => at.Should().BeWithin(TimeSpan.FromMinutes(5)).After(Now))); }
    [Fact] public void DateTime_BeWithin_After_fails_when_subject_is_before() { Assert.Throws<VouchException>(() => Now.AddMinutes(-1).Should().BeWithin(TimeSpan.FromMinutes(5)).After(Now)); }

    [Fact]
    public void Nullable_DateTime_new_assertions()
    {
        DateTime? at = Now;
        at.Should().BeSameDateAs(Now.AddHours(1));
        at.Should().NotBeSameDateAs(Yesterday);
        at.Should().HaveYear(2026).And.HaveMonth(10).And.HaveDay(19).And.HaveHour(8).And.HaveMinute(30).And.HaveSecond(0);
        at.Should().NotHaveYear(2025).And.NotHaveMonth(9).And.NotHaveDay(1).And.NotHaveHour(1).And.NotHaveMinute(1).And.NotHaveSecond(1);
        Now.AddMinutes(-3).Should().BeWithin(TimeSpan.FromMinutes(5)).Before(Now);
        DateTime? early = Now.AddMinutes(-3);
        early.Should().BeWithin(TimeSpan.FromMinutes(5)).Before(Now);
        DateTime? late = Now.AddMinutes(3);
        late.Should().BeWithin(TimeSpan.FromMinutes(5)).After(Now);

        Assert.Equal("Expected at to be the same date as 2026-10-18T08:30:00.0000000Z, but found 2026-10-19T08:30:00.0000000Z.", Message(() => at.Should().BeSameDateAs(Yesterday)));
        Assert.Equal("Expected at not to be the same date as 2026-10-19T09:30:00.0000000Z, but found 2026-10-19T08:30:00.0000000Z.", Message(() => at.Should().NotBeSameDateAs(Now.AddHours(1))));
        Assert.Equal("Expected at to have year 2025, but found 2026.", Message(() => at.Should().HaveYear(2025)));
        Assert.Equal("Expected at not to have second 0, but it did.", Message(() => at.Should().NotHaveSecond(0)));
        Assert.Equal("Expected early to be within 00:01:00 before 2026-10-19T08:30:00.0000000Z, but found 2026-10-19T08:27:00.0000000Z.", Message(() => early.Should().BeWithin(TimeSpan.FromMinutes(1)).Before(Now)));
        Assert.Equal("Expected late to be within 00:01:00 after 2026-10-19T08:30:00.0000000Z, but found 2026-10-19T08:33:00.0000000Z.", Message(() => late.Should().BeWithin(TimeSpan.FromMinutes(1)).After(Now)));

        DateTime? none = null;
        Assert.Equal("Expected none to be the same date as 2026-10-19T08:30:00.0000000Z, but found null.", Message(() => none.Should().BeSameDateAs(Now)));
        Assert.Equal("Expected none to have year 2026, but found null.", Message(() => none.Should().HaveYear(2026)));
        Assert.Equal("Expected none to have month 10, but found null.", Message(() => none.Should().HaveMonth(10)));
        Assert.Equal("Expected none to have day 19, but found null.", Message(() => none.Should().HaveDay(19)));
        Assert.Equal("Expected none to have hour 8, but found null.", Message(() => none.Should().HaveHour(8)));
        Assert.Equal("Expected none to have minute 30, but found null.", Message(() => none.Should().HaveMinute(30)));
        Assert.Equal("Expected none to have second 0, but found null.", Message(() => none.Should().HaveSecond(0)));
        Assert.Equal("Expected none to be within 00:05:00 before 2026-10-19T08:30:00.0000000Z, but found null.", Message(() => none.Should().BeWithin(TimeSpan.FromMinutes(5)).Before(Now)));
        Assert.Equal("Expected none to be within 00:05:00 after 2026-10-19T08:30:00.0000000Z, but found null.", Message(() => none.Should().BeWithin(TimeSpan.FromMinutes(5)).After(Now)));
        none.Should().NotBeSameDateAs(Now);
        none.Should().NotHaveYear(2026).And.NotHaveMonth(10).And.NotHaveDay(19).And.NotHaveHour(8).And.NotHaveMinute(30).And.NotHaveSecond(0);
    }

    [Fact]
    public void DateTimeOffset_new_assertions()
    {
        var at = Offset;
        at.Should().BeSameDateAs(Offset.AddHours(1));
        at.Should().NotBeSameDateAs(Offset.AddDays(-1));
        at.Should().HaveYear(2026).And.HaveMonth(10).And.HaveDay(19).And.HaveHour(8).And.HaveMinute(30).And.HaveSecond(0);
        at.Should().NotHaveYear(2025).And.NotHaveMonth(9).And.NotHaveDay(1).And.NotHaveHour(1).And.NotHaveMinute(1).And.NotHaveSecond(1);
        Offset.AddMinutes(-3).Should().BeWithin(TimeSpan.FromMinutes(5)).Before(Offset);
        Offset.AddMinutes(3).Should().BeWithin(TimeSpan.FromMinutes(5)).After(Offset);

        Assert.Equal("Expected at to be the same date as 2026-10-18T08:30:00.0000000+00:00, but found 2026-10-19T08:30:00.0000000+00:00.", Message(() => at.Should().BeSameDateAs(Offset.AddDays(-1))));
        Assert.Equal("Expected at not to be the same date as 2026-10-19T09:30:00.0000000+00:00, but found 2026-10-19T08:30:00.0000000+00:00.", Message(() => at.Should().NotBeSameDateAs(Offset.AddHours(1))));
        Assert.Equal("Expected at to have year 2025, but found 2026.", Message(() => at.Should().HaveYear(2025)));
        Assert.Equal("Expected at to have month 9, but found 10.", Message(() => at.Should().HaveMonth(9)));
        Assert.Equal("Expected at to have day 1, but found 19.", Message(() => at.Should().HaveDay(1)));
        Assert.Equal("Expected at to have hour 1, but found 8.", Message(() => at.Should().HaveHour(1)));
        Assert.Equal("Expected at to have minute 1, but found 30.", Message(() => at.Should().HaveMinute(1)));
        Assert.Equal("Expected at to have second 1, but found 0.", Message(() => at.Should().HaveSecond(1)));
        Assert.Equal("Expected at not to have year 2026, but it did.", Message(() => at.Should().NotHaveYear(2026)));
        Assert.Equal("Expected at not to have month 10, but it did.", Message(() => at.Should().NotHaveMonth(10)));
        Assert.Equal("Expected at not to have day 19, but it did.", Message(() => at.Should().NotHaveDay(19)));
        Assert.Equal("Expected at not to have hour 8, but it did.", Message(() => at.Should().NotHaveHour(8)));
        Assert.Equal("Expected at not to have minute 30, but it did.", Message(() => at.Should().NotHaveMinute(30)));
        Assert.Equal("Expected at not to have second 0, but it did.", Message(() => at.Should().NotHaveSecond(0)));
        var early = Offset.AddMinutes(-10);
        Assert.Equal("Expected early to be within 00:05:00 before 2026-10-19T08:30:00.0000000+00:00, but found 2026-10-19T08:20:00.0000000+00:00.", Message(() => early.Should().BeWithin(TimeSpan.FromMinutes(5)).Before(Offset)));
        var late = Offset.AddMinutes(10);
        Assert.Equal("Expected late to be within 00:05:00 after 2026-10-19T08:30:00.0000000+00:00, but found 2026-10-19T08:40:00.0000000+00:00.", Message(() => late.Should().BeWithin(TimeSpan.FromMinutes(5)).After(Offset)));
    }

    [Fact]
    public void Nullable_DateTimeOffset_new_assertions()
    {
        DateTimeOffset? at = Offset;
        at.Should().BeSameDateAs(Offset.AddHours(1));
        at.Should().NotBeSameDateAs(Offset.AddDays(-1));
        at.Should().HaveYear(2026).And.HaveMonth(10).And.HaveDay(19).And.HaveHour(8).And.HaveMinute(30).And.HaveSecond(0);
        at.Should().NotHaveYear(2025).And.NotHaveMonth(9).And.NotHaveDay(1).And.NotHaveHour(1).And.NotHaveMinute(1).And.NotHaveSecond(1);
        DateTimeOffset? early = Offset.AddMinutes(-3);
        early.Should().BeWithin(TimeSpan.FromMinutes(5)).Before(Offset);
        DateTimeOffset? late = Offset.AddMinutes(3);
        late.Should().BeWithin(TimeSpan.FromMinutes(5)).After(Offset);

        Assert.Equal("Expected at to be the same date as 2026-10-18T08:30:00.0000000+00:00, but found 2026-10-19T08:30:00.0000000+00:00.", Message(() => at.Should().BeSameDateAs(Offset.AddDays(-1))));
        Assert.Equal("Expected at not to be the same date as 2026-10-19T09:30:00.0000000+00:00, but found 2026-10-19T08:30:00.0000000+00:00.", Message(() => at.Should().NotBeSameDateAs(Offset.AddHours(1))));
        Assert.Equal("Expected at to have hour 1, but found 8.", Message(() => at.Should().HaveHour(1)));
        Assert.Equal("Expected at not to have hour 8, but it did.", Message(() => at.Should().NotHaveHour(8)));
        Assert.Equal("Expected early to be within 00:01:00 before 2026-10-19T08:30:00.0000000+00:00, but found 2026-10-19T08:27:00.0000000+00:00.", Message(() => early.Should().BeWithin(TimeSpan.FromMinutes(1)).Before(Offset)));
        Assert.Equal("Expected late to be within 00:01:00 after 2026-10-19T08:30:00.0000000+00:00, but found 2026-10-19T08:33:00.0000000+00:00.", Message(() => late.Should().BeWithin(TimeSpan.FromMinutes(1)).After(Offset)));

        DateTimeOffset? none = null;
        Assert.Equal("Expected none to be the same date as 2026-10-19T08:30:00.0000000+00:00, but found null.", Message(() => none.Should().BeSameDateAs(Offset)));
        Assert.Equal("Expected none to have year 2026, but found null.", Message(() => none.Should().HaveYear(2026)));
        Assert.Equal("Expected none to have second 0, but found null.", Message(() => none.Should().HaveSecond(0)));
        Assert.Equal("Expected none to be within 00:05:00 before 2026-10-19T08:30:00.0000000+00:00, but found null.", Message(() => none.Should().BeWithin(TimeSpan.FromMinutes(5)).Before(Offset)));
        Assert.Equal("Expected none to be within 00:05:00 after 2026-10-19T08:30:00.0000000+00:00, but found null.", Message(() => none.Should().BeWithin(TimeSpan.FromMinutes(5)).After(Offset)));
        none.Should().NotBeSameDateAs(Offset);
        none.Should().NotHaveYear(2026);
    }

    [Fact]
    public void DateOnly_year_month_day()
    {
        var d = new DateOnly(2026, 10, 19);
        d.Should().HaveYear(2026).And.HaveMonth(10).And.HaveDay(19);
        d.Should().NotHaveYear(2025).And.NotHaveMonth(9).And.NotHaveDay(1);
        Assert.Equal("Expected d to have year 2025, but found 2026.", Message(() => d.Should().HaveYear(2025)));
        Assert.Equal("Expected d to have month 9, but found 10.", Message(() => d.Should().HaveMonth(9)));
        Assert.Equal("Expected d to have day 1, but found 19.", Message(() => d.Should().HaveDay(1)));
        Assert.Equal("Expected d not to have year 2026, but it did.", Message(() => d.Should().NotHaveYear(2026)));
        Assert.Equal("Expected d not to have month 10, but it did.", Message(() => d.Should().NotHaveMonth(10)));
        Assert.Equal("Expected d not to have day 19, but it did.", Message(() => d.Should().NotHaveDay(19)));
    }

    [Fact]
    public void Nullable_DateOnly_year_month_day()
    {
        DateOnly? d = new DateOnly(2026, 10, 19);
        d.Should().HaveYear(2026).And.HaveMonth(10).And.HaveDay(19);
        d.Should().NotHaveYear(2025).And.NotHaveMonth(9).And.NotHaveDay(1);
        Assert.Equal("Expected d to have year 2025, but found 2026.", Message(() => d.Should().HaveYear(2025)));
        Assert.Equal("Expected d not to have day 19, but it did.", Message(() => d.Should().NotHaveDay(19)));
        DateOnly? none = null;
        Assert.Equal("Expected none to have year 2026, but found null.", Message(() => none.Should().HaveYear(2026)));
        Assert.Equal("Expected none to have month 10, but found null.", Message(() => none.Should().HaveMonth(10)));
        Assert.Equal("Expected none to have day 19, but found null.", Message(() => none.Should().HaveDay(19)));
        none.Should().NotHaveYear(2026).And.NotHaveMonth(10).And.NotHaveDay(19);
    }

    [Fact]
    public void Nullable_DateTime_null_fails()
    {
        DateTime? at = null;
        Assert.Equal("Expected at to be after 2026-10-19T08:30:00.0000000Z, but found null.", Message(() => at.Should().BeAfter(Now)));
        Assert.Equal("Expected at to be before 2026-10-19T08:30:00.0000000Z, but found null.", Message(() => at.Should().BeBefore(Now)));
        Assert.Equal("Expected at to be on or after 2026-10-19T08:30:00.0000000Z, but found null.", Message(() => at.Should().BeOnOrAfter(Now)));
        Assert.Equal("Expected at to be on or before 2026-10-19T08:30:00.0000000Z, but found null.", Message(() => at.Should().BeOnOrBefore(Now)));
        Assert.Equal("Expected at to be 2026-10-19T08:30:00.0000000Z, but found null.", Message(() => at.Should().Be(Now)));
        Assert.Equal("Expected at to be within 00:00:05 of 2026-10-19T08:30:00.0000000Z, but found null.", Message(() => at.Should().BeCloseTo(Now, TimeSpan.FromSeconds(5))));
        at.Should().NotBe(Now);
        at.Should().BeNull();
    }
    [Fact]
    public void Nullable_DateTime_passes()
    {
        DateTime? at = Now;
        at.Should().Be(Now).And.BeOnOrAfter(Now).And.BeOnOrBefore(Now).And.BeAfter(Yesterday).And.BeBefore(Now.AddDays(1));
        at.Should().BeCloseTo(Now, TimeSpan.Zero);
        Assert.Equal(Now, at.Should().NotBeNull().Which);
    }
    [Fact]
    public void Nullable_DateTime_fails_with_value()
    {
        DateTime? at = Now.AddMinutes(1);
        Assert.Equal("Expected at to be within 00:00:05 of 2026-10-19T08:30:00.0000000Z, but found 2026-10-19T08:31:00.0000000Z (off by 00:01:00).", Message(() => at.Should().BeCloseTo(Now, TimeSpan.FromSeconds(5))));
        Assert.Equal("Expected at to be null, but found 2026-10-19T08:31:00.0000000Z.", Message(() => at.Should().BeNull()));
        Assert.Equal("Expected at not to be 2026-10-19T08:31:00.0000000Z, but it was.", Message(() => at.Should().NotBe(Now.AddMinutes(1))));
        DateTime? none = null;
        Assert.Equal("Expected none not to be null, but it was.", Message(() => none.Should().NotBeNull()));
    }

    [Fact] public void DateTimeOffset_Be_compares_the_instant() { Offset.Should().Be(new DateTimeOffset(2026, 10, 19, 16, 30, 0, TimeSpan.FromHours(8))); }
    [Fact] public void DateTimeOffset_Be_message() { var at = Offset.AddDays(-1); Assert.Equal("Expected at to be 2026-10-19T08:30:00.0000000+00:00, but found 2026-10-18T08:30:00.0000000+00:00.", Message(() => at.Should().Be(Offset))); }
    [Fact] public void DateTimeOffset_NotBe() { Offset.Should().NotBe(Offset.AddHours(1)); Assert.Throws<VouchException>(() => Offset.Should().NotBe(Offset)); }
    [Fact]
    public void DateTimeOffset_ordering()
    {
        Offset.Should().BeAfter(Offset.AddSeconds(-1)).And.BeBefore(Offset.AddSeconds(1)).And.BeOnOrAfter(Offset).And.BeOnOrBefore(Offset);
        Assert.Throws<VouchException>(() => Offset.Should().BeAfter(Offset));
        Assert.Throws<VouchException>(() => Offset.Should().BeBefore(Offset));
        Assert.Throws<VouchException>(() => Offset.Should().BeOnOrAfter(Offset.AddSeconds(1)));
        Assert.Throws<VouchException>(() => Offset.Should().BeOnOrBefore(Offset.AddSeconds(-1)));
    }
    [Fact] public void DateTimeOffset_NotBe_message() { var at = Offset; Assert.Equal("Expected at not to be 2026-10-19T08:30:00.0000000+00:00, but it was.", Message(() => at.Should().NotBe(Offset))); }
    [Fact] public void DateTimeOffset_BeBefore_message() { var at = Offset; Assert.Equal("Expected at to be before 2026-10-18T08:30:00.0000000+00:00, but found 2026-10-19T08:30:00.0000000+00:00.", Message(() => at.Should().BeBefore(Offset.AddDays(-1)))); }
    [Fact] public void DateTimeOffset_BeOnOrAfter_message() { var at = Offset.AddDays(-1); Assert.Equal("Expected at to be on or after 2026-10-19T08:30:00.0000000+00:00, but found 2026-10-18T08:30:00.0000000+00:00.", Message(() => at.Should().BeOnOrAfter(Offset))); }
    [Fact] public void DateTimeOffset_BeOnOrBefore_message() { var at = Offset; Assert.Equal("Expected at to be on or before 2026-10-18T08:30:00.0000000+00:00, but found 2026-10-19T08:30:00.0000000+00:00.", Message(() => at.Should().BeOnOrBefore(Offset.AddDays(-1)))); }
    [Fact] public void DateTimeOffset_BeAfter_message() { var at = Offset.AddDays(-1); Assert.Equal("Expected at to be after 2026-10-19T08:30:00.0000000+00:00, but found 2026-10-18T08:30:00.0000000+00:00.", Message(() => at.Should().BeAfter(Offset))); }
    [Fact] public void DateTimeOffset_BeCloseTo() { Offset.AddSeconds(5).Should().BeCloseTo(Offset, TimeSpan.FromSeconds(5)); var at = Offset.AddMinutes(1); Assert.Equal("Expected at to be within 00:00:05 of 2026-10-19T08:30:00.0000000+00:00, but found 2026-10-19T08:31:00.0000000+00:00 (off by 00:01:00).", Message(() => at.Should().BeCloseTo(Offset, TimeSpan.FromSeconds(5)))); }
    [Fact]
    public void Nullable_DateTimeOffset()
    {
        DateTimeOffset? at = null;
        at.Should().BeNull();
        Assert.Equal("Expected at to be after 2026-10-19T08:30:00.0000000+00:00, but found null.", Message(() => at.Should().BeAfter(Offset)));
        DateTimeOffset? some = Offset;
        some.Should().Be(Offset).And.BeCloseTo(Offset, TimeSpan.Zero);
        Assert.Equal(Offset, some.Should().NotBeNull().Which);
    }

    private static readonly DateOnly Day = new(2026, 10, 19);

    [Fact] public void DateOnly_Be_passes() { Day.Should().Be(Day); }
    [Fact] public void DateOnly_Be_message() { var d = Day.AddDays(-1); Assert.Equal("Expected d to be 2026-10-19, but found 2026-10-18.", Message(() => d.Should().Be(Day))); }
    [Fact] public void DateOnly_NotBe() { Day.Should().NotBe(Day.AddDays(1)); var d = Day; Assert.Equal("Expected d not to be 2026-10-19, but it was.", Message(() => d.Should().NotBe(Day))); }
    [Fact]
    public void DateOnly_ordering()
    {
        Day.Should().BeAfter(Day.AddDays(-1)).And.BeBefore(Day.AddDays(1)).And.BeOnOrAfter(Day).And.BeOnOrBefore(Day);
        var d = Day;
        Assert.Equal("Expected d to be after 2026-10-19, but found 2026-10-19.", Message(() => d.Should().BeAfter(Day)));
        Assert.Equal("Expected d to be before 2026-10-19, but found 2026-10-19.", Message(() => d.Should().BeBefore(Day)));
        Assert.Equal("Expected d to be on or after 2026-10-20, but found 2026-10-19.", Message(() => d.Should().BeOnOrAfter(Day.AddDays(1))));
        Assert.Equal("Expected d to be on or before 2026-10-18, but found 2026-10-19.", Message(() => d.Should().BeOnOrBefore(Day.AddDays(-1))));
    }
    [Fact]
    public void Nullable_DateOnly()
    {
        DateOnly? d = null;
        d.Should().BeNull();
        Assert.Equal("Expected d to be after 2026-10-19, but found null.", Message(() => d.Should().BeAfter(Day)));
        DateOnly? some = Day;
        some.Should().Be(Day).And.BeOnOrAfter(Day);
        Assert.Equal(Day, some.Should().NotBeNull().Which);
    }

    [Fact] public void TimeSpan_still_resolves() { TimeSpan.FromSeconds(1).Should().Be(TimeSpan.FromSeconds(1)); TimeSpan? t = null; t.Should().BeNull(); }
    [Fact]
    public void Overloads_resolve_to_the_specific_assertions()
    {
        Assert.IsType<NumericAssertions<int>>(1.Should());
        Assert.IsType<NumericAssertions<long>>(1L.Should());
        Assert.IsType<NumericAssertions<decimal>>(1m.Should());
        Assert.IsType<NumericAssertions<double>>(1d.Should());
        Assert.IsType<NullableNumericAssertions<int>>(((int?)1).Should());
        Assert.IsType<DateTimeAssertions>(Now.Should());
        Assert.IsType<DateTimeOffsetAssertions>(Offset.Should());
        Assert.IsType<DateOnlyAssertions>(Day.Should());
        Assert.IsType<NullableDateTimeAssertions>(((DateTime?)Now).Should());
        Assert.IsType<ValueAssertions<TimeSpan>>(TimeSpan.Zero.Should());
    }
}
