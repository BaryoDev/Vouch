namespace Vouch;

public readonly struct GuidAssertions
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal GuidAssertions(Guid subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public Guid Subject { get; }

    internal string Name => Fail.Name(_expression);

    public AndConstraint<GuidAssertions> Be(Guid expected)
    {
        if (Subject != expected)
        {
            Fail.With($"Expected {Name} to be {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<GuidAssertions> NotBe(Guid unexpected)
    {
        if (Subject == unexpected)
        {
            Fail.With($"Expected {Name} not to be {Formatter.Format(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<GuidAssertions> BeEmpty()
    {
        if (Subject != Guid.Empty)
        {
            Fail.With($"Expected {Name} to be empty, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<GuidAssertions> NotBeEmpty()
    {
        if (Subject == Guid.Empty)
        {
            Fail.With($"Expected {Name} not to be empty, but it was.");
        }

        return new(this);
    }

    public AndConstraint<GuidAssertions> BeOneOf(params Guid[] validValues)
    {
        foreach (var value in validValues)
        {
            if (Subject == value)
            {
                return new(this);
            }
        }

        Fail.With($"Expected {Name} to be one of {Formatter.FormatList(validValues)}, but found {Formatter.Format(Subject)}.");
        return new(this);
    }

    // Kept from when a Guid went through ValueAssertions<Guid>, so those calls still compile.
    public AndConstraint<GuidAssertions> BeGreaterThan(Guid expected) =>
        Compare(Subject.CompareTo(expected) > 0, "greater than", expected);

    public AndConstraint<GuidAssertions> BeGreaterThanOrEqualTo(Guid expected) =>
        Compare(Subject.CompareTo(expected) >= 0, "greater than or equal to", expected);

    public AndConstraint<GuidAssertions> BeLessThan(Guid expected) =>
        Compare(Subject.CompareTo(expected) < 0, "less than", expected);

    public AndConstraint<GuidAssertions> BeLessThanOrEqualTo(Guid expected) =>
        Compare(Subject.CompareTo(expected) <= 0, "less than or equal to", expected);

    private AndConstraint<GuidAssertions> Compare(bool ok, string relation, Guid expected)
    {
        if (!ok)
        {
            Fail.With($"Expected {Name} to be {relation} {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<GuidAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => CheckEquivalent(expectation, EquivalencyOptions.Default);

    public AndConstraint<GuidAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
        => CheckEquivalent(expectation, Equivalency.Options(configure));

    private AndConstraint<GuidAssertions> CheckEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        Equivalency.Require(Subject, expectation, typeof(TExpectation), options, Name);
        return new(this);
    }

    public AndConstraint<GuidAssertions> Be(Guid expected, string because)
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

    public AndConstraint<GuidAssertions> NotBe(Guid unexpected, string because)
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

    public AndConstraint<GuidAssertions> BeEmpty(string because)
    {
        Reason.Set(because);
        try
        {
            return BeEmpty();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<GuidAssertions> NotBeEmpty(string because)
    {
        Reason.Set(because);
        try
        {
            return NotBeEmpty();
        }
        finally
        {
            Reason.Clear();
        }
    }
}

public readonly struct NullableGuidAssertions
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    internal NullableGuidAssertions(Guid? subject, string? expression)
    {
        Subject = subject;
        _expression = expression;
    }

    public Guid? Subject { get; }

    internal string Name => Fail.Name(_expression);

    public AndConstraint<NullableGuidAssertions> Be(Guid expected)
    {
        if (Subject is not { } actual || actual != expected)
        {
            Fail.With($"Expected {Name} to be {Formatter.Format(expected)}, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableGuidAssertions> Be(Guid? expected)
    {
        if (expected is { } value)
        {
            return Be(value);
        }

        if (Subject is not null)
        {
            Fail.With($"Expected {Name} to be null, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableGuidAssertions> NotBe(Guid unexpected)
    {
        if (Subject is { } actual && actual == unexpected)
        {
            Fail.With($"Expected {Name} not to be {Formatter.Format(unexpected)}, but it was.");
        }

        return new(this);
    }

    public AndConstraint<NullableGuidAssertions> BeEmpty()
    {
        if (Subject is not { } actual || actual != Guid.Empty)
        {
            Fail.With($"Expected {Name} to be empty, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndConstraint<NullableGuidAssertions> NotBeEmpty()
    {
        if (Subject is { } actual && actual == Guid.Empty)
        {
            Fail.With($"Expected {Name} not to be empty, but it was.");
        }

        return new(this);
    }

    public AndConstraint<NullableGuidAssertions> BeOneOf(params Guid[] validValues)
    {
        if (Subject is { } actual)
        {
            foreach (var value in validValues)
            {
                if (actual == value)
                {
                    return new(this);
                }
            }
        }

        Fail.With($"Expected {Name} to be one of {Formatter.FormatList(validValues)}, but found {Formatter.Format(Subject)}.");
        return new(this);
    }

    public AndConstraint<NullableGuidAssertions> BeNull()
    {
        if (Subject.HasValue)
        {
            Fail.With($"Expected {Name} to be null, but found {Formatter.Format(Subject)}.");
        }

        return new(this);
    }

    public AndWhichConstraint<NullableGuidAssertions, Guid> NotBeNull()
    {
        if (Subject is not { } value)
        {
            Fail.With($"Expected {Name} not to be null, but it was.");
            return new(this, default);
        }

        return new(this, value);
    }

    public AndConstraint<NullableGuidAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => CheckEquivalent(expectation, EquivalencyOptions.Default);

    public AndConstraint<NullableGuidAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
        => CheckEquivalent(expectation, Equivalency.Options(configure));

    private AndConstraint<NullableGuidAssertions> CheckEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        Equivalency.Require(Subject, expectation, typeof(TExpectation), options, Name);
        return new(this);
    }

    public AndConstraint<NullableGuidAssertions> Be(Guid expected, string because)
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

    public AndConstraint<NullableGuidAssertions> Be(Guid? expected, string because)
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

    public AndConstraint<NullableGuidAssertions> NotBe(Guid unexpected, string because)
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
}
