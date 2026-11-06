using Xunit;

namespace Vouch.Tests;

public class EnumAndTimeSpanAssertionsTests
{
    private static string Msg(Action act) => Assert.Throws<VouchException>(act).Message;

    [Fact] public void BeDefined_passes() { var s = Status.Closed; s.Should().BeDefined(); }
    [Fact] public void BeDefined_message() { var s = (Status)7; Assert.Equal("Expected s to be defined in Status, but found Status.7.", Msg(() => s.Should().BeDefined())); }
    [Fact] public void NotBeDefined_passes() { var s = (Status)7; s.Should().NotBeDefined(); }
    [Fact] public void NotBeDefined_message() { var s = Status.Closed; Assert.Equal("Expected s not to be defined in Status, but found Status.Closed.", Msg(() => s.Should().NotBeDefined())); }

    [Fact] public void HaveFlag_passes() { var p = Perm.Read | Perm.Write; p.Should().HaveFlag(Perm.Write); }
    [Fact] public void HaveFlag_message() { var p = Perm.Read; Assert.Equal("Expected p to have flag Perm.Write, but found Perm.Read.", Msg(() => p.Should().HaveFlag(Perm.Write))); }
    [Fact] public void NotHaveFlag_passes() { var p = Perm.Read; p.Should().NotHaveFlag(Perm.Write); }
    [Fact] public void NotHaveFlag_message() { var p = Perm.Read; Assert.Equal("Expected p not to have flag Perm.Read, but found Perm.Read.", Msg(() => p.Should().NotHaveFlag(Perm.Read))); }

    [Fact] public void HaveValue_passes() { var s = Status.Closed; s.Should().HaveValue(1); }
    [Fact] public void HaveValue_message() { var s = Status.Closed; Assert.Equal("Expected s to have value 0L, but found 1L.", Msg(() => s.Should().HaveValue(0))); }
    [Fact] public void HaveValue_negative_long_enum() { var s = Signed.Negative; s.Should().HaveValue(-5); }

    [Fact] public void HaveSameNameAs_passes() { var s = Status.Draft; s.Should().HaveSameNameAs(Mood.Draft); }
    [Fact] public void HaveSameNameAs_message() { var s = Status.Active; Assert.Equal("Expected s to have the same name as Mood.Draft, but found Status.Active.", Msg(() => s.Should().HaveSameNameAs(Mood.Draft))); }
    [Fact] public void HaveSameValueAs_passes() { var s = Status.Draft; s.Should().HaveSameValueAs(Mood.Draft); }
    [Fact] public void HaveSameValueAs_message() { var s = Status.Active; Assert.Equal("Expected s to have the same value as Mood.Active, but found Status.Active.", Msg(() => s.Should().HaveSameValueAs(Mood.Active))); }

    [Fact] public void HaveValue_ulong_enum_at_max() { var w = Wide.Huge; w.Should().HaveValue(ulong.MaxValue); }
    [Fact] public void HaveValue_ulong_enum_at_max_does_not_equal_minus_one() { var w = Wide.Huge; Assert.Throws<VouchException>(() => w.Should().HaveValue(-1)); }
    [Fact] public void HaveSameValueAs_ulong_max_and_long_minus_one_fails() { var w = Wide.Huge; var s = (Signed)(-1L); Assert.Throws<VouchException>(() => w.Should().HaveSameValueAs(s)); }
    [Fact] public void HaveSameNameAs_undefined_value_fails() { var s = (Status)7; var msg = Msg(() => s.Should().HaveSameNameAs(Mood.Draft)); Assert.Contains("has no name", msg); }
    [Fact] public void HaveSameNameAs_undefined_other_fails() { var s = Status.Draft; var msg = Msg(() => s.Should().HaveSameNameAs((Mood)99)); Assert.Contains("has no name", msg); }
    [Fact] public void HaveFlag_zero_member_always_passes() { var p = Perm.Read; p.Should().HaveFlag(Perm.None); var none = Perm.None; none.Should().HaveFlag(Perm.None); }
    [Fact] public void BeDefined_combined_flag_value_fails() { var p = Perm.Read | Perm.Write; Assert.Throws<VouchException>(() => p.Should().BeDefined()); }

    [Fact] public void BeCloseTo_extreme_values_fails_with_VouchException()
    {
        var t = TimeSpan.MaxValue;
        Assert.Throws<VouchException>(() => t.Should().BeCloseTo(TimeSpan.MinValue, TimeSpan.FromSeconds(1)));
        Assert.Throws<VouchException>(() => t.Should().BeCloseTo(TimeSpan.MinValue, TimeSpan.MaxValue));
    }

