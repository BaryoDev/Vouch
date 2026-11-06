using Xunit;

namespace Vouch.Tests;

public class ReturnTypeTests
{
    private static string Msg(Action act) => Assert.Throws<VouchException>(act).Message;

    [Fact]
    public void Function_NotThrow_returns_the_value_and_runs_once()
    {
        var calls = 0;
        Func<int> f = () => { calls++; return 42; };
        var result = f.Should().NotThrow();
        Assert.Equal(42, result.Which);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Function_NotThrow_because_returns_the_value_and_runs_once()
    {
        var calls = 0;
        Func<string> f = () => { calls++; return "x"; };
        Assert.Equal("x", f.Should().NotThrow("it is fine").Which);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Function_NotThrow_failure_adds_the_reason()
    {
        Func<int> f = () => throw new InvalidOperationException("boom");
        var message = Msg(() => f.Should().NotThrow("the setup says so"));
        Assert.StartsWith("Expected f not to throw, but it threw InvalidOperationException", message);
        Assert.EndsWith("Because the setup says so.", message);
    }

    [Fact]
    public void Function_NotThrow_failure_under_a_scope_gives_default()
    {
        var calls = 0;
        Func<int> f = () => ++calls == 1 ? 42 : throw new InvalidOperationException("boom");
        var assertions = f.Should();
        Assert.Equal(42, assertions.NotThrow().Which);
        var which = -1;
        Assert.Throws<VouchException>(() =>
        {
            using var scope = new AssertionScope();
            which = assertions.NotThrow().Which;
        });
        Assert.Equal(0, which);
    }

    [Fact]
    public void Contain_predicate_returns_the_first_match()
    {
        var items = new[] { 1, 2, 3, 4 };
        Assert.Equal(2, items.Should().Contain(x => x % 2 == 0).Which);
    }

    [Fact]
    public void Contain_predicate_fails_with_message()
    {
        var items = new[] { 1, 3 };
        Assert.Equal("Expected items to contain an item matching (x > 5), but it did not. It has:\n  [0] 1\n  [1] 3",
            Msg(() => items.Should().Contain(x => x > 5)));
    }

    [Fact]
    public void HaveElementAt_returns_the_element()
    {
        var items = new[] { "a", "b", "c" };
        Assert.Equal("b", items.Should().HaveElementAt(1, "b").Which);
    }

    [Fact]
    public void HaveElementAt_fails_with_message()
    {
        var items = new[] { 1, 2 };
        Assert.Equal("Expected items to have element 9 at index 1, but found 2.", Msg(() => items.Should().HaveElementAt(1, 9)));
    }

    [Fact]
    public void HaveElementAt_fails_when_index_is_out_of_range()
    {
        var items = new[] { 1, 2 };
        Assert.Equal("Expected items to have element 9 at index 5, but found no element at that index. It has:\n  [0] 1\n  [1] 2",
            Msg(() => items.Should().HaveElementAt(5, 9)));
    }

    [Fact]
    public void HaveElementAt_because_adds_the_reason()
    {
        var items = new[] { 1, 2 };
        Assert.Equal("Expected items to have element 9 at index 1, but found 2. Because it matters.", Msg(() => items.Should().HaveElementAt(1, 9, "it matters")));
    }
}
