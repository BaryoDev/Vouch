using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Vouch;

/// <summary>The time one run of an action took. It is measured once, when this value is made.</summary>
public readonly struct ExecutionTime
{
    internal ExecutionTime(TimeSpan elapsed, string? expression)
    {
        Elapsed = elapsed;
        Expression = expression;
    }

    public TimeSpan Elapsed { get; }

    /// <summary>The text of the measured expression, or null when none was captured.</summary>
    public string? Expression { get; }

    internal static ExecutionTime Measure(Action action, string? expression)
    {
        var clock = Clock.Current;
        var start = clock.Now;
        action();
        return new ExecutionTime(clock.Now - start, expression);
    }
}

public static class ExecutionTimeExtensions
{
    /// <summary>Runs the action once and measures it. An exception from the action propagates.</summary>
    public static ExecutionTime ExecutionTime(this Action action,
        [CallerArgumentExpression(nameof(action))] string? expression = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        return Vouch.ExecutionTime.Measure(action, expression);
    }

    /// <summary>Runs the function once and waits for its task, measuring until the task finishes.</summary>
    public static ExecutionTime ExecutionTime(this Func<Task> action,
        [CallerArgumentExpression(nameof(action))] string? expression = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        return Vouch.ExecutionTime.Measure(() => action().GetAwaiter().GetResult(), expression);
    }

    public static ExecutionTime ExecutionTimeOf<T>(this T subject, Expression<Action<T>> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var compiled = action.Compile();
        return Vouch.ExecutionTime.Measure(() => compiled(subject), action.Body.ToString());
    }

    public static ExecutionTime ExecutionTimeOf<T>(this T subject, Expression<Func<T, Task>> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var compiled = action.Compile();
        return Vouch.ExecutionTime.Measure(() => compiled(subject).GetAwaiter().GetResult(), action.Body.ToString());
    }

    public static ExecutionTimeAssertions Should(this ExecutionTime subject)
        => new(subject);
}

public readonly struct ExecutionTimeAssertions
{
    private readonly ExecutionTime _subject;

    internal ExecutionTimeAssertions(ExecutionTime subject) => _subject = subject;

    public ExecutionTime Subject => _subject;

    public string? Expression => _subject.Expression;

    private string Name => Fail.Name(_subject.Expression);

    public AndConstraint<ExecutionTimeAssertions> BeLessThan(TimeSpan limit)
    {
        if (_subject.Elapsed >= limit)
        {
            Fail.With($"Expected {Name} to run in less than {Formatter.Format(limit)}, but it took {Formatter.Format(_subject.Elapsed)}.");
        }

        return new(this);
    }

    public AndConstraint<ExecutionTimeAssertions> BeLessThanOrEqualTo(TimeSpan limit)
    {
        if (_subject.Elapsed > limit)
        {
            Fail.With($"Expected {Name} to run in at most {Formatter.Format(limit)}, but it took {Formatter.Format(_subject.Elapsed)}.");
        }

        return new(this);
    }

    public AndConstraint<ExecutionTimeAssertions> BeGreaterThan(TimeSpan limit)
    {
        if (_subject.Elapsed <= limit)
        {
            Fail.With($"Expected {Name} to run in more than {Formatter.Format(limit)}, but it took {Formatter.Format(_subject.Elapsed)}.");
        }

        return new(this);
    }

    public AndConstraint<ExecutionTimeAssertions> BeGreaterThanOrEqualTo(TimeSpan limit)
    {
        if (_subject.Elapsed < limit)
        {
            Fail.With($"Expected {Name} to run in at least {Formatter.Format(limit)}, but it took {Formatter.Format(_subject.Elapsed)}.");
        }

        return new(this);
    }

    public AndConstraint<ExecutionTimeAssertions> BeCloseTo(TimeSpan expected, TimeSpan precision)
    {
        if (precision < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(precision), "The precision must not be negative.");
        }

        var difference = (_subject.Elapsed - expected).Duration();
        if (difference > precision)
        {
            Fail.With($"Expected {Name} to run in {Formatter.Format(expected)} within {Formatter.Format(precision)}, but it took {Formatter.Format(_subject.Elapsed)}.");
        }

        return new(this);
    }

    public AndConstraint<ExecutionTimeAssertions> BeLessThan(TimeSpan limit, string because)
    {
        Reason.Set(because);
        try
        {
            return BeLessThan(limit);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<ExecutionTimeAssertions> BeLessThanOrEqualTo(TimeSpan limit, string because)
    {
        Reason.Set(because);
        try
        {
            return BeLessThanOrEqualTo(limit);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<ExecutionTimeAssertions> BeGreaterThan(TimeSpan limit, string because)
    {
        Reason.Set(because);
        try
        {
            return BeGreaterThan(limit);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<ExecutionTimeAssertions> BeGreaterThanOrEqualTo(TimeSpan limit, string because)
    {
        Reason.Set(because);
        try
        {
            return BeGreaterThanOrEqualTo(limit);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<ExecutionTimeAssertions> BeCloseTo(TimeSpan expected, TimeSpan precision, string because)
    {
        Reason.Set(because);
        try
        {
            return BeCloseTo(expected, precision);
        }
        finally
        {
            Reason.Clear();
        }
    }
}
