namespace Vouch;

public readonly struct StreamAssertions
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal StreamAssertions(Stream? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public Stream? Subject { get; }

    public AndConstraint<StreamAssertions> BeReadable()
    {
        if (Subject is null)
        {
            return NullSubject("to be readable");
        }

        if (!Subject.CanRead)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be readable, but it is not.");
        }

        return new(this);
    }

    public AndConstraint<StreamAssertions> NotBeReadable()
    {
        if (Subject is null)
        {
            return NullSubject("not to be readable");
        }

        if (Subject.CanRead)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be readable, but it is.");
        }

        return new(this);
    }

    public AndConstraint<StreamAssertions> BeWritable()
    {
        if (Subject is null)
        {
            return NullSubject("to be writable");
        }

        if (!Subject.CanWrite)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be writable, but it is not.");
        }

        return new(this);
    }

    public AndConstraint<StreamAssertions> NotBeWritable()
    {
        if (Subject is null)
        {
            return NullSubject("not to be writable");
        }

        if (Subject.CanWrite)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be writable, but it is.");
        }

        return new(this);
    }

    public AndConstraint<StreamAssertions> BeSeekable()
    {
        if (Subject is null)
        {
            return NullSubject("to be seekable");
        }

        if (!Subject.CanSeek)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be seekable, but it is not.");
        }

        return new(this);
    }

    public AndConstraint<StreamAssertions> NotBeSeekable()
    {
        if (Subject is null)
        {
            return NullSubject("not to be seekable");
        }

        if (Subject.CanSeek)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be seekable, but it is.");
        }

        return new(this);
    }

    // Every message below is built only on a failure path, so a passing call allocates nothing.
    public AndConstraint<StreamAssertions> HaveLength(long expected)
    {
        if (Subject is null)
        {
            return NullSubject($"to have length {expected}");
        }

        if (!Subject.CanSeek)
        {
            return Unavailable($"to have length {expected}", "length");
        }

        var error = TryRead(false, out var actual);
        if (error is not null)
        {
            return ReadFailed($"to have length {expected}", "length", error);
        }

        if (actual != expected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to have length {expected}, but it has length {actual}.");
        }

        return new(this);
    }

    public AndConstraint<StreamAssertions> NotHaveLength(long unexpected)
    {
        if (Subject is null)
        {
            return NullSubject($"not to have length {unexpected}");
        }

        if (!Subject.CanSeek)
        {
            return Unavailable($"not to have length {unexpected}", "length");
        }

        var error = TryRead(false, out var actual);
        if (error is not null)
        {
            return ReadFailed($"not to have length {unexpected}", "length", error);
        }

        if (actual == unexpected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to have length {unexpected}, but it does.");
        }

        return new(this);
    }

    public AndConstraint<StreamAssertions> HavePosition(long expected)
    {
        if (Subject is null)
        {
            return NullSubject($"to have position {expected}");
        }

        if (!Subject.CanSeek)
        {
            return Unavailable($"to have position {expected}", "position");
        }

        var error = TryRead(true, out var actual);
        if (error is not null)
        {
            return ReadFailed($"to have position {expected}", "position", error);
        }

        if (actual != expected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to have position {expected}, but it is at position {actual}.");
        }

        return new(this);
    }

    public AndConstraint<StreamAssertions> NotHavePosition(long unexpected)
    {
        if (Subject is null)
        {
            return NullSubject($"not to have position {unexpected}");
        }

        if (!Subject.CanSeek)
        {
            return Unavailable($"not to have position {unexpected}", "position");
        }

        var error = TryRead(true, out var actual);
        if (error is not null)
        {
            return ReadFailed($"not to have position {unexpected}", "position", error);
        }

        if (actual == unexpected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to have position {unexpected}, but it does.");
        }

        return new(this);
    }

    private AndConstraint<StreamAssertions> NullSubject(string expectation)
    {
        Fail.With($"Expected {Fail.Name(_expression)} {expectation}, but found null.");
        return new(this);
    }

    private AndConstraint<StreamAssertions> Unavailable(string expectation, string what)
    {
        Fail.With($"Expected {Fail.Name(_expression)} {expectation}, but its {what} is unavailable because it is not seekable.");
        return new(this);
    }

    // A stream can still throw when read (disposed, or closed under us). That is a failure, not a crash.
    private Exception? TryRead(bool position, out long value)
    {
        try
        {
            value = position ? Subject!.Position : Subject!.Length;
            return null;
        }
        catch (Exception ex) when (ex is NotSupportedException or ObjectDisposedException or IOException)
        {
            value = 0;
            return ex;
        }
    }

    private AndConstraint<StreamAssertions> ReadFailed(string expectation, string what, Exception error)
    {
        Fail.With($"Expected {Fail.Name(_expression)} {expectation}, but reading its {what} threw {Formatter.Format(error.GetType())}: {error.Message}");
        return new(this);
    }

    public AndConstraint<StreamAssertions> BeReadable(string because)
    {
        Reason.Set(because);
        try
        {
            return BeReadable();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StreamAssertions> NotBeReadable(string because)
    {
        Reason.Set(because);
        try
        {
            return NotBeReadable();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StreamAssertions> BeWritable(string because)
    {
        Reason.Set(because);
        try
        {
            return BeWritable();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StreamAssertions> NotBeWritable(string because)
    {
        Reason.Set(because);
        try
        {
            return NotBeWritable();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StreamAssertions> BeSeekable(string because)
    {
        Reason.Set(because);
        try
        {
            return BeSeekable();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StreamAssertions> NotBeSeekable(string because)
    {
        Reason.Set(because);
        try
        {
            return NotBeSeekable();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StreamAssertions> HaveLength(long expected, string because)
    {
        Reason.Set(because);
        try
        {
            return HaveLength(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StreamAssertions> NotHaveLength(long unexpected, string because)
    {
        Reason.Set(because);
        try
        {
            return NotHaveLength(unexpected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StreamAssertions> HavePosition(long expected, string because)
    {
        Reason.Set(because);
        try
        {
            return HavePosition(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<StreamAssertions> NotHavePosition(long unexpected, string because)
    {
        Reason.Set(because);
        try
        {
            return NotHavePosition(unexpected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    // Forwards to ObjectAssertions so calls that compiled against Should(object?) still compile.
    private ObjectAssertions AsObject => new(Subject, _expression);

    private AndConstraint<StreamAssertions> Self(AndConstraint<ObjectAssertions> _) => new(this);

    private AndWhichConstraint<StreamAssertions, T> Which<T>(AndWhichConstraint<ObjectAssertions, T> result) => new(this, result.Which);

    public AndConstraint<StreamAssertions> Be(object? expected)
        => Self(AsObject.Be(expected));

    public AndConstraint<StreamAssertions> Be(object? expected, string because)
        => Self(AsObject.Be(expected, because));

    public AndConstraint<StreamAssertions> NotBe(object? unexpected)
        => Self(AsObject.NotBe(unexpected));

    public AndConstraint<StreamAssertions> NotBe(object? unexpected, string because)
        => Self(AsObject.NotBe(unexpected, because));

    public AndConstraint<StreamAssertions> BeNull()
        => Self(AsObject.BeNull());

    public AndConstraint<StreamAssertions> BeNull(string because)
        => Self(AsObject.BeNull(because));

    public AndConstraint<StreamAssertions> NotBeNull()
        => Self(AsObject.NotBeNull());

    public AndConstraint<StreamAssertions> NotBeNull(string because)
        => Self(AsObject.NotBeNull(because));

    public AndWhichConstraint<StreamAssertions, T> BeOfType<T>()
        => Which(AsObject.BeOfType<T>());

    public AndWhichConstraint<StreamAssertions, T> BeOfType<T>(string because)
        => Which(AsObject.BeOfType<T>(because));

    public AndConstraint<StreamAssertions> BeOfType(Type expectedType)
        => Self(AsObject.BeOfType(expectedType));

    public AndConstraint<StreamAssertions> BeOfType(Type expectedType, string because)
        => Self(AsObject.BeOfType(expectedType, because));

    public AndWhichConstraint<StreamAssertions, T> BeAssignableTo<T>()
        => Which(AsObject.BeAssignableTo<T>());

    public AndWhichConstraint<StreamAssertions, T> BeAssignableTo<T>(string because)
        => Which(AsObject.BeAssignableTo<T>(because));

    public AndConstraint<StreamAssertions> NotBeOfType<T>()
        => Self(AsObject.NotBeOfType<T>());

    public AndConstraint<StreamAssertions> NotBeAssignableTo<T>()
        => Self(AsObject.NotBeAssignableTo<T>());

    public AndConstraint<StreamAssertions> BeSameAs(object? expected)
        => Self(AsObject.BeSameAs(expected));

    public AndConstraint<StreamAssertions> NotBeSameAs(object? unexpected)
        => Self(AsObject.NotBeSameAs(unexpected));

    public AndConstraint<StreamAssertions> BeOneOf(params object?[] validValues)
        => Self(AsObject.BeOneOf(validValues));

    public AndConstraint<StreamAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => Self(AsObject.BeEquivalentTo(expectation));

    public AndConstraint<StreamAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
        => Self(AsObject.BeEquivalentTo(expectation, configure));

    public AndConstraint<StreamAssertions> Match<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        => Self(AsObject.Match(predicate));

    public AndConstraint<StreamAssertions> Match<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate, string because)
        => Self(AsObject.Match(predicate, because));
}
