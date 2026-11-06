using Xunit;

namespace Vouch.Tests;

[Flags]
public enum Perm : byte { None = 0, Read = 1, Write = 2 }

public enum Wide : ulong { Small = 1, Huge = ulong.MaxValue }

public enum Signed : long { Negative = -5, Zero = 0 }

public enum Mood { Active = 5, Draft = 2 }

public class ShouldResolutionTests
{
    private static readonly Guid G1 = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void Task_and_stream_assertions_expose_the_object_assertions()
    {
        Task task = Task.CompletedTask;
        Task<int> number = Task.FromResult(1);
        Stream stream = new MemoryStream();
        MemoryStream memory = new();

        task.Should().Be(task).And.NotBe(number).And.NotBeNull().And.BeSameAs(task).And.NotBeSameAs(number)
            .And.BeOneOf(task).And.BeEquivalentTo(task).And.Match<Task>(t => t.IsCompleted);
        task.Should().BeOfType(task.GetType()).And.BeAssignableTo<Task>();
        task.Should().BeAssignableTo<Task>("because").Which.Should().BeSameAs(task);
        task.Should().NotBeOfType<string>().And.NotBeAssignableTo<string>();
        Task? none = null;
        none.Should().BeNull().And.BeNull("because");

        number.Should().Be(number).And.NotBe(task).And.NotBeNull().And.BeSameAs(number).And.NotBeSameAs(task)
            .And.BeOneOf(number).And.BeEquivalentTo(number).And.Match<Task<int>>(t => t.IsCompleted);
        number.Should().BeOfType(number.GetType()).And.BeAssignableTo<Task>();
        number.Should().BeAssignableTo<Task>("because").Which.Should().BeSameAs(number);
        number.Should().NotBeOfType<string>().And.NotBeAssignableTo<string>();

        stream.Should().Be(stream).And.NotBe(memory).And.NotBeNull().And.BeSameAs(stream).And.NotBeSameAs(memory)
            .And.BeOneOf(stream).And.BeEquivalentTo(stream).And.Match<Stream>(s => s.CanRead);
        stream.Should().BeOfType(stream.GetType()).And.BeAssignableTo<Stream>();
        stream.Should().BeOfType<MemoryStream>("because").Which.Should().BeSameAs(stream);
        stream.Should().NotBeOfType<string>().And.NotBeAssignableTo<string>();
        Stream? noStream = null;
        noStream.Should().BeNull().And.BeNull("because");

        memory.Should().Be(memory).And.NotBe(stream).And.NotBeNull().And.BeSameAs(memory).And.NotBeSameAs(stream)
            .And.BeOneOf(memory).And.BeEquivalentTo(memory).And.Match<MemoryStream>(s => s.CanRead);
        memory.Should().BeOfType(typeof(MemoryStream)).And.BeAssignableTo<Stream>();
        memory.Should().BeAssignableTo<Stream>("because").Which.Should().BeSameAs(memory);
        memory.Should().NotBeOfType<string>().And.NotBeAssignableTo<string>();
    }

    [Fact] public void Guid_resolves() { var g = G1; g.Should().Be(g); }
    [Fact] public void Guid_existing_members_still_compile() { var g = G1; g.Should().Be(G1).And.NotBe(Guid.Empty).And.BeOneOf(G1, Guid.Empty).And.BeEquivalentTo(G1); }
    [Fact] public void Nullable_Guid_resolves() { Guid? g = G1; g.Should().Be(G1); g.Should().BeOneOf(G1).And.BeEquivalentTo(G1); }

    [Fact] public void Enum_resolves() { var s = Status.Active; s.Should().Be(Status.Active); }
    [Fact] public void Flags_byte_enum_resolves() { var p = Perm.Read; p.Should().HaveFlag(Perm.Read); }
    [Fact] public void Ulong_enum_resolves() { var w = Wide.Huge; w.Should().Be(Wide.Huge); }
    [Fact] public void Long_enum_resolves() { var s = Signed.Negative; s.Should().Be(Signed.Negative); }
    [Fact] public void Nullable_enum_resolves() { Status? s = Status.Active; s.Should().Be(Status.Active); }