    [Fact] public void BeCloseTo_negative_precision_throws() { var t = TimeSpan.FromSeconds(1); Assert.Throws<ArgumentOutOfRangeException>(() => t.Should().BeCloseTo(t, TimeSpan.FromSeconds(-1))); }

    [Fact] public void BeCloseTo_passes() { var t = TimeSpan.FromSeconds(10); t.Should().BeCloseTo(TimeSpan.FromSeconds(11), TimeSpan.FromSeconds(1)); }
    [Fact]
    public void BeCloseTo_message()
    {
        var t = TimeSpan.FromSeconds(10);
        Assert.Equal("Expected t to be within 00:00:01 of 00:00:15, but found 00:00:10 (off by 00:00:05).",
            Msg(() => t.Should().BeCloseTo(TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(1))));
    }

    [Fact] public void BePositive_passes() { var t = TimeSpan.FromSeconds(1); t.Should().BePositive(); }
    [Fact] public void BePositive_message() { var t = TimeSpan.Zero; Assert.Equal("Expected t to be positive, but found 00:00:00.", Msg(() => t.Should().BePositive())); }
    [Fact] public void BeNegative_passes() { var t = TimeSpan.FromSeconds(-1); t.Should().BeNegative(); }
    [Fact] public void BeNegative_message() { var t = TimeSpan.FromSeconds(5); Assert.Equal("Expected t to be negative, but found 00:00:05.", Msg(() => t.Should().BeNegative())); }
}

public class TimeOnlyAssertionsTests
{
    private static readonly TimeOnly Ten = new(10, 0);
    private static readonly TimeOnly Eleven = new(11, 0);
    private static string Msg(Action act) => Assert.Throws<VouchException>(act).Message;

    [Fact] public void Be_passes() { var t = Ten; t.Should().Be(Ten); }
    [Fact] public void Be_message() { var t = Ten; Assert.Equal("Expected t to be 11:00:00.0000000, but found 10:00:00.0000000.", Msg(() => t.Should().Be(Eleven))); }
    [Fact] public void NotBe_passes() { var t = Ten; t.Should().NotBe(Eleven); }
    [Fact] public void NotBe_message() { var t = Ten; Assert.Equal("Expected t not to be 10:00:00.0000000, but it was.", Msg(() => t.Should().NotBe(Ten))); }
    [Fact] public void BeAfter_passes() { var t = Eleven; t.Should().BeAfter(Ten); }
    [Fact] public void BeAfter_message() { var t = Ten; Assert.Equal("Expected t to be after 11:00:00.0000000, but found 10:00:00.0000000.", Msg(() => t.Should().BeAfter(Eleven))); }
    [Fact] public void BeBefore_passes() { var t = Ten; t.Should().BeBefore(Eleven); }
    [Fact] public void BeBefore_message() { var t = Eleven; Assert.Equal("Expected t to be before 10:00:00.0000000, but found 11:00:00.0000000.", Msg(() => t.Should().BeBefore(Ten))); }
    [Fact] public void BeOnOrAfter_passes() { var t = Ten; t.Should().BeOnOrAfter(Ten); }
    [Fact] public void BeOnOrAfter_message() { var t = Ten; Assert.Equal("Expected t to be on or after 11:00:00.0000000, but found 10:00:00.0000000.", Msg(() => t.Should().BeOnOrAfter(Eleven))); }
    [Fact] public void BeOnOrBefore_passes() { var t = Ten; t.Should().BeOnOrBefore(Ten); }
    [Fact] public void BeOnOrBefore_message() { var t = Eleven; Assert.Equal("Expected t to be on or before 10:00:00.0000000, but found 11:00:00.0000000.", Msg(() => t.Should().BeOnOrBefore(Ten))); }

