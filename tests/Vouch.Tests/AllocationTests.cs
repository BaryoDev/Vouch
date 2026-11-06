using Xunit;

namespace Vouch.Tests;

// Measured alone: other tests running at the same time can add to this thread's byte count.
[CollectionDefinition("Allocation", DisableParallelization = true)]
public sealed class AllocationCollection;

[Collection("Allocation")]
public class AllocationTests
{
    private const int Calls = 1000;

    private static void AssertNoAllocation(Action call)
    {
        call();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < Calls; i++) call();
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    private sealed class NoopFormatter : Vouch.Sdk.IValueFormatter
    {
        public bool CanHandle(object? value) => true;
        public string Format(object? value, Vouch.Sdk.FormattingContext context) => "x";
    }

    [Fact]
    public void Passing_assertion_with_a_registered_formatter()
    {
        var formatter = new NoopFormatter();
        Vouch.Sdk.VouchConfiguration.Formatters.Add(formatter);
        try { var n = 5; AssertNoAllocation(() => n.Should().Be(5)); }
        finally { Vouch.Sdk.VouchConfiguration.Formatters.Remove(formatter); }
    }

    [Fact] public void Int_Be() { var n = 5; AssertNoAllocation(() => n.Should().Be(5)); }

    [Fact] public void ExecutionTime_BeLessThan() { var t = new ExecutionTime(TimeSpan.FromSeconds(1), "act"); var limit = TimeSpan.FromSeconds(2); AssertNoAllocation(() => t.Should().BeLessThan(limit)); }

    [Fact] public void ExecutionTime_BeLessThanOrEqualTo() { var t = new ExecutionTime(TimeSpan.FromSeconds(1), "act"); var limit = TimeSpan.FromSeconds(1); AssertNoAllocation(() => t.Should().BeLessThanOrEqualTo(limit)); }

    [Fact] public void ExecutionTime_BeGreaterThan() { var t = new ExecutionTime(TimeSpan.FromSeconds(2), "act"); var limit = TimeSpan.FromSeconds(1); AssertNoAllocation(() => t.Should().BeGreaterThan(limit)); }

    [Fact] public void ExecutionTime_BeGreaterThanOrEqualTo() { var t = new ExecutionTime(TimeSpan.FromSeconds(2), "act"); var limit = TimeSpan.FromSeconds(2); AssertNoAllocation(() => t.Should().BeGreaterThanOrEqualTo(limit)); }

    [Fact] public void ExecutionTime_BeCloseTo() { var t = new ExecutionTime(TimeSpan.FromSeconds(2), "act"); var expected = TimeSpan.FromSeconds(2); var precision = TimeSpan.FromMilliseconds(10); AssertNoAllocation(() => t.Should().BeCloseTo(expected, precision)); }

    [Fact] public void ExecutionTime_of_a_cached_action() { Action act = static () => { }; var limit = TimeSpan.FromMinutes(1); AssertNoAllocation(() => act.ExecutionTime().Should().BeLessThan(limit)); }

    [Fact] public void Action_NotThrowAfter() { Action act = static () => { }; var wait = TimeSpan.FromSeconds(1); var poll = TimeSpan.FromMilliseconds(10); AssertNoAllocation(() => act.Should().NotThrowAfter(wait, poll)); }

    [Fact] public void Stream_BeReadable() { using var s = new MemoryStream(new byte[3]); AssertNoAllocation(() => s.Should().BeReadable()); }

    [Fact] public void Stream_NotBeReadable() { var s = new MemoryStream(new byte[3]); s.Dispose(); AssertNoAllocation(() => s.Should().NotBeReadable()); }

    [Fact] public void Stream_BeWritable() { using var s = new MemoryStream(new byte[3]); AssertNoAllocation(() => s.Should().BeWritable()); }

    [Fact] public void Stream_NotBeWritable() { using var s = new MemoryStream(new byte[3], false); AssertNoAllocation(() => s.Should().NotBeWritable()); }

