using Xunit;

namespace Vouch.Tests;

public class ReasonTests
{
    private enum Level { Low, High }

    private sealed class Thing
    {
    }

    private static string Msg(Action act) => Assert.Throws<VouchException>(act).Message;

    private static async Task<string> MsgAsync(Func<Task> act) => (await Assert.ThrowsAsync<VouchException>(act)).Message;

    private static void Pins(Action plain, Action reasoned)
    {
        var plainMessage = Msg(plain);
        var separator = plainMessage.Contains('\n') ? "\n" : " ";
        Assert.Equal(plainMessage + separator + "Because the setup says so.", Msg(reasoned));
    }

    private const string R = "the setup says so";

    [Fact]
    public void The_reason_follows_the_failure_on_its_own_sentence()
    {
        var n = 3;
        Assert.Equal("Expected n to be 5, but found 3. Because the setup says so.", Msg(() => n.Should().Be(5, R)));
    }

    [Fact]
    public void A_reason_that_ends_in_a_period_gets_no_second_one()
    {
        var n = 3;
        Assert.Equal("Expected n to be 5, but found 3. Because it is.", Msg(() => n.Should().Be(5, "it is.")));
    }

    [Fact]
    public void A_null_or_blank_reason_leaves_the_message_alone()
    {
        var n = 3;
        var plain = Msg(() => n.Should().Be(5));
        Assert.Equal(plain, Msg(() => n.Should().Be(5, null!)));
        Assert.Equal(plain, Msg(() => n.Should().Be(5, " ")));
    }

    [Fact]
    public void A_reason_after_a_multi_line_failure_starts_a_new_line()
    {
        var items = new List<int> { 1, 2 };
        var plain = Msg(() => items.Should().BeEmpty());
        Assert.Contains('\n', plain);
        Assert.Equal(plain + "\nBecause the setup says so.", Msg(() => items.Should().BeEmpty(R)));
    }

    [Fact]
    public void A_reason_does_not_hide_an_argument_error()
    {
        var items = new List<int> { 1 };
        Assert.Throws<ArgumentNullException>(() => items.Should().Equal((IEnumerable<int>)null!, R));
    }

    [Fact]
    public void A_passing_assertion_with_a_reason_does_not_throw()
    {
        5.Should().Be(5, R);
        "a".Should().Be("a", R);
        true.Should().BeTrue(R);
        new List<int> { 1 }.Should().HaveCount(1, R);
    }

    [Fact]
    public void Bool_reason_overloads()
    {
        var t = true;
        var f = false;
        t.Should().BeTrue(R);
        f.Should().BeFalse(R);
        t.Should().Be(true, R);
        t.Should().NotBe(false, R);
        Pins(() => f.Should().BeTrue(), () => f.Should().BeTrue(R));
        Pins(() => t.Should().BeFalse(), () => t.Should().BeFalse(R));
        Pins(() => t.Should().Be(false), () => t.Should().Be(false, R));
        Pins(() => t.Should().NotBe(true), () => t.Should().NotBe(true, R));
    }

    [Fact]
    public void Nullable_bool_reason_overloads()
    {
        bool? t = true;
        bool? f = false;
        bool? none = null;
        t.Should().BeTrue(R);
        f.Should().BeFalse(R);
        t.Should().Be(true, R);
        t.Should().NotBe(false, R);
        none.Should().BeNull(R);
        t.Should().NotBeNull(R);
        Pins(() => none.Should().BeTrue(), () => none.Should().BeTrue(R));
        Pins(() => none.Should().BeFalse(), () => none.Should().BeFalse(R));
        Pins(() => t.Should().Be(false), () => t.Should().Be(false, R));
        Pins(() => t.Should().NotBe(true), () => t.Should().NotBe(true, R));
        Pins(() => t.Should().BeNull(), () => t.Should().BeNull(R));
        Pins(() => none.Should().NotBeNull(), () => none.Should().NotBeNull(R));
    }

