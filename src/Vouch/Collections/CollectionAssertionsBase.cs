using System.Linq.Expressions;
using System.Text;

namespace Vouch;

public abstract class CollectionAssertionsBase<T, TSelf>
    where TSelf : CollectionAssertionsBase<T, TSelf>
{
    private readonly string? _expression;

    /// <summary>The text of the expression being asserted on, or null when none was captured.</summary>
    public string? Expression => _expression;

    private IReadOnlyList<T>? _list;

    private protected CollectionAssertionsBase(IEnumerable<T>? subject, string? expression)
    {
        Source = subject;
        _list = subject as IReadOnlyList<T>;
        _expression = expression;
    }

    public IReadOnlyList<T>? Subject => Source is null ? null : Items;

    private protected IEnumerable<T>? Source { get; }

    private protected bool IsNull => Source is null;

    private protected IReadOnlyList<T> Items => TryItems(out var items) ? items : [];

    private protected bool TryItems(out IReadOnlyList<T> items)
    {
        if (_list is not null)
        {
            items = _list;
            return true;
        }

        var list = Materialise();
        if (list is null)
        {
            items = [];
            return false;
        }

        _list = list;
        items = list;
        return true;
    }

    private List<T>? Materialise()
    {
        if (Source is ICollection<T>)
        {
            return Source.ToList();
        }

        var list = new List<T>();
        foreach (var item in Source!)
        {
            if (list.Count == Equivalency.MaxItems)
            {
                Fail.With($"{Name} has more than {Equivalency.MaxItems} items, not compared.");
                return null;
            }

            list.Add(item);
        }

        return list;
    }

    private bool TryCount(out int count)
    {
        switch (Source)
        {
            case IReadOnlyCollection<T> c:
                count = c.Count;
                return true;
            case ICollection<T> c:
                count = c.Count;
                return true;
            case System.Collections.ICollection c:
                count = c.Count;
                return true;
        }

        if (TryItems(out var items))
        {
            count = items.Count;
            return true;
        }

        count = 0;
        return false;
    }

    private protected string Name => Fail.Name(_expression);

    private void ReportNull(string expectation) => Fail.With($"Expected {Name} {expectation}, but found null.");

    private AndConstraint<TSelf> FailNull(string expectation)
    {
        ReportNull(expectation);
        return And();
    }

    private bool TryRequire(string expectation, out IReadOnlyList<T> items)
    {
        if (IsNull)
        {
            ReportNull(expectation);
            items = [];
            return false;
        }

        return TryItems(out items);
    }

    private protected AndConstraint<TSelf> And() => new((TSelf)this);

    public AndConstraint<TSelf> BeEmpty()
    {
        if (IsNull)
        {
            return FailNull("to be empty");
        }

        if (!TryCount(out var count))
        {
            return And();
        }

        if (count != 0)
        {
            if (!TryItems(out var items))
            {
                return And();
            }

            Fail.With($"Expected {Name} to be empty, but found {Plural(items.Count)}{Listing(items)}");
        }

        return And();
    }

    public AndConstraint<TSelf> NotBeEmpty()
    {
        if (IsNull)
        {
            return FailNull("not to be empty");
        }

        if (!TryCount(out var count))
        {
            return And();
        }

        if (count == 0)
        {
            Fail.With($"Expected {Name} not to be empty, but it was.");
        }

        return And();
    }

    public AndConstraint<TSelf> BeNull()
    {
        if (!IsNull)
        {
            if (!TryItems(out var items))
            {
                return And();
            }

            Fail.With($"Expected {Name} to be null, but found {Formatter.Format(items)}.");
        }

        return And();
    }

    public AndConstraint<TSelf> NotBeNull()
    {
        if (IsNull)
        {
            Fail.With($"Expected {Name} not to be null, but it was.");
        }

        return And();
    }

    public AndConstraint<TSelf> HaveCount(int expected)
    {
        if (IsNull)
        {
            return FailNull($"to contain {Plural(expected)}");
        }

        if (!TryCount(out var count))
        {
            return And();
        }

        if (count != expected)
        {
            if (!TryItems(out var items))
            {
                return And();
            }

            Fail.With($"Expected {Name} to contain {Plural(expected)}, but found {items.Count}{Listing(items)}");
        }

        return And();
    }

    public AndConstraint<TSelf> HaveCountGreaterThan(int expected)
    {
        if (IsNull)
        {
            return FailNull($"to contain more than {Plural(expected)}");
        }

        if (!TryCount(out var count))
        {
            return And();
        }

        if (count <= expected)
        {
            if (!TryItems(out var items))
            {
                return And();
            }

            Fail.With($"Expected {Name} to contain more than {Plural(expected)}, but found {items.Count}{Listing(items)}");
        }

        return And();
    }

    public AndConstraint<TSelf> HaveCountGreaterThanOrEqualTo(int expected)
    {
        if (IsNull)
        {
            return FailNull($"to contain at least {Plural(expected)}");
        }

        if (!TryCount(out var count))
        {
            return And();
        }

        if (count < expected)
        {
            if (!TryItems(out var items))
            {
                return And();
            }

            Fail.With($"Expected {Name} to contain at least {Plural(expected)}, but found {items.Count}{Listing(items)}");
        }

        return And();
    }

    public AndWhichConstraint<TSelf, T> Contain(T expected)
    {
        if (IsNull)
        {
            ReportNull($"to contain {Formatter.Format(expected)}");
            return new((TSelf)this, default!);
        }

        if (!TryItems(out var items))
        {
            return new((TSelf)this, default!);
        }

        var index = IndexOf(items, expected);
        if (index < 0)
        {
            Fail.With($"Expected {Name} to contain {Formatter.Format(expected)}, {NotFound(items)}");
            return new((TSelf)this, default!);
        }

        return new((TSelf)this, items[index]);
    }

    public AndConstraint<TSelf> Contain(IEnumerable<T> expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        return ContainAll(Materialise(expected));
    }

    public AndConstraint<TSelf> Contain(params T[] expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        return ContainAll(expected);
    }

    private AndConstraint<TSelf> ContainAll(IReadOnlyList<T> expected)
    {
        if (IsNull)
        {
            return FailNull($"to contain all of {Formatter.Format(expected)}");
        }

        if (!TryItems(out var items))
        {
            return And();
        }

        List<T>? missing = null;
        for (var i = 0; i < expected.Count; i++)
        {
            if (IndexOf(items, expected[i]) < 0)
            {
                (missing ??= []).Add(expected[i]);
            }
        }

        if (missing is not null)
        {
            Fail.With($"Expected {Name} to contain all of {Formatter.Format(expected)}, but it is missing {Formatter.Format(missing)}.{HasTail(items)}");
        }

        return And();
    }

    public AndWhichConstraint<TSelf, T> Contain(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (IsNull)
        {
            ReportNull($"to contain an item matching {predicate.Body}");
            return new((TSelf)this, default!);
        }

        if (!TryItems(out var items))
        {
            return new((TSelf)this, default!);
        }

        var match = predicate.Compile();
        for (var i = 0; i < items.Count; i++)
        {
            if (match(items[i]))
            {
                return new((TSelf)this, items[i]);
            }
        }

        Fail.With($"Expected {Name} to contain an item matching {predicate.Body}, {NotFound(items)}");
        return new((TSelf)this, default!);
    }

    public AndWhichConstraint<TSelf, T> HaveElementAt(int index, T element)
    {
        if (IsNull)
        {
            ReportNull($"to have element {Formatter.Format(element)} at index {index}");
            return new((TSelf)this, default!);
        }

        if (!TryItems(out var items))
        {
            return new((TSelf)this, default!);
        }

        if (index < 0 || index >= items.Count)
        {
            Fail.With($"Expected {Name} to have element {Formatter.Format(element)} at index {index}, but found no element at that index.{HasTail(items)}");
            return new((TSelf)this, default!);
        }

        if (!EqualityComparer<T>.Default.Equals(items[index], element))
        {
            Fail.With($"Expected {Name} to have element {Formatter.Format(element)} at index {index}, but found {Formatter.Format(items[index])}.");
            return new((TSelf)this, default!);
        }

        return new((TSelf)this, items[index]);
    }

    public AndWhichConstraint<TSelf, T> HaveElementAt(int index, T element, string because)
    {
        Reason.Set(because);
        try
        {
            return HaveElementAt(index, element);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<TSelf> NotContain(T unexpected)
    {
        if (IsNull)
        {
            return FailNull($"not to contain {Formatter.Format(unexpected)}");
        }

        if (!TryItems(out var items))
        {
            return And();
        }

        var index = IndexOf(items, unexpected);
        if (index >= 0)
        {
            Fail.With($"Expected {Name} not to contain {Formatter.Format(unexpected)}, but found it at index {index}.");
        }

        return And();
    }

    public AndConstraint<TSelf> NotContain(IEnumerable<T> unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected);
        var other = Materialise(unexpected);
        if (IsNull)
        {
            return FailNull($"not to contain any of {Formatter.Format(other)}");
        }

        if (!TryItems(out var items))
        {
            return And();
        }

        List<T>? found = null;
        for (var i = 0; i < other.Count; i++)
        {
            if (IndexOf(items, other[i]) >= 0)
            {
                (found ??= []).Add(other[i]);
            }
        }

        if (found is not null)
        {
            Fail.With($"Expected {Name} not to contain any of {Formatter.Format(other)}, but found {Formatter.Format(found)}.");
        }

        return And();
    }

    public AndConstraint<TSelf> BeEquivalentTo(params T[] expectation)
    {
        ArgumentNullException.ThrowIfNull(expectation);
        return CheckEquivalent(expectation, EquivalencyOptions.Default);
    }

    public AndConstraint<TSelf> BeEquivalentTo(T[] expectation, Func<EquivalencyOptions<T>, EquivalencyOptions<T>> configure)
    {
        ArgumentNullException.ThrowIfNull(expectation);
        return CheckEquivalent(expectation, Equivalency.Options(configure));
    }

    public AndConstraint<TSelf> BeEquivalentTo<TExpectation>(IEnumerable<TExpectation> expectation)
    {
        ArgumentNullException.ThrowIfNull(expectation);
        return CheckEquivalent(expectation, EquivalencyOptions.Default);
    }

    public AndConstraint<TSelf> BeEquivalentTo<TExpectation>(IEnumerable<TExpectation> expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
    {
        ArgumentNullException.ThrowIfNull(expectation);
        return CheckEquivalent(expectation, Equivalency.Options(configure));
    }

    private AndConstraint<TSelf> CheckEquivalent(object expectation, EquivalencyOptions options)
    {
        object? subject = null;
        if (!IsNull)
        {
            if (Equivalency.IsDictionary(Source!) && Equivalency.IsDictionary(expectation))
            {
                subject = Source;
            }
            else if (TryItems(out var items))
            {
                subject = items;
            }
            else
            {
                return And();
            }
        }

        Equivalency.Require(subject, expectation, expectation.GetType(), options, Name);
        return And();
    }

    public AndConstraint<TSelf> NotContainEquivalentOf<TExpectation>(TExpectation unexpected)
    {
        if (IsNull)
        {
            return FailNull($"not to contain an item equivalent to {Formatter.Format(unexpected)}");
        }

        if (!TryItems(out var items))
        {
            return And();
        }

        var options = EquivalencyOptions.Default;
        for (var i = 0; i < items.Count; i++)
        {
            if (Equivalency.AreEquivalent(items[i], unexpected, options))
            {
                Fail.With($"Expected {Name} not to contain an item equivalent to {Formatter.Format(unexpected)}, but found one at index {i}.");
                return And();
            }
        }

        return And();
    }

    public AndConstraint<TSelf> NotContain(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (IsNull)
        {
            return FailNull($"not to contain any item matching {predicate.Body}");
        }

        if (!TryItems(out var items))
        {
            return And();
        }

        var matching = Where(items, predicate.Compile(), expected: true);
        if (matching is not null)
        {
            Fail.With($"Expected {Name} not to contain any item matching {predicate.Body}, but found{Listing(items, matching)}");
        }

        return And();
    }

    public AndWhichConstraint<TSelf, T> ContainSingle()
    {
        if (!TryRequire("to contain a single item", out var items))
        {
            return new((TSelf)this, default!);
        }

        if (items.Count != 1)
        {
            Fail.With($"Expected {Name} to contain a single item, but found {items.Count}{Listing(items)}");
            return new((TSelf)this, default!);
        }

        return new((TSelf)this, items[0]);
    }

    public AndWhichConstraint<TSelf, T> ContainSingle(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (IsNull)
        {
            ReportNull($"to contain a single item matching {predicate.Body}");
            return new((TSelf)this, default!);
        }

        if (!TryItems(out var items))
        {
            return new((TSelf)this, default!);
        }

        var matching = Where(items, predicate.Compile(), expected: true);
        if (matching is null)
        {
            Fail.With($"Expected {Name} to contain a single item matching {predicate.Body}, but found 0.{HasTail(items)}");
            return new((TSelf)this, default!);
        }

        if (matching.Count != 1)
        {
            Fail.With($"Expected {Name} to contain a single item matching {predicate.Body}, but found {matching.Count}{Listing(items, matching)}");
            return new((TSelf)this, default!);
        }

        return new((TSelf)this, items[matching[0]]);
    }

    public AndConstraint<TSelf> OnlyContain(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (IsNull)
        {
            return FailNull($"to only contain items matching {predicate.Body}");
        }

        if (!TryItems(out var items))
        {
            return And();
        }

        var failing = Where(items, predicate.Compile(), expected: false);
        if (failing is not null)
        {
            Fail.With($"Expected {Name} to only contain items matching {predicate.Body}, but these did not{Listing(items, failing)}");
        }

        return And();
    }

    public AndConstraint<TSelf> Equal(IEnumerable<T> expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        return EqualTo(Materialise(expected));
    }

    public AndConstraint<TSelf> Equal(params T[] expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        return EqualTo(expected);
    }

    private AndConstraint<TSelf> EqualTo(IReadOnlyList<T> expected)
    {
        if (!TryRequire("to equal the expected items", out var items))
        {
            return And();
        }

        var comparer = EqualityComparer<T>.Default;
        var common = Math.Min(items.Count, expected.Count);
        var differ = -1;
        for (var i = 0; i < common; i++)
        {
            if (!comparer.Equals(items[i], expected[i]))
            {
                differ = i;
                break;
            }
        }

        if (differ < 0 && items.Count == expected.Count)
        {
            return And();
        }

        if (differ < 0)
        {
            differ = common;
        }

        var found = differ < items.Count ? Formatter.Format(items[differ]) : "nothing";
        var wanted = differ < expected.Count ? Formatter.Format(expected[differ]) : "nothing";
        var at = $"differ at index {differ}: expected {wanted}, found {found}.";
        if (items.Count != expected.Count)
        {
            Fail.With($"Expected {Name} to equal the expected items, but it has {Plural(items.Count)}, expected {expected.Count}.\n  They {at}");
            return And();
        }

        Fail.With($"Expected {Name} to equal the expected items, but they {at}");
        return And();
    }

    public AndConstraint<TSelf> NotEqual(IEnumerable<T> unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected);
        var other = Materialise(unexpected);
        if (IsNull)
        {
            return FailNull($"not to equal {Formatter.Format(other)}");
        }

        if (!TryItems(out var items))
        {
            return And();
        }

        if (items.Count != other.Count)
        {
            return And();
        }

        var comparer = EqualityComparer<T>.Default;
        for (var i = 0; i < items.Count; i++)
        {
            if (!comparer.Equals(items[i], other[i]))
            {
                return And();
            }
        }

        Fail.With($"Expected {Name} not to equal {Formatter.Format(other)}, but it did.");
        return And();
    }

    public AndConstraint<TSelf> OnlyHaveUniqueItems()
    {
        if (!TryRequire("to have unique items", out var items))
        {
            return And();
        }

        var seen = new HashSet<T>();
        for (var i = 0; i < items.Count; i++)
        {
            if (seen.Add(items[i]))
            {
                continue;
            }

            var comparer = EqualityComparer<T>.Default;
            var times = 0;
            for (var j = 0; j < items.Count; j++)
            {
                if (comparer.Equals(items[j], items[i]))
                {
                    times++;
                }
            }

            Fail.With($"Expected {Name} to have unique items, but {Formatter.Format(items[i])} appears {times} times.");
            return And();
        }

        return And();
    }

    public AndConstraint<TSelf> BeInAscendingOrder() => InOrder(Comparer<T>.Default, "ascending", false);

    public AndConstraint<TSelf> BeInAscendingOrder<TKey>(Func<T, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        return InOrder(Comparer<TKey>.Default, keySelector, "ascending", false);
    }

    public AndConstraint<TSelf> BeInDescendingOrder() => InOrder(Comparer<T>.Default, "descending", true);

    public AndConstraint<TSelf> BeInDescendingOrder<TKey>(Func<T, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        return InOrder(Comparer<TKey>.Default, keySelector, "descending", true);
    }

    private AndConstraint<TSelf> InOrder(IComparer<T> comparer, string direction, bool descending) =>
        InOrder(comparer, static x => x, direction, descending);

    private AndConstraint<TSelf> InOrder<TKey>(IComparer<TKey> comparer, Func<T, TKey> key, string direction, bool descending)
    {
        if (IsNull)
        {
            return FailNull($"to be in {direction} order");
        }

        if (!TryItems(out var items))
        {
            return And();
        }

        if (items.Count < 2)
        {
            return And();
        }

        var previous = key(items[0]);
        for (var i = 1; i < items.Count; i++)
        {
            var current = key(items[i]);
            int order;
            try
            {
                order = comparer.Compare(previous, current);
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                Fail.With($"Expected {Name} to be in {direction} order, but {Formatter.Format(typeof(TKey))} is not comparable.");
                return And();
            }

            if (descending ? order < 0 : order > 0)
            {
                Fail.With($"Expected {Name} to be in {direction} order, but [{i}] {Formatter.Format(items[i])} comes after [{i - 1}] {Formatter.Format(items[i - 1])}.");
                return And();
            }

            previous = current;
        }

        return And();
    }

    public AndConstraint<TSelf> AllSatisfy(Action<T> assertion)
    {
        ArgumentNullException.ThrowIfNull(assertion);
        if (!TryRequire("to have all items satisfy the assertion", out var items))
        {
            return And();
        }

        List<(int Index, string Message)>? failures = null;
        var outerReason = Reason.Current;
        var outerRecorder = Fail.Recorder;
        Fail.Recorder = null;
        try
        {
            for (var i = 0; i < items.Count; i++)
            {
                Reason.Clear();
                try
                {
                    assertion(items[i]);
                }
                catch (Exception ex) when (Fail.IsFailure(ex))
                {
                    (failures ??= []).Add((i, Fail.FailureMessage(ex)));
                }
                finally
                {
                    Reason.Set(outerReason);
                }
            }
        }
        finally
        {
            Fail.Recorder = outerRecorder;
        }

        if (failures is not null)
        {
            var sb = new StringBuilder();
            sb.Append("Expected ").Append(Name).Append(" to have all items satisfy the assertion, but ").Append(failures.Count).Append(" did not:");
            var shown = Math.Min(failures.Count, Formatter.MaxItems);
            for (var i = 0; i < shown; i++)
            {
                sb.Append("\n  [").Append(failures[i].Index).Append("] ").Append(failures[i].Message);
            }

            AppendMore(sb, failures.Count - shown);
            Fail.With(sb.ToString());
        }

        return And();
    }

    /// <summary>Asserts every item is equivalent to <paramref name="expectation"/>.</summary>
    /// <remarks>This goes through the equivalency engine, which allocates, so like BeEquivalentTo it is exempt from the 0 B allocation rule.</remarks>
    public AndConstraint<TSelf> AllBeEquivalentTo<TExpectation>(TExpectation expectation) =>
        AllEquivalent(expectation, EquivalencyOptions.Default);

    /// <summary>Asserts every item is equivalent to <paramref name="expectation"/>, with options.</summary>
    /// <remarks>This goes through the equivalency engine, which allocates, so like BeEquivalentTo it is exempt from the 0 B allocation rule.</remarks>
    public AndConstraint<TSelf> AllBeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure) =>
        AllEquivalent(expectation, Equivalency.Options(configure));

    /// <summary>Asserts every item is equivalent to <paramref name="expectation"/>, with options and a reason.</summary>
    /// <remarks>This goes through the equivalency engine, which allocates, so like BeEquivalentTo it is exempt from the 0 B allocation rule.</remarks>
    public AndConstraint<TSelf> AllBeEquivalentTo<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure, string because)
    {
        Reason.Set(because);
        try
        {
            return AllBeEquivalentTo(expectation, configure);
        }
        finally
        {
            Reason.Clear();
        }
    }

    /// <summary>Asserts every item is equivalent to <paramref name="expectation"/>, with a reason.</summary>
    /// <remarks>This goes through the equivalency engine, which allocates, so like BeEquivalentTo it is exempt from the 0 B allocation rule.</remarks>
    public AndConstraint<TSelf> AllBeEquivalentTo<TExpectation>(TExpectation expectation, string because)
    {
        Reason.Set(because);
        try
        {
            return AllBeEquivalentTo(expectation);
        }
        finally
        {
            Reason.Clear();
        }
    }

    private AndConstraint<TSelf> AllEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        if (IsNull)
        {
            return FailNull($"to have all items equivalent to {Formatter.Format(expectation)}");
        }

        if (!TryItems(out var items))
        {
            return And();
        }

        List<int>? failing = null;
        for (var i = 0; i < items.Count; i++)
        {
            if (!Equivalency.AreEquivalent(items[i], expectation, options))
            {
                (failing ??= []).Add(i);
            }
        }

        if (failing is not null)
        {
            Fail.With($"Expected {Name} to have all items equivalent to {Formatter.Format(expectation)}, but {failing.Count} did not{Listing(items, failing)}");
        }

        return And();
    }

    public AndConstraint<TSelf> ContainInOrder(params T[] expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        return InOrderCore(expected, consecutive: false);
    }

    public AndConstraint<TSelf> ContainInOrder(T[] expected, string because)
    {
        Reason.Set(because);
        try
        {
            return ContainInOrder(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<TSelf> ContainInOrder(IEnumerable<T> expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        return InOrderCore(Materialise(expected), consecutive: false);
    }

    public AndConstraint<TSelf> ContainInOrder(IEnumerable<T> expected, string because)
    {
        Reason.Set(because);
        try
        {
            return ContainInOrder(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<TSelf> ContainInConsecutiveOrder(params T[] expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        return InOrderCore(expected, consecutive: true);
    }

    public AndConstraint<TSelf> ContainInConsecutiveOrder(T[] expected, string because)
    {
        Reason.Set(because);
        try
        {
            return ContainInConsecutiveOrder(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<TSelf> ContainInConsecutiveOrder(IEnumerable<T> expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        return InOrderCore(Materialise(expected), consecutive: true);
    }

    public AndConstraint<TSelf> ContainInConsecutiveOrder(IEnumerable<T> expected, string because)
    {
        Reason.Set(because);
        try
        {
            return ContainInConsecutiveOrder(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    private AndConstraint<TSelf> InOrderCore(IReadOnlyList<T> expected, bool consecutive)
    {
        if (IsNull)
        {
            return FailNull($"to contain {Formatter.Format(expected)} in {(consecutive ? "consecutive " : "")}order");
        }

        if (!TryItems(out var items))
        {
            return And();
        }

        var comparer = EqualityComparer<T>.Default;
        if (consecutive)
        {
            if (expected.Count == 0)
            {
                return And();
            }

            var longest = 0;
            for (var start = 0; start < items.Count; start++)
            {
                var run = 0;
                while (run < expected.Count && start + run < items.Count && comparer.Equals(items[start + run], expected[run]))
                {
                    run++;
                }

                if (run == expected.Count)
                {
                    return And();
                }

                if (run > longest)
                {
                    longest = run;
                }
            }

            Fail.With($"Expected {Name} to contain {Formatter.Format(expected)} in consecutive order, but the longest run in order was {longest} of {expected.Count}, broken at {Formatter.Format(expected[longest])} (item {longest} of the expected items).{HasTail(items)}");
            return And();
        }

        var position = -1;
        for (var j = 0; j < expected.Count; j++)
        {
            var next = position + 1;
            while (next < items.Count && !comparer.Equals(items[next], expected[j]))
            {
                next++;
            }

            if (next >= items.Count)
            {
                var where = position < 0 ? "at all" : $"after index {position}";
                Fail.With($"Expected {Name} to contain {Formatter.Format(expected)} in order, but {Formatter.Format(expected[j])} (item {j} of the expected items) was not found {where}.{HasTail(items)}");
                return And();
            }

            position = next;
        }

        return And();
    }

    public AndConstraint<TSelf> NotIntersectWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var expected = Materialise(other);
        if (IsNull)
        {
            return FailNull($"not to intersect with {Formatter.Format(expected)}");
        }

        if (!TryItems(out var items))
        {
            return And();
        }

        var set = new HashSet<T>(expected);
        var seen = new HashSet<T>();
        List<T>? common = null;
        for (var i = 0; i < items.Count; i++)
        {
            if (set.Contains(items[i]) && seen.Add(items[i]))
            {
                (common ??= []).Add(items[i]);
            }
        }

        if (common is not null)
        {
            Fail.With($"Expected {Name} not to intersect with {Formatter.Format(expected)}, but found common items {Formatter.Format(common)}.");
        }

        return And();
    }

    public AndConstraint<TSelf> BeSubsetOf(IEnumerable<T> superset)
    {
        ArgumentNullException.ThrowIfNull(superset);
        var expected = Materialise(superset);
        if (IsNull)
        {
            return FailNull($"to be a subset of {Formatter.Format(expected)}");
        }

        if (!TryItems(out var items))
        {
            return And();
        }

        var set = new HashSet<T>(expected);
        var seen = new HashSet<T>();
        List<T>? extra = null;
        for (var i = 0; i < items.Count; i++)
        {
            if (!set.Contains(items[i]) && seen.Add(items[i]))
            {
                (extra ??= []).Add(items[i]);
            }
        }

        if (extra is not null)
        {
            Fail.With($"Expected {Name} to be a subset of {Formatter.Format(expected)}, but {Formatter.Format(extra)} are not in it.");
        }

        return And();
    }

    private static IReadOnlyList<T> Materialise(IEnumerable<T> source) =>
        source as IReadOnlyList<T> ?? source.ToList();

    private static int IndexOf(IReadOnlyList<T> items, T value)
    {
        var comparer = EqualityComparer<T>.Default;
        for (var i = 0; i < items.Count; i++)
        {
            if (comparer.Equals(items[i], value))
            {
                return i;
            }
        }

        return -1;
    }

    private static List<int>? Where(IReadOnlyList<T> items, Func<T, bool> predicate, bool expected)
    {
        List<int>? indexes = null;
        for (var i = 0; i < items.Count; i++)
        {
            if (predicate(items[i]) == expected)
            {
                (indexes ??= []).Add(i);
            }
        }

        return indexes;
    }

    private protected static string Plural(int count) => count == 1 ? "1 item" : $"{count} items";

    private protected static string Listing(IReadOnlyList<T> items, List<int>? indexes = null) =>
        (indexes?.Count ?? items.Count) == 0 ? "." : ":" + Lines(items, indexes);

    private protected static string NotFound(IReadOnlyList<T> items) =>
        items.Count == 0 ? "but it was empty." : "but it did not. It has:" + Lines(items);

    private protected static string HasTail(IReadOnlyList<T> items) =>
        items.Count == 0 ? "" : " It has:" + Lines(items);

    private static string Lines(IReadOnlyList<T> items, List<int>? indexes = null)
    {
        var total = indexes?.Count ?? items.Count;
        var shown = Math.Min(total, Formatter.MaxItems);
        var sb = new StringBuilder();
        for (var i = 0; i < shown; i++)
        {
            var index = indexes?[i] ?? i;
            sb.Append("\n  [").Append(index).Append("] ").Append(Formatter.Format(items[index]));
        }

        AppendMore(sb, total - shown);
        return sb.ToString();
    }

    private static void AppendMore(StringBuilder sb, int more)
    {
        if (more > 0)
        {
            sb.Append("\n  ... (").Append(more).Append(" more)");
        }
    }

    public AndConstraint<TSelf> BeEmpty(string because)
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

    public AndConstraint<TSelf> NotBeEmpty(string because)
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

    public AndConstraint<TSelf> BeNull(string because)
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

    public AndConstraint<TSelf> NotBeNull(string because)
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

    public AndConstraint<TSelf> HaveCount(int expected, string because)
    {
        Reason.Set(because);
        try
        {
            return HaveCount(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    /// <remarks>
    /// On a collection of strings, a second string argument is read as the reason, not as another expected item (same as FluentAssertions). Use <c>Contain(["a", "b"])</c> to check several items.
    /// </remarks>
    public AndWhichConstraint<TSelf, T> Contain(T expected, string because)
    {
        Reason.Set(because);
        try
        {
            return Contain(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<TSelf> Contain(IEnumerable<T> expected, string because)
    {
        Reason.Set(because);
        try
        {
            return Contain(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndWhichConstraint<TSelf, T> Contain(Expression<Func<T, bool>> predicate, string because)
    {
        Reason.Set(because);
        try
        {
            return Contain(predicate);
        }
        finally
        {
            Reason.Clear();
        }
    }

    /// <remarks>
    /// On a collection of strings, a second string argument is read as the reason, not as another expected item (same as FluentAssertions). Use <c>Contain(["a", "b"])</c> to check several items.
    /// </remarks>
    public AndConstraint<TSelf> NotContain(T unexpected, string because)
    {
        Reason.Set(because);
        try
        {
            return NotContain(unexpected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<TSelf> NotContain(IEnumerable<T> unexpected, string because)
    {
        Reason.Set(because);
        try
        {
            return NotContain(unexpected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<TSelf> NotContain(Expression<Func<T, bool>> predicate, string because)
    {
        Reason.Set(because);
        try
        {
            return NotContain(predicate);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndWhichConstraint<TSelf, T> ContainSingle(string because)
    {
        Reason.Set(because);
        try
        {
            return ContainSingle();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndWhichConstraint<TSelf, T> ContainSingle(Expression<Func<T, bool>> predicate, string because)
    {
        Reason.Set(because);
        try
        {
            return ContainSingle(predicate);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<TSelf> OnlyContain(Expression<Func<T, bool>> predicate, string because)
    {
        Reason.Set(because);
        try
        {
            return OnlyContain(predicate);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<TSelf> Equal(IEnumerable<T> expected, string because)
    {
        Reason.Set(because);
        try
        {
            return Equal(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<TSelf> BeEquivalentTo<TExpectation>(IEnumerable<TExpectation> expectation, string because)
    {
        Reason.Set(because);
        try
        {
            return BeEquivalentTo<TExpectation>(expectation);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<TSelf> BeNullOrEmpty()
    {
        if (IsNull)
        {
            return And();
        }

        if (!TryCount(out var count))
        {
            return And();
        }

        if (count != 0)
        {
            if (!TryItems(out var items))
            {
                return And();
            }

            Fail.With($"Expected {Name} to be null or empty, but found {Plural(items.Count)}{Listing(items)}");
        }

        return And();
    }

    public AndConstraint<TSelf> BeNullOrEmpty(string because)
    {
        Reason.Set(because);
        try
        {
            return BeNullOrEmpty();
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<TSelf> BeSameAs(IEnumerable<T> expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        if (IsNull)
        {
            return FailNull($"to refer to {Formatter.Format(expected)}");
        }

        if (!ReferenceEquals(Source, expected))
        {
            if (!TryItems(out var other))
            {
                return And();
            }

            Fail.With($"Expected {Name} to refer to {Formatter.Format(expected)}, but found another collection: {Formatter.Format(other)}.");
        }

        return And();
    }

    public AndConstraint<TSelf> EndWith(IEnumerable<T> expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        var tail = Materialise(expected);
        if (IsNull)
        {
            return FailNull($"to end with {Formatter.Format(tail)}");
        }

        if (!TryItems(out var items))
        {
            return And();
        }

        var offset = items.Count - tail.Count;
        var comparer = EqualityComparer<T>.Default;
        var matches = offset >= 0;
        for (var i = 0; matches && i < tail.Count; i++)
        {
            matches = comparer.Equals(items[offset + i], tail[i]);
        }

        if (!matches)
        {
            Fail.With($"Expected {Name} to end with {Formatter.Format(tail)}, but found {Formatter.Format(items)}.");
        }

        return And();
    }

    public AndConstraint<TSelf> BeInAscendingOrder(IComparer<T> comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        return InOrder(comparer, "ascending", false);
    }

    public AndConstraint<TSelf> BeInDescendingOrder(IComparer<T> comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        return InOrder(comparer, "descending", true);
    }
}
