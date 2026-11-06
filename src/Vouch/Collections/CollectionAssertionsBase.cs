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

    // Counts items for a check that only needs the count. A source that knows its size, or one already read, gives its exact
    // count. A lazy sequence is read once and kept (for the failure listing) up to the item cap; past the cap it is counted
    // without storing, up to limit, so a long sequence is judged on its real length instead of being rejected for it.
    private int CountUpTo(int limit)
    {
        switch (Source)
        {
            case IReadOnlyCollection<T> c:
                return c.Count;
            case ICollection<T> c:
                return c.Count;
            case System.Collections.ICollection c:
                return c.Count;
        }

        if (_list is not null)
        {
            return _list.Count;
        }

        var kept = new List<T>();
        var storing = true;
        var count = 0;
        foreach (var item in Source!)
        {
            if (storing)
            {
                if (kept.Count == Equivalency.MaxItems)
                {
                    storing = false;
                }
                else
                {
                    kept.Add(item);
                }
            }

            count++;
            if (!storing && count >= limit)
            {
                break;
            }
        }

        if (storing)
        {
            _list = kept;
        }

        return count;
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

    /// <remarks>Returns a constraint that also offers <c>ThenBeInAscendingOrder</c> and <c>ThenBeInDescendingOrder</c>.</remarks>
    public OrderedAndConstraint<TSelf, T, TKey> BeInAscendingOrder<TKey>(Func<T, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        return InOrderKeyed(Comparer<TKey>.Default, keySelector, "ascending", false);
    }

    public AndConstraint<TSelf> BeInDescendingOrder() => InOrder(Comparer<T>.Default, "descending", true);

    /// <remarks>Returns a constraint that also offers <c>ThenBeInAscendingOrder</c> and <c>ThenBeInDescendingOrder</c>.</remarks>
    public OrderedAndConstraint<TSelf, T, TKey> BeInDescendingOrder<TKey>(Func<T, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        return InOrderKeyed(Comparer<TKey>.Default, keySelector, "descending", true);
    }

    private AndConstraint<TSelf> InOrder(IComparer<T> comparer, string direction, bool descending)
    {
        CheckOrder(comparer, static x => x, direction, descending, out _);
        return And();
    }

    private OrderedAndConstraint<TSelf, T, TKey> InOrderKeyed<TKey>(IComparer<TKey> comparer, Func<T, TKey> key, string direction, bool descending)
    {
        CheckOrder(comparer, key, direction, descending, out var items);
        return new((TSelf)this, items, Name, key, comparer, null);
    }

    private void CheckOrder<TKey>(IComparer<TKey> comparer, Func<T, TKey> key, string direction, bool descending, out IReadOnlyList<T>? items)
    {
        if (IsNull)
        {
            ReportNull($"to be in {direction} order");
            items = null;
            return;
        }

        if (!TryItems(out var list))
        {
            items = null;
            return;
        }

        items = list;
        OrderCheck.Run(Name, list, key, comparer, null, direction, descending);
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
        if (IsNull)
        {
            return FailNull($"not to intersect with {Formatter.Format(Materialise(other))}");
        }

        if (!TryItems(out var items))
        {
            return And();
        }

        // The pass path scans in place: a list or ICollection<T> other is never copied, and no set is built unless they share an item.
        IReadOnlyList<T>? expected = null;
        bool intersects;
        switch (other)
        {
            case IReadOnlyList<T> list:
                intersects = Intersects(items, list);
                break;
            case ICollection<T> collection:
                intersects = false;
                for (var i = 0; !intersects && i < items.Count; i++)
                {
                    intersects = collection.Contains(items[i]);
                }

                break;
            case IReadOnlyCollection<T> sized:
                intersects = false;
                foreach (var candidate in sized)
                {
                    if (IndexOf(items, candidate) >= 0)
                    {
                        intersects = true;
                        break;
                    }
                }

                break;
            default:
                expected = Materialise(other);
                intersects = Intersects(items, expected);
                break;
        }

        if (!intersects)
        {
            return And();
        }

        expected ??= Materialise(other);
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

    public AndWhichConstraint<TSelf, T> HaveElementPreceding(T successor, T expected)
    {
        if (IsNull)
        {
            ReportNull($"to have element {Formatter.Format(expected)} preceding {Formatter.Format(successor)}");
            return new((TSelf)this, default!);
        }

        if (!TryItems(out var items))
        {
            return new((TSelf)this, default!);
        }

        var index = IndexOf(items, successor);
        if (index < 0)
        {
            Fail.With($"Expected {Name} to have element {Formatter.Format(expected)} preceding {Formatter.Format(successor)}, but {Formatter.Format(successor)} was not found.{HasTail(items)}");
            return new((TSelf)this, default!);
        }

        if (index == 0)
        {
            Fail.With($"Expected {Name} to have element {Formatter.Format(expected)} preceding {Formatter.Format(successor)}, but it is the first item and has nothing before it.");
            return new((TSelf)this, default!);
        }

        if (!EqualityComparer<T>.Default.Equals(items[index - 1], expected))
        {
            Fail.With($"Expected {Name} to have element {Formatter.Format(expected)} preceding {Formatter.Format(successor)}, but found {Formatter.Format(items[index - 1])}.");
            return new((TSelf)this, default!);
        }

        return new((TSelf)this, items[index - 1]);
    }

    public AndWhichConstraint<TSelf, T> HaveElementPreceding(T successor, T expected, string because)
    {
        Reason.Set(because);
        try
        {
            return HaveElementPreceding(successor, expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndWhichConstraint<TSelf, T> HaveElementSucceeding(T predecessor, T expected)
    {
        if (IsNull)
        {
            ReportNull($"to have element {Formatter.Format(expected)} succeeding {Formatter.Format(predecessor)}");
            return new((TSelf)this, default!);
        }

        if (!TryItems(out var items))
        {
            return new((TSelf)this, default!);
        }

        var index = IndexOf(items, predecessor);
        if (index < 0)
        {
            Fail.With($"Expected {Name} to have element {Formatter.Format(expected)} succeeding {Formatter.Format(predecessor)}, but {Formatter.Format(predecessor)} was not found.{HasTail(items)}");
            return new((TSelf)this, default!);
        }

        if (index == items.Count - 1)
        {
            Fail.With($"Expected {Name} to have element {Formatter.Format(expected)} succeeding {Formatter.Format(predecessor)}, but it is the last item and has nothing after it.");
            return new((TSelf)this, default!);
        }

        if (!EqualityComparer<T>.Default.Equals(items[index + 1], expected))
        {
            Fail.With($"Expected {Name} to have element {Formatter.Format(expected)} succeeding {Formatter.Format(predecessor)}, but found {Formatter.Format(items[index + 1])}.");
            return new((TSelf)this, default!);
        }

        return new((TSelf)this, items[index + 1]);
    }

    public AndWhichConstraint<TSelf, T> HaveElementSucceeding(T predecessor, T expected, string because)
    {
        Reason.Set(because);
        try
        {
            return HaveElementSucceeding(predecessor, expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndWhichConstraint<TSelf, T> ContainEquivalentOf<TExpectation>(TExpectation expectation) =>
        ContainEquivalent(expectation, EquivalencyOptions.Default);

    public AndWhichConstraint<TSelf, T> ContainEquivalentOf<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure) =>
        ContainEquivalent(expectation, Equivalency.Options(configure));

    public AndWhichConstraint<TSelf, T> ContainEquivalentOf<TExpectation>(TExpectation expectation, string because)
    {
        Reason.Set(because);
        try
        {
            return ContainEquivalentOf(expectation);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndWhichConstraint<TSelf, T> ContainEquivalentOf<TExpectation>(TExpectation expectation, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure, string because)
    {
        Reason.Set(because);
        try
        {
            return ContainEquivalentOf(expectation, configure);
        }
        finally
        {
            Reason.Clear();
        }
    }

    private AndWhichConstraint<TSelf, T> ContainEquivalent<TExpectation>(TExpectation expectation, EquivalencyOptions options)
    {
        if (IsNull)
        {
            ReportNull($"to contain an item equivalent to {Formatter.Format(expectation)}");
            return new((TSelf)this, default!);
        }

        if (!TryItems(out var items))
        {
            return new((TSelf)this, default!);
        }

        for (var i = 0; i < items.Count; i++)
        {
            if (Equivalency.AreEquivalent(items[i], expectation, options))
            {
                return new((TSelf)this, items[i]);
            }
        }

        Fail.With($"Expected {Name} to contain an item equivalent to {Formatter.Format(expectation)}, {NotFound(items)}");
        return new((TSelf)this, default!);
    }

    /// <summary>One assertion per item, in order. The number of assertions must equal the number of items.</summary>
    public AndConstraint<TSelf> SatisfyRespectively(params Action<T>[] assertions)
    {
        ArgumentNullException.ThrowIfNull(assertions);
        for (var i = 0; i < assertions.Length; i++)
        {
            ArgumentNullException.ThrowIfNull(assertions[i], nameof(assertions));
        }

        if (!TryRequire("to satisfy the assertions respectively", out var items))
        {
            return And();
        }

        if (items.Count != assertions.Length)
        {
            Fail.With($"Expected {Name} to contain {Plural(assertions.Length)} to satisfy the assertions respectively, but found {items.Count}{Listing(items)}");
            return And();
        }

        List<(int Index, string Message)>? failures = null;
        for (var i = 0; i < items.Count; i++)
        {
            var message = RunInspector(assertions[i], items[i]);
            if (message is not null)
            {
                (failures ??= []).Add((i, message));
            }
        }

        if (failures is not null)
        {
            var sb = new StringBuilder();
            sb.Append("Expected ").Append(Name).Append(" to satisfy the assertions respectively, but ").Append(failures.Count).Append(" did not:");
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

    // Runs one inspector with failures collected, the way AllSatisfy does: no recorder and no reason while it runs.
    private static string? RunInspector(Action<T> assertion, T item)
    {
        var outerReason = Reason.Current;
        var outerRecorder = Fail.Recorder;
        Fail.Recorder = null;
        Reason.Clear();
        try
        {
            assertion(item);
            return null;
        }
        catch (Exception ex) when (Fail.IsFailure(ex))
        {
            return Fail.FailureMessage(ex);
        }
        finally
        {
            Fail.Recorder = outerRecorder;
            Reason.Set(outerReason);
        }
    }

    /// <summary>Each predicate is matched by its own item, in any order. The number of predicates must equal the number of items.</summary>
    public AndConstraint<TSelf> Satisfy(params Expression<Func<T, bool>>[] predicates)
    {
        ArgumentNullException.ThrowIfNull(predicates);
        for (var i = 0; i < predicates.Length; i++)
        {
            ArgumentNullException.ThrowIfNull(predicates[i], nameof(predicates));
        }

        if (!TryRequire("to satisfy the predicates", out var items))
        {
            return And();
        }

        if (predicates.Length > MaxSatisfy || items.Count > MaxSatisfy)
        {
            Fail.With($"Expected {Name} to satisfy the predicates, but {items.Count} items and {predicates.Length} predicates are more than the {MaxSatisfy} that Satisfy matches.");
            return And();
        }

        var n = predicates.Length;
        if (items.Count != n)
        {
            Fail.With($"Expected {Name} to contain {Plural(n)} to satisfy the predicates, but found {items.Count}{Listing(items)}");
            return And();
        }

        if (n == 0)
        {
            return And();
        }

        var matches = new bool[n * n];
        for (var p = 0; p < n; p++)
        {
            var test = predicates[p].Compile();
            for (var j = 0; j < n; j++)
            {
                matches[p * n + j] = test(items[j]);
            }
        }

        var owner = new int[n];
        Array.Fill(owner, -1);
        var seen = new bool[n];
        var path = new int[n];
        var next = new int[n];
        var via = new int[n];
        List<int>? unmatched = null;
        for (var p = 0; p < n; p++)
        {
            Array.Clear(seen);
            if (!Assign(p, n, matches, seen, owner, path, next, via))
            {
                (unmatched ??= []).Add(p);
            }
        }

        if (unmatched is not null)
        {
            var sb = new StringBuilder();
            sb.Append("Expected ").Append(Name).Append(" to satisfy the predicates with a distinct item each, but no unused item matched:");
            var shown = Math.Min(unmatched.Count, Formatter.MaxItems);
            for (var i = 0; i < shown; i++)
            {
                sb.Append("\n  ").Append(predicates[unmatched[i]].Body);
            }

            AppendMore(sb, unmatched.Count - shown);
            Fail.With(sb.ToString());
        }

        return And();
    }

    private const int MaxSatisfy = 10_000;

    // Augmenting-path matching, so a predicate that fits several items never starves a stricter one. The search keeps
    // its own stack (at most one frame per item), so a long chain of re-assignments cannot overflow the call stack.
    private static bool Assign(int start, int n, bool[] matches, bool[] seen, int[] owner, int[] path, int[] next, int[] via)
    {
        var depth = 0;
        path[0] = start;
        next[0] = 0;
        while (depth >= 0)
        {
            var predicate = path[depth];
            var descended = false;
            for (var j = next[depth]; j < n; j++)
            {
                if (!matches[predicate * n + j] || seen[j])
                {
                    continue;
                }

                seen[j] = true;
                next[depth] = j + 1;
                via[depth] = j;
                if (owner[j] < 0)
                {
                    for (var d = depth; d >= 0; d--)
                    {
                        owner[via[d]] = path[d];
                    }

                    return true;
                }

                depth++;
                path[depth] = owner[j];
                next[depth] = 0;
                descended = true;
                break;
            }

            if (!descended)
            {
                depth--;
            }
        }

        return false;
    }

    public AndConstraint<TSelf> AllBe(T expected)
    {
        // The expectation text is built only on a failing path, so a pass allocates nothing.
        if (IsNull)
        {
            return FailNull($"to have all items be {Formatter.Format(expected)}");
        }

        if (!TryItems(out var items))
        {
            return And();
        }

        var comparer = EqualityComparer<T>.Default;
        List<int>? differing = null;
        for (var i = 0; i < items.Count; i++)
        {
            if (!comparer.Equals(items[i], expected))
            {
                (differing ??= []).Add(i);
            }
        }

        if (differing is not null)
        {
            Fail.With($"Expected {Name} to have all items be {Formatter.Format(expected)}, but found {differing.Count} that differ{Listing(items, differing)}");
        }

        return And();
    }

    public AndConstraint<TSelf> AllBe(T expected, string because)
    {
        Reason.Set(because);
        try
        {
            return AllBe(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    /// <summary>Every item is exactly of type <typeparamref name="TExpected"/>. Which is the items cast to it.</summary>
    /// <remarks>
    /// Unlike the other assertions this allocates when it passes (the array behind <c>Which</c>), so it is exempt from the 0 B rule.
    /// After a failure inside an <see cref="AssertionScope"/>, <c>Which</c> is empty. A nullable <typeparamref name="TExpected"/> matches its underlying type.
    /// </remarks>
    public AndWhichConstraint<TSelf, IReadOnlyList<TExpected>> AllBeOfType<TExpected>() => AllOfType<TExpected>(exact: true);

    public AndWhichConstraint<TSelf, IReadOnlyList<TExpected>> AllBeOfType<TExpected>(string because)
    {
        Reason.Set(because);
        try
        {
            return AllOfType<TExpected>(exact: true);
        }
        finally
        {
            Reason.Clear();
        }
    }

    /// <summary>Every item is assignable to <typeparamref name="TExpected"/>. Which is the items cast to it.</summary>
    /// <remarks>
    /// Unlike the other assertions this allocates when it passes (the array behind <c>Which</c>), so it is exempt from the 0 B rule.
    /// After a failure inside an <see cref="AssertionScope"/>, <c>Which</c> is empty.
    /// </remarks>
    public AndWhichConstraint<TSelf, IReadOnlyList<TExpected>> AllBeAssignableTo<TExpected>() => AllOfType<TExpected>(exact: false);

    public AndWhichConstraint<TSelf, IReadOnlyList<TExpected>> AllBeAssignableTo<TExpected>(string because)
    {
        Reason.Set(because);
        try
        {
            return AllOfType<TExpected>(exact: false);
        }
        finally
        {
            Reason.Clear();
        }
    }

    private AndWhichConstraint<TSelf, IReadOnlyList<TExpected>> AllOfType<TExpected>(bool exact)
    {
        if (IsNull)
        {
            ReportNull($"to have all items {TypeWording<TExpected>(exact)}");
            return new((TSelf)this, []);
        }

        if (!TryItems(out var items))
        {
            return new((TSelf)this, []);
        }

        // A nullable target is matched by its underlying type, because a boxed int? is a boxed int.
        var exactType = Nullable.GetUnderlyingType(typeof(TExpected)) ?? typeof(TExpected);
        List<int>? wrong = null;
        for (var i = 0; i < items.Count; i++)
        {
            // Tested on the item itself, so a value-type item is not boxed for the check.
            var item = items[i];
            if (item is TExpected && (!exact || item.GetType() == exactType))
            {
                continue;
            }

            (wrong ??= []).Add(i);
        }

        if (wrong is not null)
        {
            Fail.With($"Expected {Name} to have all items {TypeWording<TExpected>(exact)}, but found {wrong.Count} that are not{Listing(items, wrong)}");
            return new((TSelf)this, []);
        }

        // Only a passing check builds the array behind Which.
        var cast = new TExpected[items.Count];
        for (var i = 0; i < cast.Length; i++)
        {
            cast[i] = (TExpected)(object)items[i]!;
        }

        return new((TSelf)this, cast);
    }

    private static string TypeWording<TExpected>(bool exact) =>
        (exact ? "be of type " : "be assignable to ") + Formatter.Format(typeof(TExpected));

    public AndConstraint<TSelf> NotHaveCount(int unexpected)
    {
        if (IsNull)
        {
            return FailNull($"not to contain {Plural(unexpected)}");
        }

        // Counts without storing past the item cap, so a long lazy sequence is judged on its real length.
        var count = CountUpTo(unexpected == int.MaxValue ? unexpected : unexpected + 1);
        if (count == unexpected)
        {
            Fail.With($"Expected {Name} not to contain {Plural(unexpected)}, but it did{ListingIfStored()}");
        }

        return And();
    }

    public AndConstraint<TSelf> NotHaveCount(int unexpected, string because)
    {
        Reason.Set(because);
        try
        {
            return NotHaveCount(unexpected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<TSelf> HaveCountLessThan(int expected)
    {
        if (IsNull)
        {
            return FailNull($"to contain fewer than {Plural(expected)}");
        }

        var count = CountUpTo(expected);
        if (count >= expected)
        {
            Fail.With($"Expected {Name} to contain fewer than {Plural(expected)}, but found {FoundCount(count)}");
        }

        return And();
    }

    public AndConstraint<TSelf> HaveCountLessThan(int expected, string because)
    {
        Reason.Set(because);
        try
        {
            return HaveCountLessThan(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<TSelf> HaveCountLessThanOrEqualTo(int expected)
    {
        if (IsNull)
        {
            return FailNull($"to contain at most {Plural(expected)}");
        }

        var count = CountUpTo(expected == int.MaxValue ? expected : expected + 1);
        if (count > expected)
        {
            Fail.With($"Expected {Name} to contain at most {Plural(expected)}, but found {FoundCount(count)}");
        }

        return And();
    }

    public AndConstraint<TSelf> HaveCountLessThanOrEqualTo(int expected, string because)
    {
        Reason.Set(because);
        try
        {
            return HaveCountLessThanOrEqualTo(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<TSelf> HaveSameCount<TOther>(IEnumerable<TOther> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (IsNull)
        {
            return FailNull("to contain the same number of items as the other collection");
        }

        if (!TryCountOf(other, out var expected))
        {
            return And();
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

            Fail.With($"Expected {Name} to contain the same number of items as the other collection ({Plural(expected)}), but found {items.Count}{Listing(items)}");
        }

        return And();
    }

    public AndConstraint<TSelf> HaveSameCount<TOther>(IEnumerable<TOther> other, string because)
    {
        Reason.Set(because);
        try
        {
            return HaveSameCount(other);
        }
        finally
        {
            Reason.Clear();
        }
    }

    // Counts without copying when the other collection already knows its size. A sequence is counted up to the same
    // item cap as the subject, and a longer one is reported as a failure.
    private static bool TryCountOf<TOther>(IEnumerable<TOther> other, out int count)
    {
        switch (other)
        {
            case IReadOnlyCollection<TOther> c:
                count = c.Count;
                return true;
            case ICollection<TOther> c:
                count = c.Count;
                return true;
            case System.Collections.ICollection c:
                count = c.Count;
                return true;
        }

        count = 0;
        foreach (var _ in other)
        {
            if (count == Equivalency.MaxItems)
            {
                Fail.With($"The other collection has more than {Equivalency.MaxItems} items, not compared.");
                return false;
            }

            count++;
        }

        return true;
    }

    // The count and listing when the items were kept; otherwise the count reached (a lazy sequence past the cap).
    private string FoundCount(int count) =>
        _list is not null ? $"{_list.Count}{Listing(_list)}" : $"at least {count}.";

    private string ListingIfStored() =>
        _list is not null ? Listing(_list) : ".";

    public AndConstraint<TSelf> NotBeEquivalentTo(params T[] unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected);
        return CheckNotEquivalent(unexpected, EquivalencyOptions.Default);
    }

    public AndConstraint<TSelf> NotBeEquivalentTo(T[] unexpected, Func<EquivalencyOptions<T>, EquivalencyOptions<T>> configure)
    {
        ArgumentNullException.ThrowIfNull(unexpected);
        return CheckNotEquivalent(unexpected, Equivalency.Options(configure));
    }

    public AndConstraint<TSelf> NotBeEquivalentTo<TExpectation>(IEnumerable<TExpectation> unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected);
        return CheckNotEquivalent(unexpected, EquivalencyOptions.Default);
    }

    public AndConstraint<TSelf> NotBeEquivalentTo<TExpectation>(IEnumerable<TExpectation> unexpected, Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>> configure)
    {
        ArgumentNullException.ThrowIfNull(unexpected);
        return CheckNotEquivalent(unexpected, Equivalency.Options(configure));
    }

    public AndConstraint<TSelf> NotBeEquivalentTo<TExpectation>(IEnumerable<TExpectation> unexpected, string because)
    {
        Reason.Set(because);
        try
        {
            return NotBeEquivalentTo<TExpectation>(unexpected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    private AndConstraint<TSelf> CheckNotEquivalent(object unexpected, EquivalencyOptions options)
    {
        if (IsNull)
        {
            return FailNull($"not to be equivalent to {Formatter.Format(unexpected)}");
        }

        object? subject;
        if (Equivalency.IsDictionary(Source!) && Equivalency.IsDictionary(unexpected))
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

        if (Equivalency.AreEquivalent(subject, unexpected, options))
        {
            Fail.With($"Expected {Name} not to be equivalent to {Formatter.Format(unexpected)}, but it was.");
        }

        return And();
    }

    public AndConstraint<TSelf> IntersectWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var expected = Materialise(other);
        if (IsNull)
        {
            return FailNull($"to intersect with {Formatter.Format(expected)}");
        }

        if (!TryItems(out var items))
        {
            return And();
        }

        if (!Intersects(items, expected))
        {
            Fail.With($"Expected {Name} to intersect with {Formatter.Format(expected)}, but they have no common items.");
        }

        return And();
    }

    public AndConstraint<TSelf> IntersectWith(IEnumerable<T> other, string because)
    {
        Reason.Set(because);
        try
        {
            return IntersectWith(other);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<TSelf> NotIntersectWith(IEnumerable<T> other, string because)
    {
        Reason.Set(because);
        try
        {
            return NotIntersectWith(other);
        }
        finally
        {
            Reason.Clear();
        }
    }

    // Small inputs are scanned in place so a passing check allocates nothing; large ones go through a set.
    private static bool Intersects(IReadOnlyList<T> items, IReadOnlyList<T> expected)
    {
        if ((long)items.Count * expected.Count <= 4096)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (IndexOf(expected, items[i]) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        var set = new HashSet<T>(expected);
        for (var i = 0; i < items.Count; i++)
        {
            if (set.Contains(items[i]))
            {
                return true;
            }
        }

        return false;
    }

    public AndConstraint<TSelf> NotContainNulls()
    {
        if (!TryRequire("not to contain nulls", out var items))
        {
            return And();
        }

        List<int>? nulls = null;
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i] is null)
            {
                (nulls ??= []).Add(i);
            }
        }

        if (nulls is not null)
        {
            Fail.With($"Expected {Name} not to contain nulls, but found {nulls.Count}{Listing(items, nulls)}");
        }

        return And();
    }

    public AndConstraint<TSelf> NotContainNulls(string because)
    {
        Reason.Set(because);
        try
        {
            return NotContainNulls();
        }
        finally
        {
            Reason.Clear();
        }
    }
}