    [Fact] public void Stream_BeSeekable() { using var s = new MemoryStream(new byte[3]); AssertNoAllocation(() => s.Should().BeSeekable()); }

    [Fact] public void Stream_NotBeSeekable() { var s = new MemoryStream(new byte[3]); s.Dispose(); AssertNoAllocation(() => s.Should().NotBeSeekable()); }

    [Fact] public void Stream_HaveLength() { using var s = new MemoryStream(new byte[3]); AssertNoAllocation(() => s.Should().HaveLength(3)); }

    [Fact] public void Stream_NotHaveLength() { using var s = new MemoryStream(new byte[3]); AssertNoAllocation(() => s.Should().NotHaveLength(4)); }

    [Fact] public void Stream_HavePosition() { using var s = new MemoryStream(new byte[3]); AssertNoAllocation(() => s.Should().HavePosition(0)); }

    [Fact] public void Stream_NotHavePosition() { using var s = new MemoryStream(new byte[3]); AssertNoAllocation(() => s.Should().NotHavePosition(1)); }

    [Fact] public void Passing_assertion_inside_scope() { var n = 5; using var scope = new AssertionScope(); AssertNoAllocation(() => n.Should().Be(5)); }

    [Fact] public void Guid_Be() { var id = Guid.NewGuid(); AssertNoAllocation(() => id.Should().Be(id)); }

    [Fact] public void Int_BeCloseTo() { var n = 5; AssertNoAllocation(() => n.Should().BeCloseTo(7, 2)); }

    [Fact] public void Int_NotBeCloseTo_across_the_full_range() { var n = int.MinValue; AssertNoAllocation(() => n.Should().NotBeCloseTo(int.MaxValue, 1)); }

    [Fact] public void Double_NotBeCloseTo() { var d = 5.0; AssertNoAllocation(() => d.Should().NotBeCloseTo(9.0, 2.0)); }

    [Fact] public void Double_NotBeCloseTo_across_the_full_range() { var d = double.MinValue; AssertNoAllocation(() => d.Should().NotBeCloseTo(double.MaxValue, 1.0)); }

    [Fact] public void Int_NotBeCloseTo() { var n = 5; AssertNoAllocation(() => n.Should().NotBeCloseTo(9, 2)); }

    [Fact] public void Int_BeNegative() { var n = -5; AssertNoAllocation(() => n.Should().BeNegative()); }

    [Fact] public void Double_BeNaN() { var n = double.NaN; AssertNoAllocation(() => n.Should().BeNaN()); }

    [Fact] public void Double_NotBeNaN() { var n = 1.5; AssertNoAllocation(() => n.Should().NotBeNaN()); }

    [Fact] public void Double_BeFinite() { var n = 1.5; AssertNoAllocation(() => n.Should().BeFinite()); }

    [Fact] public void Double_NotBeFinite() { var n = double.PositiveInfinity; AssertNoAllocation(() => n.Should().NotBeFinite()); }

    [Fact] public void Float_BeNaN() { var n = float.NaN; AssertNoAllocation(() => n.Should().BeNaN()); }

    [Fact] public void Float_BeFinite() { var n = 1.5f; AssertNoAllocation(() => n.Should().BeFinite()); }

    [Fact] public void Half_BeNaN() { var n = Half.NaN; AssertNoAllocation(() => n.Should().BeNaN()); }

    [Fact] public void Half_NotBeNaN() { var n = (Half)1; AssertNoAllocation(() => n.Should().NotBeNaN()); }

    [Fact] public void Half_BeFinite() { var n = (Half)1; AssertNoAllocation(() => n.Should().BeFinite()); }

    [Fact] public void Half_NotBeFinite() { var n = Half.PositiveInfinity; AssertNoAllocation(() => n.Should().NotBeFinite()); }

    [Fact] public void NullableInt_HaveValue() { int? n = 5; AssertNoAllocation(() => n.Should().HaveValue()); }

    [Fact] public void NullableInt_NotHaveValue() { int? n = null; AssertNoAllocation(() => n.Should().NotHaveValue()); }

