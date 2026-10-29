using Xunit;

namespace Vouch.Tests;

public class NumericAssertionsTests
{
    private static string Message(Action act) => Assert.Throws<VouchException>(act).Message;

    [Fact] public void Be_passes() { 5.Should().Be(5); }
    [Fact] public void Be_fails() { Assert.Throws<VouchException>(() => 5.Should().Be(6)); }
    [Fact] public void Be_message() { var n = 3; Assert.Equal("Expected n to be 5, but found 3.", Message(() => n.Should().Be(5))); }

    [Fact] public void NotBe_passes() { 5.Should().NotBe(6); }
    [Fact] public void NotBe_fails() { Assert.Throws<VouchException>(() => 5.Should().NotBe(5)); }
    [Fact] public void NotBe_message() { var n = 5; Assert.Equal("Expected n not to be 5, but it was.", Message(() => n.Should().NotBe(5))); }

    [Fact] public void BeGreaterThan_passes() { 6.Should().BeGreaterThan(5); }
    [Fact] public void BeGreaterThan_fails_on_equal() { Assert.Throws<VouchException>(() => 5.Should().BeGreaterThan(5)); }
    [Fact] public void BeGreaterThan_message() { var n = 3; Assert.Equal("Expected n to be greater than 5, but found 3.", Message(() => n.Should().BeGreaterThan(5))); }

    [Fact] public void BeGreaterThanOrEqualTo_passes() { 5.Should().BeGreaterThanOrEqualTo(5); }
    [Fact] public void BeGreaterThanOrEqualTo_fails() { Assert.Throws<VouchException>(() => 4.Should().BeGreaterThanOrEqualTo(5)); }
    [Fact] public void BeGreaterThanOrEqualTo_message() { var n = 4; Assert.Equal("Expected n to be greater than or equal to 5, but found 4.", Message(() => n.Should().BeGreaterThanOrEqualTo(5))); }

    [Fact] public void BeLessThan_passes() { 4.Should().BeLessThan(5); }
    [Fact] public void BeLessThan_fails_on_equal() { Assert.Throws<VouchException>(() => 5.Should().BeLessThan(5)); }
    [Fact] public void BeLessThan_message() { var n = 7; Assert.Equal("Expected n to be less than 5, but found 7.", Message(() => n.Should().BeLessThan(5))); }

    [Fact] public void BeLessThanOrEqualTo_passes() { 5.Should().BeLessThanOrEqualTo(5); }
    [Fact] public void BeLessThanOrEqualTo_fails() { Assert.Throws<VouchException>(() => 6.Should().BeLessThanOrEqualTo(5)); }
    [Fact] public void BeLessThanOrEqualTo_message() { var n = 6; Assert.Equal("Expected n to be less than or equal to 5, but found 6.", Message(() => n.Should().BeLessThanOrEqualTo(5))); }

    [Fact] public void BePositive_passes() { 1.Should().BePositive(); }
    [Fact] public void BePositive_fails_on_zero() { Assert.Throws<VouchException>(() => 0.Should().BePositive()); }
    [Fact] public void BePositive_message() { var n = -2; Assert.Equal("Expected n to be positive, but found -2.", Message(() => n.Should().BePositive())); }

    [Fact] public void BeInRange_is_inclusive() { 10.Should().BeInRange(1, 10); 1.Should().BeInRange(1, 10); }
    [Fact] public void BeInRange_fails() { Assert.Throws<VouchException>(() => 11.Should().BeInRange(1, 10)); }
    [Fact] public void BeInRange_message() { var n = 11; Assert.Equal("Expected n to be between 1 and 10, but found 11.", Message(() => n.Should().BeInRange(1, 10))); }

