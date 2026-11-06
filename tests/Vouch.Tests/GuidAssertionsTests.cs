using Xunit;

namespace Vouch.Tests;

public class GuidAssertionsTests
{
    private static readonly Guid A = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid B = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private const string AText = "11111111-1111-1111-1111-111111111111";
    private const string BText = "22222222-2222-2222-2222-222222222222";

    private static string Msg(Action act) => Assert.Throws<VouchException>(act).Message;

    [Fact] public void Be_passes() { var g = A; g.Should().Be(A); }
    [Fact] public void Be_message() { var g = A; Assert.Equal($"Expected g to be {BText}, but found {AText}.", Msg(() => g.Should().Be(B))); }
    [Fact] public void Be_because() { var g = A; Assert.Equal($"Expected g to be {BText}, but found {AText}. Because it matters.", Msg(() => g.Should().Be(B, "it matters"))); }

    [Fact] public void NotBe_passes() { var g = A; g.Should().NotBe(B); }
    [Fact] public void NotBe_message() { var g = A; Assert.Equal($"Expected g not to be {AText}, but it was.", Msg(() => g.Should().NotBe(A))); }

    [Fact] public void BeEmpty_passes() { var g = Guid.Empty; g.Should().BeEmpty(); }
    [Fact] public void BeEmpty_message() { var g = A; Assert.Equal($"Expected g to be empty, but found {AText}.", Msg(() => g.Should().BeEmpty())); }

    [Fact] public void NotBeEmpty_passes() { var g = A; g.Should().NotBeEmpty(); }
    [Fact] public void NotBeEmpty_message() { var g = Guid.Empty; Assert.Equal("Expected g not to be empty, but it was.", Msg(() => g.Should().NotBeEmpty())); }

    [Fact] public void BeOneOf_passes() { var g = A; g.Should().BeOneOf(B, A); }
    [Fact] public void BeOneOf_message() { var g = A; Assert.Equal($"Expected g to be one of [{BText}], but found {AText}.", Msg(() => g.Should().BeOneOf(B))); }

    [Fact] public void BeEquivalentTo_passes() { var g = A; g.Should().BeEquivalentTo(A); }

    [Fact] public void Nullable_Be_passes() { Guid? g = A; g.Should().Be(A); }
    [Fact] public void Nullable_Be_message() { Guid? g = null; Assert.Equal($"Expected g to be {AText}, but found null.", Msg(() => g.Should().Be(A))); }
    [Fact] public void Nullable_Be_null_passes() { Guid? g = null; g.Should().Be((Guid?)null); }
    [Fact] public void Nullable_NotBe_passes() { Guid? g = A; g.Should().NotBe(B); }
    [Fact] public void Nullable_NotBe_message() { Guid? g = A; Assert.Equal($"Expected g not to be {AText}, but it was.", Msg(() => g.Should().NotBe(A))); }
    [Fact] public void Nullable_BeEmpty_passes() { Guid? g = Guid.Empty; g.Should().BeEmpty(); }
    [Fact] public void Nullable_BeEmpty_message() { Guid? g = null; Assert.Equal("Expected g to be empty, but found null.", Msg(() => g.Should().BeEmpty())); }
    [Fact] public void Nullable_NotBeEmpty_passes() { Guid? g = A; g.Should().NotBeEmpty(); }
    [Fact] public void Nullable_NotBeEmpty_message() { Guid? g = Guid.Empty; Assert.Equal("Expected g not to be empty, but it was.", Msg(() => g.Should().NotBeEmpty())); }
    [Fact] public void Nullable_BeNull_passes() { Guid? g = null; g.Should().BeNull(); }
    [Fact] public void Nullable_BeNull_message() { Guid? g = A; Assert.Equal($"Expected g to be null, but found {AText}.", Msg(() => g.Should().BeNull())); }
    [Fact] public void Nullable_NotBeNull_returns_value() { Guid? g = A; Assert.Equal(A, g.Should().NotBeNull().Which); }
    [Fact] public void Nullable_NotBeNull_message() { Guid? g = null; Assert.Equal("Expected g not to be null, but it was.", Msg(() => g.Should().NotBeNull())); }
    [Fact] public void Nullable_BeOneOf_message() { Guid? g = null; Assert.Equal($"Expected g to be one of [{AText}], but found null.", Msg(() => g.Should().BeOneOf(A))); }
}
