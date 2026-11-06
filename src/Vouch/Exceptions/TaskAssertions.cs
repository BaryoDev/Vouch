namespace Vouch;

public readonly struct TaskAssertions
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal TaskAssertions(Task? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public Task? Subject { get; }

    /// <summary>Requires the task to finish within the time. A task that fails rethrows its exception.</summary>
    public Task<AndConstraint<TaskAssertions>> CompleteWithinAsync(TimeSpan timeout)
        => CompleteCoreAsync(timeout, null);

    public Task<AndConstraint<TaskAssertions>> CompleteWithinAsync(TimeSpan timeout, string because)
        => CompleteCoreAsync(timeout, because);

    /// <summary>Requires the task to still be running when the time is up.</summary>
    public Task<AndConstraint<TaskAssertions>> NotCompleteWithinAsync(TimeSpan timeout)
        => NotCompleteCoreAsync(timeout, null);

    public Task<AndConstraint<TaskAssertions>> NotCompleteWithinAsync(TimeSpan timeout, string because)
        => NotCompleteCoreAsync(timeout, because);

    private async Task<AndConstraint<TaskAssertions>> CompleteCoreAsync(TimeSpan timeout, string? because)
    {
        TimeoutGuard.Require(timeout, nameof(timeout));
        var name = Fail.Name(_expression);
        var expectation = $"to complete within {Formatter.Format(timeout)}";
        var task = Subject;
        if (task is null)
        {
            TaskMessages.Report(name, expectation, because, null);
            return new(this);
        }

        if (await TimeoutGuard.FinishesInAsync(task, timeout).ConfigureAwait(false))
        {
            await task.ConfigureAwait(false);
        }
        else
        {
            TaskMessages.Report(name, expectation, because, "but it did not.");
        }

        return new(this);
    }

    private async Task<AndConstraint<TaskAssertions>> NotCompleteCoreAsync(TimeSpan timeout, string? because)
    {
        TimeoutGuard.Require(timeout, nameof(timeout));
        var name = Fail.Name(_expression);
        var expectation = $"not to complete within {Formatter.Format(timeout)}";
        var task = Subject;
        if (task is null)
        {
            TaskMessages.Report(name, expectation, because, null);
            return new(this);
        }

        if (await TimeoutGuard.FinishesInAsync(task, timeout).ConfigureAwait(false))
        {
            TaskMessages.Observe(task);
            TaskMessages.Report(name, expectation, because, "but it did.");
        }

        return new(this);
    }
}

public readonly struct TaskAssertions<T>
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal TaskAssertions(Task<T>? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public Task<T>? Subject { get; }

    /// <summary>Requires the task to finish within the time. Which is its result.</summary>
    public Task<AndWhichConstraint<TaskAssertions<T>, T>> CompleteWithinAsync(TimeSpan timeout)
        => CompleteCoreAsync(timeout, null);

    public Task<AndWhichConstraint<TaskAssertions<T>, T>> CompleteWithinAsync(TimeSpan timeout, string because)
        => CompleteCoreAsync(timeout, because);

    public Task<AndConstraint<TaskAssertions<T>>> NotCompleteWithinAsync(TimeSpan timeout)
        => NotCompleteCoreAsync(timeout, null);

    public Task<AndConstraint<TaskAssertions<T>>> NotCompleteWithinAsync(TimeSpan timeout, string because)
        => NotCompleteCoreAsync(timeout, because);

    private async Task<AndWhichConstraint<TaskAssertions<T>, T>> CompleteCoreAsync(TimeSpan timeout, string? because)
    {
        TimeoutGuard.Require(timeout, nameof(timeout));
        var name = Fail.Name(_expression);
        var expectation = $"to complete within {Formatter.Format(timeout)}";
        var task = Subject;
        if (task is null)
        {
            TaskMessages.Report(name, expectation, because, null);
            return new(this, default!);
        }

        if (await TimeoutGuard.FinishesInAsync(task, timeout).ConfigureAwait(false))
        {
            return new(this, await task.ConfigureAwait(false));
        }

        TaskMessages.Report(name, expectation, because, "but it did not.");
        return new(this, default!);
    }

    private async Task<AndConstraint<TaskAssertions<T>>> NotCompleteCoreAsync(TimeSpan timeout, string? because)
    {
        TimeoutGuard.Require(timeout, nameof(timeout));
        var name = Fail.Name(_expression);
        var expectation = $"not to complete within {Formatter.Format(timeout)}";
        var task = Subject;
        if (task is null)
        {
            TaskMessages.Report(name, expectation, because, null);
            return new(this);
        }

        if (await TimeoutGuard.FinishesInAsync(task, timeout).ConfigureAwait(false))
        {
            TaskMessages.Observe(task);
            TaskMessages.Report(name, expectation, because, "but it did.");
        }

        return new(this);
    }
}

internal static class TaskMessages
{
    // The reason is a thread-local, so it is applied only around this synchronous step.
    // A null outcome means the subject was null.
    public static void Report(string name, string expectation, string? because, string? outcome)
    {
        Reason.Set(because);
        try
        {
            if (outcome is null)
            {
                ExceptionMessages.NullDelegate(name, expectation);
            }
            else
            {
                Fail.With($"Expected {name} {expectation}, {outcome}");
            }
        }
        finally
        {
            Reason.Clear();
        }
    }

    // Reads a failed task's exception so it does not surface later as unobserved.
    public static void Observe(Task task)
    {
        if (task.IsFaulted)
        {
            _ = task.Exception;
        }
    }
}
