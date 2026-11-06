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

    private ExceptionAssertions<TException> ThrowCore<TException>(string? because) where TException : Exception
    {
        var name = Fail.Name(_expression);
        if (Subject is null)
        {
            Reason.Set(because);
            try
            {
                ExceptionMessages.NullDelegate(name, $"to throw {Formatter.Format(typeof(TException))}");
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
            return ExceptionMessages.Check<TException>(name, thrown);
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
