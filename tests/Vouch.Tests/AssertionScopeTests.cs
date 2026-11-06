using Xunit;

namespace Vouch.Tests;

public class AssertionScopeTests
{
    [Fact]
    public void Two_failures_are_reported_together()
    {
        var first = 1;
        var second = "a";
        var ex = Assert.Throws<VouchException>(() =>
        {
            using var scope = new AssertionScope();
            first.Should().Be(2);
            second.Should().Be("b");
        });
        Assert.Contains("first", ex.Message);
        Assert.Contains("second", ex.Message);
        Assert.True(ex.Message.IndexOf("first", StringComparison.Ordinal) < ex.Message.IndexOf("second", StringComparison.Ordinal));
        Assert.Equal(2, ex.Message.Split("\n\n").Length);
    }

    [Fact]
    public void Nested_scopes_throw_once_from_the_outer()
    {
        var inner = 1;
        var outer = 2;
        var ex = Assert.Throws<VouchException>(() =>
        {
            using var o = new AssertionScope();
            using (var i = new AssertionScope())
            {
                inner.Should().Be(10);
            }

            outer.Should().Be(20);
        });
        Assert.Contains("inner", ex.Message);
        Assert.Contains("outer", ex.Message);
    }

    [Fact]
    public async Task Failure_after_await_is_collected()
    {
        var late = 1;
        var ex = await Assert.ThrowsAsync<VouchException>(async () =>
        {
            using var scope = new AssertionScope();
            await Task.Delay(10);
            late.Should().Be(2);
            await Task.Yield();
            late.Should().Be(3);
        });
        Assert.Equal(2, ex.Message.Split("\n\n").Length);
        Assert.Contains("late", ex.Message);
    }

    [Fact]
    public void Without_a_scope_the_first_failure_throws()
    {
        var n = 1;
        Assert.Throws<VouchException>(() => n.Should().Be(2));
    }

    [Fact]
    public void Scope_without_failure_throws_nothing()
    {
        var n = 1;
        using var scope = new AssertionScope();
        n.Should().Be(1);
    }

    [Fact]
    public void Discard_returns_messages_and_dispose_throws_nothing()
    {
        var n = 1;
        string[] messages;
        using (var scope = new AssertionScope())
        {
            n.Should().Be(2);
            messages = scope.Discard();
        }

        Assert.Single(messages);
        Assert.Contains("n", messages[0]);
    }

    [Fact]
    public async Task Scopes_on_two_threads_collect_only_their_own_failures()
    {
        using var barrier = new Barrier(2);

        Task<string> Run(string label) => Task.Run(() =>
        {
            try
            {
                using var scope = new AssertionScope();
                barrier.SignalAndWait();
                label.Should().Be("never");
                barrier.SignalAndWait();
                return "";
            }
            catch (VouchException e)
            {
                return e.Message;
            }
        });

        var a = Run("alpha");
        var b = Run("beta");
        var results = await Task.WhenAll(a, b);
        Assert.Contains("alpha", results[0]);
        Assert.DoesNotContain("beta", results[0]);
        Assert.Contains("beta", results[1]);
        Assert.DoesNotContain("alpha", results[1]);
    }
}
