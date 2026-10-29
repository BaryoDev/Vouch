using Xunit;

namespace Vouch.Tests;

public class ExceptionAssertionsTests
{
    private sealed class NoStackException() : Exception("m")
    {
        public override string? StackTrace => null;
    }

    private static string Msg(Action a) => Assert.Throws<VouchException>(a).Message;

    private static async Task<string> MsgAsync(Func<Task> a) => (await Assert.ThrowsAsync<VouchException>(a)).Message;

    [Fact]
    public void Throw_passes()
    {
        Action act = () => throw new InvalidOperationException("x");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Throw_accepts_derived()
    {
        Action act = () => throw new ArgumentNullException("p");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Throw_fails_when_nothing_is_thrown_with_exact_message()
    {
        Action act = () => { };
        Assert.Equal("Expected act to throw InvalidOperationException, but it did not throw.",
            Msg(() => act.Should().Throw<InvalidOperationException>()));
    }

    [Fact]
    public void Throw_fails_on_wrong_type_with_exact_message()
    {
        Action act = () => throw new ArgumentException("Name is required.");
        Assert.Equal("Expected act to throw InvalidOperationException, but it threw ArgumentException:\n  \"Name is required.\"",
            Msg(() => act.Should().Throw<InvalidOperationException>()));
    }

    [Fact]
    public void Throw_fails_on_null_action_with_exact_message()
    {
        Action? act = null;
        Assert.Equal("Expected act to throw Exception, but found null.", Msg(() => act.Should().Throw<Exception>()));
    }

    [Fact]
    public void Throw_does_not_mistake_an_VouchException_for_the_expected_type()
    {
        Action act = () => throw new VouchException("inner");
        Assert.Equal("Expected act to throw InvalidOperationException, but it threw VouchException:\n  \"inner\"",
            Msg(() => act.Should().Throw<InvalidOperationException>()));
    }

    [Fact]
    public void Throw_can_expect_an_VouchException()
    {
        Action act = () => throw new VouchException("inner");
        act.Should().Throw<VouchException>().WithMessage("inner");
    }

    [Fact]
    public void Throw_unwraps_an_AggregateException_holding_the_expected_type()
    {
        var inner = new InvalidOperationException("x");
        Action act = () => throw new AggregateException(inner);
        Assert.Same(inner, act.Should().Throw<InvalidOperationException>().Which);
    }

    [Fact]
    public void Throw_unwraps_nested_AggregateExceptions()
    {
        var inner = new InvalidOperationException("x");
        Action act = () => throw new AggregateException(new AggregateException(inner));
        Assert.Same(inner, act.Should().Throw<InvalidOperationException>().Which);
    }

    [Fact]
    public void Throw_finds_the_expected_type_among_several_inner_exceptions()
    {
        var inner = new InvalidOperationException("y");
        Action act = () => throw new AggregateException(new FormatException("x"), inner);
        Assert.Same(inner, act.Should().Throw<InvalidOperationException>().Which);
    }

    [Fact]
    public void Throw_of_AggregateException_returns_the_aggregate()
    {
        var aggregate = new AggregateException(new InvalidOperationException("x"));
        Action act = () => throw aggregate;
        Assert.Same(aggregate, act.Should().Throw<AggregateException>().Which);
    }

    [Fact]
    public void Throw_fails_when_no_inner_exception_has_the_expected_type()
    {
        Action act = () => throw new AggregateException(new FormatException("x"));
        Assert.StartsWith("Expected act to throw InvalidOperationException, but it threw AggregateException:",
            Msg(() => act.Should().Throw<InvalidOperationException>()));
    }

    [Fact]
    public void Throw_of_Exception_fails_on_an_empty_AggregateException()
    {
        Action act = () => throw new AggregateException();
        Assert.StartsWith("Expected act to throw Exception, but it threw AggregateException:", Msg(() => act.Should().Throw<Exception>()));
        act.Should().Throw<AggregateException>();
    }

    [Fact]
    public void WithMessage_passes_when_any_matching_inner_exception_matches()
    {
        Action act = () => throw new AggregateException(new InvalidOperationException("x"), new ArgumentException("inner msg"));
        act.Should().Throw<Exception>().WithMessage("inner*");
        act.Should().Throw<Exception>().WithMessage("x");
        Assert.Equal("Expected act's exception message to match \"zzz\", but found \"x | inner msg\".", Msg(() => act.Should().Throw<Exception>().WithMessage("zzz")));
    }

    [Fact]
    public void Which_fails_when_several_inner_exceptions_match()
    {
        Action act = () => throw new AggregateException(new ArgumentException("a"), new ArgumentException("b"));
        var assertions = act.Should().Throw<ArgumentException>();
        assertions.WithMessage("b");
        Assert.Equal("More than one exception was thrown. Vouch cannot tell which one was meant.", Assert.Throws<VouchException>(() => assertions.Which).Message);
    }

    [Fact]
    public void Throw_of_Exception_looks_at_the_inner_exceptions_not_the_aggregate()
    {
        Action act = () => throw new AggregateException("outer", new ArgumentException("inner"));
        Assert.NotNull(Msg(() => act.Should().Throw<Exception>().WithMessage("outer*")));
        act.Should().Throw<AggregateException>().WithMessage("outer*");
    }

    [Fact]
    public void Throw_fails_on_an_empty_AggregateException()
    {
        Action act = () => throw new AggregateException();
        Assert.StartsWith("Expected act to throw InvalidOperationException, but it threw AggregateException:",
            Msg(() => act.Should().Throw<InvalidOperationException>()));
    }

    [Fact]
    public void WithMessage_after_an_unwrapped_Throw_reads_the_inner_message()
    {
        Action act = () => throw new AggregateException("outer", new InvalidOperationException("inner"));
        act.Should().Throw<InvalidOperationException>().WithMessage("inner");
    }

    [Fact]
    public void Which_is_the_exception()
    {
        Action act = () => throw new ArgumentException("m", "p");
        Assert.Equal("p", act.Should().Throw<ArgumentException>().Which.ParamName);
    }

    [Fact]
    public void Subject_and_And_return_the_exception_assertions()
    {
        Action act = () => throw new ArgumentException("m");
        var result = act.Should().Throw<ArgumentException>();
        Assert.Same(result.Which, result.Subject);
        Assert.Same(result, result.And);
    }

    [Fact]
    public void Func_Throw_passes()
    {
        Func<int> act = () => throw new InvalidOperationException();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Func_Throw_fails_with_exact_message()
    {
        Func<int> act = () => 1;
        Assert.Equal("Expected act to throw InvalidOperationException, but it did not throw.",
            Msg(() => act.Should().Throw<InvalidOperationException>()));
    }

    [Fact]
    public void Func_Throw_fails_on_null()
    {
        Func<int>? act = null;
        Assert.Equal("Expected act to throw Exception, but found null.", Msg(() => act.Should().Throw<Exception>()));
    }

    [Fact]
    public void Func_NotThrow_passes_and_fails()
    {
        Func<int> ok = () => 1;
        ok.Should().NotThrow();
        Func<int> bad = () => throw new ArgumentException("boom");
        Assert.StartsWith("Expected bad not to throw, but it threw ArgumentException:\n  \"boom\"", Msg(() => bad.Should().NotThrow()));
    }

    [Fact]
    public void NotThrow_passes()
    {
        Action act = () => { };
        act.Should().NotThrow().And.NotThrow();
    }

    [Fact]
    public void NotThrow_fails_with_exact_message()
    {
        Action act = () => throw new ArgumentException("Name is required.");
        var message = Msg(() => act.Should().NotThrow());
        Assert.StartsWith("Expected act not to throw, but it threw ArgumentException:\n  \"Name is required.\"\n  at ", message);
        Assert.Equal(3, message.Split('\n').Length);
    }

    [Fact]
    public async Task NotThrowAsync_omits_the_stack_line_when_the_exception_has_none()
    {
        Func<Task> act = () => throw new NoStackException();
        Assert.Equal("Expected act not to throw, but it threw NoStackException:\n  \"m\"",
            await MsgAsync(() => act.Should().NotThrowAsync()));
    }

    [Fact]
    public void NotThrow_fails_on_null_action()
    {
        Action? act = null;
        Assert.Equal("Expected act not to throw, but found null.", Msg(() => act.Should().NotThrow()));
    }

    [Fact]
    public void WithMessage_passes_with_wildcards()
    {
        Action act = () => throw new InvalidOperationException("Name is required.");
        act.Should().Throw<InvalidOperationException>().WithMessage("Name * required.");
    }

    [Theory]
    [InlineData("Name is required.", "name*")]
    [InlineData("NAME IS REQUIRED.", "name is required.")]
    [InlineData("a\r\nb", "a\nb")]
    [InlineData("a\nb", "a\r\nb")]
    [InlineData("abc\n", "abc")]
    [InlineData("abc", "abc\n")]
    [InlineData("a\n\nb", "a\nb")]
    [InlineData("a\nb", "a*b")]
    public void WithMessage_ignores_case_and_newline_differences(string message, string pattern)
    {
        Action act = () => throw new InvalidOperationException(message);
        act.Should().Throw<InvalidOperationException>().WithMessage(pattern);
    }

    [Theory]
    [InlineData("a\nb", "a?b")]
    [InlineData("abc ", "abc")]
    [InlineData(" abc", "abc")]
    [InlineData("a b", "a  b")]
    public void WithMessage_still_fails_on_other_differences(string message, string pattern)
    {
        Action act = () => throw new InvalidOperationException(message);
        Assert.NotNull(Msg(() => act.Should().Throw<InvalidOperationException>().WithMessage(pattern)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void WithMessage_empty_pattern_throws_ArgumentException(string message)
    {
        Action act = () => throw new InvalidOperationException(message);
        var assertions = act.Should().Throw<InvalidOperationException>();
        var ex = Assert.Throws<ArgumentException>(() => assertions.WithMessage(""));
        Assert.Equal("pattern", ex.ParamName);
        Assert.StartsWith("Cannot match string against an empty string. Provide a wildcard pattern or use the BeEmpty method.", ex.Message);
    }

    [Fact]
    public async Task WithMessage_on_the_task_ignores_case()
    {
        Func<Task> act = () => throw new InvalidOperationException("Name is required.");
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NAME*");
    }

    [Fact]
    public void WithMessage_fails_with_exact_message()
    {
        Action act = () => throw new InvalidOperationException("Value is missing.");
        Assert.Equal("Expected act's exception message to match \"Name * required.\", but found \"Value is missing.\".",
            Msg(() => act.Should().Throw<InvalidOperationException>().WithMessage("Name * required.")));
    }

    [Fact]
    public void WithMessage_null_pattern_throws_ArgumentNullException()
    {
        Action act = () => throw new InvalidOperationException("x");
        var assertions = act.Should().Throw<InvalidOperationException>();
        Assert.Throws<ArgumentNullException>(() => assertions.WithMessage(null!));
    }

    [Fact]
    public async Task ThrowAsync_passes()
    {
        Func<Task> act = async () => { await Task.Yield(); throw new InvalidOperationException(); };
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ThrowAsync_then_WithMessage_on_the_task()
    {
        Func<Task> act = () => throw new InvalidOperationException("Name is required.");
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Name*required.");
    }

    [Fact]
    public async Task ThrowAsync_awaited_then_WithMessage()
    {
        Func<Task> act = async () => { await Task.Yield(); throw new ArgumentException("x"); };
        (await act.Should().ThrowAsync<ArgumentException>()).WithMessage("x");
    }

    [Fact]
    public async Task WithMessage_on_the_task_fails_with_exact_message()
    {
        Func<Task> act = () => Task.FromException(new InvalidOperationException("Value is missing."));
        Assert.Equal("Expected act's exception message to match \"Name * required.\", but found \"Value is missing.\".",
            await MsgAsync(() => act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Name * required.")));
    }

    [Fact]
    public async Task WithMessage_on_the_task_propagates_the_throw_failure()
    {
        Func<Task> act = () => Task.CompletedTask;
        Assert.Equal("Expected act to throw InvalidOperationException, but it did not throw.",
            await MsgAsync(() => act.Should().ThrowAsync<InvalidOperationException>().WithMessage("x")));
    }

    [Fact]
    public async Task WithMessage_on_the_task_null_pattern_throws_ArgumentNullException()
    {
        Func<Task> act = () => throw new InvalidOperationException("x");
        await Assert.ThrowsAsync<ArgumentNullException>(() => act.Should().ThrowAsync<InvalidOperationException>().WithMessage(null!));
    }

    [Fact]
    public async Task ThrowAsync_accepts_derived()
    {
        Func<Task> act = () => Task.FromException(new ArgumentNullException("p"));
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Sync_throw_counts_for_ThrowAsync()
    {
        Func<Task> act = () => throw new InvalidOperationException();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ThrowAsync_fails_when_nothing_is_thrown()
    {
        Func<Task> act = () => Task.CompletedTask;
        Assert.Equal("Expected act to throw InvalidOperationException, but it did not throw.",
            await MsgAsync(() => act.Should().ThrowAsync<InvalidOperationException>()));
    }

    [Fact]
    public async Task Null_task_fails_with_message()
    {
        Func<Task> act = () => null!;
        Assert.Equal("Expected act to throw Exception, but it returned a null Task.",
            await MsgAsync(() => act.Should().ThrowAsync<Exception>()));
    }

    [Fact]
    public async Task Null_delegate_fails_with_message()
    {
        Func<Task>? act = null;
        Assert.Equal("Expected act to throw Exception, but found null.",
            await MsgAsync(() => act.Should().ThrowAsync<Exception>()));
    }

    [Fact]
    public async Task Func_of_Task_of_T_works()
    {
        Func<Task<int>> act = () => throw new InvalidOperationException();
        await act.Should().ThrowAsync<InvalidOperationException>();
        Func<Task<int>> async = async () => { await Task.Yield(); throw new InvalidOperationException(); };
        await async.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Func_of_Task_of_T_null_task_and_null_delegate_fail()
    {
        Func<Task<int>> act = () => null!;
        Assert.Equal("Expected act to throw Exception, but it returned a null Task.",
            await MsgAsync(() => act.Should().ThrowAsync<Exception>()));
        Func<Task<int>>? none = null;
        Assert.Equal("Expected none to throw Exception, but found null.",
            await MsgAsync(() => none.Should().ThrowAsync<Exception>()));
    }

    [Fact]
    public async Task Wrong_type_message()
    {
        Func<Task> act = () => throw new ArgumentException("Name is required.");
        Assert.Equal("Expected act to throw InvalidOperationException, but it threw ArgumentException:\n  \"Name is required.\"",
            await MsgAsync(() => act.Should().ThrowAsync<InvalidOperationException>()));
    }

    [Fact]
    public async Task Async_VouchException_is_not_mistaken_for_the_expected_type()
    {
        Func<Task> act = async () => { await Task.Yield(); throw new VouchException("inner"); };
        Assert.Equal("Expected act to throw InvalidOperationException, but it threw VouchException:\n  \"inner\"",
            await MsgAsync(() => act.Should().ThrowAsync<InvalidOperationException>()));
    }

    [Fact]
    public async Task Canceled_task_counts_as_thrown()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        Func<Task> act = () => Task.FromCanceled(cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ThrowAsync_unwraps_an_AggregateException_holding_the_expected_type()
    {
        var inner = new InvalidOperationException("x");
        Func<Task> act = () => throw new AggregateException(inner);
        Assert.Same(inner, (await act.Should().ThrowAsync<InvalidOperationException>()).Which);
    }

    [Fact]
    public async Task ThrowAsync_fails_when_no_inner_exception_has_the_expected_type()
    {
        Func<Task> act = () => throw new AggregateException(new FormatException("x"));
        Assert.StartsWith("Expected act to throw InvalidOperationException, but it threw AggregateException:",
            await MsgAsync(() => act.Should().ThrowAsync<InvalidOperationException>()));
    }

    private sealed class PumpContext : SynchronizationContext
    {
        private readonly System.Collections.Concurrent.BlockingCollection<(SendOrPostCallback Callback, object? State)> _queue = new();
        private int _posts;

        public int Posts => _posts;

        public override void Post(SendOrPostCallback d, object? state)
        {
            Interlocked.Increment(ref _posts);
            _queue.Add((d, state));
        }

        public Task RunOnPumpThread(Func<Task> body, TimeSpan timeout)
        {
            Task? result = null;
            var thread = new Thread(() =>
            {
                SetSynchronizationContext(this);
                result = body();
                while (!result.IsCompleted)
                {
                    if (_queue.TryTake(out var work, 20))
                    {
                        work.Callback(work.State);
                    }
                }
            })
            { IsBackground = true };
            thread.Start();
            Assert.True(thread.Join(timeout), "the assertion blocked the single-threaded context");
            return result!;
        }
    }

    [Fact]
    public async Task ThrowAsync_neither_blocks_nor_captures_a_single_threaded_context()
    {
        var context = new PumpContext();
        Func<Task> act = async () => { await Task.Delay(50).ConfigureAwait(false); throw new InvalidOperationException(); };
        var task = context.RunOnPumpThread(() => act.Should().ThrowAsync<InvalidOperationException>(), TimeSpan.FromSeconds(5));
        await task;
        Assert.Equal(0, context.Posts);
    }

    [Fact]
    public async Task NotThrowAsync_neither_blocks_nor_captures_a_single_threaded_context()
    {
        var context = new PumpContext();
        Func<Task> act = async () => await Task.Delay(50).ConfigureAwait(false);
        await context.RunOnPumpThread(() => act.Should().NotThrowAsync(), TimeSpan.FromSeconds(5));
        Assert.Equal(0, context.Posts);
    }

    [Fact]
    public async Task ThrowAsync_does_not_block_when_the_subject_resumes_on_the_single_threaded_context()
    {
        var context = new PumpContext();
        Func<Task> act = async () => { await Task.Delay(50); throw new InvalidOperationException(); };
        await context.RunOnPumpThread(() => act.Should().ThrowAsync<InvalidOperationException>(), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task NotThrowAsync_does_not_block_when_the_subject_resumes_on_the_single_threaded_context()
    {
        var context = new PumpContext();
        Func<Task> act = async () => await Task.Delay(50);
        await context.RunOnPumpThread(() => act.Should().NotThrowAsync(), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task NotThrowAsync_fails_on_a_canceled_task_with_exact_message()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        Func<Task> act = () => Task.FromCanceled(cts.Token);
        var message = await MsgAsync(() => act.Should().NotThrowAsync());
        Assert.StartsWith("Expected act not to throw, but it threw TaskCanceledException:\n  \"A task was canceled.\"", message);
    }

    [Fact]
    public async Task ThrowAsync_accepts_a_derived_TaskCanceledException()
    {
        Func<Task> act = () => Task.FromException(new TaskCanceledException("stopped"));
        var result = await act.Should().ThrowAsync<OperationCanceledException>();
        Assert.IsType<TaskCanceledException>(result.Which);
    }

    [Fact]
    public async Task ThrowAsync_fails_when_a_base_OperationCanceledException_is_thrown_for_a_derived_expectation()
    {
        Func<Task> act = () => throw new OperationCanceledException("stopped");
        Assert.Equal("Expected act to throw TaskCanceledException, but it threw OperationCanceledException:\n  \"stopped\"",
            await MsgAsync(() => act.Should().ThrowAsync<TaskCanceledException>()));
    }

    [Fact]
    public void WithMessage_on_the_task_checks_arguments_before_returning_a_task()
    {
        Func<Task> act = () => throw new InvalidOperationException("x");
        var pending = act.Should().ThrowAsync<InvalidOperationException>();
        Assert.Throws<ArgumentNullException>(() => { _ = pending.WithMessage(null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = ExceptionTaskExtensions.WithMessage<InvalidOperationException>(null!, "x"); });
    }

    [Fact]
    public async Task NotThrowAsync_passes()
    {
        Func<Task> act = async () => await Task.Yield();
        await act.Should().NotThrowAsync();
        Func<Task<int>> typed = () => Task.FromResult(1);
        await typed.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NotThrowAsync_fails_with_exact_message()
    {
        Func<Task> act = () => Task.FromException(new ArgumentException("Name is required."));
        var message = await MsgAsync(() => act.Should().NotThrowAsync());
        Assert.StartsWith("Expected act not to throw, but it threw ArgumentException:\n  \"Name is required.\"", message);
    }

    [Fact]
    public async Task NotThrowAsync_fails_on_sync_throw()
    {
        Func<Task> act = () => throw new ArgumentException("m");
        Assert.StartsWith("Expected act not to throw, but it threw ArgumentException:\n  \"m\"", await MsgAsync(() => act.Should().NotThrowAsync()));
    }

    [Fact]
    public async Task NotThrowAsync_fails_on_null_delegate_and_null_task()
    {
        Func<Task>? none = null;
        Assert.Equal("Expected none not to throw, but found null.", await MsgAsync(() => none.Should().NotThrowAsync()));
        Func<Task> act = () => null!;
        Assert.Equal("Expected act not to throw, but it returned a null Task.", await MsgAsync(() => act.Should().NotThrowAsync()));
    }

    private sealed class Service
    {
        public int Value() => 7;

        public void Run(bool fail) { if (fail) throw new InvalidOperationException("run"); }

        public Task RunAsync(bool fail) => fail ? Task.FromException(new InvalidOperationException("run")) : Task.CompletedTask;

        public Task<int> ComputeAsync(bool fail) => fail ? Task.FromException<int>(new InvalidOperationException("run")) : Task.FromResult(7);
    }

    [Fact]
    public void WithParameterName_passes_on_the_name()
    {
        Action act = () => throw new ArgumentException("bad", "reason");
        act.Should().Throw<ArgumentException>().WithParameterName("reason");
    }

    [Fact]
    public void WithParameterName_fails_on_another_name()
    {
        Action act = () => throw new ArgumentException("bad", "reason");
        var thrown = act.Should().Throw<ArgumentException>();
        Assert.Equal("Expected act's exception parameter name to be \"other\", but found \"reason\".", Msg(() => thrown.WithParameterName("other")));
    }

    [Fact]
    public void WithParameterName_fails_when_there_is_no_name()
    {
        Action act = () => throw new ArgumentException("bad");
        var thrown = act.Should().Throw<ArgumentException>();
        Assert.Equal("Expected act's exception parameter name to be \"other\", but found null.", Msg(() => thrown.WithParameterName("other")));
    }

    [Fact]
    public void WithParameterName_accepts_a_derived_exception()
    {
        Action act = () => throw new ArgumentNullException("item");
        act.Should().Throw<ArgumentNullException>().WithParameterName("item");
    }

    [Fact]
    public void WithParameterName_null_argument_throws_ArgumentNullException()
    {
        Action act = () => throw new ArgumentException("bad", "reason");
        var thrown = act.Should().Throw<ArgumentException>();
        Assert.Throws<ArgumentNullException>(() => thrown.WithParameterName(null!));
    }

    [Fact]
    public async Task WithParameterName_chains_after_an_async_WithMessage()
    {
        Func<Task> act = () => Task.FromException(new ArgumentException("bad input", "reason"));
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("bad input*").WithParameterName("reason");
    }

    [Fact]
    public async Task WithParameterName_on_a_task_fails_on_another_name()
    {
        Func<Task> act = () => Task.FromException(new ArgumentException("bad", "reason"));
        Assert.Equal("Expected act's exception parameter name to be \"other\", but found \"reason\".", await MsgAsync(() => act.Should().ThrowAsync<ArgumentException>().WithParameterName("other")));
    }

    [Fact]
    public async Task WithParameterName_on_a_task_null_argument_throws_ArgumentNullException()
    {
        Func<Task> act = () => Task.FromException(new ArgumentException("bad", "reason"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => act.Should().ThrowAsync<ArgumentException>().WithParameterName(null!));
    }

    [Fact]
    public void Invoking_wraps_a_void_call()
    {
        var service = new Service();
        service.Invoking(s => s.Run(true)).Should().Throw<InvalidOperationException>().WithMessage("run");
        service.Invoking(s => s.Run(false)).Should().NotThrow();
    }

    [Fact]
    public void Invoking_wraps_a_call_with_a_result()
    {
        var service = new Service();
        service.Invoking(s => s.Value()).Should().NotThrow();
        Assert.Throws<VouchException>(() => service.Invoking(s => s.Value()).Should().Throw<InvalidOperationException>());
    }

    [Fact]
    public async Task Invoking_wraps_an_async_call()
    {
        var service = new Service();
        await service.Invoking(s => s.RunAsync(true)).Should().ThrowAsync<InvalidOperationException>().WithMessage("run");
        await service.Invoking(s => s.RunAsync(false)).Should().NotThrowAsync();
        await service.Invoking(s => s.ComputeAsync(true)).Should().ThrowAsync<InvalidOperationException>();
        await service.Invoking(s => s.ComputeAsync(false)).Should().NotThrowAsync();
    }

    [Fact]
    public void Invoking_runs_nothing_until_asserted()
    {
        var calls = 0;
        var action = this.Invoking(_ => calls++);
        Assert.Equal(0, calls);
        action.Should().NotThrow();
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Invoking_failure_names_the_expression()
    {
        var service = new Service();
        var act = service.Invoking(s => s.Run(true));
        Assert.StartsWith("Expected act not to throw, but it threw InvalidOperationException", Msg(() => act.Should().NotThrow()));
    }

    [Fact]
    public void Invoking_null_action_throws_ArgumentNullException()
    {
        var service = new Service();
        Assert.Throws<ArgumentNullException>(() => service.Invoking((Action<Service>)null!));
        Assert.Throws<ArgumentNullException>(() => service.Invoking((Func<Service, int>)null!));
    }

    [Fact]
    public void FluentActions_Invoking_wraps_an_action_and_a_function()
    {
        FluentActions.Invoking(() => new Service().Run(true)).Should().Throw<InvalidOperationException>();
        FluentActions.Invoking(() => new Service().Run(false)).Should().NotThrow();
        FluentActions.Invoking(() => new Service().Value()).Should().NotThrow();
    }

    [Fact]
    public async Task FluentActions_Awaiting_wraps_a_task_and_a_task_with_a_result()
    {
        await FluentActions.Awaiting(() => new Service().RunAsync(true)).Should().ThrowAsync<InvalidOperationException>();
        await FluentActions.Awaiting(() => new Service().RunAsync(false)).Should().NotThrowAsync();
        await FluentActions.Awaiting(() => new Service().ComputeAsync(true)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void FluentActions_null_arguments_throw_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => FluentActions.Invoking((Action)null!));
        Assert.Throws<ArgumentNullException>(() => FluentActions.Invoking((Func<int>)null!));
        Assert.Throws<ArgumentNullException>(() => FluentActions.Awaiting((Func<Task>)null!));
        Assert.Throws<ArgumentNullException>(() => FluentActions.Awaiting((Func<Task<int>>)null!));
    }

    [Fact]
    public async Task NotThrowAsync_on_a_typed_task_returns_the_result()
    {
        Func<Task<string>> act = () => Task.FromResult("done");
        var result = await act.Should().NotThrowAsync();
        Assert.Equal("done", result.Subject);
        Assert.Equal("done", result.Which);
    }

    [Fact]
    public async Task NotThrowAsync_on_a_typed_task_fails_when_it_throws()
    {
        Func<Task<string>> act = () => Task.FromException<string>(new ArgumentException("Name is required."));
        var ex = await Assert.ThrowsAsync<VouchException>(async () => await act.Should().NotThrowAsync());
        Assert.StartsWith("Expected act not to throw, but it threw ArgumentException", ex.Message);
    }

    [Fact]
    public async Task NotThrowAsync_on_a_typed_task_adds_the_reason()
    {
        Func<Task<string>> act = () => Task.FromException<string>(new ArgumentException("x"));
        var ex = await Assert.ThrowsAsync<VouchException>(async () => await act.Should().NotThrowAsync("it is valid"));
        Assert.EndsWith("Because it is valid.", ex.Message);
    }

    private const string AsyncVoidMessage = "Cannot use action assertions on an async void method. Assign the async method to a variable of type Func<Task> instead of Action so that it can be awaited.";

    [Fact]
    public void Throw_on_an_async_void_action_refuses_and_never_runs_it()
    {
        var invoked = false;
        Action act = async () => { invoked = true; await Task.Yield(); throw new InvalidOperationException("boom"); };
        var ex = Assert.Throws<InvalidOperationException>(() => act.Should().Throw<Exception>());
        Assert.Equal(AsyncVoidMessage, ex.Message);
        Assert.False(invoked);
    }

    [Fact]
    public void NotThrow_on_an_async_void_action_refuses_and_never_runs_it()
    {
        var invoked = false;
        Action act = async () => { invoked = true; await Task.Yield(); throw new InvalidOperationException("boom"); };
        var ex = Assert.Throws<InvalidOperationException>(() => act.Should().NotThrow());
        Assert.Equal(AsyncVoidMessage, ex.Message);
        Assert.False(invoked);
    }

    [Fact]
    public void Throw_with_a_reason_on_an_async_void_action_refuses()
    {
        Action act = async () => await Task.Yield();
        Assert.Equal(AsyncVoidMessage, Assert.Throws<InvalidOperationException>(() => act.Should().Throw<Exception>("why")).Message);
        Assert.Equal(AsyncVoidMessage, Assert.Throws<InvalidOperationException>(() => act.Should().NotThrow("why")).Message);
    }

    [Fact]
    public void ValueTask_func_not_throw_sees_the_exception()
    {
        Func<ValueTask> f = async () => { await Task.Yield(); throw new InvalidOperationException(); };
        Assert.Throws<VouchException>(() => f.Should().NotThrow());
    }

    [Fact]
    public void ValueTask_of_int_func_not_throw_sees_the_exception()
    {
        Func<ValueTask<int>> f = async () => { await Task.Yield(); throw new InvalidOperationException(); };
        Assert.Throws<VouchException>(() => f.Should().NotThrow());
    }

    [Fact]
    public void ValueTask_func_not_throw_passes_when_it_completes()
    {
        Func<ValueTask> f = async () => await Task.Yield();
        f.Should().NotThrow();
    }

    [Fact]
    public void ValueTask_of_int_func_not_throw_returns_the_value()
    {
        Func<ValueTask<int>> f = async () => { await Task.Yield(); return 7; };
        Assert.Equal(7, f.Should().NotThrow().Which);
    }

    [Fact]
    public void ValueTask_func_throw_returns_the_exception()
    {
        Func<ValueTask> f = async () => { await Task.Yield(); throw new InvalidOperationException("boom"); };
        Assert.Equal("boom", f.Should().Throw<InvalidOperationException>().Which.Message);
    }

    [Fact]
    public void ValueTask_of_int_func_throw_returns_the_exception()
    {
        Func<ValueTask<int>> f = async () => { await Task.Yield(); throw new InvalidOperationException("boom"); };
        Assert.Equal("boom", f.Should().Throw<InvalidOperationException>().Which.Message);
    }

    [Fact]
    public void ValueTask_func_throw_fails_when_it_completes()
    {
        Func<ValueTask> f = async () => await Task.Yield();
        Assert.Throws<VouchException>(() => f.Should().Throw<InvalidOperationException>());
    }

    [Fact]
    public async Task ValueTask_func_not_throw_async_sees_the_exception()
    {
        Func<ValueTask> f = async () => { await Task.Yield(); throw new InvalidOperationException(); };
        await Assert.ThrowsAsync<VouchException>(() => f.Should().NotThrowAsync());
    }

    [Fact]
    public async Task ValueTask_of_int_func_not_throw_async_sees_the_exception()
    {
        Func<ValueTask<int>> f = async () => { await Task.Yield(); throw new InvalidOperationException(); };
        await Assert.ThrowsAsync<VouchException>(() => f.Should().NotThrowAsync());
    }

    [Fact]
    public async Task ValueTask_of_int_func_not_throw_async_returns_the_value()
    {
        Func<ValueTask<int>> f = async () => { await Task.Yield(); return 7; };
        Assert.Equal(7, (await f.Should().NotThrowAsync()).Which);
    }

    [Fact]
    public async Task ValueTask_func_throw_async_returns_the_exception()
    {
        Func<ValueTask> f = async () => { await Task.Yield(); throw new InvalidOperationException("boom"); };
        Assert.Equal("boom", (await f.Should().ThrowAsync<InvalidOperationException>()).Which.Message);
    }

    [Fact]
    public async Task ValueTask_of_int_func_throw_async_returns_the_exception()
    {
        Func<ValueTask<int>> f = async () => { await Task.Yield(); throw new InvalidOperationException("boom"); };
        Assert.Equal("boom", (await f.Should().ThrowAsync<InvalidOperationException>()).Which.Message);
    }

    [Fact]
    public void ValueTask_func_runs_the_delegate_once()
    {
        var calls = 0;
        Func<ValueTask> f = () => { calls++; return ValueTask.CompletedTask; };
        f.Should().NotThrow();
        Assert.Equal(1, calls);
    }
}
