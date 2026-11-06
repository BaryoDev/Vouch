using Xunit;

namespace Vouch.Tests;

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

    [Fact] public void Guid_Be() { var id = Guid.NewGuid(); AssertNoAllocation(() => id.Should().Be(id)); }

    [Fact] public void Enum_Be() { var s = Status.Active; AssertNoAllocation(() => s.Should().Be(Status.Active)); }

    [Fact] public void Bool_BeTrue() { var b = true; AssertNoAllocation(() => b.Should().BeTrue()); }

    [Fact] public void Nullable_int_Be() { int? n = 5; AssertNoAllocation(() => n.Should().Be(5)); }

    [Fact] public void Decimal_Be() { var m = 100m; AssertNoAllocation(() => m.Should().Be(100m)); }

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

    private static long BytesForAllSatisfy(int size)
    {
        var items = new int[size];
        Action<int> check = static _ => { };
        items.Should().AllSatisfy(check);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < Calls; i++) items.Should().AllSatisfy(check);
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [Fact]
    public void Passing_AllSatisfy_has_no_per_item_allocation()
    {
        Assert.Equal(BytesForAllSatisfy(10), BytesForAllSatisfy(1000));
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
