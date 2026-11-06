using System.Linq.Expressions;

namespace Vouch;

public readonly struct ObjectAssertions
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal ObjectAssertions(object? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public object? Subject { get; }

    public AndConstraint<ObjectAssertions> Be(object? expected)
    {
        if (!Numbers.ValueEquals(Subject, expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<ObjectAssertions> NotBe(object? unexpected)
    {
        if (Numbers.ValueEquals(Subject, unexpected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be {Formatter.Format(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<ObjectAssertions> BeNull()
    {
        if (Subject is not null)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be null, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<ObjectAssertions> NotBeNull()
    {
        if (Subject is null)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be null, but it was.");
        }

        return new(this);
    }

    public AndWhichConstraint<ObjectAssertions, T> BeOfType<T>()
    {
        if (Subject is null || Subject.GetType() != typeof(T))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be of type {Formatter.Format(typeof(T))}, but found {TypeOrNull()}.");
            return new(this, default!);
        }

        return new(this, (T)Subject);
    }

    public AndWhichConstraint<ObjectAssertions, T> BeAssignableTo<T>()
    {
        if (Subject is not T typed)
        {
            if (Subject is null)
            {
                Fail.With($"Expected {Fail.Name(_expression)} to be assignable to {Formatter.Format(typeof(T))}, but found null.");
                return new(this, default!);
            }

            Fail.With($"Expected {Fail.Name(_expression)} to be assignable to {Formatter.Format(typeof(T))}, but {Formatter.Format(Subject.GetType())} is not.");
            return new(this, default!);
        }

        return new(this, typed);
    }

    public AndConstraint<ObjectAssertions> NotBeOfType<T>()
    {
        if (Subject is null)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be of type {Formatter.Format(typeof(T))}, but found null.");
            return new(this);
        }

        if (Subject.GetType() == typeof(T))
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be of type {Formatter.Format(typeof(T))}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<ObjectAssertions> NotBeAssignableTo<T>()
    {
        if (Subject is null)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be assignable to {Formatter.Format(typeof(T))}, but found null.");
            return new(this);
        }

        if (Subject is T)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be assignable to {Formatter.Format(typeof(T))}, but {Formatter.Format(Subject.GetType())} is.");
        }

        return new(this);
    }

    public AndConstraint<ObjectAssertions> BeSameAs(object? expected)
    {
        if (!ReferenceEquals(Subject, expected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to refer to {Formatter.Format(expected)}, but found another object: {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<ObjectAssertions> NotBeSameAs(object? unexpected)
    {
        if (ReferenceEquals(Subject, unexpected))
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to refer to {Formatter.Format(unexpected)}, but it did.");
        }

        return new(this);
    }

    public AndConstraint<ObjectAssertions> BeOneOf(params object?[] validValues)
    {
        foreach (var value in validValues)
        {
            if (object.Equals(Subject, value))
            {
                return new(this);
            }
        }

        Fail.With($"Expected {Fail.Name(_expression)} to be one of {Formatter.FormatList(validValues)}, but found {Formatter.Format(Subject)}.");
        return new(this);
    }

    public AndConstraint<ObjectAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => CheckEquivalent(expectation, EquivalencyOptions.Default);

    public AndConstraint<ObjectAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
        => CheckEquivalent(expectation, Equivalency.Options(configure));

    private AndConstraint<ObjectAssertions> CheckEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        Equivalency.Require(Subject, expectation, typeof(TExpectation), options, Fail.Name(_expression));
        return new(this);
    }

    private string TypeOrNull() => Subject is null ? "null" : Formatter.Format(Subject.GetType());

    public AndConstraint<ObjectAssertions> Be(object? expected, string because)
    {
        Reason.Set(because);
        try
        {
            return Be(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<ObjectAssertions> NotBe(object? unexpected, string because)
    {
        Reason.Set(because);
        try
        {
            return NotBe(unexpected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<ObjectAssertions> BeNull(string because)
    {
        Reason.Set(because);
        try
        {
            return BeNull();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<ObjectAssertions> NotBeNull(string because)
    {
        Reason.Set(because);
        try
        {
            return NotBeNull();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndWhichConstraint<ObjectAssertions, T> BeOfType<T>(string because)
    {
        Reason.Set(because);
        try
        {
            return BeOfType<T>();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndWhichConstraint<ObjectAssertions, T> BeAssignableTo<T>(string because)
    {
        Reason.Set(because);
        try
        {
            return BeAssignableTo<T>();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<ObjectAssertions> BeOfType(Type expectedType)
    {
        ArgumentNullException.ThrowIfNull(expectedType);
        if (Subject is null || Subject.GetType() != expectedType)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be of type {Formatter.Format(expectedType)}, but found {TypeOrNull()}.");
        }

        return new(this);
    }

    public AndConstraint<ObjectAssertions> BeOfType(Type expectedType, string because)
    {
        Reason.Set(because);
        try
        {
            return BeOfType(expectedType);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<ObjectAssertions> Match<T>(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (Subject is null)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to match {predicate.Body}, but found null.");
            return new(this);
        }

        if (Subject is not T typed)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be of type {Formatter.Format(typeof(T))} matching {predicate.Body}, but found {TypeOrNull()}.");
            return new(this);
        }

        if (!predicate.Compile()(typed))
        {
            Fail.With($"Expected {Fail.Name(_expression)} to match {predicate.Body}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<ObjectAssertions> Match<T>(Expression<Func<T, bool>> predicate, string because)
    {
        Reason.Set(because);
        try
        {
            return Match(predicate);
        }
        finally
        {
            Reason.Clear();
        }
    }
}
