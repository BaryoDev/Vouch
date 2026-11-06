using System.Linq.Expressions;

namespace Vouch;

/// <summary>
/// Assertions on a non-generic <see cref="System.Collections.IEnumerable"/> (an ArrayList, a Hashtable, a user type).
/// It has the collection assertions over <see cref="object"/>. Every member that <see cref="ObjectAssertions"/> offered for
/// such a subject before is kept with the same parameters and result, forwarding to an <see cref="ObjectAssertions"/> over the
/// original object, so a call that compiled when these subjects were asserted as plain objects still does.
/// </summary>
public sealed class NonGenericCollectionAssertions : CollectionAssertionsBase<object, NonGenericCollectionAssertions>
{
    private readonly object? _original;

    internal NonGenericCollectionAssertions(object? original, IEnumerable<object>? view, string? expression)
        : base(view, expression)
    {
        _original = original;
    }

    /// <summary>The object that was asserted on, as given to <c>Should()</c>.</summary>
    public new object? Subject => _original;

    private ObjectAssertions AsObject => new(_original, Expression);

    public new AndConstraint<ObjectAssertions> BeNull() => AsObject.BeNull();

    public new AndConstraint<ObjectAssertions> BeNull(string because) => AsObject.BeNull(because);

    public new AndConstraint<ObjectAssertions> NotBeNull() => AsObject.NotBeNull();

    public new AndConstraint<ObjectAssertions> NotBeNull(string because) => AsObject.NotBeNull(because);

    public AndConstraint<ObjectAssertions> Be(object? expected) => AsObject.Be(expected);

    public AndConstraint<ObjectAssertions> Be(object? expected, string because) => AsObject.Be(expected, because);

    public AndConstraint<ObjectAssertions> NotBe(object? unexpected) => AsObject.NotBe(unexpected);

    public AndConstraint<ObjectAssertions> NotBe(object? unexpected, string because) => AsObject.NotBe(unexpected, because);

    public AndConstraint<ObjectAssertions> BeSameAs(object? expected) => AsObject.BeSameAs(expected);

    public AndConstraint<ObjectAssertions> NotBeSameAs(object? unexpected) => AsObject.NotBeSameAs(unexpected);

    public AndWhichConstraint<ObjectAssertions, TExpected> BeOfType<TExpected>() => AsObject.BeOfType<TExpected>();

    public AndWhichConstraint<ObjectAssertions, TExpected> BeOfType<TExpected>(string because) => AsObject.BeOfType<TExpected>(because);

    public AndConstraint<ObjectAssertions> BeOfType(Type expectedType) => AsObject.BeOfType(expectedType);

    public AndConstraint<ObjectAssertions> BeOfType(Type expectedType, string because) => AsObject.BeOfType(expectedType, because);

    public AndConstraint<ObjectAssertions> NotBeOfType<TUnexpected>() => AsObject.NotBeOfType<TUnexpected>();

    public AndWhichConstraint<ObjectAssertions, TExpected> BeAssignableTo<TExpected>() => AsObject.BeAssignableTo<TExpected>();

    public AndWhichConstraint<ObjectAssertions, TExpected> BeAssignableTo<TExpected>(string because) => AsObject.BeAssignableTo<TExpected>(because);

    public AndConstraint<ObjectAssertions> NotBeAssignableTo<TUnexpected>() => AsObject.NotBeAssignableTo<TUnexpected>();

    public AndConstraint<ObjectAssertions> Match<TExpected>(Expression<Func<TExpected, bool>> predicate) => AsObject.Match(predicate);

    public AndConstraint<ObjectAssertions> Match<TExpected>(Expression<Func<TExpected, bool>> predicate, string because) => AsObject.Match(predicate, because);

    public AndConstraint<ObjectAssertions> BeOneOf(params object?[] validValues) => AsObject.BeOneOf(validValues);

    public AndConstraint<ObjectAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation) => AsObject.BeEquivalentTo(expectation);

    public AndConstraint<ObjectAssertions> BeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure) =>
        AsObject.BeEquivalentTo(expectation, configure);
}
