namespace Vouch;

/// <summary>
/// Returned by <c>BeInAscendingOrder</c> and <c>BeInDescendingOrder</c> with a key selector. Besides <see cref="And"/>
/// it carries the subject's items and the keys already checked, so a <c>ThenBe...</c> call can order the items that
/// tied on every earlier key.
/// </summary>
/// <remarks>
/// The constraint is generic on the key of the last ordering, so the first ordering allocates nothing: the earlier-keys
/// "tied" test is only built (as a closure) when a <c>ThenBe...</c> call is made.
/// </remarks>
public readonly struct OrderedAndConstraint<TAssertions, T, TKey>
{
    private readonly IReadOnlyList<T>? _items;
    private readonly string _name;
    private readonly Func<T, TKey>? _key;
    private readonly IComparer<TKey>? _comparer;
    private readonly Func<int, int, bool>? _tiedBefore;

    internal OrderedAndConstraint(TAssertions and, IReadOnlyList<T>? items, string name, Func<T, TKey> key, IComparer<TKey> comparer, Func<int, int, bool>? tiedBefore)
    {
        And = and;
        _items = items;
        _name = name;
        _key = key;
        _comparer = comparer;
        _tiedBefore = tiedBefore;
    }

    public TAssertions And { get; }

    public OrderedAndConstraint<TAssertions, T, TNext> ThenBeInAscendingOrder<TNext>(Func<T, TNext> keySelector) =>
        Then(keySelector, "ascending", false);

    public OrderedAndConstraint<TAssertions, T, TNext> ThenBeInDescendingOrder<TNext>(Func<T, TNext> keySelector) =>
        Then(keySelector, "descending", true);

    private OrderedAndConstraint<TAssertions, T, TNext> Then<TNext>(Func<T, TNext> next, string direction, bool descending)
    {
        ArgumentNullException.ThrowIfNull(next);
        var comparer = Comparer<TNext>.Default;
        var items = _items;
        if (items is null)
        {
            // The first ordering already failed or could not run, and reported it.
            return new(And, null, _name, next, comparer, null);
        }

        var key = _key!;
        var earlier = _comparer!;
        var before = _tiedBefore;
        bool Tied(int a, int b)
        {
            if (before is not null && !before(a, b))
            {
                return false;
            }

            try
            {
                return earlier.Compare(key(items[a]), key(items[b])) == 0;
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                return false;
            }
        }

        OrderCheck.Run(_name, items, next, comparer, Tied, direction, descending);
        return new(And, items, _name, next, comparer, Tied);
    }
}

internal static class OrderCheck
{
    /// <summary>Checks adjacent pairs. With <paramref name="tied"/>, only pairs that tied on every earlier key are compared.</summary>
    internal static void Run<T, TKey>(string name, IReadOnlyList<T> items, Func<T, TKey> key, IComparer<TKey> comparer, Func<int, int, bool>? tied, string direction, bool descending)
    {
        if (items.Count < 2)
        {
            return;
        }

        var previous = key(items[0]);
        for (var i = 1; i < items.Count; i++)
        {
            var current = key(items[i]);
            if (tied is null || tied(i - 1, i))
            {
                int order;
                try
                {
                    order = comparer.Compare(previous, current);
                }
                catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
                {
                    Fail.With($"Expected {name} to be in {direction} order, but {Formatter.Format(typeof(TKey))} is not comparable.");
                    return;
                }

                if (descending ? order < 0 : order > 0)
                {
                    var among = tied is null ? "" : ", among items with equal earlier keys";
                    Fail.With($"Expected {name} to be in {direction} order, but [{i}] {Formatter.Format(items[i])} comes after [{i - 1}] {Formatter.Format(items[i - 1])}{among}.");
                    return;
                }
            }

            previous = current;
        }
    }
}
