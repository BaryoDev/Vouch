using Xunit;

namespace Vouch.Tests;

public class TaskTimingStreamTests
{
    private static readonly TimeSpan Second = TimeSpan.FromSeconds(1);

    private sealed class NoStackException() : Exception("m")
    {
        public override string? StackTrace => null;
    }

    private static string Msg(Action a) => Assert.Throws<VouchException>(a).Message;

    private static async Task<string> MsgAsync(Func<Task> a) => (await Assert.ThrowsAsync<VouchException>(a)).Message;

    // ---- Task.CompleteWithinAsync ----

    [Fact]
    public async Task Task_CompleteWithinAsync_passes_for_a_finished_task()
    {
        using var _ = Clock.Use(new FakeTime());
        await Task.CompletedTask.Should().CompleteWithinAsync(Second);
    }

    [Fact]
    public async Task Task_CompleteWithinAsync_passes_when_the_task_finishes_before_the_deadline()
    {
        var fake = new FakeTime();
        using var _ = Clock.Use(fake);
        var source = new TaskCompletionSource();
        var pending = source.Task.Should().CompleteWithinAsync(Second);
        Assert.False(pending.IsCompleted);
        source.SetResult();
        await pending;
    }

    [Fact]
    public async Task Task_CompleteWithinAsync_fails_at_the_deadline_with_exact_message()
    {
        var fake = new FakeTime();
        using var _ = Clock.Use(fake);
        var task = new TaskCompletionSource().Task;
        var pending = MsgAsync(() => task.Should().CompleteWithinAsync(Second));
        Assert.False(pending.IsCompleted);
        fake.Advance(Second);
        Assert.Equal("Expected task to complete within 00:00:01, but it did not.", await pending);
    }

    [Fact]
    public async Task Task_CompleteWithinAsync_fails_for_null_with_exact_message()
    {
        using var _ = Clock.Use(new FakeTime());
        Task? task = null;
        Assert.Equal("Expected task to complete within 00:00:01, but found null.",
            await MsgAsync(() => task.Should().CompleteWithinAsync(Second)));
    }

    [Fact]
    public async Task Task_CompleteWithinAsync_reports_a_faulted_task_with_exact_message()
    {
        using var _ = Clock.Use(new FakeTime());
        var task = Task.FromException(new InvalidOperationException("boom"));
        Assert.Equal("Expected task to complete within 00:00:01, but it faulted with InvalidOperationException: boom",
            await MsgAsync(() => task.Should().CompleteWithinAsync(Second)));
    }

    [Fact]
    public async Task Task_CompleteWithinAsync_reports_a_cancelled_task_with_exact_message()
    {
        using var _ = Clock.Use(new FakeTime());
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var task = Task.FromCanceled(cts.Token);
        Assert.Equal("Expected task to complete within 00:00:01, but it was cancelled.",
            await MsgAsync(() => task.Should().CompleteWithinAsync(Second)));
    }

    [Fact]
    public async Task TaskOfT_CompleteWithinAsync_reports_a_faulted_task_with_exact_message()
    {
        using var _ = Clock.Use(new FakeTime());
        var number = Task.FromException<int>(new InvalidOperationException("boom"));
        Assert.Equal("Expected number to complete within 00:00:01, but it faulted with InvalidOperationException: boom",
            await MsgAsync(() => number.Should().CompleteWithinAsync(Second)));
    }

    [Fact]
    public async Task TaskOfT_CompleteWithinAsync_reports_a_cancelled_task_with_exact_message()
    {
        using var _ = Clock.Use(new FakeTime());
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var number = Task.FromCanceled<int>(cts.Token);
        Assert.Equal("Expected number to complete within 00:00:01, but it was cancelled.",
            await MsgAsync(() => number.Should().CompleteWithinAsync(Second)));
    }

    [Fact]
    public async Task Task_CompleteWithinAsync_records_a_faulted_or_cancelled_task_inside_a_scope()
    {
        using var _ = Clock.Use(new FakeTime());
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var faulted = Task.FromException(new InvalidOperationException("boom"));
        var cancelled = Task.FromCanceled(cts.Token);
        var scope = new AssertionScope();
        await faulted.Should().CompleteWithinAsync(Second);
        await cancelled.Should().CompleteWithinAsync(Second);
        var error = Assert.Throws<VouchException>(scope.Dispose);
        Assert.Contains("but it faulted with InvalidOperationException: boom", error.Message);
        Assert.Contains("but it was cancelled.", error.Message);
    }

    [Fact]
    public async Task Task_CompleteWithinAsync_with_a_zero_timeout_passes_a_finished_task_and_fails_a_pending_one()
    {
        var fake = new FakeTime();
        using var _ = Clock.Use(fake);
        await Task.CompletedTask.Should().CompleteWithinAsync(TimeSpan.Zero);
        var task = new TaskCompletionSource().Task;
        var pending = MsgAsync(() => task.Should().CompleteWithinAsync(TimeSpan.Zero));
        fake.Advance(TimeSpan.Zero);
        Assert.Equal("Expected task to complete within 00:00:00, but it did not.", await pending);
    }

