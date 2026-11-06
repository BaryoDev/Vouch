using Xunit;

namespace Vouch.Tests;

public class NumericDatesFixRound1Tests
{
    private static string Message(Action act) => Assert.Throws<VouchException>(act).Message;

    // Delta validation

    [Fact]
    public void NaN_delta_throws_for_every_closeness_assertion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => 1.0.Should().BeCloseTo(1.0, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => 1.0.Should().NotBeCloseTo(2.0, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => 1.0.Should().BeApproximately(1.0, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => 1f.Should().BeCloseTo(1f, float.NaN));
        double? d = 1.0;
        double? none = null;
        Assert.Throws<ArgumentOutOfRangeException>(() => d.Should().BeCloseTo(1.0, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => none.Should().NotBeCloseTo(1.0, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => d.Should().BeApproximately(1.0, double.NaN));
    }

    [Fact]
    public void Negative_delta_throws_for_every_closeness_assertion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => 5.Should().BeCloseTo(5, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => 5.Should().NotBeCloseTo(9, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => 1.0.Should().BeApproximately(1.0, -0.1));
        Assert.Throws<ArgumentOutOfRangeException>(() => 1m.Should().BeApproximately(1m, -0.1m));
        int? n = 5;
        int? none = null;
        Assert.Throws<ArgumentOutOfRangeException>(() => n.Should().BeCloseTo(5, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => none.Should().BeCloseTo(5, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => n.Should().NotBeCloseTo(9, -1));
    }

    [Fact]
    public void Zero_delta_is_still_valid()
    {
        5.Should().BeCloseTo(5, 0);
        5.Should().NotBeCloseTo(6, 0);
    }

    [Fact]
    public void Non_finite_floating_point_difference_uses_the_out_of_range_wording()
    {
        var d = double.MinValue;
        Assert.EndsWith("(difference is out of range).", Message(() => d.Should().BeCloseTo(double.MaxValue, 1)));
        var inf = double.PositiveInfinity;
        Assert.EndsWith("(difference is out of range).", Message(() => inf.Should().BeApproximately(double.NegativeInfinity, 1)));
    }

    [Fact]
    public void Off_by_detail_is_still_reported_on_failure()
    {
        var n = 5;
        Assert.Equal("Expected n to be within 2 of 9, but found 5 (off by 4).", Message(() => n.Should().BeCloseTo(9, 2)));
        var d = 1.75;
        Assert.Equal("Expected d to approximate 1.5 +/- 0.125, but found 1.75 (off by 0.25).", Message(() => d.Should().BeApproximately(1.5, 0.125)));
    }

    // Half

    [Fact]
    public void Half_resolves_to_numeric_assertions()
    {
        Assert.IsType<NumericAssertions<Half>>(Half.One.Should());
        Assert.IsType<NullableNumericAssertions<Half>>(((Half?)Half.One).Should());
    }

    [Fact]
    public void Half_numeric_assertions_pass()
    {
        var h = (Half)1.5;
        h.Should().BeCloseTo((Half)1.75, (Half)0.25).And.BeApproximately((Half)1.5, (Half)0).And.BePositive();
        ((Half)(-2)).Should().BeNegative();
        Half.NaN.Should().BeNaN();
        Half.One.Should().BeFinite();
        Half.PositiveInfinity.Should().NotBeFinite();
        Half.One.Should().NotBeNaN();
        Half.MinValue.Should().NotBeCloseTo(Half.MaxValue, Half.One);
        Assert.Throws<VouchException>(() => Half.One.Should().BeCloseTo((Half)3, Half.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => Half.One.Should().BeCloseTo(Half.One, Half.NaN));
    }

    [Fact]
    public void Nullable_Half_assertions()
    {
        Half? h = Half.One;
        Half? none = null;
        h.Should().BeCloseTo((Half)1.5, (Half)0.5).And.BePositive();
        h.Should().BeFinite();
        Assert.Throws<VouchException>(() => none.Should().BeFinite());
        Assert.Throws<VouchException>(() => none.Should().BeNaN());
    }

    // Nullable BeNaN / BeFinite

    [Fact]
    public void Nullable_double_and_float_BeNaN_and_BeFinite()
    {
        double? nan = double.NaN;
        double? one = 1.0;
        double? none = null;
        nan.Should().BeNaN();
        one.Should().BeFinite();
        Assert.Equal("Expected one to be NaN, but found 1.", Message(() => one.Should().BeNaN()));
        Assert.StartsWith("Expected nan to be finite, but found ", Message(() => nan.Should().BeFinite()));
        Assert.Equal("Expected none to be NaN, but found null.", Message(() => none.Should().BeNaN()));
        Assert.StartsWith("Expected none to be finite, but found ", Message(() => none.Should().BeFinite()));

        float? fnan = float.NaN;
        float? finf = float.PositiveInfinity;
        fnan.Should().BeNaN();
        Assert.Throws<VouchException>(() => finf.Should().BeFinite());
        float? fone = 1f;
        fone.Should().BeFinite();
        Assert.Throws<VouchException>(() => fone.Should().BeNaN());
    }

    // HaveValue / NotHaveValue

    [Fact]
    public void DateTime_HaveValue_and_NotHaveValue()
    {
        DateTime? some = new DateTime(2026, 1, 2);
        DateTime? none = null;
        Assert.Equal(new DateTime(2026, 1, 2), some.Should().HaveValue().Which);
        none.Should().NotHaveValue();
        Assert.StartsWith("Expected none to have a value, but found null.", Message(() => none.Should().HaveValue()));
        Assert.StartsWith("Expected some not to have a value, but found ", Message(() => some.Should().NotHaveValue()));
    }

    [Fact]
    public void DateTimeOffset_HaveValue_and_NotHaveValue()
    {
        DateTimeOffset? some = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        DateTimeOffset? none = null;
        Assert.Equal(some.Value, some.Should().HaveValue().Which);
        none.Should().NotHaveValue();
        Assert.StartsWith("Expected none to have a value, but found null.", Message(() => none.Should().HaveValue()));
        Assert.StartsWith("Expected some not to have a value, but found ", Message(() => some.Should().NotHaveValue()));
    }

    [Fact]
    public void DateOnly_HaveValue_and_NotHaveValue()
    {
        DateOnly? some = new DateOnly(2026, 1, 2);
        DateOnly? none = null;
        Assert.Equal(new DateOnly(2026, 1, 2), some.Should().HaveValue().Which);
        none.Should().NotHaveValue();
        Assert.StartsWith("Expected none to have a value, but found null.", Message(() => none.Should().HaveValue()));
        Assert.StartsWith("Expected some not to have a value, but found ", Message(() => some.Should().NotHaveValue()));
    }

    [Fact]
    public void TimeOnly_HaveValue_and_NotHaveValue()
    {
        TimeOnly? some = new TimeOnly(3, 4);
        TimeOnly? none = null;
        Assert.Equal(new TimeOnly(3, 4), some.Should().HaveValue().Which);
        none.Should().NotHaveValue();
        Assert.StartsWith("Expected none to have a value, but found null.", Message(() => none.Should().HaveValue()));
        Assert.StartsWith("Expected some not to have a value, but found ", Message(() => some.Should().NotHaveValue()));
    }

    [Fact]
    public void Boolean_HaveValue_and_NotHaveValue()
    {
        bool? some = false;
        bool? none = null;
        Assert.False(some.Should().HaveValue().Which);
        none.Should().NotHaveValue();
        Assert.StartsWith("Expected none to have a value, but found null.", Message(() => none.Should().HaveValue()));
        Assert.StartsWith("Expected some not to have a value, but found ", Message(() => some.Should().NotHaveValue()));
    }

    // BeWithin

    [Fact]
    public void BeWithin_throws_for_a_negative_span()
    {
        var negative = TimeSpan.FromSeconds(-1);
        var dt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var dto = new DateTimeOffset(dt);
        DateTime? ndt = dt;
        DateTimeOffset? ndto = dto;
        Assert.Throws<ArgumentOutOfRangeException>(() => dt.Should().BeWithin(negative));
        Assert.Throws<ArgumentOutOfRangeException>(() => dto.Should().BeWithin(negative));
        Assert.Throws<ArgumentOutOfRangeException>(() => ndt.Should().BeWithin(negative));
        Assert.Throws<ArgumentOutOfRangeException>(() => ndto.Should().BeWithin(negative));
        dt.Should().BeWithin(TimeSpan.Zero).After(dt);
    }

    // Offset-local semantics

    [Fact]
    public void DateTimeOffset_date_parts_use_the_local_offset_fields()
    {
        // 23:30+02:00 on Jan 1 is 21:30 UTC on Jan 1; 01:30+02:00 on Jan 2 is 23:30 UTC on Jan 1.
        var plusTwo = new DateTimeOffset(2026, 1, 2, 1, 30, 0, TimeSpan.FromHours(2));
        var utcSameInstantDay = new DateTimeOffset(2026, 1, 1, 23, 30, 0, TimeSpan.Zero);
        Assert.Equal(plusTwo.UtcDateTime, utcSameInstantDay.UtcDateTime);

        plusTwo.Should().HaveDay(2).And.HaveHour(1).And.HaveMonth(1).And.HaveYear(2026);
        plusTwo.Should().NotHaveDay(1).And.NotHaveHour(23);
        plusTwo.Should().BeSameDateAs(new DateTimeOffset(2026, 1, 2, 22, 0, 0, TimeSpan.Zero));
        Assert.Throws<VouchException>(() => plusTwo.Should().BeSameDateAs(utcSameInstantDay));
        plusTwo.Should().NotBeSameDateAs(utcSameInstantDay);

        DateTimeOffset? nullable = plusTwo;
        nullable.Should().HaveDay(2).And.HaveHour(1);
        nullable.Should().BeSameDateAs(new DateTimeOffset(2026, 1, 2, 22, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void DateTime_BeSameDateAs_ignores_Kind()
    {
        var utc = new DateTime(2026, 1, 2, 23, 0, 0, DateTimeKind.Utc);
        var local = new DateTime(2026, 1, 2, 1, 0, 0, DateTimeKind.Local);
        var unspecified = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Unspecified);
        utc.Should().BeSameDateAs(local);
        local.Should().BeSameDateAs(unspecified);
        Assert.Throws<VouchException>(() => utc.Should().NotBeSameDateAs(local));
    }
}