    [Fact] public void Char_resolves() { var c = 'a'; c.Should().Be('a'); }
    [Fact] public void Nullable_char_resolves() { char? c = 'a'; c.Should().Be('a'); }
    [Fact] public void Sbyte_resolves() { sbyte n = -3; n.Should().Be(-3); }
    [Fact] public void Nullable_sbyte_resolves() { sbyte? n = -3; n.Should().Be(-3); }
    [Fact] public void Ushort_resolves() { ushort n = 3; n.Should().Be(3); }
    [Fact] public void Nullable_ushort_resolves() { ushort? n = 3; n.Should().Be(3); }
    [Fact] public void Uint_resolves() { uint n = 3; n.Should().Be(3u); }
    [Fact] public void Nullable_uint_resolves() { uint? n = 3; n.Should().Be(3u); }
    [Fact] public void Ulong_resolves() { ulong n = 3; n.Should().Be(3ul); }
    [Fact] public void Nullable_ulong_resolves() { ulong? n = 3; n.Should().Be(3ul); }

    [Fact] public void TimeOnly_resolves() { var t = new TimeOnly(10, 0); t.Should().Be(t); }
    [Fact] public void Nullable_TimeOnly_resolves() { TimeOnly? t = new TimeOnly(10, 0); t.Should().Be(new TimeOnly(10, 0)); }

    [Fact]
    public void TimeOnly_all_members_compile()
    {
        var t = new TimeOnly(10, 0);
        var later = new TimeOnly(11, 0);
        t.Should().BeOneOf(t, later).And.BeEquivalentTo(t).And.BeEquivalentTo(t, o => o)
            .And.BeGreaterThan(new TimeOnly(9, 0)).And.BeGreaterThanOrEqualTo(t)
            .And.BeLessThan(later).And.BeLessThanOrEqualTo(t)
            .And.Be(t, "because").And.NotBe(later, "because");
    }

    [Fact]
    public void Nullable_TimeOnly_all_members_compile()
    {
        TimeOnly? t = new TimeOnly(10, 0);
        TimeOnly? other = new TimeOnly(10, 0);
        TimeOnly? none = null;
        t.Should().BeOneOf(new TimeOnly(10, 0), new TimeOnly(11, 0)).And.BeEquivalentTo(t).And.BeEquivalentTo(t, o => o)
            .And.Be(other).And.Be(other, "because").And.Be(new TimeOnly(10, 0), "because").And.NotBe(new TimeOnly(11, 0), "because");
        none.Should().Be(null).And.Be(none).And.Be(null, "because");
    }

    [Fact] public void Nullable_uint_be_overloads() { uint? n = 3; uint? o = 3; uint? none = null; n.Should().Be(o).And.Be(3u, "because").And.NotBe(4u, "because"); none.Should().Be(null).And.Be(none, "because"); }
    [Fact] public void Nullable_char_be_overloads() { char? n = 'a'; char? o = 'a'; char? none = null; n.Should().Be(o).And.Be('a', "because").And.NotBe('b', "because"); none.Should().Be(null).And.Be(none, "because"); }
    [Fact] public void Nullable_sbyte_be_overloads() { sbyte? n = 3; sbyte? o = 3; sbyte? none = null; n.Should().Be(o).And.Be((sbyte)3, "because").And.NotBe((sbyte)4, "because"); none.Should().Be(null).And.Be(none, "because"); }
    [Fact] public void Nullable_ushort_be_overloads() { ushort? n = 3; ushort? o = 3; ushort? none = null; n.Should().Be(o).And.Be((ushort)3, "because").And.NotBe((ushort)4, "because"); none.Should().Be(null).And.Be(none, "because"); }
    [Fact] public void Nullable_ulong_be_overloads() { ulong? n = 3; ulong? o = 3; ulong? none = null; n.Should().Be(o).And.Be(3ul, "because").And.NotBe(4ul, "because"); none.Should().Be(null).And.Be(none, "because"); }

    [Fact] public void Char_comparison_and_set_members() { var c = 'b'; c.Should().BeGreaterThan('a').And.BeOneOf('a', 'b').And.BeEquivalentTo('b'); }
    [Fact] public void Uint_comparison_and_set_members() { uint n = 5; n.Should().BeGreaterThan(3u).And.BeOneOf(4u, 5u).And.BeEquivalentTo(5u); }

    [Fact] public void TimeSpan_resolves() { var t = TimeSpan.FromSeconds(5); t.Should().Be(t).And.BePositive(); }
    [Fact] public void Nullable_TimeSpan_resolves() { TimeSpan? t = TimeSpan.FromSeconds(5); t.Should().Be(TimeSpan.FromSeconds(5)); }