    [Fact]
    public void Object_reason_overloads()
    {
        object a = new Thing();
        object? none = null;
        object other = new Thing();
        a.Should().Be(a, R);
        a.Should().NotBe(other, R);
        none.Should().BeNull(R);
        a.Should().NotBeNull(R);
        a.Should().BeOfType<Thing>(R);
        a.Should().BeOfType(typeof(Thing), R);
        a.Should().BeOfType(typeof(Thing));
        a.Should().BeAssignableTo<object>(R);
        Pins(() => a.Should().Be(other), () => a.Should().Be(other, R));
        Pins(() => a.Should().NotBe(a), () => a.Should().NotBe(a, R));
        Pins(() => a.Should().BeNull(), () => a.Should().BeNull(R));
        Pins(() => none.Should().NotBeNull(), () => none.Should().NotBeNull(R));
        Pins(() => a.Should().BeOfType<string>(), () => a.Should().BeOfType<string>(R));
        Pins(() => a.Should().BeOfType(typeof(string)), () => a.Should().BeOfType(typeof(string), R));
        Pins(() => a.Should().BeAssignableTo<string>(), () => a.Should().BeAssignableTo<string>(R));
    }

    [Fact]
    public void Value_reason_overloads()
    {
        var level = Level.Low;
        level.Should().Be(Level.Low, R);
        level.Should().NotBe(Level.High, R);
        Pins(() => level.Should().Be(Level.High), () => level.Should().Be(Level.High, R));
        Pins(() => level.Should().NotBe(Level.Low), () => level.Should().NotBe(Level.Low, R));
        Level? maybe = Level.Low;
        maybe.Should().Be(Level.Low, R);
        Pins(() => maybe.Should().Be(Level.High), () => maybe.Should().Be(Level.High, R));
    }

    [Fact]
    public void Numeric_reason_overloads()
    {
        var n = 3;
        n.Should().Be(3, R);
        n.Should().NotBe(4, R);
        n.Should().BeGreaterThan(2, R);
        n.Should().BeGreaterThanOrEqualTo(3, R);
        n.Should().BeLessThan(4, R);
        n.Should().BeLessThanOrEqualTo(3, R);
        n.Should().BePositive(R);
        n.Should().BeInRange(1, 5, R);
        Pins(() => n.Should().Be(4), () => n.Should().Be(4, R));
        Pins(() => n.Should().NotBe(3), () => n.Should().NotBe(3, R));
        Pins(() => n.Should().BeGreaterThan(3), () => n.Should().BeGreaterThan(3, R));
        Pins(() => n.Should().BeGreaterThanOrEqualTo(4), () => n.Should().BeGreaterThanOrEqualTo(4, R));
        Pins(() => n.Should().BeLessThan(3), () => n.Should().BeLessThan(3, R));
        Pins(() => n.Should().BeLessThanOrEqualTo(2), () => n.Should().BeLessThanOrEqualTo(2, R));
        var zero = 0;
        Pins(() => zero.Should().BePositive(), () => zero.Should().BePositive(R));
        Pins(() => n.Should().BeInRange(4, 5), () => n.Should().BeInRange(4, 5, R));
    }

    [Fact]
    public void Nullable_numeric_reason_overloads()
    {
        int? n = 3;
        n.Should().Be(3, R);
        n.Should().BeGreaterThan(2, R);
        Pins(() => n.Should().Be(4), () => n.Should().Be(4, R));
        Pins(() => n.Should().BeGreaterThan(3), () => n.Should().BeGreaterThan(3, R));
    }

