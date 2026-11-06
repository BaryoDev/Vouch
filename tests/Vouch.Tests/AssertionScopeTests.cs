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
            await Task.Yield();
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
        Assert.Contains("Expected n to be", messages[0]);
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
                label.Should().Be("again");
                barrier.SignalAndWait();
                return "";
            }
            catch (VouchException e)
            {
                return e.Message;
            }
        });

        var results = await Task.WhenAll(Run("alpha"), Run("beta"));
        Assert.Equal(2, results[0].Split("\n\n").Length);
        Assert.Equal(2, results[1].Split("\n\n").Length);
        Assert.Contains("alpha", results[0]);
        Assert.DoesNotContain("beta", results[0]);
        Assert.Contains("beta", results[1]);
        Assert.DoesNotContain("alpha", results[1]);
    }

    [Fact]
    public void Out_of_order_dispose_leaves_no_ambient_scope()
    {
        var a = 1;
        var outer = new AssertionScope();
        var inner = new AssertionScope();
        outer.Dispose();
        inner.Dispose();
        Assert.Throws<VouchException>(() => a.Should().Be(2));
    }

    [Fact]
    public void Double_dispose_throws_nothing_the_second_time()
    {
        var a = 1;
        var scope = new AssertionScope();
        a.Should().Be(2);
        Assert.Throws<VouchException>(() => scope.Dispose());
        scope.Dispose();
    }

    [Fact]
    public void AllSatisfy_inside_a_scope_is_collected_by_the_scope()
    {
        var items = new[] { 1, 2 };
        var ex = Assert.Throws<VouchException>(() =>
        {
            using var scope = new AssertionScope();
            items.Should().AllSatisfy(x => x.Should().Be(1));
        });
        Assert.Contains("did not", ex.Message);
        Assert.Contains("[1]", ex.Message);
    }

    [Fact]
    public void Scope_inside_AllSatisfy_callback_inside_outer_scope_stays_with_AllSatisfy()
    {
        var items = new[] { 1, 2 };
        var ex = Assert.Throws<VouchException>(() =>
        {
            using var outer = new AssertionScope();
            items.Should().AllSatisfy(x =>
            {
                using var inner = new AssertionScope();
                x.Should().Be(1);
            });
        });
        Assert.Contains("did not", ex.Message);
        Assert.Contains("[1]", ex.Message);
    }

    [Fact]
    public void Throw_on_action_that_throws_nothing_collects_one_failure()
    {
        Action fine = () => { };
        var ex = Assert.Throws<VouchException>(() =>
        {
            using var scope = new AssertionScope();
            fine.Should().Throw<InvalidOperationException>();
        });
        Assert.Single(ex.Message.Split("\n\n"));
    }

    [Fact]
    public void Which_with_two_matches_and_no_scope_throws_on_every_access()
    {
        var assertions = new ExceptionAssertions<Exception>(new Exception[] { new("a"), new("b") }, "act");
        Assert.Throws<VouchException>(() => assertions.Which);
        Assert.Throws<VouchException>(() => assertions.Which);
    }

    [Fact]
    public void Recording_after_the_scope_is_disposed_throws_directly()
    {
        var scope = new AssertionScope();
        var recorder = Fail.Recorder!;
        scope.Dispose();
        Assert.Throws<VouchException>(() => recorder(new VouchException("late")));
    }
}
