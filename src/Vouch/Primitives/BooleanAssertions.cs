namespace Vouch;

public readonly struct BooleanAssertions
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal BooleanAssertions(bool subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public bool Subject { get; }

    public AndConstraint<BooleanAssertions> BeTrue() => Be(true);

    public AndConstraint<BooleanAssertions> BeFalse() => Be(false);

    public AndConstraint<BooleanAssertions> Be(bool expected)
    {
        if (Subject != expected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be {Word(expected)}, but found {Word(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<BooleanAssertions> NotBe(bool unexpected)
    {
        if (Subject == unexpected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be {Word(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<BooleanAssertions> BeTrue(string because)
    {
        Reason.Set(because);
        try
        {
            return BeTrue();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<BooleanAssertions> BeFalse(string because)
    {
        Reason.Set(because);
        try
        {
            return BeFalse();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<BooleanAssertions> Be(bool expected, string because)
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

    public AndConstraint<BooleanAssertions> NotBe(bool unexpected, string because)
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

    private static string Word(bool value) => value ? "true" : "false";
}

public readonly struct NullableBooleanAssertions
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal NullableBooleanAssertions(bool? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public bool? Subject { get; }

    public AndConstraint<NullableBooleanAssertions> BeTrue() => Be(true);

    public AndConstraint<NullableBooleanAssertions> BeFalse() => Be(false);

    public AndConstraint<NullableBooleanAssertions> Be(bool expected)
    {
        if (Subject != expected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be {Word(expected)}, but found {Word(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableBooleanAssertions> NotBe(bool unexpected)
    {
        if (Subject == unexpected)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be {Word(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<NullableBooleanAssertions> BeNull()
    {
        if (Subject.HasValue)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to be null, but found {Word(Subject)}.");
        }

        return new(this);
    }

    /// <summary>Same as <see cref="NotBeNull"/>: the subject must have a value, which is returned as Which.</summary>
    public AndWhichConstraint<NullableBooleanAssertions, bool> HaveValue()
    {
        if (Subject is not { } value)
        {
            Fail.With($"Expected {Fail.Name(_expression)} to have a value, but found null.");
            return new(this, default);
        }

        return new(this, value);
    }

    /// <summary>Same as <see cref="BeNull"/>: the subject must be null.</summary>
    public AndConstraint<NullableBooleanAssertions> NotHaveValue()
    {
        if (Subject.HasValue)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to have a value, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndWhichConstraint<NullableBooleanAssertions, bool> NotBeNull()
    {
        if (Subject is not { } value)
        {
            Fail.With($"Expected {Fail.Name(_expression)} not to be null, but it was.");
            return new(this, default);
        }

        return new(this, value);
    }

    public AndConstraint<NullableBooleanAssertions> BeTrue(string because)
    {
        Reason.Set(because);
        try
        {
            return BeTrue();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<NullableBooleanAssertions> BeFalse(string because)
    {
        Reason.Set(because);
        try
        {
            return BeFalse();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<NullableBooleanAssertions> Be(bool expected, string because)
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

    public AndConstraint<NullableBooleanAssertions> NotBe(bool unexpected, string because)
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

    public AndConstraint<NullableBooleanAssertions> BeNull(string because)
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

    public AndWhichConstraint<NullableBooleanAssertions, bool> NotBeNull(string because)
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

    private static string Word(bool? value) => value is null ? "null" : value.Value ? "true" : "false";
}