    // ImmutableArray<T> has its own assertions type: the collection assertions plus every member ValueAssertions<ImmutableArray<T>> had.
    [Fact] public void ImmutableArray_resolves_to_its_own_assertions() => Assert.IsType<ImmutableArrayAssertions<int>>(System.Collections.Immutable.ImmutableArray.Create(1).Should());

    [Fact]
    public void ImmutableArray_every_ValueAssertions_member_still_compiles()
    {
        var a = System.Collections.Immutable.ImmutableArray.Create(1, 2);
        var same = a;
        var other = System.Collections.Immutable.ImmutableArray.Create(1, 2);
        a.Should().Be(same).And.NotBe(other).And.BeOneOf(other, same).And.BeEquivalentTo(a);
        a.Should().Be(same, "because").And.NotBe(other, "because");
        a.Should().BeEquivalentTo(new[] { 1, 2 });
        a.Should().BeEquivalentTo(a, o => o);
        Assert.Equal(a, a.Should().Subject);
        Assert.Equal("a", a.Should().Expression);
        a.Should().Be(same).And.HaveCount(2).And.Contain(2);
        Assert.Throws<VouchException>(() => a.Should().Be(other));
        Assert.Throws<VouchException>(() => a.Should().NotBe(same));
        Assert.Throws<VouchException>(() => a.Should().BeOneOf(other));
    }

    private sealed class Bag(params object[] items) : System.Collections.IEnumerable
    {
        public System.Collections.IEnumerator GetEnumerator() => items.GetEnumerator();
    }

    // A non-generic IEnumerable keeps every ObjectAssertions member and gains the collection assertions.
    [Fact]
    public void Non_generic_IEnumerable_keeps_every_ObjectAssertions_member()
    {
        var list = new System.Collections.ArrayList { 1, "a" };
        var otherList = new System.Collections.ArrayList { 1, "a" };
        list.Should().Be(list).And.NotBe(otherList).And.NotBeNull().And.BeSameAs(list).And.NotBeSameAs(otherList);
        list.Should().Be(list, "because").And.NotBe(otherList, "because").And.NotBeNull("because");
        list.Should().BeOfType<System.Collections.ArrayList>().Which.Count.Should().Be(2);
        list.Should().BeOfType<System.Collections.ArrayList>("because");
        list.Should().BeOfType(typeof(System.Collections.ArrayList)).And.BeOfType(typeof(System.Collections.ArrayList), "because");
        list.Should().NotBeOfType<string>().And.NotBeAssignableTo<string>();
        list.Should().BeAssignableTo<System.Collections.IList>().And.BeAssignableTo<System.Collections.IList>("because");
        list.Should().Match<System.Collections.ArrayList>(l => l.Count == 2).And.Match<System.Collections.ArrayList>(l => l.Count == 2, "because");
        list.Should().BeOneOf(otherList, list).And.BeEquivalentTo(list);
        list.Should().BeEquivalentTo(list, o => o);
        Assert.Same(list, list.Should().Subject);
        list.Should().HaveCount(2).And.Contain("a");
        Assert.Throws<VouchException>(() => list.Should().BeNull());
        Assert.Throws<VouchException>(() => list.Should().Be(otherList));
        Assert.Throws<VouchException>(() => list.Should().BeSameAs(otherList));
        System.Collections.ArrayList? none = null;
        none.Should().BeNull().And.BeNull("because");
        Assert.Throws<VouchException>(() => none.Should().NotBeNull());
        Assert.Null(none.Should().Subject);
    }

    [Fact]
    public void IEnumerable_only_user_type_keeps_every_ObjectAssertions_member()
    {
        var bag = new Bag(1, "a");
        var other = new Bag(1, "a");
        bag.Should().Be(bag).And.NotBe(other).And.NotBeNull().And.BeSameAs(bag).And.NotBeSameAs(other);
        bag.Should().BeOfType<Bag>().And.NotBeOfType<string>();
        bag.Should().BeOfType(typeof(Bag));
        bag.Should().BeAssignableTo<System.Collections.IEnumerable>().And.NotBeAssignableTo<string>();
        bag.Should().Match<Bag>(b => b != null);
        bag.Should().BeOneOf(other, bag).And.BeEquivalentTo(bag);
        Assert.Same(bag, bag.Should().Subject);
        bag.Should().HaveCount(2);
        Assert.Throws<VouchException>(() => bag.Should().BeNull());
        Assert.Throws<VouchException>(() => bag.Should().Be(other));
    }
}
