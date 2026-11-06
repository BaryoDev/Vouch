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

    /// <summary>
    /// Requires the task to finish within the time. A task that faults or is cancelled within the time is
    /// reported as a failure, so an <see cref="AssertionScope"/> records it; the exception is not rethrown.
    /// </summary>
    public Task<AndConstraint<TaskAssertions>> CompleteWithinAsync(TimeSpan timeout)
        => CompleteCoreAsync(timeout, null);

    public Task<AndConstraint<TaskAssertions>> CompleteWithinAsync(TimeSpan timeout, string because)
        => CompleteCoreAsync(timeout, because);

    /// <summary>Requires the task to still be running when the time is up.</summary>
    public Task<AndConstraint<TaskAssertions>> NotCompleteWithinAsync(TimeSpan timeout)
        => NotCompleteCoreAsync(timeout, null);

    public Task<AndConstraint<TaskAssertions>> NotCompleteWithinAsync(TimeSpan timeout, string because)
        => NotCompleteCoreAsync(timeout, because);

    // Forwards to ObjectAssertions so calls that compiled against Should(object?) still compile.
    private ObjectAssertions AsObject => new(Subject, _expression);

    private AndConstraint<TaskAssertions> Self(AndConstraint<ObjectAssertions> _) => new(this);

    private AndWhichConstraint<TaskAssertions, TWhich> Which<TWhich>(AndWhichConstraint<ObjectAssertions, TWhich> result) => new(this, result.Which);

    public AndConstraint<TaskAssertions> Be(object? expected)
        => Self(AsObject.Be(expected));

    public AndConstraint<TaskAssertions> Be(object? expected, string because)
        => Self(AsObject.Be(expected, because));

    public AndConstraint<TaskAssertions> NotBe(object? unexpected)
        => Self(AsObject.NotBe(unexpected));

    public AndConstraint<TaskAssertions> NotBe(object? unexpected, string because)
        => Self(AsObject.NotBe(unexpected, because));

    public AndConstraint<TaskAssertions> BeNull()
        => Self(AsObject.BeNull());

    public AndConstraint<TaskAssertions> BeNull(string because)
        => Self(AsObject.BeNull(because));

    public AndConstraint<TaskAssertions> NotBeNull()
        => Self(AsObject.NotBeNull());

    public AndConstraint<TaskAssertions> NotBeNull(string because)
        => Self(AsObject.NotBeNull(because));

    public AndWhichConstraint<TaskAssertions, TType> BeOfType<TType>()
        => Which(AsObject.BeOfType<TType>());

    public AndWhichConstraint<TaskAssertions, TType> BeOfType<TType>(string because)
        => Which(AsObject.BeOfType<TType>(because));

    public AndConstraint<TaskAssertions> BeOfType(Type expectedType)
        => Self(AsObject.BeOfType(expectedType));

    public AndConstraint<TaskAssertions> BeOfType(Type expectedType, string because)
        => Self(AsObject.BeOfType(expectedType, because));

    public AndWhichConstraint<TaskAssertions, TType> BeAssignableTo<TType>()
        => Which(AsObject.BeAssignableTo<TType>());

    public AndWhichConstraint<TaskAssertions, TType> BeAssignableTo<TType>(string because)
        => Which(AsObject.BeAssignableTo<TType>(because));

    public AndConstraint<TaskAssertions> NotBeOfType<TType>()
        => Self(AsObject.NotBeOfType<TType>());

    public AndConstraint<TaskAssertions> NotBeAssignableTo<TType>()
        => Self(AsObject.NotBeAssignableTo<TType>());

    public AndConstraint<TaskAssertions> BeSameAs(object? expected)
        => Self(AsObject.BeSameAs(expected));

    public AndConstraint<TaskAssertions> NotBeSameAs(object? unexpected)
        => Self(AsObject.NotBeSameAs(unexpected));

    public AndConstraint<TaskAssertions> BeOneOf(params object?[] validValues)
        => Self(AsObject.BeOneOf(validValues));

    public AndConstraint<TaskAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => Self(AsObject.BeEquivalentTo(expectation));

    public AndConstraint<TaskAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
        => Self(AsObject.BeEquivalentTo(expectation, configure));

    public AndConstraint<TaskAssertions> Match<TType>(System.Linq.Expressions.Expression<Func<TType, bool>> predicate)
        => Self(AsObject.Match(predicate));

    public AndConstraint<TaskAssertions> Match<TType>(System.Linq.Expressions.Expression<Func<TType, bool>> predicate, string because)
        => Self(AsObject.Match(predicate, because));

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
            var failure = TaskMessages.Failure(task);
            if (failure is not null)
            {
                TaskMessages.Report(name, expectation, because, failure);
            }
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
        else
        {
            TaskMessages.ObserveLater(task);
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

    /// <summary>
    /// Requires the task to finish within the time. Which is its result. A task that faults or is cancelled
    /// is reported as a failure, not rethrown.
    /// </summary>
    public Task<AndWhichConstraint<TaskAssertions<T>, T>> CompleteWithinAsync(TimeSpan timeout)
        => CompleteCoreAsync(timeout, null);

    public Task<AndWhichConstraint<TaskAssertions<T>, T>> CompleteWithinAsync(TimeSpan timeout, string because)
        => CompleteCoreAsync(timeout, because);

    public Task<AndConstraint<TaskAssertions<T>>> NotCompleteWithinAsync(TimeSpan timeout)
        => NotCompleteCoreAsync(timeout, null);

    public Task<AndConstraint<TaskAssertions<T>>> NotCompleteWithinAsync(TimeSpan timeout, string because)
        => NotCompleteCoreAsync(timeout, because);

    // Forwards to ObjectAssertions so calls that compiled against Should(object?) still compile.
    private ObjectAssertions AsObject => new(Subject, _expression);

    private AndConstraint<TaskAssertions<T>> Self(AndConstraint<ObjectAssertions> _) => new(this);

    private AndWhichConstraint<TaskAssertions<T>, TWhich> Which<TWhich>(AndWhichConstraint<ObjectAssertions, TWhich> result) => new(this, result.Which);

    public AndConstraint<TaskAssertions<T>> Be(object? expected)
        => Self(AsObject.Be(expected));

    public AndConstraint<TaskAssertions<T>> Be(object? expected, string because)
        => Self(AsObject.Be(expected, because));

    public AndConstraint<TaskAssertions<T>> NotBe(object? unexpected)
        => Self(AsObject.NotBe(unexpected));

    public AndConstraint<TaskAssertions<T>> NotBe(object? unexpected, string because)
        => Self(AsObject.NotBe(unexpected, because));

    public AndConstraint<TaskAssertions<T>> BeNull()
        => Self(AsObject.BeNull());

    public AndConstraint<TaskAssertions<T>> BeNull(string because)
        => Self(AsObject.BeNull(because));

    public AndConstraint<TaskAssertions<T>> NotBeNull()
        => Self(AsObject.NotBeNull());

    public AndConstraint<TaskAssertions<T>> NotBeNull(string because)
        => Self(AsObject.NotBeNull(because));

    public AndWhichConstraint<TaskAssertions<T>, TType> BeOfType<TType>()
        => Which(AsObject.BeOfType<TType>());

    public AndWhichConstraint<TaskAssertions<T>, TType> BeOfType<TType>(string because)
        => Which(AsObject.BeOfType<TType>(because));

    public AndConstraint<TaskAssertions<T>> BeOfType(Type expectedType)
        => Self(AsObject.BeOfType(expectedType));

    public AndConstraint<TaskAssertions<T>> BeOfType(Type expectedType, string because)
        => Self(AsObject.BeOfType(expectedType, because));

    public AndWhichConstraint<TaskAssertions<T>, TType> BeAssignableTo<TType>()
        => Which(AsObject.BeAssignableTo<TType>());

    public AndWhichConstraint<TaskAssertions<T>, TType> BeAssignableTo<TType>(string because)
        => Which(AsObject.BeAssignableTo<TType>(because));

    public AndConstraint<TaskAssertions<T>> NotBeOfType<TType>()
        => Self(AsObject.NotBeOfType<TType>());

    public AndConstraint<TaskAssertions<T>> NotBeAssignableTo<TType>()
        => Self(AsObject.NotBeAssignableTo<TType>());

    public AndConstraint<TaskAssertions<T>> BeSameAs(object? expected)
        => Self(AsObject.BeSameAs(expected));

    public AndConstraint<TaskAssertions<T>> NotBeSameAs(object? unexpected)
        => Self(AsObject.NotBeSameAs(unexpected));

    public AndConstraint<TaskAssertions<T>> BeOneOf(params object?[] validValues)
        => Self(AsObject.BeOneOf(validValues));

    public AndConstraint<TaskAssertions<T>> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => Self(AsObject.BeEquivalentTo(expectation));

    public AndConstraint<TaskAssertions<T>> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
        => Self(AsObject.BeEquivalentTo(expectation, configure));

    public AndConstraint<TaskAssertions<T>> Match<TType>(System.Linq.Expressions.Expression<Func<TType, bool>> predicate)
        => Self(AsObject.Match(predicate));

    public AndConstraint<TaskAssertions<T>> Match<TType>(System.Linq.Expressions.Expression<Func<TType, bool>> predicate, string because)
        => Self(AsObject.Match(predicate, because));

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
            var failure = TaskMessages.Failure(task);
            if (failure is null)
            {
                return new(this, task.Result);
            }

            TaskMessages.Report(name, expectation, because, failure);
            return new(this, default!);
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
        else
        {
            TaskMessages.ObserveLater(task);
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

    // For a task that is still running: its later fault is read when it happens, so it is never unobserved.
    public static void ObserveLater(Task task)
    {
        if (task.IsCompleted)
        {
            Observe(task);
            return;
        }

        _ = task.ContinueWith(
            static t => _ = t.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    // The outcome text for a finished task that did not succeed, or null when it ran to completion.
    // Reading the exception also marks it observed.
    public static string? Failure(Task task)
    {
        if (task.IsFaulted)
        {
            var error = task.Exception!.InnerExceptions.Count > 0 ? task.Exception.InnerExceptions[0] : task.Exception;
            return $"but it faulted with {Formatter.Format(error.GetType())}: {error.Message}";
        }

        return task.IsCanceled ? "but it was cancelled." : null;
    }
}