    [Fact] public void BeOneOf_passes() { 2.Should().BeOneOf(1, 2, 3); }
    [Fact] public void BeOneOf_fails() { Assert.Throws<VouchException>(() => 4.Should().BeOneOf(1, 2, 3)); }
    [Fact] public void BeOneOf_message() { var n = 4; Assert.Equal("Expected n to be one of [1, 2, 3], but found 4.", Message(() => n.Should().BeOneOf(1, 2, 3))); }

    [Fact] public void BeApproximately_passes_on_the_boundary() { 1.75.Should().BeApproximately(1.5, 0.25); }
    [Fact] public void BeApproximately_fails() { Assert.Throws<VouchException>(() => 1.75.Should().BeApproximately(1.5, 0.125)); }
    [Fact] public void BeApproximately_message() { var d = 1.75; Assert.Equal("Expected d to approximate 1.5 +/- 0.125, but found 1.75 (off by 0.25).", Message(() => d.Should().BeApproximately(1.5, 0.125))); }
    [Fact] public void NaN_never_approximates() { double d = double.NaN; Assert.Throws<VouchException>(() => d.Should().BeApproximately(1.0, 100)); }
    [Fact] public void Decimal_BeApproximately_passes() { 1.6m.Should().BeApproximately(1.5m, 0.1m); }
    [Fact] public void Decimal_BeApproximately_message() { var m = 1.6m; Assert.Equal("Expected m to approximate 1.5m +/- 0.01m, but found 1.6m (off by 0.1m).", Message(() => m.Should().BeApproximately(1.5m, 0.01m))); }
    [Fact] public void Float_BeApproximately_passes() { 1.5f.Should().BeApproximately(1.5f, 0f); }

    [Fact] public void NaN_equals_NaN_in_Be() { double d = double.NaN; d.Should().Be(double.NaN); float f = float.NaN; f.Should().Be(float.NaN); }
    [Fact] public void NaN_fails_NotBe_NaN() { double d = double.NaN; Assert.Equal("Expected d not to be NaN, but it was.", Message(() => d.Should().NotBe(double.NaN))); float f = float.NaN; Assert.Equal("Expected f not to be NaNf, but it was.", Message(() => f.Should().NotBe(float.NaN))); }
    [Fact] public void NaN_passes_NotBe_a_number() { double.NaN.Should().NotBe(1.0); float.NaN.Should().NotBe(1f); }
    [Fact] public void NaN_passes_BeOneOf_NaN() { double.NaN.Should().BeOneOf(1.0, double.NaN); float.NaN.Should().BeOneOf(1f, float.NaN); }
    [Fact] public void NaN_fails_BeOneOf_without_NaN() { double d = double.NaN; Assert.Throws<VouchException>(() => d.Should().BeOneOf(1.0, 2.0)); }
    [Fact] public void Nullable_NaN_equals_NaN_in_Be() { double? d = double.NaN; d.Should().Be(double.NaN); float? f = float.NaN; f.Should().Be(float.NaN); }
    [Fact] public void Nullable_NaN_fails_NotBe_NaN() { double? d = double.NaN; Assert.Throws<VouchException>(() => d.Should().NotBe(double.NaN)); float? f = float.NaN; Assert.Throws<VouchException>(() => f.Should().NotBe(float.NaN)); }
    [Fact] public void Nullable_NaN_passes_BeOneOf_NaN() { double? d = double.NaN; d.Should().BeOneOf(1.0, double.NaN); float? f = float.NaN; f.Should().BeOneOf(1f, float.NaN); }
    [Fact] public void Null_still_fails_Be_NaN_and_passes_NotBe_NaN() { double? d = null; Assert.Throws<VouchException>(() => d.Should().Be(double.NaN)); d.Should().NotBe(double.NaN); Assert.Throws<VouchException>(() => d.Should().BeOneOf(double.NaN)); }
    [Fact] public void NaN_fails_comparisons() { double d = double.NaN; Assert.Throws<VouchException>(() => d.Should().BeGreaterThan(0)); Assert.Throws<VouchException>(() => d.Should().BeLessThan(0)); Assert.Throws<VouchException>(() => d.Should().BeInRange(-1, 1)); }

