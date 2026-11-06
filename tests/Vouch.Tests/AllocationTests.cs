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

    [Fact] public void Int_Be() { var n = 5; AssertNoAllocation(() => n.Should().Be(5)); }

    [Fact] public void Passing_assertion_inside_scope() { var n = 5; using var scope = new AssertionScope(); AssertNoAllocation(() => n.Should().Be(5)); }

    [Fact] public void Guid_Be() { var id = Guid.NewGuid(); AssertNoAllocation(() => id.Should().Be(id)); }

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
}
