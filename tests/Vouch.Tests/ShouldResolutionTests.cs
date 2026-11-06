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

    [Fact] public void TimeSpan_resolves() { var t = TimeSpan.FromSeconds(5); t.Should().Be(t).And.BePositive(); }
    [Fact] public void Nullable_TimeSpan_resolves() { TimeSpan? t = TimeSpan.FromSeconds(5); t.Should().Be(TimeSpan.FromSeconds(5)); }
}