    [Fact]
    public async Task Task_CompleteWithinAsync_rejects_a_timeout_above_the_Task_Delay_maximum()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => Task.CompletedTask.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(uint.MaxValue)));
    }

    [Fact]
    public async Task ThrowWithinAsync_rejects_a_timeout_above_the_Task_Delay_maximum_with_the_timeout_parameter()
    {
        Func<Task> act = () => Task.FromException(new InvalidOperationException());
        var error = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => act.Should().ThrowWithinAsync<InvalidOperationException>(TimeSpan.FromMilliseconds(uint.MaxValue)));
        Assert.Equal("timeout", error.ParamName);
    }

    [Fact]
    public void NotThrowAfter_rejects_a_poll_interval_above_the_Task_Delay_maximum()
    {
        Action act = () => { };
        var error = Assert.Throws<ArgumentOutOfRangeException>(
            () => act.Should().NotThrowAfter(Second, TimeSpan.FromMilliseconds(uint.MaxValue)));
        Assert.Equal("pollInterval", error.ParamName);
    }

    [Fact]
    public void Where_reports_a_predicate_that_throws_as_a_failure_naming_the_exception_type()
    {
        Action act = () => throw new InvalidOperationException("x");
        var message = Msg(() => act.Should().Throw<InvalidOperationException>()
            .Where(e => ((string)null!).Length == 0));
        Assert.Contains("the predicate threw NullReferenceException", message);
    }

    [Fact]
    public void Stream_HaveLength_reports_a_disposed_stream_as_a_failure()
    {
        var stream = new MemoryStream();
        stream.Dispose();
        Assert.Contains("ObjectDisposedException", Msg(() => stream.Should().HaveLength(0)));
        Assert.Contains("ObjectDisposedException", Msg(() => stream.Should().NotHaveLength(0)));
        Assert.Contains("ObjectDisposedException", Msg(() => stream.Should().HavePosition(0)));
        Assert.Contains("ObjectDisposedException", Msg(() => stream.Should().NotHavePosition(0)));
    }

    [Fact]
    public void ThrowExactly_Exception_on_an_aggregate_wrapping_a_plain_exception_matches_Throw()
    {
        Action act = () => throw new AggregateException(new Exception("inner"));
        act.Should().Throw<Exception>();
        var viaThrow = act.Should().Throw<Exception>();
        var viaExactly = act.Should().ThrowExactly<Exception>();
        Assert.Equal(viaThrow.Which.Message, viaExactly.Which.Message);
    }

    [Fact]
    public async Task Task_CompleteWithinAsync_adds_the_reason()
    {
        var fake = new FakeTime();
        using var _ = Clock.Use(fake);
        var task = new TaskCompletionSource().Task;
        var pending = MsgAsync(() => task.Should().CompleteWithinAsync(Second, "it is quick"));
        fake.Advance(Second);
        Assert.EndsWith("Because it is quick.", await pending);
    }

    [Fact]
    public async Task Task_CompleteWithinAsync_rejects_a_negative_timeout()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => Task.CompletedTask.Should().CompleteWithinAsync(TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public async Task Task_CompleteWithinAsync_cancels_its_timer_when_the_task_wins()
    {
        var fake = new FakeTime();
        using var _ = Clock.Use(fake);
        var source = new TaskCompletionSource();
        var pending = source.Task.Should().CompleteWithinAsync(Second);
        Assert.Equal(1, fake.PendingCount);
        source.SetResult();
        await pending;
        Assert.Equal(0, fake.PendingCount);
    }

    [Fact]
    public async Task Task_CompleteWithinAsync_cancels_its_timer_at_the_deadline()
    {
        var fake = new FakeTime();
        using var _ = Clock.Use(fake);
        var task = new TaskCompletionSource().Task;
        var pending = MsgAsync(() => task.Should().CompleteWithinAsync(Second));
        fake.Advance(Second);
        await pending;
        Assert.Equal(0, fake.PendingCount);
    }

    // ---- Task<T>.CompleteWithinAsync ----

    [Fact]
    public async Task TaskOfT_CompleteWithinAsync_returns_the_result_as_which()
    {
        using var _ = Clock.Use(new FakeTime());
        var result = await Task.FromResult(5).Should().CompleteWithinAsync(Second);
        Assert.Equal(5, result.Which);
    }

    [Fact]
    public async Task TaskOfT_CompleteWithinAsync_fails_at_the_deadline_with_exact_message()
    {
        var fake = new FakeTime();
        using var _ = Clock.Use(fake);
        var number = new TaskCompletionSource<int>().Task;
        var pending = MsgAsync(() => number.Should().CompleteWithinAsync(Second));
        fake.Advance(Second);
        Assert.Equal("Expected number to complete within 00:00:01, but it did not.", await pending);
    }

    [Fact]
    public async Task TaskOfT_CompleteWithinAsync_fails_for_null_with_exact_message()
    {
        using var _ = Clock.Use(new FakeTime());
        Task<int>? number = null;
        Assert.Equal("Expected number to complete within 00:00:01, but found null.",
            await MsgAsync(() => number.Should().CompleteWithinAsync(Second)));
    }

    // ---- Task.NotCompleteWithinAsync ----

    [Fact]
    public async Task Task_NotCompleteWithinAsync_passes_when_the_task_is_still_running()
    {
        var fake = new FakeTime();
        using var _ = Clock.Use(fake);
        var task = new TaskCompletionSource().Task;
        var pending = task.Should().NotCompleteWithinAsync(Second);
        fake.Advance(Second);
        await pending;
    }

    [Fact]
    public async Task Task_NotCompleteWithinAsync_fails_for_a_finished_task_with_exact_message()
    {
        using var _ = Clock.Use(new FakeTime());
        var task = Task.CompletedTask;
        Assert.Equal("Expected task not to complete within 00:00:01, but it did.",
            await MsgAsync(() => task.Should().NotCompleteWithinAsync(Second)));
    }

    [Fact]
    public async Task Task_NotCompleteWithinAsync_fails_for_null_with_exact_message()
    {
        using var _ = Clock.Use(new FakeTime());
        Task? task = null;
        Assert.Equal("Expected task not to complete within 00:00:01, but found null.",
            await MsgAsync(() => task.Should().NotCompleteWithinAsync(Second)));
    }

    [Fact]
    public async Task Task_NotCompleteWithinAsync_adds_the_reason()
    {
        using var _ = Clock.Use(new FakeTime());
        var task = Task.CompletedTask;
        Assert.EndsWith("Because it hangs.", await MsgAsync(() => task.Should().NotCompleteWithinAsync(Second, "it hangs")));
    }

    [Fact]
    public async Task Task_NotCompleteWithinAsync_observes_a_pending_task_that_faults_later()
    {
        var fake = new FakeTime();
        using var _ = Clock.Use(fake);
        var unobserved = 0;
        void Handler(object? sender, UnobservedTaskExceptionEventArgs e) => Interlocked.Increment(ref unobserved);
        TaskScheduler.UnobservedTaskException += Handler;
        try
        {
            var source = new TaskCompletionSource();
            var pending = source.Task.Should().NotCompleteWithinAsync(Second);
            fake.Advance(Second);
            await pending;
            source.SetException(new InvalidOperationException("late boom"));
            source = null;
            pending = default!;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Assert.Equal(0, unobserved);
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= Handler;
        }
    }

    // ---- Task<T>.NotCompleteWithinAsync ----

    [Fact]
    public async Task TaskOfT_NotCompleteWithinAsync_passes_when_the_task_is_still_running()
    {
        var fake = new FakeTime();
        using var _ = Clock.Use(fake);
        var number = new TaskCompletionSource<int>().Task;
        var pending = number.Should().NotCompleteWithinAsync(Second);
        fake.Advance(Second);
        await pending;
    }

    [Fact]
    public async Task TaskOfT_NotCompleteWithinAsync_fails_for_a_finished_task_with_exact_message()
    {
        using var _ = Clock.Use(new FakeTime());
        var number = Task.FromResult(1);
        Assert.Equal("Expected number not to complete within 00:00:01, but it did.",
            await MsgAsync(() => number.Should().NotCompleteWithinAsync(Second)));
    }

    [Fact]
    public async Task TaskOfT_NotCompleteWithinAsync_fails_for_null_with_exact_message()
    {
        using var _ = Clock.Use(new FakeTime());
        Task<int>? number = null;
        Assert.Equal("Expected number not to complete within 00:00:01, but found null.",
            await MsgAsync(() => number.Should().NotCompleteWithinAsync(Second)));
    }

    // ---- ThrowExactly ----

    [Fact]
    public void ThrowExactly_passes_for_the_exact_type()
    {
        Action act = () => throw new InvalidOperationException("x");
        act.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void ThrowExactly_fails_for_a_subtype_with_exact_message()
    {
        Action act = () => throw new ArgumentException("boom");
        Assert.Equal("Expected act to throw exactly Exception, but it threw ArgumentException:\n  \"boom\"",
            Msg(() => act.Should().ThrowExactly<Exception>()));
    }

    [Fact]
    public void ThrowExactly_fails_when_nothing_is_thrown_with_exact_message()
    {
        Action act = () => { };
        Assert.Equal("Expected act to throw exactly InvalidOperationException, but it did not throw.",
            Msg(() => act.Should().ThrowExactly<InvalidOperationException>()));
    }

    [Fact]
    public void ThrowExactly_fails_for_null_with_exact_message()
    {
        Action? act = null;
        Assert.Equal("Expected act to throw exactly InvalidOperationException, but found null.",
            Msg(() => act.Should().ThrowExactly<InvalidOperationException>()));
    }

    [Fact]
    public void ThrowExactly_adds_the_reason()
    {
        Action act = () => { };
        Assert.EndsWith("Because it must.", Msg(() => act.Should().ThrowExactly<InvalidOperationException>("it must")));
    }

    [Fact]
    public void Function_ThrowExactly_passes_and_fails()
    {
        Func<int> f = () => throw new InvalidOperationException("x");
        f.Should().ThrowExactly<InvalidOperationException>();
        Assert.Equal("Expected f to throw exactly Exception, but it threw InvalidOperationException:\n  \"x\"",
            Msg(() => f.Should().ThrowExactly<Exception>()));
    }

    [Fact]
    public void Function_ThrowExactly_fails_for_null_with_exact_message()
    {
        Func<int>? f = null;
        Assert.Equal("Expected f to throw exactly InvalidOperationException, but found null.",
            Msg(() => f.Should().ThrowExactly<InvalidOperationException>()));
    }

    // ---- ThrowExactlyAsync ----

    [Fact]
    public async Task ThrowExactlyAsync_passes_for_the_exact_type()
    {
        Func<Task> act = () => Task.FromException(new InvalidOperationException("x"));
        await act.Should().ThrowExactlyAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ThrowExactlyAsync_fails_for_a_subtype_with_exact_message()
    {
        Func<Task> act = () => Task.FromException(new ArgumentException("boom"));
        Assert.Equal("Expected act to throw exactly Exception, but it threw ArgumentException:\n  \"boom\"",
            await MsgAsync(() => act.Should().ThrowExactlyAsync<Exception>()));
    }

    [Fact]
    public async Task ThrowExactlyAsync_fails_when_nothing_is_thrown_with_exact_message()
    {
        Func<Task> act = () => Task.CompletedTask;
        Assert.Equal("Expected act to throw exactly InvalidOperationException, but it did not throw.",
            await MsgAsync(() => act.Should().ThrowExactlyAsync<InvalidOperationException>()));
    }

    [Fact]
    public async Task ThrowExactlyAsync_fails_for_null_with_exact_message()
    {
        Func<Task>? act = null;
        Assert.Equal("Expected act to throw exactly InvalidOperationException, but found null.",
            await MsgAsync(() => act.Should().ThrowExactlyAsync<InvalidOperationException>()));
    }

    [Fact]
    public async Task ThrowExactlyAsync_adds_the_reason()
    {
        Func<Task> act = () => Task.CompletedTask;
        Assert.EndsWith("Because it must.", await MsgAsync(() => act.Should().ThrowExactlyAsync<InvalidOperationException>("it must")));
    }

    [Fact]
    public async Task FunctionOfTask_ThrowExactlyAsync_passes_and_fails()
    {
        Func<Task<int>> act = () => Task.FromException<int>(new InvalidOperationException("x"));
        await act.Should().ThrowExactlyAsync<InvalidOperationException>();
        Assert.Equal("Expected act to throw exactly Exception, but it threw InvalidOperationException:\n  \"x\"",
            await MsgAsync(() => act.Should().ThrowExactlyAsync<Exception>()));
    }

    // ---- ThrowWithinAsync ----

    [Fact]
    public async Task ThrowWithinAsync_passes_when_the_task_fails_in_time()
    {
        using var _ = Clock.Use(new FakeTime());
        Func<Task> act = () => Task.FromException(new InvalidOperationException("x"));
        await act.Should().ThrowWithinAsync<InvalidOperationException>(Second);
    }

    [Fact]
    public async Task ThrowWithinAsync_fails_at_the_deadline_with_exact_message()
    {
        var fake = new FakeTime();
        using var _ = Clock.Use(fake);
        var never = new TaskCompletionSource().Task;
        Func<Task> act = () => never;
        var pending = MsgAsync(() => act.Should().ThrowWithinAsync<InvalidOperationException>(Second));
        Assert.False(pending.IsCompleted);
        fake.Advance(Second);
        Assert.Equal("Expected act to throw InvalidOperationException within 00:00:01, but it had not finished by then.", await pending);
        Assert.Equal(0, fake.PendingCount);
    }

    [Fact]
    public async Task ThrowWithinAsync_fails_when_the_task_completes_without_throwing_with_exact_message()
    {
        using var _ = Clock.Use(new FakeTime());
        Func<Task> act = () => Task.CompletedTask;
        Assert.Equal("Expected act to throw InvalidOperationException within 00:00:01, but it did not throw.",
            await MsgAsync(() => act.Should().ThrowWithinAsync<InvalidOperationException>(Second)));
    }

    [Fact]
    public async Task ThrowWithinAsync_fails_for_the_wrong_type_with_exact_message()
    {
        using var _ = Clock.Use(new FakeTime());
        Func<Task> act = () => Task.FromException(new ArgumentException("boom"));
        Assert.Equal("Expected act to throw InvalidOperationException within 00:00:01, but it threw ArgumentException:\n  \"boom\"",
            await MsgAsync(() => act.Should().ThrowWithinAsync<InvalidOperationException>(Second)));
    }

    [Fact]
    public async Task ThrowWithinAsync_fails_for_null_with_exact_message()
    {
        using var _ = Clock.Use(new FakeTime());
        Func<Task>? act = null;
        Assert.Equal("Expected act to throw InvalidOperationException within 00:00:01, but found null.",
            await MsgAsync(() => act.Should().ThrowWithinAsync<InvalidOperationException>(Second)));
    }

    [Fact]
    public async Task ThrowWithinAsync_adds_the_reason()
    {
        using var _ = Clock.Use(new FakeTime());
        Func<Task> act = () => Task.CompletedTask;
        Assert.EndsWith("Because it must.",
            await MsgAsync(() => act.Should().ThrowWithinAsync<InvalidOperationException>(Second, "it must")));
    }

    [Fact]
    public async Task ThrowWithinAsync_rejects_a_negative_timeout()
    {
        Func<Task> act = () => Task.CompletedTask;
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => act.Should().ThrowWithinAsync<InvalidOperationException>(TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public async Task FunctionOfTask_ThrowWithinAsync_passes_and_fails()
    {
        using var _ = Clock.Use(new FakeTime());
        Func<Task<int>> act = () => Task.FromException<int>(new InvalidOperationException("x"));
        await act.Should().ThrowWithinAsync<InvalidOperationException>(Second);
        Assert.Equal("Expected act to throw ArgumentException within 00:00:01, but it threw InvalidOperationException:\n  \"x\"",
            await MsgAsync(() => act.Should().ThrowWithinAsync<ArgumentException>(Second)));
    }

    // ---- NotThrowAfter ----

    [Fact]
    public void NotThrowAfter_passes_when_the_action_stops_throwing()
    {
        var fake = new FakeTime { AutoAdvance = true };
        using var _ = Clock.Use(fake);
        var calls = 0;
        Action act = () =>
        {
            if (++calls < 3)
            {
                throw new InvalidOperationException("not yet");
            }
        };
        act.Should().NotThrowAfter(Second, TimeSpan.FromMilliseconds(100));
        Assert.Equal(3, calls);
    }

    [Fact]
    public void NotThrowAfter_polls_until_the_wait_time_is_used_up_then_fails_with_exact_message()
    {
        var fake = new FakeTime { AutoAdvance = true };
        using var _ = Clock.Use(fake);
        var calls = 0;
        Action act = () =>
        {
            calls++;
            throw new NoStackException();
        };
        Assert.Equal("Expected act not to throw after 00:00:01, but it threw NoStackException:\n  \"m\"",
            Msg(() => act.Should().NotThrowAfter(Second, TimeSpan.FromMilliseconds(100))));
        Assert.Equal(11, calls);
        Assert.Equal(Second, fake.Now);
    }

    [Fact]
    public void NotThrowAfter_fails_for_null_with_exact_message()
    {
        Action? act = null;
        Assert.Equal("Expected act not to throw after 00:00:01, but found null.",
            Msg(() => act.Should().NotThrowAfter(Second, TimeSpan.FromMilliseconds(100))));
    }

    [Fact]
    public void NotThrowAfter_adds_the_reason()
    {
        using var _ = Clock.Use(new FakeTime { AutoAdvance = true });
        Action act = () => throw new NoStackException();
        Assert.EndsWith("Because it should recover.",
            Msg(() => act.Should().NotThrowAfter(Second, TimeSpan.FromMilliseconds(100), "it should recover")));
    }

    [Fact]
    public void NotThrowAfter_rejects_bad_waits()
    {
        Action act = () => { };
        Assert.Throws<ArgumentOutOfRangeException>(() => act.Should().NotThrowAfter(TimeSpan.FromSeconds(-1), Second));
        Assert.Throws<ArgumentOutOfRangeException>(() => act.Should().NotThrowAfter(Second, TimeSpan.Zero));
    }

    [Fact]
    public void Function_NotThrowAfter_returns_the_value_as_which()
    {
        using var _ = Clock.Use(new FakeTime { AutoAdvance = true });
        var calls = 0;
        Func<int> f = () => ++calls < 2 ? throw new InvalidOperationException("not yet") : 7;
        var result = f.Should().NotThrowAfter(Second, TimeSpan.FromMilliseconds(100));
        Assert.Equal(7, result.Which);
    }

    [Fact]
    public void Function_NotThrowAfter_fails_with_exact_message()
    {
        using var _ = Clock.Use(new FakeTime { AutoAdvance = true });
        Func<int> f = () => throw new NoStackException();
        Assert.Equal("Expected f not to throw after 00:00:01, but it threw NoStackException:\n  \"m\"",
            Msg(() => f.Should().NotThrowAfter(Second, TimeSpan.FromMilliseconds(100))));
    }

    [Fact]
    public void Function_NotThrowAfter_fails_for_null_with_exact_message()
    {
        Func<int>? f = null;
        Assert.Equal("Expected f not to throw after 00:00:01, but found null.",
            Msg(() => f.Should().NotThrowAfter(Second, TimeSpan.FromMilliseconds(100))));
    }

    // ---- ExceptionAssertions.Where ----

    [Fact]
    public void Where_passes_when_the_predicate_holds()
    {
        Action act = () => throw new InvalidOperationException("x");
        act.Should().Throw<InvalidOperationException>().Where(e => e.Message == "x");
    }

    [Fact]
    public void Where_fails_with_the_expression_text()
    {
        Action act = () => throw new InvalidOperationException("x");
        var message = Msg(() => act.Should().Throw<InvalidOperationException>().Where(e => e.Message == "y"));
        Assert.StartsWith("Expected act's exception to match e => ", message);
        Assert.Contains("e.Message", message);
        Assert.EndsWith(", but it did not.", message);
    }

    [Fact]
    public void Where_adds_the_reason()
    {
        Action act = () => throw new InvalidOperationException("x");
        Assert.EndsWith("Because it matters.",
            Msg(() => act.Should().Throw<InvalidOperationException>().Where(e => e.Message == "y", "it matters")));
    }

    [Fact]
    public void Where_rejects_a_null_predicate()
    {
        Action act = () => throw new InvalidOperationException("x");
        Assert.Throws<ArgumentNullException>(() => act.Should().Throw<InvalidOperationException>().Where(null!));
    }

    // ---- ExecutionTime ----

    private static ExecutionTime Took(TimeSpan time)
    {
        var fake = new FakeTime();
        using var _ = Clock.Use(fake);
        Action act = () => fake.Advance(time);
        return act.ExecutionTime();
    }

    [Fact]
    public void ExecutionTime_measures_through_the_time_source()
    {
        var measured = Took(TimeSpan.FromSeconds(2));
        Assert.Equal(TimeSpan.FromSeconds(2), measured.Elapsed);
        Assert.Equal("act", measured.Expression);
    }

    [Fact]
    public void ExecutionTime_propagates_an_exception_from_the_action()
    {
        Action act = () => throw new InvalidOperationException("x");
        Assert.Throws<InvalidOperationException>(() => act.ExecutionTime());
    }

    [Fact]
    public void ExecutionTime_rejects_a_null_action()
    {
        Action? act = null;
        Assert.Throws<ArgumentNullException>(() => act!.ExecutionTime());
    }

    [Fact]
    public void ExecutionTime_waits_for_an_async_function()
    {
        var fake = new FakeTime();
        using var clock = Clock.Use(fake);
        Func<Task> act = () =>
        {
            var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = Task.Run(() =>
            {
                fake.Advance(Second);
                source.SetResult();
            });
            return source.Task;
        };
        Assert.Equal(Second, act.ExecutionTime().Elapsed);
    }

    [Fact]
    public void ExecutionTimeOf_measures_a_call_and_names_it_by_its_expression()
    {
        var fake = new FakeTime();
        using var _ = Clock.Use(fake);
        var measured = fake.ExecutionTimeOf(f => f.Advance(Second));
        Assert.Equal(Second, measured.Elapsed);
        Assert.Contains("Advance", measured.Expression);
    }

    [Fact]
    public void ExecutionTimeOf_measures_an_async_call()
    {
        var fake = new FakeTime();
        using var _ = Clock.Use(fake);
        var measured = fake.ExecutionTimeOf(f => Task.Run(() => f.Advance(Second)));
        Assert.Equal(Second, measured.Elapsed);
    }

    [Fact]
    public void ExecutionTime_BeLessThan_passes_and_fails_with_exact_message()
    {
        var act = new ExecutionTime(TimeSpan.FromSeconds(2), "act");
        act.Should().BeLessThan(TimeSpan.FromSeconds(3));
        Assert.Equal("Expected act to run in less than 00:00:02, but it took 00:00:02.",
            Msg(() => act.Should().BeLessThan(TimeSpan.FromSeconds(2))));
    }

    [Fact]
    public void ExecutionTime_BeLessThanOrEqualTo_passes_and_fails_with_exact_message()
    {
        var act = new ExecutionTime(TimeSpan.FromSeconds(2), "act");
        act.Should().BeLessThanOrEqualTo(TimeSpan.FromSeconds(2));
        Assert.Equal("Expected act to run in at most 00:00:01, but it took 00:00:02.",
            Msg(() => act.Should().BeLessThanOrEqualTo(Second)));
    }

    [Fact]
    public void ExecutionTime_BeGreaterThan_passes_and_fails_with_exact_message()
    {
        var act = new ExecutionTime(TimeSpan.FromSeconds(2), "act");
        act.Should().BeGreaterThan(Second);
        Assert.Equal("Expected act to run in more than 00:00:02, but it took 00:00:02.",
            Msg(() => act.Should().BeGreaterThan(TimeSpan.FromSeconds(2))));
    }

    [Fact]
    public void ExecutionTime_BeGreaterThanOrEqualTo_passes_and_fails_with_exact_message()
    {
        var act = new ExecutionTime(TimeSpan.FromSeconds(2), "act");
        act.Should().BeGreaterThanOrEqualTo(TimeSpan.FromSeconds(2));
        Assert.Equal("Expected act to run in at least 00:00:03, but it took 00:00:02.",
            Msg(() => act.Should().BeGreaterThanOrEqualTo(TimeSpan.FromSeconds(3))));
    }

    [Fact]
    public void ExecutionTime_BeCloseTo_passes_and_fails_with_exact_message()
    {
        var act = new ExecutionTime(TimeSpan.FromSeconds(2), "act");
        act.Should().BeCloseTo(Second, Second);
        act.Should().BeCloseTo(TimeSpan.FromSeconds(3), Second);
        Assert.Equal("Expected act to run in 00:00:05 within 00:00:01, but it took 00:00:02.",
            Msg(() => act.Should().BeCloseTo(TimeSpan.FromSeconds(5), Second)));
    }

    [Fact]
    public void ExecutionTime_BeCloseTo_rejects_a_negative_precision()
    {
        var act = new ExecutionTime(TimeSpan.FromSeconds(2), "act");
        Assert.Throws<ArgumentOutOfRangeException>(() => act.Should().BeCloseTo(Second, TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void ExecutionTime_assertions_add_the_reason()
    {
        var act = new ExecutionTime(TimeSpan.FromSeconds(2), "act");
        Assert.EndsWith("Because it is slow.", Msg(() => act.Should().BeLessThan(Second, "it is slow")));
        Assert.EndsWith("Because it is slow.", Msg(() => act.Should().BeLessThanOrEqualTo(Second, "it is slow")));
        Assert.EndsWith("Because it is slow.", Msg(() => act.Should().BeGreaterThan(Second * 3, "it is slow")));
        Assert.EndsWith("Because it is slow.", Msg(() => act.Should().BeGreaterThanOrEqualTo(Second * 3, "it is slow")));
        Assert.EndsWith("Because it is slow.", Msg(() => act.Should().BeCloseTo(Second * 5, Second, "it is slow")));
    }

    [Fact]
    public void ExecutionTime_failure_without_an_expression_names_it_value()
    {
        var act = new ExecutionTime(TimeSpan.FromSeconds(2), null);
        Assert.Equal("Expected value to run in less than 00:00:01, but it took 00:00:02.",
            Msg(() => act.Should().BeLessThan(Second)));
    }

    // ---- Streams ----

    private static MemoryStream Live() => new(new byte[3]);

    private static MemoryStream Closed()
    {
        var stream = Live();
        stream.Dispose();
        return stream;
    }

    [Fact]
    public void Stream_BeReadable_passes_and_fails_with_exact_message()
    {
        using var s = Live();
        s.Should().BeReadable();
        var closed = Closed();
        Assert.Equal("Expected closed to be readable, but it is not.", Msg(() => closed.Should().BeReadable()));
    }

    [Fact]
    public void Stream_NotBeReadable_passes_and_fails_with_exact_message()
    {
        Closed().Should().NotBeReadable();
        using var s = Live();
        Assert.Equal("Expected s not to be readable, but it is.", Msg(() => s.Should().NotBeReadable()));
    }

    [Fact]
    public void Stream_BeWritable_passes_and_fails_with_exact_message()
    {
        using var s = Live();
        s.Should().BeWritable();
        using var readOnly = new MemoryStream(new byte[3], false);
        Assert.Equal("Expected readOnly to be writable, but it is not.", Msg(() => readOnly.Should().BeWritable()));
    }

    [Fact]
    public void Stream_NotBeWritable_passes_and_fails_with_exact_message()
    {
        using var readOnly = new MemoryStream(new byte[3], false);
        readOnly.Should().NotBeWritable();
        using var s = Live();
        Assert.Equal("Expected s not to be writable, but it is.", Msg(() => s.Should().NotBeWritable()));
    }

    [Fact]
    public void Stream_BeSeekable_passes_and_fails_with_exact_message()
    {
        using var s = Live();
        s.Should().BeSeekable();
        var closed = Closed();
        Assert.Equal("Expected closed to be seekable, but it is not.", Msg(() => closed.Should().BeSeekable()));
    }

    [Fact]
    public void Stream_NotBeSeekable_passes_and_fails_with_exact_message()
    {
        Closed().Should().NotBeSeekable();
        using var s = Live();
        Assert.Equal("Expected s not to be seekable, but it is.", Msg(() => s.Should().NotBeSeekable()));
    }

    [Fact]
    public void Stream_HaveLength_passes_and_fails_with_exact_message()
    {
        using var s = Live();
        s.Should().HaveLength(3);
        Assert.Equal("Expected s to have length 4, but it has length 3.", Msg(() => s.Should().HaveLength(4)));
    }

    [Fact]
    public void Stream_NotHaveLength_passes_and_fails_with_exact_message()
    {
        using var s = Live();
        s.Should().NotHaveLength(4);
        Assert.Equal("Expected s not to have length 3, but it does.", Msg(() => s.Should().NotHaveLength(3)));
    }

    [Fact]
    public void Stream_HavePosition_passes_and_fails_with_exact_message()
    {
        using var s = Live();
        s.Should().HavePosition(0);
        s.Position = 2;
        s.Should().HavePosition(2);
        Assert.Equal("Expected s to have position 1, but it is at position 2.", Msg(() => s.Should().HavePosition(1)));
    }

    [Fact]
    public void Stream_NotHavePosition_passes_and_fails_with_exact_message()
    {
        using var s = Live();
        s.Should().NotHavePosition(1);
        Assert.Equal("Expected s not to have position 0, but it does.", Msg(() => s.Should().NotHavePosition(0)));
    }

    [Fact]
    public void Stream_length_and_position_fail_clearly_when_not_seekable()
    {
        var closed = Closed();
        Assert.Equal("Expected closed to have length 3, but its length is unavailable because it is not seekable.",
            Msg(() => closed.Should().HaveLength(3)));
        Assert.Equal("Expected closed not to have length 3, but its length is unavailable because it is not seekable.",
            Msg(() => closed.Should().NotHaveLength(3)));
        Assert.Equal("Expected closed to have position 0, but its position is unavailable because it is not seekable.",
            Msg(() => closed.Should().HavePosition(0)));
        Assert.Equal("Expected closed not to have position 0, but its position is unavailable because it is not seekable.",
            Msg(() => closed.Should().NotHavePosition(0)));
    }

    [Fact]
    public void Stream_assertions_fail_for_null_with_exact_message()
    {
        Stream? s = null;
        Assert.Equal("Expected s to be readable, but found null.", Msg(() => s.Should().BeReadable()));
        Assert.Equal("Expected s not to be readable, but found null.", Msg(() => s.Should().NotBeReadable()));
        Assert.Equal("Expected s to be writable, but found null.", Msg(() => s.Should().BeWritable()));
        Assert.Equal("Expected s not to be writable, but found null.", Msg(() => s.Should().NotBeWritable()));
        Assert.Equal("Expected s to be seekable, but found null.", Msg(() => s.Should().BeSeekable()));
        Assert.Equal("Expected s not to be seekable, but found null.", Msg(() => s.Should().NotBeSeekable()));
        Assert.Equal("Expected s to have length 3, but found null.", Msg(() => s.Should().HaveLength(3)));
        Assert.Equal("Expected s not to have length 3, but found null.", Msg(() => s.Should().NotHaveLength(3)));
        Assert.Equal("Expected s to have position 0, but found null.", Msg(() => s.Should().HavePosition(0)));
        Assert.Equal("Expected s not to have position 0, but found null.", Msg(() => s.Should().NotHavePosition(0)));
    }

    [Fact]
    public void Stream_assertions_add_the_reason()
    {
        var closed = Closed();
        using var s = Live();
        Assert.EndsWith("Because it matters.", Msg(() => closed.Should().BeReadable("it matters")));
        Assert.EndsWith("Because it matters.", Msg(() => s.Should().NotBeReadable("it matters")));
        Assert.EndsWith("Because it matters.", Msg(() => closed.Should().BeWritable("it matters")));
        Assert.EndsWith("Because it matters.", Msg(() => s.Should().NotBeWritable("it matters")));
        Assert.EndsWith("Because it matters.", Msg(() => closed.Should().BeSeekable("it matters")));
        Assert.EndsWith("Because it matters.", Msg(() => s.Should().NotBeSeekable("it matters")));
        Assert.EndsWith("Because it matters.", Msg(() => s.Should().HaveLength(9, "it matters")));
        Assert.EndsWith("Because it matters.", Msg(() => s.Should().NotHaveLength(3, "it matters")));
        Assert.EndsWith("Because it matters.", Msg(() => s.Should().HavePosition(9, "it matters")));
        Assert.EndsWith("Because it matters.", Msg(() => s.Should().NotHavePosition(0, "it matters")));
    }

    // ---- Time source scope ----

    [Fact]
    public void Clock_uses_the_stopwatch_by_default()
    {
        Assert.Same(StopwatchTimeSource.Instance, Clock.Current);
    }

    [Fact]
    public void Clock_scope_restores_the_previous_source_when_disposed()
    {
        var outer = new FakeTime();
        var inner = new FakeTime();
        using (Clock.Use(outer))
        {
            using (Clock.Use(inner))
            {
                Assert.Same(inner, Clock.Current);
            }

            Assert.Same(outer, Clock.Current);
        }

        Assert.Same(StopwatchTimeSource.Instance, Clock.Current);
    }

    [Fact]
    public async Task Clock_scope_set_in_a_child_task_does_not_leak_to_the_caller()
    {
        var outer = new FakeTime();
        using var _ = Clock.Use(outer);
        await Task.Run(() =>
        {
            using var scope = Clock.Use(new FakeTime());
        });
        Assert.Same(outer, Clock.Current);
    }

    [Fact]
    public async Task Clock_scope_follows_the_test_into_child_tasks()
    {
        var fake = new FakeTime();
        using var _ = Clock.Use(fake);
        var seen = await Task.Run(async () =>
        {
            await Task.Yield();
            return Clock.Current;
        });
        Assert.Same(fake, seen);
    }

    [Fact]
    public async Task Parallel_tests_do_not_see_each_others_fake()
    {
        var a = new FakeTime();
        var b = new FakeTime();
        var gate = new TaskCompletionSource();
        var first = Task.Run(async () =>
        {
            using var _ = Clock.Use(a);
            await gate.Task;
            return Clock.Current;
        });
        var second = Task.Run(async () =>
        {
            using var _ = Clock.Use(b);
            gate.SetResult();
            await Task.Yield();
            return Clock.Current;
        });
        Assert.Same(a, await first);
        Assert.Same(b, await second);
    }
}