    [Fact] public void NullableInt_BeCloseTo() { int? n = 5; AssertNoAllocation(() => n.Should().BeCloseTo(7, 2)); }

    [Fact] public void NullableInt_NotBeCloseTo() { int? n = 5; AssertNoAllocation(() => n.Should().NotBeCloseTo(9, 2)); }

    [Fact] public void NullableInt_BeNegative() { int? n = -5; AssertNoAllocation(() => n.Should().BeNegative()); }

    [Fact] public void NullableTimeSpan_HaveValue() { TimeSpan? t = TimeSpan.Zero; AssertNoAllocation(() => t.Should().HaveValue()); }

    [Fact] public void NullableTimeSpan_NotHaveValue() { TimeSpan? t = null; AssertNoAllocation(() => t.Should().NotHaveValue()); }

    [Fact]
    public void DateTime_new_assertions()
    {
        var at = new DateTime(2026, 10, 19, 8, 30, 15, DateTimeKind.Utc);
        var other = at.AddHours(1);
        AssertNoAllocation(() => at.Should().BeSameDateAs(other));
        AssertNoAllocation(() => at.Should().NotBeSameDateAs(other.AddDays(1)));
        AssertNoAllocation(() => at.Should().HaveYear(2026));
        AssertNoAllocation(() => at.Should().NotHaveYear(2025));
        AssertNoAllocation(() => at.Should().HaveMonth(10));
        AssertNoAllocation(() => at.Should().NotHaveMonth(9));
        AssertNoAllocation(() => at.Should().HaveDay(19));
        AssertNoAllocation(() => at.Should().NotHaveDay(1));
        AssertNoAllocation(() => at.Should().HaveHour(8));
        AssertNoAllocation(() => at.Should().NotHaveHour(1));
        AssertNoAllocation(() => at.Should().HaveMinute(30));
        AssertNoAllocation(() => at.Should().NotHaveMinute(1));
        AssertNoAllocation(() => at.Should().HaveSecond(15));
        AssertNoAllocation(() => at.Should().NotHaveSecond(1));
        AssertNoAllocation(() => at.Should().BeWithin(TimeSpan.FromMinutes(5)).Before(at.AddMinutes(3)));
        AssertNoAllocation(() => other.Should().BeWithin(TimeSpan.FromHours(2)).After(at));
    }

    [Fact]
    public void NullableDateTime_new_assertions()
    {
        DateTime? at = new DateTime(2026, 10, 19, 8, 30, 15, DateTimeKind.Utc);
        DateTime? none = null;
        var other = at.Value.AddHours(1);
        AssertNoAllocation(() => at.Should().BeSameDateAs(other));
        AssertNoAllocation(() => none.Should().NotBeSameDateAs(other));
        AssertNoAllocation(() => at.Should().NotBeSameDateAs(other.AddDays(1)));
        AssertNoAllocation(() => at.Should().HaveYear(2026));
        AssertNoAllocation(() => none.Should().NotHaveYear(2026));
        AssertNoAllocation(() => at.Should().HaveSecond(15));
        AssertNoAllocation(() => at.Should().BeWithin(TimeSpan.FromMinutes(5)).Before(at.Value.AddMinutes(3)));
    }

    [Fact]
    public void DateTimeOffset_new_assertions()
    {
        var at = new DateTimeOffset(2026, 10, 19, 8, 30, 15, TimeSpan.Zero);
        var other = at.AddHours(1);
        AssertNoAllocation(() => at.Should().BeSameDateAs(other));
        AssertNoAllocation(() => at.Should().NotBeSameDateAs(other.AddDays(1)));
        AssertNoAllocation(() => at.Should().HaveYear(2026));
        AssertNoAllocation(() => at.Should().NotHaveMonth(9));
        AssertNoAllocation(() => at.Should().HaveDay(19));
        AssertNoAllocation(() => at.Should().NotHaveHour(1));
        AssertNoAllocation(() => at.Should().HaveMinute(30));
        AssertNoAllocation(() => at.Should().NotHaveSecond(1));
        AssertNoAllocation(() => at.Should().BeWithin(TimeSpan.FromMinutes(5)).Before(at.AddMinutes(3)));
        AssertNoAllocation(() => other.Should().BeWithin(TimeSpan.FromHours(2)).After(at));
    }

