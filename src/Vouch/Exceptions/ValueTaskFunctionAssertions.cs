namespace Vouch;

public readonly struct ValueTaskFunctionAssertions
{
    private readonly string? _expression;

    internal ValueTaskFunctionAssertions(Func<ValueTask>? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public Func<ValueTask>? Subject { get; }

    public Task<ExceptionAssertions<TException>> ThrowAsync<TException>() where TException : Exception
        => Inner().ThrowAsync<TException>();

    public Task<ExceptionAssertions<TException>> ThrowAsync<TException>(string because) where TException : Exception
        => Inner().ThrowAsync<TException>(because);

    public Task NotThrowAsync() => Inner().NotThrowAsync();

    public Task NotThrowAsync(string because) => Inner().NotThrowAsync(because);

    public ExceptionAssertions<TException> Throw<TException>() where TException : Exception
    {
        var inner = Inner();
        return Blocking.Run(() => inner.ThrowAsync<TException>());
    }

    public ExceptionAssertions<TException> Throw<TException>(string because) where TException : Exception
    {
        var inner = Inner();
        return Blocking.Run(() => inner.ThrowAsync<TException>(because));
    }

    public AndConstraint<ValueTaskFunctionAssertions> NotThrow()
    {
        var inner = Inner();
        Blocking.Run(() => inner.NotThrowAsync());
        return new(this);
    }

    public AndConstraint<ValueTaskFunctionAssertions> NotThrow(string because)
    {
        var inner = Inner();
        Blocking.Run(() => inner.NotThrowAsync(because));
        return new(this);
    }

    private AsyncFunctionAssertions Inner()
    {
        var subject = Subject;
        return new(subject is null ? null : () => subject().AsTask(), _expression);
    }
}

public readonly struct ValueTaskFunctionAssertions<T>
{
    private readonly string? _expression;

    internal ValueTaskFunctionAssertions(Func<ValueTask<T>>? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public Func<ValueTask<T>>? Subject { get; }

    public Task<ExceptionAssertions<TException>> ThrowAsync<TException>() where TException : Exception
        => Inner().ThrowAsync<TException>();

    public Task<ExceptionAssertions<TException>> ThrowAsync<TException>(string because) where TException : Exception
        => Inner().ThrowAsync<TException>(because);

    public async Task<AndWhichConstraint<ValueTaskFunctionAssertions<T>, T>> NotThrowAsync()
    {
        var result = await Inner().NotThrowAsync().ConfigureAwait(false);
        return new(this, result.Which);
    }

    public async Task<AndWhichConstraint<ValueTaskFunctionAssertions<T>, T>> NotThrowAsync(string because)
    {
        var result = await Inner().NotThrowAsync(because).ConfigureAwait(false);
        return new(this, result.Which);
    }

    public ExceptionAssertions<TException> Throw<TException>() where TException : Exception
    {
        var inner = Inner();
        return Blocking.Run(() => inner.ThrowAsync<TException>());
    }

    public ExceptionAssertions<TException> Throw<TException>(string because) where TException : Exception
    {
        var inner = Inner();
        return Blocking.Run(() => inner.ThrowAsync<TException>(because));
    }

    public AndWhichConstraint<ValueTaskFunctionAssertions<T>, T> NotThrow()
    {
        var inner = Inner();
        var result = Blocking.Run(() => inner.NotThrowAsync());
        return new(this, result.Which);
    }

    public AndWhichConstraint<ValueTaskFunctionAssertions<T>, T> NotThrow(string because)
    {
        var inner = Inner();
        var result = Blocking.Run(() => inner.NotThrowAsync(because));
        return new(this, result.Which);
    }

    private AsyncFunctionAssertions<T> Inner()
    {
        var subject = Subject;
        return new(subject is null ? null : () => subject().AsTask(), _expression);
    }
}

internal static class Blocking
{
    // Runs on the thread pool so a captured synchronization context cannot deadlock the wait.
    public static void Run(Func<Task> work) => Task.Run(work).GetAwaiter().GetResult();

    public static TResult Run<TResult>(Func<Task<TResult>> work) => Task.Run(work).GetAwaiter().GetResult();
}