    [Fact]
    public void Decimal_message_uses_m()
    {
        var total = 100m;
        Assert.Equal("Expected total to be 120m, but found 100m.", Message(() => total.Should().Be(120m)));
    }
    [Fact] public void Long_message_uses_L() { var big = 3L; Assert.Equal("Expected big to be 5L, but found 3L.", Message(() => big.Should().Be(5L))); }

    [Fact]
    public void Other_numeric_types_resolve()
    {
        ((short)1).Should().Be(1);
        ((byte)1).Should().Be(1);
        1f.Should().Be(1f);
        1L.Should().BeGreaterThan(0L);
        1m.Should().BePositive();
        1d.Should().BePositive();
    }

    [Fact] public void Nullable_Be_passes() { int? n = 5; n.Should().Be(5); }
    [Fact] public void Null_decimal_fails_comparison() { decimal? d = null; Assert.Throws<VouchException>(() => d.Should().BeGreaterThan(0)); }
    [Fact] public void Null_comparison_message() { decimal? d = null; Assert.Equal("Expected d to be greater than 0m, but found null.", Message(() => d.Should().BeGreaterThan(0m))); }
    [Fact] public void Nullable_BeNull_passes() { int? n = null; n.Should().BeNull(); }
    [Fact] public void Nullable_BeNull_fails() { int? n = 1; Assert.Equal("Expected n to be null, but found 1.", Message(() => n.Should().BeNull())); }
    [Fact] public void Nullable_NotBeNull_gives_value() { int? n = 7; Assert.Equal(7, n.Should().NotBeNull().Which); }
    [Fact] public void Nullable_NotBeNull_fails() { int? n = null; Assert.Equal("Expected n not to be null, but it was.", Message(() => n.Should().NotBeNull())); }
    [Fact] public void Nullable_NotBe_passes_on_null() { int? n = null; n.Should().NotBe(5); }
    [Fact] public void Nullable_NotBe_message() { int? n = 5; Assert.Equal("Expected n not to be 5, but it was.", Message(() => n.Should().NotBe(5))); }
    [Fact]
    public void Nullable_range_one_of_and_ordering()
    {
        int? n = 5;
        n.Should().BeInRange(1, 5).And.BeOneOf(5, 6).And.BePositive();
        n.Should().BeGreaterThanOrEqualTo(5).And.BeLessThanOrEqualTo(5).And.BeLessThan(6);
    }
    [Fact]
    public void Nullable_ordering_fails_with_message()
    {
        int? n = null;
        Assert.Equal("Expected n to be between 1 and 10, but found null.", Message(() => n.Should().BeInRange(1, 10)));
        Assert.Equal("Expected n to be positive, but found null.", Message(() => n.Should().BePositive()));
        Assert.Equal("Expected n to be one of [1], but found null.", Message(() => n.Should().BeOneOf(1)));
        Assert.Equal("Expected n to be less than 1, but found null.", Message(() => n.Should().BeLessThan(1)));
        Assert.Equal("Expected n to be less than or equal to 1, but found null.", Message(() => n.Should().BeLessThanOrEqualTo(1)));
        Assert.Equal("Expected n to be greater than or equal to 1, but found null.", Message(() => n.Should().BeGreaterThanOrEqualTo(1)));
        Assert.Equal("Expected n to be 1, but found null.", Message(() => n.Should().Be(1)));
    }
    [Fact]
    public void Nullable_approximately()
    {
        double? d = 1.5;
        d.Should().BeApproximately(1.5, 0.01);
        double? none = null;
        Assert.Equal("Expected none to approximate 1.5 +/- 0.01, but found null.", Message(() => none.Should().BeApproximately(1.5, 0.01)));
        double? far = 1.75;
        Assert.Equal("Expected far to approximate 1.5 +/- 0.125, but found 1.75 (off by 0.25).", Message(() => far.Should().BeApproximately(1.5, 0.125)));
    }