    [Fact]
    public void NullableDateTimeOffset_new_assertions()
    {
        DateTimeOffset? at = new DateTimeOffset(2026, 10, 19, 8, 30, 15, TimeSpan.Zero);
        DateTimeOffset? none = null;
        var other = at.Value.AddHours(1);
        AssertNoAllocation(() => at.Should().BeSameDateAs(other));
        AssertNoAllocation(() => none.Should().NotBeSameDateAs(other));
        AssertNoAllocation(() => at.Should().HaveHour(8));
        AssertNoAllocation(() => none.Should().NotHaveHour(8));
        AssertNoAllocation(() => at.Should().BeWithin(TimeSpan.FromMinutes(5)).Before(at.Value.AddMinutes(3)));
    }

    [Fact]
    public void DateOnly_new_assertions()
    {
        var d = new DateOnly(2026, 10, 19);
        DateOnly? n = d;
        AssertNoAllocation(() => d.Should().HaveYear(2026));
        AssertNoAllocation(() => d.Should().NotHaveMonth(9));
        AssertNoAllocation(() => d.Should().HaveDay(19));
        AssertNoAllocation(() => n.Should().HaveYear(2026));
        AssertNoAllocation(() => n.Should().NotHaveDay(1));
    }

    [Fact] public void Char_Be() { var c = 'a'; AssertNoAllocation(() => c.Should().Be('a')); }

    [Fact] public void TimeOnly_BeAfter() { var t = new TimeOnly(11, 0); var earlier = new TimeOnly(10, 0); AssertNoAllocation(() => t.Should().BeAfter(earlier)); }

    [Fact] public void Enum_HaveValue() { var s = Signed.Negative; var w = Wide.Huge; AssertNoAllocation(() => s.Should().HaveValue(-5)); AssertNoAllocation(() => w.Should().HaveValue(ulong.MaxValue)); }

    [Fact] public void Enum_HaveSameValueAs() { var s = Status.Draft; AssertNoAllocation(() => s.Should().HaveSameValueAs(Mood.Draft)); }

    [Fact] public void Enum_HaveSameNameAs() { var s = Status.Draft; AssertNoAllocation(() => s.Should().HaveSameNameAs(Mood.Draft)); }

    [Fact] public void Enum_BeDefined() { var s = Status.Closed; AssertNoAllocation(() => s.Should().BeDefined()); }

    [Fact] public void Enum_HaveFlag() { var p = Perm.Read | Perm.Write; AssertNoAllocation(() => p.Should().HaveFlag(Perm.Read)); }

    [Fact] public void TimeSpan_BePositive() { var t = TimeSpan.FromSeconds(1); AssertNoAllocation(() => t.Should().BePositive()); }

    [Fact] public void Enum_Be() { var s = Status.Active; AssertNoAllocation(() => s.Should().Be(Status.Active)); }

    [Fact] public void Bool_BeTrue() { var b = true; AssertNoAllocation(() => b.Should().BeTrue()); }

    [Fact] public void Nullable_int_Be() { int? n = 5; AssertNoAllocation(() => n.Should().Be(5)); }

    [Fact] public void Decimal_Be() { var m = 100m; AssertNoAllocation(() => m.Should().Be(100m)); }

    [Fact] public void Custom_extension_assertion() { object n = 4; AssertNoAllocation(() => n.Should().BeEven()); }

    [Fact] public void Custom_user_type_assertion() { var m = new Money(1m, "PHP"); AssertNoAllocation(() => m.Should().BeIn("PHP")); }

    [Fact] public void Int_BeGreaterThan() { var n = 5; AssertNoAllocation(() => n.Should().BeGreaterThan(3)); }

    [Fact] public void DateTime_BeAfter() { var now = DateTime.UtcNow; var past = now.AddDays(-1); AssertNoAllocation(() => now.Should().BeAfter(past)); }

