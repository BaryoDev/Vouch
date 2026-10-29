using Xunit;

namespace Vouch.Tests;

public class StringAssertionsTests
{
    private static string Message(Action act) => Assert.Throws<VouchException>(act).Message;

    [Fact] public void Be_passes() { var s = "abc"; s.Should().Be("abc"); }
    [Fact] public void Be_is_case_sensitive() { var s = "abc"; Assert.Throws<VouchException>(() => s.Should().Be("ABC")); }
    [Fact] public void Be_fails_on_null() { string? s = null; Assert.Throws<VouchException>(() => s.Should().Be("a")); }
    [Fact] public void Be_null_passes_on_null() { string? s = null; s.Should().Be(null); }
    [Fact]
    public void Be_message_has_the_diff_line()
    {
        var name = "Juan Dela Kruz";
        Assert.Equal("Expected name to be \"Juan Dela Cruz\", but found \"Juan Dela Kruz\".\n  They differ at index 10: \"Cruz\" against \"Kruz\".",
            Message(() => name.Should().Be("Juan Dela Cruz")));
    }
    [Fact] public void Be_message_on_null_has_no_diff() { string? s = null; Assert.Equal("Expected s to be \"a\", but found null.", Message(() => s.Should().Be("a"))); }
    [Fact] public void Be_null_message() { var s = "a"; Assert.Equal("Expected s to be null, but found \"a\".", Message(() => s.Should().Be(null))); }

    [Fact] public void NotBe_passes() { var s = "abc"; s.Should().NotBe("abd"); }
    [Fact] public void NotBe_fails() { var s = "abc"; Assert.Throws<VouchException>(() => s.Should().NotBe("abc")); }
    [Fact] public void NotBe_message() { var s = "abc"; Assert.Equal("Expected s not to be \"abc\", but it was.", Message(() => s.Should().NotBe("abc"))); }

    [Fact] public void BeEmpty_passes() { var s = ""; s.Should().BeEmpty(); }
    [Fact] public void BeEmpty_fails() { var s = "a"; Assert.Throws<VouchException>(() => s.Should().BeEmpty()); }
    [Fact] public void BeEmpty_message() { var s = "a"; Assert.Equal("Expected s to be empty, but found \"a\".", Message(() => s.Should().BeEmpty())); }
    [Fact] public void BeEmpty_fails_on_null() { string? s = null; Assert.Equal("Expected s to be empty, but found null.", Message(() => s.Should().BeEmpty())); }

    [Fact] public void NotBeEmpty_passes() { var s = "a"; s.Should().NotBeEmpty(); }
    [Fact] public void NotBeEmpty_fails() { var s = ""; Assert.Throws<VouchException>(() => s.Should().NotBeEmpty()); }
    [Fact] public void NotBeEmpty_message() { var s = ""; Assert.Equal("Expected s not to be empty, but it was.", Message(() => s.Should().NotBeEmpty())); }
    [Fact] public void NotBeEmpty_fails_on_null() { string? s = null; Assert.Equal("Expected s not to be empty, but found null.", Message(() => s.Should().NotBeEmpty())); }

    [Fact] public void BeNull_passes() { string? s = null; s.Should().BeNull(); }
    [Fact] public void BeNull_fails() { var s = "a"; Assert.Throws<VouchException>(() => s.Should().BeNull()); }
    [Fact] public void BeNull_message() { var s = "a"; Assert.Equal("Expected s to be null, but found \"a\".", Message(() => s.Should().BeNull())); }

    [Fact] public void NotBeNull_passes() { var s = "a"; s.Should().NotBeNull(); }
    [Fact] public void NotBeNull_fails() { string? s = null; Assert.Throws<VouchException>(() => s.Should().NotBeNull()); }
    [Fact] public void NotBeNull_message() { string? s = null; Assert.Equal("Expected s not to be null, but it was.", Message(() => s.Should().NotBeNull())); }

    [Fact] public void BeNullOrEmpty_passes() { string? s = null; s.Should().BeNullOrEmpty(); var e = ""; e.Should().BeNullOrEmpty(); }
    [Fact] public void BeNullOrEmpty_fails() { var s = "a"; Assert.Throws<VouchException>(() => s.Should().BeNullOrEmpty()); }
    [Fact] public void BeNullOrEmpty_message() { var s = "a"; Assert.Equal("Expected s to be null or empty, but found \"a\".", Message(() => s.Should().BeNullOrEmpty())); }