    [Fact]
    public void Decimal_BeApproximately_overflow_fails_with_VouchException()
    {
        var m = decimal.MaxValue;
        Assert.Equal("Expected m to approximate -1m +/- 1m, but found 79228162514264337593543950335m (difference is out of range).", Message(() => m.Should().BeApproximately(-1m, 1m)));
    }
    [Fact]
    public void Nullable_decimal_BeApproximately_overflow_fails_with_VouchException()
    {
        decimal? m = decimal.MaxValue;
        Assert.Throws<VouchException>(() => m.Should().BeApproximately(-1m, 1m));
    }
    [Fact] public void Equal_infinities_approximate() { double.PositiveInfinity.Should().BeApproximately(double.PositiveInfinity, 0); double? d = double.PositiveInfinity; d.Should().BeApproximately(double.PositiveInfinity, 0); }
    [Fact] public void Opposite_infinities_do_not_approximate() { double d = double.PositiveInfinity; Assert.Throws<VouchException>(() => d.Should().BeApproximately(double.NegativeInfinity, 1)); }
    [Fact] public void Float_BeApproximately_fails() { Assert.Throws<VouchException>(() => 1.75f.Should().BeApproximately(1.5f, 0.125f)); }
    [Fact] public void Float_BeApproximately_message() { var f = 1.75f; Assert.Equal("Expected f to approximate 1.5f +/- 0.125f, but found 1.75f (off by 0.25f).", Message(() => f.Should().BeApproximately(1.5f, 0.125f))); }

    [Fact]
    public void Double_NaN_fails_every_ordering_on_either_side()
    {
        var n = double.NaN;
        Assert.Throws<VouchException>(() => n.Should().BeLessThan(1.0));
        Assert.Throws<VouchException>(() => n.Should().BeGreaterThan(1.0));
        Assert.Throws<VouchException>(() => n.Should().BeLessThanOrEqualTo(1.0));
        Assert.Throws<VouchException>(() => n.Should().BeGreaterThanOrEqualTo(1.0));
        Assert.Throws<VouchException>(() => n.Should().BeInRange(0.0, 1.0));
        var one = 1.0;
        Assert.Throws<VouchException>(() => one.Should().BeLessThan(n));
        Assert.Throws<VouchException>(() => one.Should().BeGreaterThan(n));
        Assert.Throws<VouchException>(() => one.Should().BeLessThanOrEqualTo(n));
        Assert.Throws<VouchException>(() => one.Should().BeGreaterThanOrEqualTo(n));
        Assert.Throws<VouchException>(() => one.Should().BeInRange(n, 2.0));
        Assert.Throws<VouchException>(() => one.Should().BeInRange(0.0, n));
    }

    [Fact]
    public void Float_NaN_fails_every_ordering_on_either_side()
    {
        var n = float.NaN;
        Assert.Throws<VouchException>(() => n.Should().BeLessThan(1f));
        Assert.Throws<VouchException>(() => n.Should().BeGreaterThan(1f));
        Assert.Throws<VouchException>(() => n.Should().BeLessThanOrEqualTo(1f));
        Assert.Throws<VouchException>(() => n.Should().BeGreaterThanOrEqualTo(1f));
        Assert.Throws<VouchException>(() => n.Should().BeInRange(0f, 1f));
        var one = 1f;
        Assert.Throws<VouchException>(() => one.Should().BeLessThan(n));
        Assert.Throws<VouchException>(() => one.Should().BeGreaterThan(n));
        Assert.Throws<VouchException>(() => one.Should().BeLessThanOrEqualTo(n));
        Assert.Throws<VouchException>(() => one.Should().BeGreaterThanOrEqualTo(n));
        Assert.Throws<VouchException>(() => one.Should().BeInRange(n, 2f));
        Assert.Throws<VouchException>(() => one.Should().BeInRange(0f, n));
    }
}
