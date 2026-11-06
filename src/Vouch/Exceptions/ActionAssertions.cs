using System.Runtime.CompilerServices;

namespace Vouch;

public readonly struct ActionAssertions
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal ActionAssertions(Action? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public Action? Subject { get; }

    public ExceptionAssertions<TException> Throw<TException>() where TException : Exception
        => ThrowCore<TException>(null);

    public AndConstraint<ActionAssertions> NotThrow()
        => NotThrowCore(null);

    public ExceptionAssertions<TException> ThrowExactly<TException>() where TException : Exception
        => ThrowCore<TException>(null, true);

    public ExceptionAssertions<TException> ThrowExactly<TException>(string because) where TException : Exception
        => ThrowCore<TException>(because, true);

    /// <summary>
    /// Runs the action, and again after each poll interval while it throws, until it stops throwing or the
    /// wait time is used up. Fails if it still throws at the end.
    /// </summary>
    public AndConstraint<ActionAssertions> NotThrowAfter(TimeSpan waitTime, TimeSpan pollInterval)
        => NotThrowAfterCore(waitTime, pollInterval, null);

    public AndConstraint<ActionAssertions> NotThrowAfter(TimeSpan waitTime, TimeSpan pollInterval, string because)
        => NotThrowAfterCore(waitTime, pollInterval, because);

    private AndConstraint<ActionAssertions> NotThrowAfterCore(TimeSpan waitTime, TimeSpan pollInterval, string? because)
    {
        if (waitTime < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(waitTime), "The wait time must not be negative.");
        }

        if (pollInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(pollInterval), "The poll interval must be greater than zero.");
        }

        if (Subject is null)
        {
            Reason.Set(because);
            try
            {
                ExceptionMessages.NullDelegate(Fail.Name(_expression), $"not to throw after {Formatter.Format(waitTime)}");
            }
            finally
            {
                Reason.Clear();
            }

            return new(this);
        }

        RejectAsyncVoid(Subject);
        var clock = Clock.Current;
        var start = clock.Now;
        while (true)
        {
            var thrown = Capture.Run(Subject);
            if (thrown is null)
            {
                return new(this);
            }

            var remaining = waitTime - (clock.Now - start);
            if (remaining <= TimeSpan.Zero)
            {
                Reason.Set(because);
                try
                {
                    ExceptionMessages.Threw(Fail.Name(_expression), thrown, $" after {Formatter.Format(waitTime)}");
                }
                finally
                {
                    Reason.Clear();
                }

                return new(this);
            }

            clock.Delay(remaining < pollInterval ? remaining : pollInterval, CancellationToken.None).GetAwaiter().GetResult();
        }
    }

    private ExceptionAssertions<TException> ThrowCore<TException>(string? because, bool exact = false) where TException : Exception
    {
        var name = Fail.Name(_expression);
        if (Subject is null)
        {
            Reason.Set(because);
            try
            {
                ExceptionMessages.NullDelegate(name, $"{(exact ? "to throw exactly" : "to throw")} {Formatter.Format(typeof(TException))}");
            }
            finally
            {
                Reason.Clear();
            }

            return ExceptionMessages.Empty<TException>(name);
        }

        RejectAsyncVoid(Subject);
        var thrown = Capture.Run(Subject);
        Reason.Set(because);
        try
        {
            return ExceptionMessages.Check<TException>(name, thrown, exact);
        }
        finally
        {
            Reason.Clear();
        }
    }

    private AndConstraint<ActionAssertions> NotThrowCore(string? because)
    {
        var name = Fail.Name(_expression);
        if (Subject is null)
        {
            Reason.Set(because);
            try
            {
                ExceptionMessages.NullDelegate(name, "not to throw");
            }
            finally
            {
                Reason.Clear();
            }

            return new(this);
        }

        RejectAsyncVoid(Subject);
        var thrown = Capture.Run(Subject);
        if (thrown is not null)
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

        return new(this);
    }

    private static void RejectAsyncVoid(Action action)
    {
        if (action.Method.IsDefined(typeof(AsyncStateMachineAttribute), false))
        {
            throw new InvalidOperationException("Cannot use action assertions on an async void method. Assign the async method to a variable of type Func<Task> instead of Action so that it can be awaited.");
        }
    }

    public ExceptionAssertions<TException> Throw<TException>(string because) where TException : Exception
        => ThrowCore<TException>(because);

    public AndConstraint<ActionAssertions> NotThrow(string because)
        => NotThrowCore(because);
}

public readonly struct FunctionAssertions<T>
{
    private readonly ActionAssertions _inner;
    private readonly Box? _box;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _inner.Expression;

    internal FunctionAssertions(Func<T>? subject, string? expression)
    {
        Subject = subject;
        if (subject is null)
        {
            _inner = new ActionAssertions(null, expression);
        }
        else
        {
            var box = new Box();
            _box = box;
            _inner = new ActionAssertions(() =>
            {
                box.Value = default!;
                box.Value = subject();
            }, expression);
        }
    }

    public Func<T>? Subject { get; }

    public ExceptionAssertions<TException> Throw<TException>() where TException : Exception
        => _inner.Throw<TException>();

    public AndWhichConstraint<FunctionAssertions<T>, T> NotThrow()
    {
        _inner.NotThrow();
        return new(this, _box is null ? default! : _box.Value);
    }

    public ExceptionAssertions<TException> Throw<TException>(string because) where TException : Exception
        => _inner.Throw<TException>(because);

    public AndWhichConstraint<FunctionAssertions<T>, T> NotThrow(string because)
    {
        _inner.NotThrow(because);
        return new(this, _box is null ? default! : _box.Value);
    }

    public ExceptionAssertions<TException> ThrowExactly<TException>() where TException : Exception
        => _inner.ThrowExactly<TException>();

    public ExceptionAssertions<TException> ThrowExactly<TException>(string because) where TException : Exception
        => _inner.ThrowExactly<TException>(because);

    public AndWhichConstraint<FunctionAssertions<T>, T> NotThrowAfter(TimeSpan waitTime, TimeSpan pollInterval)
    {
        _inner.NotThrowAfter(waitTime, pollInterval);
        return new(this, _box is null ? default! : _box.Value);
    }

    public AndWhichConstraint<FunctionAssertions<T>, T> NotThrowAfter(TimeSpan waitTime, TimeSpan pollInterval, string because)
    {
        _inner.NotThrowAfter(waitTime, pollInterval, because);
        return new(this, _box is null ? default! : _box.Value);
    }

    private sealed class Box
    {
        public T Value = default!;
    }
}

internal static class Capture
{
    public static Exception? Run(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