    [Fact] public void NotBeNullOrEmpty_passes() { var s = "a"; s.Should().NotBeNullOrEmpty(); }
    [Fact] public void NotBeNullOrEmpty_fails() { var s = ""; Assert.Throws<VouchException>(() => s.Should().NotBeNullOrEmpty()); }
    [Fact] public void NotBeNullOrEmpty_message() { string? s = null; Assert.Equal("Expected s not to be null or empty, but found null.", Message(() => s.Should().NotBeNullOrEmpty())); }
    [Fact] public void NotBeNullOrEmpty_message_on_empty() { var s = ""; Assert.Equal("Expected s not to be null or empty, but found \"\".", Message(() => s.Should().NotBeNullOrEmpty())); }

    [Fact] public void NotBeNullOrWhiteSpace_passes() { var s = " a "; s.Should().NotBeNullOrWhiteSpace(); }
    [Fact] public void NotBeNullOrWhiteSpace_fails() { var s = "  "; Assert.Throws<VouchException>(() => s.Should().NotBeNullOrWhiteSpace()); }
    [Fact] public void NotBeNullOrWhiteSpace_message() { var s = "  "; Assert.Equal("Expected s not to be null or white space, but found \"  \".", Message(() => s.Should().NotBeNullOrWhiteSpace())); }
    [Fact] public void NotBeNullOrWhiteSpace_fails_on_null() { string? s = null; Assert.Equal("Expected s not to be null or white space, but found null.", Message(() => s.Should().NotBeNullOrWhiteSpace())); }

    [Fact] public void StartWith_passes() { var s = "ABC"; s.Should().StartWith("AB"); }
    [Fact] public void StartWith_is_case_sensitive() { var s = "ABC"; Assert.Throws<VouchException>(() => s.Should().StartWith("ab")); }
    [Fact] public void StartWith_message() { var s = "XBC"; Assert.Equal("Expected s to start with \"ABC\", but found \"XBC\".", Message(() => s.Should().StartWith("ABC"))); }
    [Fact] public void Null_StartWith_fails_with_message() { string? s = null; Assert.Equal("Expected s to start with \"A\", but found null.", Message(() => s.Should().StartWith("A"))); }

    [Fact] public void NotStartWith_passes() { var s = "ABC"; s.Should().NotStartWith("X"); }
    [Fact] public void NotStartWith_fails() { var s = "ABC"; Assert.Throws<VouchException>(() => s.Should().NotStartWith("AB")); }
    [Fact] public void NotStartWith_message() { var s = "ABC"; Assert.Equal("Expected s not to start with \"AB\", but found \"ABC\".", Message(() => s.Should().NotStartWith("AB"))); }
    [Fact] public void NotStartWith_fails_on_null() { string? s = null; Assert.Equal("Expected s not to start with \"A\", but found null.", Message(() => s.Should().NotStartWith("A"))); }

    [Fact] public void EndWith_passes() { var s = "ABC"; s.Should().EndWith("BC"); }
    [Fact] public void EndWith_fails() { var s = "ABC"; Assert.Throws<VouchException>(() => s.Should().EndWith("bc")); }
    [Fact] public void EndWith_message() { var s = "ABX"; Assert.Equal("Expected s to end with \"BC\", but found \"ABX\".", Message(() => s.Should().EndWith("BC"))); }
    [Fact] public void EndWith_fails_on_null() { string? s = null; Assert.Equal("Expected s to end with \"A\", but found null.", Message(() => s.Should().EndWith("A"))); }

    [Fact] public void Contain_passes() { var s = "abc"; s.Should().Contain("b"); }
    [Fact] public void Contain_fails() { var s = "abc"; Assert.Throws<VouchException>(() => s.Should().Contain("B")); }
    [Fact] public void Contain_message() { var s = "abc"; Assert.Equal("Expected s to contain \"x\", but found \"abc\".", Message(() => s.Should().Contain("x"))); }
    [Fact] public void Contain_fails_on_null() { string? s = null; Assert.Equal("Expected s to contain \"x\", but found null.", Message(() => s.Should().Contain("x"))); }

    [Fact] public void NotContain_passes() { var s = "abc"; s.Should().NotContain("x"); }
    [Fact] public void NotContain_fails() { var s = "abc"; Assert.Throws<VouchException>(() => s.Should().NotContain("b")); }
    [Fact] public void NotContain_message() { var s = "abc"; Assert.Equal("Expected s not to contain \"b\", but found \"abc\".", Message(() => s.Should().NotContain("b"))); }
    [Fact] public void NotContain_fails_on_null() { string? s = null; Assert.Equal("Expected s not to contain \"b\", but found null.", Message(() => s.Should().NotContain("b"))); }

    [Theory]
    [InlineData("abc")]
    [InlineData(null)]
    public void Contain_empty_expected_throws_ArgumentException(string? text)
    {
        var ex = Assert.Throws<ArgumentException>(() => text.Should().Contain(""));
        Assert.Equal("expected", ex.ParamName);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData(null)]
    public void NotContain_empty_unexpected_throws_ArgumentException(string? text)
    {
        var ex = Assert.Throws<ArgumentException>(() => text.Should().NotContain(""));
        Assert.Equal("unexpected", ex.ParamName);
    }