    [Fact] public void Nullable_Be_passes() { TimeOnly? t = Ten; t.Should().Be(Ten); }
    [Fact] public void Nullable_Be_message() { TimeOnly? t = null; Assert.Equal("Expected t to be 10:00:00.0000000, but found null.", Msg(() => t.Should().Be(Ten))); }
    [Fact] public void Nullable_NotBe_passes() { TimeOnly? t = Ten; t.Should().NotBe(Eleven); }
    [Fact] public void Nullable_NotBe_message() { TimeOnly? t = Ten; Assert.Equal("Expected t not to be 10:00:00.0000000, but it was.", Msg(() => t.Should().NotBe(Ten))); }
    [Fact] public void Nullable_BeAfter_passes() { TimeOnly? t = Eleven; t.Should().BeAfter(Ten); }
    [Fact] public void Nullable_BeAfter_message() { TimeOnly? t = null; Assert.Equal("Expected t to be after 10:00:00.0000000, but found null.", Msg(() => t.Should().BeAfter(Ten))); }
    [Fact] public void Nullable_BeBefore_passes() { TimeOnly? t = Ten; t.Should().BeBefore(Eleven); }
    [Fact] public void Nullable_BeBefore_message() { TimeOnly? t = Eleven; Assert.Equal("Expected t to be before 10:00:00.0000000, but found 11:00:00.0000000.", Msg(() => t.Should().BeBefore(Ten))); }
    [Fact] public void Nullable_BeOnOrAfter_passes() { TimeOnly? t = Ten; t.Should().BeOnOrAfter(Ten); }
    [Fact] public void Nullable_BeOnOrAfter_message() { TimeOnly? t = Ten; Assert.Equal("Expected t to be on or after 11:00:00.0000000, but found 10:00:00.0000000.", Msg(() => t.Should().BeOnOrAfter(Eleven))); }
    [Fact] public void Nullable_BeOnOrBefore_passes() { TimeOnly? t = Ten; t.Should().BeOnOrBefore(Ten); }
    [Fact] public void Nullable_BeOnOrBefore_message() { TimeOnly? t = Eleven; Assert.Equal("Expected t to be on or before 10:00:00.0000000, but found 11:00:00.0000000.", Msg(() => t.Should().BeOnOrBefore(Ten))); }
    [Fact] public void Nullable_BeNull_passes() { TimeOnly? t = null; t.Should().BeNull(); }
    [Fact] public void Nullable_BeNull_message() { TimeOnly? t = Ten; Assert.Equal("Expected t to be null, but found 10:00:00.0000000.", Msg(() => t.Should().BeNull())); }
    [Fact] public void Nullable_NotBeNull_returns_value() { TimeOnly? t = Ten; Assert.Equal(Ten, t.Should().NotBeNull().Which); }
    [Fact] public void Nullable_NotBeNull_message() { TimeOnly? t = null; Assert.Equal("Expected t not to be null, but it was.", Msg(() => t.Should().NotBeNull())); }
}

public class UnsignedAndCharAssertionsTests
{
    private static string Msg(Action act) => Assert.Throws<VouchException>(act).Message;

    [Fact] public void Char_Be_passes() { var c = 'a'; c.Should().Be('a'); }
    [Fact] public void Char_Be_message() { var c = 'a'; Assert.Equal("Expected c to be 'b', but found 'a'.", Msg(() => c.Should().Be('b'))); }
    [Fact] public void Sbyte_Be_passes() { sbyte n = -3; n.Should().Be(-3); }
    [Fact] public void Sbyte_Be_message() { sbyte n = -3; Assert.Equal("Expected n to be 5, but found -3.", Msg(() => n.Should().Be(5))); }
    [Fact] public void Ushort_BeGreaterThan_passes() { ushort n = 4; n.Should().BeGreaterThan(3); }
    [Fact] public void Ushort_BeGreaterThan_message() { ushort n = 3; Assert.Equal("Expected n to be greater than 5, but found 3.", Msg(() => n.Should().BeGreaterThan(5))); }
    [Fact] public void Uint_Be_passes() { uint n = 3; n.Should().Be(3u); }
    [Fact] public void Uint_Be_message() { uint n = 3; Assert.Equal("Expected n to be 5, but found 3.", Msg(() => n.Should().Be(5u))); }
    [Fact] public void Ulong_BePositive_passes() { ulong n = 3; n.Should().BePositive(); }
    [Fact] public void Ulong_Be_message() { ulong n = 3; Assert.Equal("Expected n to be 5, but found 3.", Msg(() => n.Should().Be(5ul))); }

    [Fact] public void Nullable_char_Be_message() { char? c = null; Assert.Equal("Expected c to be 'a', but found null.", Msg(() => c.Should().Be('a'))); }
    [Fact] public void Nullable_sbyte_Be_passes() { sbyte? n = -3; n.Should().Be(-3); }
    [Fact] public void Nullable_ushort_Be_message() { ushort? n = 3; Assert.Equal("Expected n to be 5, but found 3.", Msg(() => n.Should().Be(5))); }
    [Fact] public void Nullable_uint_NotBeNull_returns_value() { uint? n = 3; Assert.Equal(3u, n.Should().NotBeNull().Which); }
    [Fact] public void Nullable_ulong_BeNull_message() { ulong? n = 3; Assert.Equal("Expected n to be null, but found 3.", Msg(() => n.Should().BeNull())); }
}
