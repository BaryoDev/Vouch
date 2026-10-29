using Xunit;

namespace Vouch.Tests;


public readonly record struct Point(int X, int Y);

public class ValueAssertionsTests
{
    [Fact] public void Int_Be_passes() => 5.Should().Be(5);

    [Fact]
    public void Int_Be_fails_with_exact_message()
    {
        var count = 3;
        var ex = Assert.Throws<VouchException>(() => count.Should().Be(5));
        Assert.Equal("Expected count to be 5, but found 3.", ex.Message);
    }

    [Fact] public void Enum_Be_passes() { var status = Status.Active; status.Should().Be(Status.Active); }

    [Fact]
    public void Enum_Be_message()
    {
        var status = Status.Draft;
        Assert.Equal("Expected status to be Status.Active, but found Status.Draft.",
            Assert.Throws<VouchException>(() => status.Should().Be(Status.Active)).Message);
    }

    [Fact] public void Guid_goes_to_ValueAssertions() => Assert.IsType<ValueAssertions<Guid>>(Guid.Empty.Should());

    [Fact] public void Guid_Be_passes() { var id = Guid.NewGuid(); id.Should().Be(id); }

    [Fact]
    public void Tuple_Be_passes_and_fails()
    {
        var pair = (1, "a");
        pair.Should().Be((1, "a"));
        Assert.Throws<VouchException>(() => pair.Should().Be((2, "a")));
    }

    [Fact]
    public void User_struct_uses_its_own_equality()
    {
        var point = new Point(1, 2);
        point.Should().Be(new Point(1, 2));
        var ex = Assert.Throws<VouchException>(() => point.Should().Be(new Point(3, 4)));
        Assert.Equal("Expected point to be Point { X = 3, Y = 4 }, but found Point { X = 1, Y = 2 }.", ex.Message);
    }

    [Fact] public void NotBe_passes() => 1.Should().NotBe(2);

    [Fact]
    public void NotBe_fails_with_exact_message()
    {
        var count = 2;
        var ex = Assert.Throws<VouchException>(() => count.Should().NotBe(2));
        Assert.Equal("Expected count not to be 2, but it was.", ex.Message);
    }

    [Fact] public void BeOneOf_passes() => 2.Should().BeOneOf(1, 2, 3);

    [Fact]
    public void BeOneOf_fails_with_exact_message()
    {
        var n = 9;
        var ex = Assert.Throws<VouchException>(() => n.Should().BeOneOf(1, 2));
        Assert.Equal("Expected n to be one of [1, 2], but found 9.", ex.Message);
    }

    [Fact]
    public void Missing_expression_reads_as_value() =>
        Assert.Equal("Expected value to be 2, but found 1.",
            Assert.Throws<VouchException>(() => new ValueAssertions<int>(1, null).Be(2)).Message);

    [Fact] public void Nullable_Be_passes() { int? n = 5; n.Should().Be(5); }

    [Fact]
    public void Nullable_Be_fails_on_null_with_exact_message()
    {
        int? n = null;
        var ex = Assert.Throws<VouchException>(() => n.Should().Be(5));
        Assert.Equal("Expected n to be 5, but found null.", ex.Message);
    }

    [Fact] public void Nullable_Be_fails_on_other_value() { int? n = 4; Assert.Throws<VouchException>(() => n.Should().Be(5)); }

    [Fact] public void Nullable_NotBe_passes_on_null_and_other() { int? n = null; n.Should().NotBe(5); int? m = 4; m.Should().NotBe(5); }

    [Fact]
    public void Nullable_NotBe_fails_with_exact_message()
    {
        int? n = 5;
        Assert.Equal("Expected n not to be 5, but it was.", Assert.Throws<VouchException>(() => n.Should().NotBe(5)).Message);
    }

    [Fact]
    public void Nullable_BeOneOf_passes_and_fails()
    {
        int? n = 2;
        n.Should().BeOneOf(1, 2);
        int? m = null;
        Assert.Equal("Expected m to be one of [1, 2], but found null.",
            Assert.Throws<VouchException>(() => m.Should().BeOneOf(1, 2)).Message);
    }

    [Fact] public void Nullable_BeNull_passes() { int? n = null; n.Should().BeNull(); }

    [Fact]
    public void Nullable_BeNull_fails_with_exact_message()
    {
        int? n = 5;
        Assert.Equal("Expected n to be null, but found 5.", Assert.Throws<VouchException>(() => n.Should().BeNull()).Message);
    }

    [Fact] public void Nullable_NotBeNull_Which_is_the_value() { int? n = 5; Assert.Equal(5, n.Should().NotBeNull().Which); }

    [Fact]
    public void Nullable_NotBeNull_fails_with_exact_message()
    {
        int? n = null;
        Assert.Equal("Expected n not to be null, but it was.", Assert.Throws<VouchException>(() => n.Should().NotBeNull()).Message);
    }

    private static string Msg(Action act) => Assert.Throws<VouchException>(act).Message;

