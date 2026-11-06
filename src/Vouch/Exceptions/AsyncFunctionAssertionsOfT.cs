namespace Vouch;

public readonly struct AsyncFunctionAssertions<T>
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal AsyncFunctionAssertions(Func<Task<T>>? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public Func<Task<T>>? Subject { get; }

    public Task<ExceptionAssertions<TException>> ThrowAsync<TException>() where TException : Exception
        => Untyped(out _).ThrowAsync<TException>();

    public Task<ExceptionAssertions<TException>> ThrowAsync<TException>(string because) where TException : Exception
        => Untyped(out _).ThrowAsync<TException>(because);

    public async Task<AndWhichConstraint<AsyncFunctionAssertions<T>, T>> NotThrowAsync()
    {
        await Untyped(out var result).NotThrowAsync().ConfigureAwait(false);
        return new(this, result.Value);
    }

    public async Task<AndWhichConstraint<AsyncFunctionAssertions<T>, T>> NotThrowAsync(string because)
    {
        await Untyped(out var result).NotThrowAsync(because).ConfigureAwait(false);
        return new(this, result.Value);
    }

    private AsyncFunctionAssertions Untyped(out Holder result)
    {
        var holder = new Holder();
        result = holder;
        var subject = Subject;
        Func<Task>? wrapped = subject is null
            ? null
            : () => Capture(subject, holder)!;
        return new(wrapped, _expression);
    }

    private static Task? Capture(Func<Task<T>> subject, Holder holder)
    {
        var task = subject();
        return task is null ? null : Await(task, holder);
    }

    private static async Task Await(Task<T> task, Holder holder)
        => holder.Value = await task.ConfigureAwait(false);

    internal sealed class Holder
    {
        public T Value { get; set; } = default!;
    }
}
