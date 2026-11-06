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
        => ThrowCoreAsync<TException>(null);

    public Task NotThrowAsync()
        => NotThrowCoreAsync(null);

    public Task<ExceptionAssertions<TException>> ThrowAsync<TException>(string because) where TException : Exception
        => ThrowCoreAsync<TException>(because);

    public Task NotThrowAsync(string because)
        => NotThrowCoreAsync(because);

    // The reason is a thread-local, so it is applied only around each synchronous
    // reporting step and never held across an await.
    private async Task<ExceptionAssertions<TException>> ThrowCoreAsync<TException>(string? because) where TException : Exception
    {
        var name = Fail.Name(_expression);
        var expectation = $"to throw {Formatter.Format(typeof(TException))}";
        var (ran, thrown) = await RunAsync(name, expectation, because).ConfigureAwait(false);
        if (!ran)
        {
            return ExceptionMessages.Empty<TException>(name);
        }

        Reason.Set(because);
        try
        {
            return ExceptionMessages.Check<TException>(name, thrown);
        }
        finally
        {
            Reason.Clear();
        }
    }

    private async Task NotThrowCoreAsync(string? because)
    {
        var name = Fail.Name(_expression);
        var (ran, thrown) = await RunAsync(name, "not to throw", because).ConfigureAwait(false);
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

    private async Task<(bool Ran, Exception? Thrown)> RunAsync(string name, string expectation, string? because)
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

            return (false, null);
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

                return (false, null);
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

        return (true, thrown);
    }
}