    [Fact] public void TimeSpan_BeGreaterThan_passes_and_fails() { var t = TimeSpan.FromMinutes(2); t.Should().BeGreaterThan(TimeSpan.Zero); Assert.Throws<VouchException>(() => t.Should().BeGreaterThan(t)); }
    [Fact] public void TimeSpan_BeGreaterThan_message() { var t = TimeSpan.FromMinutes(2); Assert.Equal("Expected t to be greater than 00:03:00, but found 00:02:00.", Msg(() => t.Should().BeGreaterThan(TimeSpan.FromMinutes(3)))); }
    [Fact] public void TimeSpan_BeGreaterThanOrEqualTo_passes_and_fails() { var t = TimeSpan.FromMinutes(2); t.Should().BeGreaterThanOrEqualTo(t); Assert.Throws<VouchException>(() => t.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMinutes(3))); }
    [Fact] public void TimeSpan_BeGreaterThanOrEqualTo_message() { var t = TimeSpan.FromMinutes(2); Assert.Equal("Expected t to be greater than or equal to 00:03:00, but found 00:02:00.", Msg(() => t.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMinutes(3)))); }
    [Fact] public void TimeSpan_BeLessThan_passes_and_fails() { var t = TimeSpan.FromMinutes(2); t.Should().BeLessThan(TimeSpan.FromMinutes(3)); Assert.Throws<VouchException>(() => t.Should().BeLessThan(t)); }
    [Fact] public void TimeSpan_BeLessThan_message() { var t = TimeSpan.FromMinutes(2); Assert.Equal("Expected t to be less than 00:01:00, but found 00:02:00.", Msg(() => t.Should().BeLessThan(TimeSpan.FromMinutes(1)))); }
    [Fact] public void TimeSpan_BeLessThanOrEqualTo_passes_and_fails() { var t = TimeSpan.FromMinutes(2); t.Should().BeLessThanOrEqualTo(t); Assert.Throws<VouchException>(() => t.Should().BeLessThanOrEqualTo(TimeSpan.FromMinutes(1))); }
    [Fact] public void TimeSpan_BeLessThanOrEqualTo_message() { var t = TimeSpan.FromMinutes(2); Assert.Equal("Expected t to be less than or equal to 00:01:00, but found 00:02:00.", Msg(() => t.Should().BeLessThanOrEqualTo(TimeSpan.FromMinutes(1)))); }
    [Fact] public void Comparisons_chain() { TimeSpan.FromSeconds(5).Should().BeGreaterThan(TimeSpan.Zero).And.BeLessThan(TimeSpan.FromMinutes(1)); }

    [Fact]
    public void Half_NaN_fails_every_ordering_on_either_side()
    {
        var n = Half.NaN;
        var one = Half.One;
        Assert.Throws<VouchException>(() => n.Should().BeLessThan(one));
        Assert.Throws<VouchException>(() => n.Should().BeLessThanOrEqualTo(one));
        Assert.Throws<VouchException>(() => n.Should().BeGreaterThan(one));
        Assert.Throws<VouchException>(() => n.Should().BeGreaterThanOrEqualTo(one));
        Assert.Throws<VouchException>(() => one.Should().BeLessThan(n));
        Assert.Throws<VouchException>(() => one.Should().BeLessThanOrEqualTo(n));
        Assert.Throws<VouchException>(() => one.Should().BeGreaterThan(n));
        Assert.Throws<VouchException>(() => one.Should().BeGreaterThanOrEqualTo(n));
        Assert.Throws<VouchException>(() => n.Should().BeLessThanOrEqualTo(n));
        Assert.Throws<VouchException>(() => n.Should().BeGreaterThanOrEqualTo(n));
    }

    [Fact]
    public void Half_numbers_still_compare()
    {
        var one = Half.One;
        Half.Zero.Should().BeLessThan(one);
        Half.Zero.Should().BeLessThanOrEqualTo(Half.Zero);
        one.Should().BeGreaterThan(Half.Zero);
        one.Should().BeGreaterThanOrEqualTo(one);
        Assert.Throws<VouchException>(() => one.Should().BeLessThan(Half.Zero));
    }

    [Fact] public void Guid_NotBeEmpty_passes_on_a_real_guid() { var g = Guid.NewGuid(); g.Should().NotBeEmpty(); }
    [Fact] public void Guid_NotBeEmpty_fails_on_empty() { var g = Guid.Empty; Assert.Throws<VouchException>(() => g.Should().NotBeEmpty()); }
    [Fact] public void Guid_NotBeEmpty_message() { var g = Guid.Empty; Assert.Equal("Expected g not to be empty, but it was.", Msg(() => g.Should().NotBeEmpty())); }

    [Fact]
    public void Nullable_Be_accepts_a_nullable_expected_value()
    {
        Guid? id = Guid.NewGuid();
        Guid? same = id;
        Guid? none = null;
        id.Should().Be(same);
        none.Should().Be(none);
        Assert.Throws<VouchException>(() => id.Should().Be(none));
        Assert.Throws<VouchException>(() => none.Should().Be(id));
    }
}