    [Fact]
    public void String_reason_overloads()
    {
        var s = "abc";
        var empty = "";
        string? none = null;
        s.Should().Be("abc", R);
        s.Should().NotBe("x", R);
        empty.Should().BeEmpty(R);
        s.Should().NotBeEmpty(R);
        none.Should().BeNull(R);
        s.Should().NotBeNull(R);
        none.Should().BeNullOrEmpty(R);
        s.Should().NotBeNullOrEmpty(R);
        s.Should().NotBeNullOrWhiteSpace(R);
        s.Should().StartWith("a", R);
        s.Should().NotStartWith("b", R);
        s.Should().EndWith("c", R);
        s.Should().Contain("b", R);
        s.Should().NotContain("x", R);
        s.Should().Match("a*", R);
        s.Should().MatchRegex("^a", R);
        s.Should().HaveLength(3, R);
        s.Should().BeOneOf(new[] { "abc", "x" }, R);
        s.Should().BeEquivalentTo("ABC", R);
        Pins(() => s.Should().Be("x"), () => s.Should().Be("x", R));
        Pins(() => s.Should().NotBe("abc"), () => s.Should().NotBe("abc", R));
        Pins(() => s.Should().BeEmpty(), () => s.Should().BeEmpty(R));
        Pins(() => empty.Should().NotBeEmpty(), () => empty.Should().NotBeEmpty(R));
        Pins(() => s.Should().BeNull(), () => s.Should().BeNull(R));
        Pins(() => none.Should().NotBeNull(), () => none.Should().NotBeNull(R));
        Pins(() => s.Should().BeNullOrEmpty(), () => s.Should().BeNullOrEmpty(R));
        Pins(() => empty.Should().NotBeNullOrEmpty(), () => empty.Should().NotBeNullOrEmpty(R));
        Pins(() => empty.Should().NotBeNullOrWhiteSpace(), () => empty.Should().NotBeNullOrWhiteSpace(R));
        Pins(() => s.Should().StartWith("b"), () => s.Should().StartWith("b", R));
        Pins(() => s.Should().NotStartWith("a"), () => s.Should().NotStartWith("a", R));
        Pins(() => s.Should().EndWith("b"), () => s.Should().EndWith("b", R));
        Pins(() => s.Should().Contain("x"), () => s.Should().Contain("x", R));
        Pins(() => s.Should().NotContain("b"), () => s.Should().NotContain("b", R));
        Pins(() => s.Should().Match("x*"), () => s.Should().Match("x*", R));
        Pins(() => s.Should().MatchRegex("^x"), () => s.Should().MatchRegex("^x", R));
        Pins(() => s.Should().HaveLength(2), () => s.Should().HaveLength(2, R));
        Pins(() => s.Should().BeOneOf("x", "y"), () => s.Should().BeOneOf(new[] { "x", "y" }, R));
        Pins(() => s.Should().BeEquivalentTo("x"), () => s.Should().BeEquivalentTo("x", R));
    }

    [Fact]
    public void String_BeOneOf_takes_a_collection_expression_with_a_reason()
    {
        var s = "b";
        s.Should().BeOneOf(["a", "b"], R);
        Pins(() => s.Should().BeOneOf("x", "y"), () => s.Should().BeOneOf(["x", "y"], R));
    }

    [Fact]
    public void Collection_reason_overloads()
    {
        var items = new List<int> { 1, 2, 3 };
        var none = new List<int>();
        List<int>? missing = null;
        none.Should().BeEmpty(R);
        items.Should().NotBeEmpty(R);
        missing.Should().BeNull(R);
        items.Should().NotBeNull(R);
        items.Should().HaveCount(3, R);
        items.Should().Contain(2, R);
        items.Should().Contain(new[] { 1, 2 }, R);
        items.Should().Contain(i => i > 2, R);
        items.Should().NotContain(9, R);
        items.Should().NotContain(new[] { 8, 9 }, R);
        items.Should().NotContain(i => i > 5, R);
        new List<int> { 1 }.Should().ContainSingle(R);
        items.Should().ContainSingle(i => i == 2, R);
        items.Should().OnlyContain(i => i > 0, R);
        items.Should().Equal(new[] { 1, 2, 3 }, R);
        items.Should().BeEquivalentTo(new[] { 3, 2, 1 }, R);
        Pins(() => items.Should().BeEmpty(), () => items.Should().BeEmpty(R));
        Pins(() => none.Should().NotBeEmpty(), () => none.Should().NotBeEmpty(R));
        Pins(() => items.Should().BeNull(), () => items.Should().BeNull(R));
        Pins(() => missing.Should().NotBeNull(), () => missing.Should().NotBeNull(R));
        Pins(() => items.Should().HaveCount(2), () => items.Should().HaveCount(2, R));
        Pins(() => items.Should().Contain(9), () => items.Should().Contain(9, R));
        Pins(() => items.Should().Contain(new[] { 9 }), () => items.Should().Contain(new[] { 9 }, R));
        Pins(() => items.Should().Contain(i => i > 5), () => items.Should().Contain(i => i > 5, R));
        Pins(() => items.Should().NotContain(2), () => items.Should().NotContain(2, R));
        Pins(() => items.Should().NotContain(new[] { 2 }), () => items.Should().NotContain(new[] { 2 }, R));
        Pins(() => items.Should().NotContain(i => i > 2), () => items.Should().NotContain(i => i > 2, R));
        Pins(() => items.Should().ContainSingle(), () => items.Should().ContainSingle(R));
        Pins(() => items.Should().ContainSingle(i => i > 0), () => items.Should().ContainSingle(i => i > 0, R));
        Pins(() => items.Should().OnlyContain(i => i > 1), () => items.Should().OnlyContain(i => i > 1, R));
        Pins(() => items.Should().Equal(1, 2), () => items.Should().Equal(new[] { 1, 2 }, R));
        Pins(() => items.Should().BeEquivalentTo(1, 2), () => items.Should().BeEquivalentTo(new[] { 1, 2 }, R));
    }

