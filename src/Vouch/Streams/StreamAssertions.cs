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

    public AndConstraint<StreamAssertions> HaveLength(long expected)
    {
        var expectation = $"to have length {expected}";
        if (Subject is null)
        {
            return NullSubject(expectation);
        }

        if (!Subject.CanSeek)
        {
            return Unavailable(expectation, "length");
        }

        var actual = Subject.Length;
        if (actual != expected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} {expectation}, but it has length {actual}.");
        }

        return new(this);
    }

    public AndConstraint<StreamAssertions> NotHaveLength(long unexpected)
    {
        var expectation = $"not to have length {unexpected}";
        if (Subject is null)
        {
            return NullSubject(expectation);
        }

        if (!Subject.CanSeek)
        {
            return Unavailable(expectation, "length");
        }

        if (Subject.Length == unexpected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} {expectation}, but it does.");
        }

        return new(this);
    }

    public AndConstraint<StreamAssertions> HavePosition(long expected)
    {
        var expectation = $"to have position {expected}";
        if (Subject is null)
        {
            return NullSubject(expectation);
        }

        if (!Subject.CanSeek)
        {
            return Unavailable(expectation, "position");
        }

        var actual = Subject.Position;
        if (actual != expected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} {expectation}, but it is at position {actual}.");
        }

        return new(this);
    }

    public AndConstraint<StreamAssertions> NotHavePosition(long unexpected)
    {
        var expectation = $"not to have position {unexpected}";
        if (Subject is null)
        {
            return NullSubject(expectation);
        }

        if (!Subject.CanSeek)
        {
            return Unavailable(expectation, "position");
        }

        if (Subject.Position == unexpected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} {expectation}, but it does.");
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

    public AndConstraint<StreamAssertions> BeReadable(string because) => Reasoned(because, static a => a.BeReadable());

    public AndConstraint<StreamAssertions> NotBeReadable(string because) => Reasoned(because, static a => a.NotBeReadable());

    public AndConstraint<StreamAssertions> BeWritable(string because) => Reasoned(because, static a => a.BeWritable());

    public AndConstraint<StreamAssertions> NotBeWritable(string because) => Reasoned(because, static a => a.NotBeWritable());

    public AndConstraint<StreamAssertions> BeSeekable(string because) => Reasoned(because, static a => a.BeSeekable());

    public AndConstraint<StreamAssertions> NotBeSeekable(string because) => Reasoned(because, static a => a.NotBeSeekable());

    public AndConstraint<StreamAssertions> HaveLength(long expected, string because)
        => Reasoned(because, a => a.HaveLength(expected));

    public AndConstraint<StreamAssertions> NotHaveLength(long unexpected, string because)
        => Reasoned(because, a => a.NotHaveLength(unexpected));

    public AndConstraint<StreamAssertions> HavePosition(long expected, string because)
        => Reasoned(because, a => a.HavePosition(expected));

    public AndConstraint<StreamAssertions> NotHavePosition(long unexpected, string because)
        => Reasoned(because, a => a.NotHavePosition(unexpected));

    private AndConstraint<StreamAssertions> Reasoned(string because, Func<StreamAssertions, AndConstraint<StreamAssertions>> run)
    {
        Reason.Set(because);
        try
        {
            return run(this);
        }
        finally
        {
            Reason.Clear();
        }
    }
}
