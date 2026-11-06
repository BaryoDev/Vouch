using System.Globalization;
using System.Text.RegularExpressions;
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
            case "MatchRegex": subject.Should().MatchRegex((string)null!); break;
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

    [Fact] public void Contain_occurrence_passes() { var s = "banana"; s.Should().Contain("an", Exactly.Twice()).And.Contain("a", AtLeast.Times(3)).And.Contain("b", AtMost.Once()).And.Contain("n", MoreThan.Once()).And.Contain("x", LessThan.Once()); }
    [Fact] public void Contain_occurrence_fails() { var s = "banana"; Assert.Equal("Expected s to contain \"an\" exactly 1 time, but found it 2 times in \"banana\".", Message(() => s.Should().Contain("an", Exactly.Once()))); }
    [Fact]
    public void Contain_occurrence_message_for_each_kind()
    {
        var s = "banana";
        Assert.Equal("Expected s to contain \"a\" more than 3 times, but found it 3 times in \"banana\".", Message(() => s.Should().Contain("a", MoreThan.Times(3))));
        Assert.Equal("Expected s to contain \"a\" less than 3 times, but found it 3 times in \"banana\".", Message(() => s.Should().Contain("a", LessThan.Times(3))));
        Assert.Equal("Expected s to contain \"a\" at least 4 times, but found it 3 times in \"banana\".", Message(() => s.Should().Contain("a", AtLeast.Times(4))));
        Assert.Equal("Expected s to contain \"a\" at most 2 times, but found it 3 times in \"banana\".", Message(() => s.Should().Contain("a", AtMost.Twice())));
    }
    [Fact] public void Contain_occurrence_counts_without_overlap() { var s = "aaaa"; s.Should().Contain("aa", Exactly.Twice()); }
    [Fact] public void Contain_occurrence_fails_on_null() { string? s = null; Assert.Equal("Expected s to contain \"x\" exactly 1 time, but found null.", Message(() => s.Should().Contain("x", Exactly.Once()))); }
    [Fact] public void Contain_occurrence_null_argument_throws() { var s = "a"; Assert.Throws<ArgumentNullException>(() => s.Should().Contain(null!, Exactly.Once())); }
    [Fact] public void Contain_occurrence_empty_argument_throws() { var s = "a"; Assert.Throws<ArgumentException>(() => s.Should().Contain("", Exactly.Once())); }
    [Fact] public void Contain_occurrence_takes_a_reason() { var s = "abc"; Assert.Equal("Expected s to contain \"a\" exactly 2 times, but found it 1 time in \"abc\". Because twice.", Message(() => s.Should().Contain("a", Exactly.Twice(), "twice"))); }
    [Fact] public void Occurrence_negative_count_throws() { Assert.Throws<ArgumentOutOfRangeException>(() => Exactly.Times(-1)); }

    [Fact] public void StartWithEquivalentOf_passes() { var s = "abc"; s.Should().StartWithEquivalentOf("AB"); }
    [Fact] public void StartWithEquivalentOf_fails() { var s = "abc"; Assert.Equal("Expected s to start with \"X\" (ignoring case), but found \"abc\".", Message(() => s.Should().StartWithEquivalentOf("X"))); }
    [Fact] public void StartWithEquivalentOf_fails_on_null() { string? s = null; Assert.Equal("Expected s to start with \"X\" (ignoring case), but found null.", Message(() => s.Should().StartWithEquivalentOf("X"))); }
    [Fact] public void NotStartWithEquivalentOf_passes() { var s = "abc"; s.Should().NotStartWithEquivalentOf("X"); }
    [Fact] public void NotStartWithEquivalentOf_fails() { var s = "abc"; Assert.Equal("Expected s not to start with \"AB\" (ignoring case), but found \"abc\".", Message(() => s.Should().NotStartWithEquivalentOf("AB"))); }
    [Fact] public void NotStartWithEquivalentOf_fails_on_null() { string? s = null; Assert.Equal("Expected s not to start with \"X\" (ignoring case), but found null.", Message(() => s.Should().NotStartWithEquivalentOf("X"))); }
    [Fact] public void EndWithEquivalentOf_passes() { var s = "abc"; s.Should().EndWithEquivalentOf("BC"); }
    [Fact] public void EndWithEquivalentOf_fails() { var s = "abc"; Assert.Equal("Expected s to end with \"X\" (ignoring case), but found \"abc\".", Message(() => s.Should().EndWithEquivalentOf("X"))); }
    [Fact] public void EndWithEquivalentOf_fails_on_null() { string? s = null; Assert.Equal("Expected s to end with \"X\" (ignoring case), but found null.", Message(() => s.Should().EndWithEquivalentOf("X"))); }
    [Fact] public void NotEndWithEquivalentOf_passes() { var s = "abc"; s.Should().NotEndWithEquivalentOf("X"); }
    [Fact] public void NotEndWithEquivalentOf_fails() { var s = "abc"; Assert.Equal("Expected s not to end with \"BC\" (ignoring case), but found \"abc\".", Message(() => s.Should().NotEndWithEquivalentOf("BC"))); }
    [Fact] public void NotEndWithEquivalentOf_fails_on_null() { string? s = null; Assert.Equal("Expected s not to end with \"X\" (ignoring case), but found null.", Message(() => s.Should().NotEndWithEquivalentOf("X"))); }
    [Fact] public void MatchEquivalentOf_passes() { var s = "abc"; s.Should().MatchEquivalentOf("A?C").And.MatchEquivalentOf("*B*"); }
    [Fact] public void MatchEquivalentOf_fails() { var s = "abc"; Assert.Equal("Expected s to match \"x*\" (ignoring case), but found \"abc\".", Message(() => s.Should().MatchEquivalentOf("x*"))); }
    [Fact] public void MatchEquivalentOf_fails_on_null() { string? s = null; Assert.Equal("Expected s to match \"x*\" (ignoring case), but found null.", Message(() => s.Should().MatchEquivalentOf("x*"))); }
    [Fact] public void MatchEquivalentOf_null_argument_throws() { var s = "a"; Assert.Equal("wildcardPattern", Assert.Throws<ArgumentNullException>(() => s.Should().MatchEquivalentOf(null!)).ParamName); }
    [Fact] public void NotMatchEquivalentOf_passes() { var s = "abc"; s.Should().NotMatchEquivalentOf("x*"); }
    [Fact] public void NotMatchEquivalentOf_fails() { var s = "abc"; Assert.Equal("Expected s not to match \"A*\" (ignoring case), but found \"abc\".", Message(() => s.Should().NotMatchEquivalentOf("A*"))); }
    [Fact] public void NotMatchEquivalentOf_fails_on_null() { string? s = null; Assert.Equal("Expected s not to match \"x*\" (ignoring case), but found null.", Message(() => s.Should().NotMatchEquivalentOf("x*"))); }
    [Fact] public void NotMatch_passes() { var s = "abc"; s.Should().NotMatch("x*"); }
    [Fact] public void NotMatch_is_case_sensitive() { var s = "abc"; s.Should().NotMatch("A*"); }
    [Fact] public void NotMatch_fails() { var s = "abc"; Assert.Equal("Expected s not to match \"a*\", but found \"abc\".", Message(() => s.Should().NotMatch("a*"))); }
    [Fact] public void NotMatch_fails_on_null() { string? s = null; Assert.Equal("Expected s not to match \"x*\", but found null.", Message(() => s.Should().NotMatch("x*"))); }
    [Fact] public void NotMatch_null_argument_throws() { var s = "a"; Assert.Equal("wildcardPattern", Assert.Throws<ArgumentNullException>(() => s.Should().NotMatch(null!)).ParamName); }

    [Fact] public void BeUpperCased_passes() { var s = "ABC 1"; s.Should().BeUpperCased(); }
    [Fact] public void BeUpperCased_fails() { var s = "Abc"; Assert.Equal("Expected s to be upper-cased, but found \"Abc\".", Message(() => s.Should().BeUpperCased())); }
    [Fact] public void BeUpperCased_fails_on_null() { string? s = null; Assert.Equal("Expected s to be upper-cased, but found null.", Message(() => s.Should().BeUpperCased())); }
    [Fact] public void NotBeUpperCased_passes() { var s = "Abc"; s.Should().NotBeUpperCased(); }
    [Fact] public void NotBeUpperCased_fails() { var s = "ABC"; Assert.Equal("Expected s not to be upper-cased, but found \"ABC\".", Message(() => s.Should().NotBeUpperCased())); }
    [Fact] public void NotBeUpperCased_fails_on_null() { string? s = null; Assert.Equal("Expected s not to be upper-cased, but found null.", Message(() => s.Should().NotBeUpperCased())); }
    [Fact] public void BeLowerCased_passes() { var s = "abc 1"; s.Should().BeLowerCased(); }
    [Fact] public void BeLowerCased_fails() { var s = "aBc"; Assert.Equal("Expected s to be lower-cased, but found \"aBc\".", Message(() => s.Should().BeLowerCased())); }
    [Fact] public void BeLowerCased_fails_on_null() { string? s = null; Assert.Equal("Expected s to be lower-cased, but found null.", Message(() => s.Should().BeLowerCased())); }
    [Fact] public void NotBeLowerCased_passes() { var s = "aBc"; s.Should().NotBeLowerCased(); }
    [Fact] public void NotBeLowerCased_fails() { var s = "abc"; Assert.Equal("Expected s not to be lower-cased, but found \"abc\".", Message(() => s.Should().NotBeLowerCased())); }
    [Fact] public void NotBeLowerCased_fails_on_null() { string? s = null; Assert.Equal("Expected s not to be lower-cased, but found null.", Message(() => s.Should().NotBeLowerCased())); }

    [Fact] public void BeNullOrWhiteSpace_passes() { string? n = null; n.Should().BeNullOrWhiteSpace(); var s = " \t"; s.Should().BeNullOrWhiteSpace(); }
    [Fact] public void BeNullOrWhiteSpace_fails() { var s = "a"; Assert.Equal("Expected s to be null or white space, but found \"a\".", Message(() => s.Should().BeNullOrWhiteSpace())); }

    [Fact] public void MatchRegex_regex_passes() { var s = "abc"; s.Should().MatchRegex(new Regex("^a")); }
    [Fact] public void MatchRegex_regex_fails() { var s = "abc"; Assert.Equal("Expected s to match regex \"^b\", but found \"abc\".", Message(() => s.Should().MatchRegex(new Regex("^b")))); }
    [Fact] public void MatchRegex_regex_fails_on_null() { string? s = null; Assert.Equal("Expected s to match regex \"a\", but found null.", Message(() => s.Should().MatchRegex(new Regex("a")))); }
    [Fact] public void MatchRegex_regex_null_argument_throws() { var s = "a"; Assert.Equal("regex", Assert.Throws<ArgumentNullException>(() => s.Should().MatchRegex((Regex)null!)).ParamName); }
    [Fact] public void MatchRegex_options_passes() { var s = "abc"; s.Should().MatchRegex("^ABC$", RegexOptions.IgnoreCase); }
    [Fact] public void MatchRegex_options_fails() { var s = "abc"; Assert.Equal("Expected s to match regex \"^ABC$\", but found \"abc\".", Message(() => s.Should().MatchRegex("^ABC$", RegexOptions.None))); }
    [Fact] public void MatchRegex_options_fails_on_null() { string? s = null; Assert.Equal("Expected s to match regex \"a\", but found null.", Message(() => s.Should().MatchRegex("a", RegexOptions.IgnoreCase))); }
    [Fact] public void MatchRegex_occurrence_passes() { var s = "a1b2"; s.Should().MatchRegex("[0-9]", Exactly.Twice()); s.Should().MatchRegex(new Regex("[a-z]"), AtLeast.Twice()); }
    [Fact] public void MatchRegex_occurrence_fails() { var s = "a1b2"; Assert.Equal("Expected s to match regex \"[0-9]\" exactly 1 time, but it matched 2 times in \"a1b2\".", Message(() => s.Should().MatchRegex("[0-9]", Exactly.Once()))); }
    [Fact] public void MatchRegex_regex_occurrence_fails() { var s = "a1b2"; Assert.Equal("Expected s to match regex \"[a-z]\" more than 2 times, but it matched 2 times in \"a1b2\".", Message(() => s.Should().MatchRegex(new Regex("[a-z]"), MoreThan.Twice()))); }
    [Fact] public void MatchRegex_occurrence_fails_on_null() { string? s = null; Assert.Equal("Expected s to match regex \"a\" exactly 1 time, but found null.", Message(() => s.Should().MatchRegex("a", Exactly.Once()))); }
    [Fact] public void MatchRegex_occurrence_takes_a_reason() { var s = "abc"; Assert.Equal("Expected s to match regex \"a\" exactly 2 times, but it matched 1 time in \"abc\". Because twice.", Message(() => s.Should().MatchRegex("a", Exactly.Twice(), "twice"))); }
    [Fact]
    public void MatchRegex_regex_without_a_timeout_still_times_out_on_a_hostile_pattern()
    {
        var s = new string('a', 60) + "!";
        var text = Message(() => s.Should().MatchRegex(new Regex("^(a+)+$")));
        Assert.Contains("the match timed out after 1 second.", text);
    }
    [Fact] public void MatchRegex_options_invalid_pattern_fails_with_a_message() { var s = "abc"; Assert.StartsWith("Expected s to match regex \"(\", but the pattern is not a valid regular expression: ", Message(() => s.Should().MatchRegex("(", RegexOptions.None))); }

    [Fact] public void NotMatchRegex_passes() { var s = "abc"; s.Should().NotMatchRegex("^b"); }
    [Fact] public void NotMatchRegex_fails() { var s = "abc"; Assert.Equal("Expected s not to match regex \"^a\", but found \"abc\".", Message(() => s.Should().NotMatchRegex("^a"))); }
    [Fact] public void NotMatchRegex_fails_on_null() { string? s = null; Assert.Equal("Expected s not to match regex \"a\", but found null.", Message(() => s.Should().NotMatchRegex("a"))); }
    [Fact] public void NotMatchRegex_null_argument_throws() { var s = "a"; Assert.Equal("pattern", Assert.Throws<ArgumentNullException>(() => s.Should().NotMatchRegex((string)null!)).ParamName); }
    [Fact] public void NotMatchRegex_options_passes() { var s = "abc"; s.Should().NotMatchRegex("^B", RegexOptions.None); }
    [Fact] public void NotMatchRegex_options_fails() { var s = "abc"; Assert.Equal("Expected s not to match regex \"^A\", but found \"abc\".", Message(() => s.Should().NotMatchRegex("^A", RegexOptions.IgnoreCase))); }
    [Fact] public void NotMatchRegex_options_fails_on_null() { string? s = null; Assert.Equal("Expected s not to match regex \"a\", but found null.", Message(() => s.Should().NotMatchRegex("a", RegexOptions.None))); }
    [Fact] public void NotMatchRegex_regex_passes() { var s = "abc"; s.Should().NotMatchRegex(new Regex("^b")); }
    [Fact] public void NotMatchRegex_regex_fails() { var s = "abc"; Assert.Equal("Expected s not to match regex \"^a\", but found \"abc\".", Message(() => s.Should().NotMatchRegex(new Regex("^a")))); }
    [Fact] public void NotMatchRegex_regex_fails_on_null() { string? s = null; Assert.Equal("Expected s not to match regex \"a\", but found null.", Message(() => s.Should().NotMatchRegex(new Regex("a")))); }

    [Fact] public void BeParsableInto_returns_the_parsed_value() { var s = "42"; Assert.Equal(42, s.Should().BeParsableInto<int>().Which); }
    [Fact]
    public void BeParsableInto_uses_the_invariant_culture()
    {
        var s = "1.5";
        var old = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try { Assert.Equal(1.5, s.Should().BeParsableInto<double>().Which); }
        finally { CultureInfo.CurrentCulture = old; }
    }
    [Fact] public void BeParsableInto_fails() { var s = "x"; Assert.Equal("Expected s to be parsable into Int32, but found \"x\".", Message(() => s.Should().BeParsableInto<int>())); }
    [Fact] public void BeParsableInto_fails_on_null() { string? s = null; Assert.Equal("Expected s to be parsable into Int32, but found null.", Message(() => s.Should().BeParsableInto<int>())); }
    [Fact] public void BeParsableInto_takes_a_reason() { var s = "x"; Assert.Equal("Expected s to be parsable into Int32, but found \"x\". Because number.", Message(() => s.Should().BeParsableInto<int>("number"))); }
    [Fact] public void BeParsableInto_inside_a_scope_returns_the_real_assertion() { var s = "x"; using var scope = new AssertionScope(); Assert.Equal(0, s.Should().BeParsableInto<int>().Which); }
}