    [Fact]
    public void Contain_empty_expected_with_reason_throws_ArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => "abc".Should().Contain("", "because"));
        Assert.Equal("expected", ex.ParamName);
    }


    [Fact] public void Match_passes() { var s = "INV-001"; s.Should().Match("INV-*"); }
    [Fact] public void Match_fails() { var s = "PO-1"; Assert.Throws<VouchException>(() => s.Should().Match("INV-*")); }
    [Fact] public void Match_message() { var s = "PO-1"; Assert.Equal("Expected s to match \"INV-*\", but found \"PO-1\".", Message(() => s.Should().Match("INV-*"))); }
    [Theory]
    [InlineData("abc\n", "abc")]
    [InlineData("abc\n", "abc?")]
    [InlineData("a\nb", "a?b")]
    [InlineData("a\nb", "a*b")]
    public void Match_passes_across_newlines_like_a_regex_dollar(string text, string pattern) => text.Should().Match(pattern);

    [Theory]
    [InlineData("abc", "abc\n")]
    [InlineData("abc\n\n", "abc")]
    [InlineData("abc\r\n", "abc")]
    [InlineData("a\r\nb", "a\nb")]
    [InlineData("ABC", "abc")]
    public void Match_still_fails_on_other_differences(string text, string pattern) =>
        Assert.Throws<VouchException>(() => text.Should().Match(pattern));

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void Match_empty_pattern_throws_ArgumentException(string text)
    {
        var ex = Assert.Throws<ArgumentException>(() => text.Should().Match(""));
        Assert.Equal("wildcardPattern", ex.ParamName);
        Assert.StartsWith("Cannot match string against an empty string. Provide a wildcard pattern or use the BeEmpty method.", ex.Message);
    }

    [Fact] public void BeEquivalentTo_null_passes_on_a_null_subject() { string? s = null; s.Should().BeEquivalentTo(null); }
    [Fact] public void BeEquivalentTo_null_fails_on_a_text_subject() { var s = "a"; Assert.Equal("Expected s to be equivalent to null, but found \"a\".", Message(() => s.Should().BeEquivalentTo(null))); }
    [Fact] public void BeEquivalentTo_null_with_a_reason_fails() { var s = "a"; Assert.Equal("Expected s to be equivalent to null, but found \"a\". Because it is gone.", Message(() => s.Should().BeEquivalentTo(null, "it is gone"))); }

    [Fact] public void Match_fails_on_null() { string? s = null; Assert.Equal("Expected s to match \"A*\", but found null.", Message(() => s.Should().Match("A*"))); }

    [Fact] public void MatchRegex_passes() { var s = "INV-001"; s.Should().MatchRegex(@"^INV-\d+$"); }
    [Fact] public void MatchRegex_fails() { var s = "PO-1"; Assert.Throws<VouchException>(() => s.Should().MatchRegex(@"^INV-\d+$")); }
    [Fact] public void MatchRegex_message() { var s = "PO-1"; Assert.Equal("Expected s to match regex \"^INV-\\\\d+$\", but found \"PO-1\".", Message(() => s.Should().MatchRegex(@"^INV-\d+$"))); }
    [Fact] public void MatchRegex_fails_on_null() { string? s = null; Assert.Equal("Expected s to match regex \"a\", but found null.", Message(() => s.Should().MatchRegex("a"))); }
    [Fact]
    public void MatchRegex_times_out_with_a_message()
    {
        var s = new string('a', 60) + "!";
        var text = Message(() => s.Should().MatchRegex("^(a+)+$"));
        Assert.StartsWith("Expected s to match regex \"^(a+)+$\", but the match timed out after 1 second", text);
    }

    [Fact] public void HaveLength_passes() { var s = "abc"; s.Should().HaveLength(3); }
    [Fact] public void HaveLength_fails() { var s = "abc"; Assert.Throws<VouchException>(() => s.Should().HaveLength(5)); }
    [Fact] public void HaveLength_message() { var s = "abc"; Assert.Equal("Expected s to have length 5, but found \"abc\" with length 3.", Message(() => s.Should().HaveLength(5))); }
    [Fact] public void HaveLength_fails_on_null() { string? s = null; Assert.Equal("Expected s to have length 5, but found null.", Message(() => s.Should().HaveLength(5))); }

    [Fact] public void BeOneOf_passes() { var s = "b"; s.Should().BeOneOf("a", "b"); }
    [Fact] public void BeOneOf_passes_on_null() { string? s = null; s.Should().BeOneOf("a", null); }
    [Fact] public void BeOneOf_fails() { var s = "c"; Assert.Throws<VouchException>(() => s.Should().BeOneOf("a", "b")); }
    [Fact] public void BeOneOf_message() { var s = "c"; Assert.Equal("Expected s to be one of [\"a\", \"b\"], but found \"c\".", Message(() => s.Should().BeOneOf("a", "b"))); }

    [Fact] public void BeEquivalentTo_passes() { var s = "ABC"; s.Should().BeEquivalentTo("abc"); }
    [Fact] public void BeEquivalentTo_fails() { var s = "ABC"; Assert.Throws<VouchException>(() => s.Should().BeEquivalentTo("abd")); }
    [Fact] public void BeEquivalentTo_message() { var s = "ABC"; Assert.Equal("Expected s to be equivalent to \"abd\", but found \"ABC\".", Message(() => s.Should().BeEquivalentTo("abd"))); }
    [Fact] public void BeEquivalentTo_fails_on_null() { string? s = null; Assert.Equal("Expected s to be equivalent to \"a\", but found null.", Message(() => s.Should().BeEquivalentTo("a"))); }

    [Fact] public void And_chains() { var s = "abc"; s.Should().StartWith("a").And.EndWith("c"); }
    [Fact] public void Object_overload_still_works_for_strings_cast_to_object() { object s = "abc"; s.Should().BeOfType<string>(); }

    public static TheoryData<string, string?> NullArgumentCases => new()
    {
        { "expected", "StartWith" }, { "expected", "EndWith" }, { "unexpected", "NotStartWith" },
        { "expected", "Contain" }, { "unexpected", "NotContain" }, { "wildcardPattern", "Match" },
        { "pattern", "MatchRegex" }, { "validValues", "BeOneOf" },
    };

    private static void Call(string? subject, string method)
    {
        switch (method)
        {
            case "StartWith": subject.Should().StartWith(null!); break;
            case "EndWith": subject.Should().EndWith(null!); break;
            case "NotStartWith": subject.Should().NotStartWith(null!); break;
            case "Contain": subject.Should().Contain(null!); break;
            case "NotContain": subject.Should().NotContain(null!); break;
            case "Match": subject.Should().Match(null!); break;
            case "MatchRegex": subject.Should().MatchRegex(null!); break;
            case "BeOneOf": subject.Should().BeOneOf(null!); break;
        }
    }

    [Theory]
    [MemberData(nameof(NullArgumentCases))]
    public void Null_argument_throws_for_a_non_null_subject(string parameter, string? method)
    {
        var ex = Assert.Throws<ArgumentNullException>(() => Call("abc", method!));
        Assert.Equal(parameter, ex.ParamName);
    }

    [Theory]
    [MemberData(nameof(NullArgumentCases))]
    public void Null_argument_throws_for_a_null_subject(string parameter, string? method)
    {
        var ex = Assert.Throws<ArgumentNullException>(() => Call(null, method!));
        Assert.Equal(parameter, ex.ParamName);
    }

    [Fact]
    public void MatchRegex_invalid_pattern_fails_with_a_message()
    {
        var s = "abc";
        var text = Message(() => s.Should().MatchRegex("("));
        Assert.StartsWith("Expected s to match regex \"(\", but the pattern is not a valid regular expression: ", text);
        Assert.EndsWith(".", text);
        Assert.False(text.EndsWith("..", StringComparison.Ordinal));
    }

    [Fact] public void NotContainEquivalentOf_passes_when_absent() { var s = "Header: value"; s.Should().NotContainEquivalentOf("token "); }
    [Fact] public void NotContainEquivalentOf_ignores_case() { var s = "Header: TOKEN abc"; Assert.Throws<VouchException>(() => s.Should().NotContainEquivalentOf("token ")); }
    [Fact] public void NotContainEquivalentOf_message() { var s = "a PASSWORD here"; Assert.Equal("Expected s not to contain \"password\" (ignoring case), but found \"a PASSWORD here\".", Message(() => s.Should().NotContainEquivalentOf("password"))); }
    [Fact] public void NotContainEquivalentOf_fails_on_null() { string? s = null; Assert.Equal("Expected s not to contain \"x\" (ignoring case), but found null.", Message(() => s.Should().NotContainEquivalentOf("x"))); }
    [Fact] public void NotContainEquivalentOf_null_argument_throws_ArgumentNullException() { var s = "a"; Assert.Throws<ArgumentNullException>(() => s.Should().NotContainEquivalentOf(null!)); }
    [Fact] public void NotContainEquivalentOf_takes_a_reason() { var s = "abc"; Assert.Equal("Expected s not to contain \"B\" (ignoring case), but found \"abc\". Because it must not.", Message(() => s.Should().NotContainEquivalentOf("B", "it must not"))); }
}
