namespace Vouch;

internal sealed class EquivalencyOptions
{
    internal static readonly EquivalencyOptions Default = new();

    internal EquivalencyOptions()
    {
    }

    internal bool StrictOrdering { get; private init; }

    internal EquivalencyOptions WithStrictOrdering() => new() { StrictOrdering = true };
}

public sealed class EquivalencyOptions<TExpectation>
{
    internal EquivalencyOptions(EquivalencyOptions state)
    {
        State = state;
    }

    internal EquivalencyOptions State { get; }

    public EquivalencyOptions<TExpectation> WithStrictOrdering() => new(State.WithStrictOrdering());

    internal static EquivalencyOptions Resolve(Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>>? configure)
    {
        if (configure is null)
        {
            return EquivalencyOptions.Default;
        }

        var result = configure(new EquivalencyOptions<TExpectation>(EquivalencyOptions.Default));
        return (result ?? throw new InvalidOperationException("The configure callback returned null.")).State;
    }
}