    [Fact]
    public void Dictionary_reason_overloads()
    {
        var map = new Dictionary<string, int> { ["a"] = 1 };
        map.Should().ContainKey("a", R);
        map.Should().NotContainKey("b", R);
        Pins(() => map.Should().ContainKey("b"), () => map.Should().ContainKey("b", R));
        Pins(() => map.Should().NotContainKey("a"), () => map.Should().NotContainKey("a", R));
    }

    [Fact]
    public async Task Exception_reason_overloads()
    {
        Action fine = () => { };
        Action boom = () => throw new InvalidOperationException("no");
        Func<Task> fineAsync = () => Task.CompletedTask;
        Func<Task> boomAsync = () => throw new InvalidOperationException("no");
        boom.Should().Throw<InvalidOperationException>(R);
        fine.Should().NotThrow(R);
        await boomAsync.Should().ThrowAsync<InvalidOperationException>(R);
        await fineAsync.Should().NotThrowAsync(R);
        Pins(() => fine.Should().Throw<InvalidOperationException>(), () => fine.Should().Throw<InvalidOperationException>(R));
        Pins(() => boom.Should().NotThrow(), () => boom.Should().NotThrow(R));
        var plain = await MsgAsync(() => fineAsync.Should().ThrowAsync<InvalidOperationException>());
        Assert.Equal(plain + " Because the setup says so.", await MsgAsync(() => fineAsync.Should().ThrowAsync<InvalidOperationException>(R)));
        var plainNot = await MsgAsync(() => boomAsync.Should().NotThrowAsync());
        Assert.Equal(plainNot + (plainNot.Contains('\n') ? "\n" : " ") + "Because the setup says so.", await MsgAsync(() => boomAsync.Should().NotThrowAsync(R)));
    }

    [Fact]
    public void Function_reason_overloads()
    {
        Func<int> fine = () => 1;
        Func<int> boom = () => throw new InvalidOperationException("no");
        boom.Should().Throw<InvalidOperationException>(R);
        fine.Should().NotThrow(R);
        Pins(() => fine.Should().Throw<InvalidOperationException>(), () => fine.Should().Throw<InvalidOperationException>(R));
        Pins(() => boom.Should().NotThrow(), () => boom.Should().NotThrow(R));
    }

    [Fact]
    public void An_interpolated_reason_appears_in_the_message()
    {
        var id = 42;
        var n = 3;
        var ex = Assert.Throws<VouchException>(() => n.Should().Be(5, $"order {id} is special"));
        Assert.EndsWith("Because order 42 is special.", ex.Message);
    }

    [Fact]
    public void A_second_string_on_a_string_collection_is_read_as_the_reason()
    {
        var xs = new List<string> { "a" };
        xs.Should().Contain("a", "b");
        xs.Should().NotContain("z", "b");
        var ex = Assert.Throws<VouchException>(() => xs.Should().Contain("z", "b"));
        Assert.EndsWith("Because b.", ex.Message);
        Assert.Throws<VouchException>(() => xs.Should().Contain(new[] { "a", "b" }));
    }
}
