using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace Vouch;

/// <summary>
/// Assertions on an <see cref="ImmutableArray{T}"/>. It has the collection assertions over the items, and keeps every member
/// that <see cref="ValueAssertions{T}"/> offered for an immutable array before (<c>Be</c>, <c>NotBe</c>, <c>BeOneOf</c>,
/// <c>BeEquivalentTo</c>, <c>Subject</c>), so a call that compiled then still does. A default (uninitialised) array is treated
/// as null by the collection assertions.
/// </summary>
public sealed class ImmutableArrayAssertions<T> : CollectionAssertionsBase<T, ImmutableArrayAssertions<T>>
{
    private readonly ImmutableArray<T> _array;

    internal ImmutableArrayAssertions(ImmutableArray<T> subject, string? expression)
        : base(ImmutableCollectionsMarshal.AsArray(subject), expression)
    {
        _array = subject;
    }

    /// <summary>The array that was asserted on.</summary>
    public new ImmutableArray<T> Subject => _array;

    /// <summary>Equality is <see cref="ImmutableArray{T}.Equals(ImmutableArray{T})"/>: the same backing array, not equal items.</summary>
    public AndConstraint<ImmutableArrayAssertions<T>> Be(ImmutableArray<T> expected)
    {
        if (!_array.Equals(expected))
        {
            Fail.With($"Expected {Name} to be {Formatter.Format(expected)}, but found {Formatter.Format(_array)}.");
        }

        return And();
    }

    public AndConstraint<ImmutableArrayAssertions<T>> Be(ImmutableArray<T> expected, string because)
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

    public AndConstraint<ImmutableArrayAssertions<T>> NotBe(ImmutableArray<T> unexpected)
    {
        if (_array.Equals(unexpected))
        {
            Fail.With($"Expected {Name} not to be {Formatter.Format(unexpected)}, but it was.");
        }

        return And();
    }

    public AndConstraint<ImmutableArrayAssertions<T>> NotBe(ImmutableArray<T> unexpected, string because)
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

    public AndConstraint<ImmutableArrayAssertions<T>> BeOneOf(params ImmutableArray<T>[] validValues)
    {
        ArgumentNullException.ThrowIfNull(validValues);
        foreach (var value in validValues)
        {
            if (_array.Equals(value))
            {
                return And();
            }
        }

        Fail.With($"Expected {Name} to be one of {Formatter.FormatList(validValues)}, but found {Formatter.Format(_array)}.");
        return And();
    }

    // These two take any expectation, as ValueAssertions<ImmutableArray<T>> did, and compare the array itself.
    public AndConstraint<ImmutableArrayAssertions<T>> BeEquivalentTo<TExpectation>(TExpectation expectation)
        => CheckEquivalent(expectation, EquivalencyOptions.Default);

    public AndConstraint<ImmutableArrayAssertions<T>> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
        => CheckEquivalent(expectation, Equivalency.Options(configure));

    private AndConstraint<ImmutableArrayAssertions<T>> CheckEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        Equivalency.Require(_array, expectation, typeof(TExpectation), options, Name);
        return And();
    }
}