    [Fact] public void String_Be() { var s = "abc"; AssertNoAllocation(() => s.Should().Be("abc")); }

    [Fact] public void String_StartWith() { var s = "abc"; AssertNoAllocation(() => s.Should().StartWith("ab")); }

    [Fact] public void String_Contain() { var s = "abc"; AssertNoAllocation(() => s.Should().Contain("b")); }

    [Fact] public void String_Contain_occurrence() { var s = "banana"; AssertNoAllocation(() => s.Should().Contain("an", Exactly.Twice())); }

    [Fact] public void String_StartWithEquivalentOf() { var s = "abc"; AssertNoAllocation(() => s.Should().StartWithEquivalentOf("AB")); }

    [Fact] public void String_NotStartWithEquivalentOf() { var s = "abc"; AssertNoAllocation(() => s.Should().NotStartWithEquivalentOf("X")); }

    [Fact] public void String_EndWithEquivalentOf() { var s = "abc"; AssertNoAllocation(() => s.Should().EndWithEquivalentOf("BC")); }

    [Fact] public void String_NotEndWithEquivalentOf() { var s = "abc"; AssertNoAllocation(() => s.Should().NotEndWithEquivalentOf("X")); }

    [Fact] public void String_MatchEquivalentOf() { var s = "abc"; AssertNoAllocation(() => s.Should().MatchEquivalentOf("A*C")); }

    [Fact] public void String_NotMatchEquivalentOf() { var s = "abc"; AssertNoAllocation(() => s.Should().NotMatchEquivalentOf("X*")); }

    [Fact] public void String_NotMatch() { var s = "abc"; AssertNoAllocation(() => s.Should().NotMatch("x*")); }

    [Fact] public void String_BeUpperCased() { var s = "ABC"; AssertNoAllocation(() => s.Should().BeUpperCased()); }

    [Fact] public void String_NotBeUpperCased() { var s = "Abc"; AssertNoAllocation(() => s.Should().NotBeUpperCased()); }

    [Fact] public void String_BeLowerCased() { var s = "abc"; AssertNoAllocation(() => s.Should().BeLowerCased()); }

    [Fact] public void String_NotBeLowerCased() { var s = "aBc"; AssertNoAllocation(() => s.Should().NotBeLowerCased()); }

    [Fact] public void String_BeNullOrWhiteSpace() { var s = "  "; AssertNoAllocation(() => s.Should().BeNullOrWhiteSpace()); }

