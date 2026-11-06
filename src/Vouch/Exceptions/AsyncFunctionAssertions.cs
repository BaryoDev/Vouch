namespace Vouch;

public readonly struct AsyncFunctionAssertions
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal AsyncFunctionAssertions(Func<Task>? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public Func<Task>? Subject { get; }

    public Task<ExceptionAssertions<TException>> ThrowAsync<TException>() where TException : Exception
        => ThrowCoreAsync<TException>(null, false, null);

    public Task NotThrowAsync()
        => NotThrowCoreAsync(null);

    public Task<ExceptionAssertions<TException>> ThrowAsync<TException>(string because) where TException : Exception
        => ThrowCoreAsync<TException>(because, false, null);

    public Task NotThrowAsync(string because)
        => NotThrowCoreAsync(because);

    /// <summary>Like ThrowAsync, but the exception must be of exactly the given type, not a subtype.</summary>
    public Task<ExceptionAssertions<TException>> ThrowExactlyAsync<TException>() where TException : Exception
        => ThrowCoreAsync<TException>(null, true, null);

    public Task<ExceptionAssertions<TException>> ThrowExactlyAsync<TException>(string because) where TException : Exception
        => ThrowCoreAsync<TException>(because, true, null);

    /// <summary>Requires the task to fail with the exception before the time runs out.</summary>
    public Task<ExceptionAssertions<TException>> ThrowWithinAsync<TException>(TimeSpan timeout) where TException : Exception
        => ThrowCoreAsync<TException>(null, false, timeout);

    public Task<ExceptionAssertions<TException>> ThrowWithinAsync<TException>(TimeSpan timeout, string because) where TException : Exception
        => ThrowCoreAsync<TException>(because, false, timeout);

    // The reason is a thread-local, so it is applied only around each synchronous
    // reporting step and never held across an await.
    private async Task<ExceptionAssertions<TException>> ThrowCoreAsync<TException>(string? because, bool exact, TimeSpan? within) where TException : Exception
    {
        if (within is { } limit)
        {
            TimeoutGuard.Require(limit, "timeout");
        }

        var name = Fail.Name(_expression);
        var withinText = within is { } w ? $" within {Formatter.Format(w)}" : "";
        var expectation = $"{(exact ? "to throw exactly" : "to throw")} {Formatter.Format(typeof(TException))}{withinText}";
        var (ran, thrown, timedOut) = await RunAsync(name, expectation, because, within).ConfigureAwait(false);
        if (!ran)
        {
            return ExceptionMessages.Empty<TException>(name);
        }

        Reason.Set(because);
        try
        {
            if (timedOut)
            {
                Fail.With($"Expected {name} {expectation}, but it had not finished by then.");
                return ExceptionMessages.Empty<TException>(name);
            }

            return ExceptionMessages.Check<TException>(name, thrown, exact, withinText);
        }
        finally
        {
            Reason.Clear();
        }
    }

    private async Task NotThrowCoreAsync(string? because)
    {
        var name = Fail.Name(_expression);
        var (ran, thrown, _) = await RunAsync(name, "not to throw", because, null).ConfigureAwait(false);
        if (ran && thrown is not null)
        {
            Reason.Set(because);
            try
            {
                ExceptionMessages.Threw(name, thrown);
            }
            finally
            {
                Reason.Clear();
            }
        }
    }

    private async Task<(bool Ran, Exception? Thrown, bool TimedOut)> RunAsync(string name, string expectation, string? because, TimeSpan? within)
    {
        var subject = Subject;
        if (subject is null)
        {
            Reason.Set(because);
            try
            {
                ExceptionMessages.NullDelegate(name, expectation);
            }
            finally
            {
                Reason.Clear();
            }

            return (false, null, false);
        }

        Task? task = null;
        Exception? thrown = null;
        try
        {
            task = subject();
        }
        catch (Exception ex)
        {
            thrown = ex;
        }

        if (thrown is null)
        {
            if (task is null)
            {
                Reason.Set(because);
                try
                {
                    ExceptionMessages.NullTask(name, expectation);
                }
                finally
                {
                    Reason.Clear();
                }

                return (false, null, false);
            }

            if (within is { } limit && !await TimeoutGuard.FinishesInAsync(task, limit).ConfigureAwait(false))
            {
                return (true, null, true);
            }

            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                thrown = ex;
            }
        }

        return (true, thrown, false);
    }
}

internal static class TimeoutGuard
{
    public static void Require(TimeSpan timeout, string parameter)
    {
        if (timeout < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(parameter, "The time limit must not be negative.");
        }

        RequireDelayable(timeout, parameter);
    }

    /// <summary>Task.Delay accepts at most uint.MaxValue - 1 milliseconds.</summary>
    public static void RequireDelayable(TimeSpan value, string parameter)
    {
        if (value.TotalMilliseconds > uint.MaxValue - 1)
        {
            throw new ArgumentOutOfRangeException(parameter, "The time limit must not exceed the Task.Delay maximum of uint.MaxValue - 1 milliseconds.");
        }
    }

    /// <summary>
    /// True if the task finished before the time ran out. The wait is cancelled either way, so no timer
    /// outlives the call. The task itself is not awaited, so its outcome is not observed here.
    /// </summary>
    public static async Task<bool> FinishesInAsync(Task task, TimeSpan timeout)
    {
        if (task.IsCompleted)
        {
            return true;
        }

        using var cts = new CancellationTokenSource();
        var delay = Clock.Current.Delay(timeout, cts.Token);
        var winner = await Task.WhenAny(task, delay).ConfigureAwait(false);
        cts.Cancel();
        if (ReferenceEquals(winner, task))
        {
            return true;
        }

        // A timer that stopped on its own and a task that finished at the same moment: prefer the task.
        return task.IsCompleted;
    }
}