    [Fact] public void String_MatchRegex_options() { var s = "abc"; AssertNoAllocation(() => s.Should().MatchRegex("^ABC$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)); }

    [Fact] public void String_MatchRegex_regex() { var s = "abc"; var r = new System.Text.RegularExpressions.Regex("^a", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(1)); AssertNoAllocation(() => s.Should().MatchRegex(r)); }

    [Fact] public void String_NotMatchRegex() { var s = "abc"; AssertNoAllocation(() => s.Should().NotMatchRegex("^b")); }

    [Fact] public void String_NotMatchRegex_options() { var s = "abc"; AssertNoAllocation(() => s.Should().NotMatchRegex("^B", System.Text.RegularExpressions.RegexOptions.None)); }

    [Fact] public void String_NotMatchRegex_regex() { var s = "abc"; var r = new System.Text.RegularExpressions.Regex("^b", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(1)); AssertNoAllocation(() => s.Should().NotMatchRegex(r)); }

    [Fact]
    public void String_MatchRegex_regex_without_timeout()
    {
        var s = "abc";
        var r = new System.Text.RegularExpressions.Regex("a");
        s.Should().MatchRegex(r);
        AssertNoAllocation(() => s.Should().MatchRegex(r));
    }

    [Fact] public void String_BeLowerCased_surrogate_pair() { var s = "\U00010428"; AssertNoAllocation(() => s.Should().BeLowerCased()); }

    [Fact] public void String_NotEndWith() { var s = "abc"; AssertNoAllocation(() => s.Should().NotEndWith("x")); }

    [Fact] public void String_BeParsableInto() { var s = "42"; AssertNoAllocation(() => s.Should().BeParsableInto<int>()); }

    [Fact] public void String_NotBeNullOrEmpty() { var s = "abc"; AssertNoAllocation(() => s.Should().NotBeNullOrEmpty()); }

    [Fact] public void Int_BeEquivalentTo() { var n = 5; AssertNoAllocation(() => n.Should().BeEquivalentTo(5)); }

    [Fact] public void Guid_BeEquivalentTo() { var id = Guid.NewGuid(); AssertNoAllocation(() => id.Should().BeEquivalentTo(id)); }

    [Fact] public void Object_string_BeEquivalentTo() { object s = new string('a', 3); var t = new string('a', 3); AssertNoAllocation(() => s.Should().BeEquivalentTo(t)); }

    [Fact]
    public void Small_record_BeEquivalentTo_allocation_is_bounded()
    {
        var a = new Row("A", 1);
        var b = new Row("A", 1);
        a.Should().BeEquivalentTo(b);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < Calls; i++) a.Should().BeEquivalentTo(b);
        Assert.True((GC.GetAllocatedBytesForCurrentThread() - before) / Calls <= 512);
    }

    [Fact]
    public void Default_options_BeEquivalentTo_takes_the_zero_option_path()
    {
        Assert.False(EquivalencyOptions.Default.HasPathRules);
        var a = new Row("A", 1);
        var b = new Row("A", 1);
        a.Should().BeEquivalentTo(b);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < Calls; i++) a.Should().BeEquivalentTo(b);
        Assert.True((GC.GetAllocatedBytesForCurrentThread() - before) / Calls <= 512);
    }

    private static long BytesForContainKey(int size)
    {
        var d = Enumerable.Range(0, size).ToDictionary(i => i, i => i);
        d.Should().ContainKey(size - 1);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < Calls; i++) d.Should().ContainKey(size - 1);
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [Fact]
    public void Dictionary_ContainKey_allocation_does_not_grow_with_size()
    {
        var small = BytesForContainKey(10);
        var large = BytesForContainKey(10_000);
        Assert.Equal(small, large);
        Assert.True(large / Calls <= 128);
    }

    [Fact]
    public void Passing_AllSatisfy_has_no_per_item_allocation()
    {
        // One call over many items: a per-item allocation would cost megabytes here,
        // while runtime noise during a long loop of calls stays far below the bound.
        var items = new int[100_000];
        Action<int> check = static _ => { };
        for (var i = 0; i < 10; i++) items.Should().AllSatisfy(check);
        var before = GC.GetAllocatedBytesForCurrentThread();
        items.Should().AllSatisfy(check);
        Assert.True(GC.GetAllocatedBytesForCurrentThread() - before <= 1024);
    }

    private static long Bytes(Action call)
    {
        call();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < Calls; i++) call();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    private static void AssertNoCopy<T>(IEnumerable<T> subject, Action<GenericCollectionAssertions<T>> assertion)
    {
        var baseline = Bytes(() => subject.Should());
        Assert.Equal(baseline, Bytes(() => assertion(subject.Should())));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(10_000)]
    public void Count_methods_do_not_copy_a_set(int size)
    {
        var set = Enumerable.Range(0, size).ToHashSet();
        AssertNoCopy(set, a => a.HaveCount(size));
        AssertNoCopy(set, a => a.HaveCountGreaterThan(size - 1));
        AssertNoCopy(set, a => a.HaveCountGreaterThanOrEqualTo(size));
        AssertNoCopy(set, a => a.NotBeEmpty());
        AssertNoCopy(new HashSet<int>(), a => a.BeEmpty());
    }

    [Theory]
    [InlineData(10)]
    [InlineData(10_000)]
    public void Count_methods_do_not_copy_a_dictionary(int size)
    {
        var d = Enumerable.Range(0, size).ToDictionary(i => i, i => i);
        var baseline = Bytes(() => d.Should());
        Assert.Equal(baseline, Bytes(() => d.Should().HaveCount(size)));
        Assert.Equal(baseline, Bytes(() => d.Should().NotBeEmpty()));
    }

    private static Dictionary<string, int> Dict() => new() { ["a"] = 1, ["b"] = 2, ["c"] = 3 };

    // The assertions object from Should() is a class and costs a few bytes on its own, so a passing assertion is measured
    // against a call that creates the same object and does nothing with it: the difference is what the assertion allocates.
    private static void AssertAssertionAllocatesNothing(Dictionary<string, int> d, Action<DictionaryAssertions<string, int>> assertion)
    {
        Action<DictionaryAssertions<string, int>> none = static _ => { };
        var baseline = Bytes(() => none(d.Should()));
        Assert.Equal(baseline, Bytes(() => assertion(d.Should())));
    }

    [Fact] public void Dictionary_ContainValue() => AssertAssertionAllocatesNothing(Dict(), a => a.ContainValue(3));

    [Fact]
    public void Dictionary_ContainValues()
    {
        var wanted = new[] { 1, 3 };
        AssertAssertionAllocatesNothing(Dict(), a => a.ContainValues(wanted));
    }

    [Fact] public void Dictionary_NotContainValue() => AssertAssertionAllocatesNothing(Dict(), a => a.NotContainValue(9));

    [Fact]
    public void Dictionary_NotContainValues()
    {
        var unwanted = new[] { 8, 9 };
        AssertAssertionAllocatesNothing(Dict(), a => a.NotContainValues(unwanted));
    }

    [Fact]
    public void Dictionary_Contain_pair()
    {
        var pair = new KeyValuePair<string, int>("b", 2);
        AssertAssertionAllocatesNothing(Dict(), a => a.Contain(pair));
    }

    [Fact] public void Dictionary_Contain_key_and_value() => AssertAssertionAllocatesNothing(Dict(), a => a.Contain("b", 2));

    [Fact]
    public void Dictionary_NotContain_pair()
    {
        var pair = new KeyValuePair<string, int>("b", 9);
        AssertAssertionAllocatesNothing(Dict(), a => a.NotContain(pair));
    }

    [Fact] public void Dictionary_NotContain_key_and_value() => AssertAssertionAllocatesNothing(Dict(), a => a.NotContain("b", 9));

    [Fact]
    public void Dictionary_NotContainKeys()
    {
        var keys = new[] { "x", "y" };
        AssertAssertionAllocatesNothing(Dict(), a => a.NotContainKeys(keys));
    }

    [Fact]
    public void Dictionary_Equal()
    {
        var expected = Dict();
        AssertAssertionAllocatesNothing(Dict(), a => a.Equal(expected));
    }

    [Fact]
    public void Dictionary_NotEqual()
    {
        var other = Dict();
        other["c"] = 4;
        AssertAssertionAllocatesNothing(Dict(), a => a.NotEqual(other));
    }

    [Fact] public void String_ContainAll() { var s = "abc"; var values = new[] { "a", "c" }; AssertNoAllocation(() => s.Should().ContainAll(values)); }

    [Fact] public void String_ContainAny() { var s = "abc"; var values = new[] { "x", "c" }; AssertNoAllocation(() => s.Should().ContainAny(values)); }

    [Fact] public void String_NotContainAll() { var s = "abc"; var values = new[] { "a", "x" }; AssertNoAllocation(() => s.Should().NotContainAll(values)); }

    [Fact] public void String_NotContainAny() { var s = "abc"; var values = new[] { "x", "y" }; AssertNoAllocation(() => s.Should().NotContainAny(values)); }

    [Fact]
    public void Collection_ContainInOrder()
    {
        var expected = new[] { 1, 3, 5 };
        AssertNoCopy(new[] { 1, 2, 3, 4, 5 }, a => a.ContainInOrder(expected));
    }

    [Fact]
    public void Collection_ContainInConsecutiveOrder()
    {
        var expected = new[] { 3, 4 };
        AssertNoCopy(new[] { 1, 2, 3, 4, 5 }, a => a.ContainInConsecutiveOrder(expected));
    }
}
