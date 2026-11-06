using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Vouch;

public sealed class DictionaryAssertions<TKey, TValue>
    : CollectionAssertionsBase<KeyValuePair<TKey, TValue>, DictionaryAssertions<TKey, TValue>>
    where TKey : notnull
{
    internal DictionaryAssertions(IEnumerable<KeyValuePair<TKey, TValue>>? subject, string? expression)
        : base(subject, expression)
    {
    }

    public AndWhichConstraint<DictionaryAssertions<TKey, TValue>, TValue> ContainKey(TKey expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        if (IsNull)
        {
            Fail.With($"Expected {Name} to contain key {Formatter.Format(expected)}, but found null.");
            return new(this, default!);
        }

        if (!Prepare())
        {
            return new(this, default!);
        }

        if (!TryGetValue(expected, out var value))
        {
            if (TryItems(out var items))
            {
                Fail.With($"Expected {Name} to contain key {Formatter.Format(expected)}, {NotFound(items)}");
            }

            return new(this, default!);
        }

        return new(this, value);
    }

    public AndConstraint<DictionaryAssertions<TKey, TValue>> NotContainKey(TKey unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected);
        if (IsNull)
        {
            Fail.With($"Expected {Name} not to contain key {Formatter.Format(unexpected)}, but found null.");
            return And();
        }

        if (!Prepare())
        {
            return And();
        }

        if (TryGetValue(unexpected, out var value))
        {
            Fail.With($"Expected {Name} not to contain key {Formatter.Format(unexpected)}, but found it with value {Formatter.Format(value)}.");
        }

        return And();
    }

    public AndConstraint<DictionaryAssertions<TKey, TValue>> ContainKeys(params TKey[] expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        foreach (var key in expected)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(expected));
        }

        if (IsNull)
        {
            Fail.With($"Expected {Name} to contain keys {Formatter.Format(expected)}, but found null.");
            return And();
        }

        if (!Prepare())
        {
            return And();
        }

        List<TKey>? missing = null;
        foreach (var key in expected)
        {
            if (!TryGetValue(key, out _))
            {
                (missing ??= []).Add(key);
            }
        }

        if (missing is not null && TryItems(out var items))
        {
            Fail.With($"Expected {Name} to contain keys {Formatter.Format(expected)}, but it is missing {Formatter.Format(missing)}.{HasTail(items)}");
        }

        return And();
    }

    public AndConstraint<DictionaryAssertions<TKey, TValue>> NotContainKeys(params TKey[] unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected);
        foreach (var key in unexpected)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(unexpected));
        }

        if (IsNull)
        {
            Fail.With($"Expected {Name} not to contain keys {Formatter.Format(unexpected)}, but found null.");
            return And();
        }

        if (!Prepare())
        {
            return And();
        }

        List<TKey>? found = null;
        foreach (var key in unexpected)
        {
            if (TryGetValue(key, out _) && (found is null || !found.Contains(key, KeyComparer)))
            {
                (found ??= []).Add(key);
            }
        }

        if (found is not null)
        {
            Fail.With($"Expected {Name} not to contain keys {Formatter.Format(unexpected)}, but found {Formatter.Format(found)}.");
        }

        return And();
    }

    public AndWhichConstraint<DictionaryAssertions<TKey, TValue>, TValue> ContainValue(TValue expected)
    {
        if (IsNull)
        {
            Fail.With($"Expected {Name} to contain value {Formatter.Format(expected)}, but found null.");
            return new(this, default!);
        }

        if (!TryFindValue(expected, out _, out var found, out var scanned))
        {
            if (scanned && TryItems(out var items))
            {
                Fail.With($"Expected {Name} to contain value {Formatter.Format(expected)}, {NotFound(items)}");
            }

            return new(this, default!);
        }

        return new(this, found);
    }

    public AndConstraint<DictionaryAssertions<TKey, TValue>> ContainValues(params TValue[] expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        return ContainAllValues(expected);
    }

    public AndConstraint<DictionaryAssertions<TKey, TValue>> ContainValues(IEnumerable<TValue> expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        return ContainAllValues(expected as IReadOnlyList<TValue> ?? expected.ToList());
    }

    private AndConstraint<DictionaryAssertions<TKey, TValue>> ContainAllValues(IReadOnlyList<TValue> expected)
    {
        if (IsNull)
        {
            Fail.With($"Expected {Name} to contain values {Formatter.Format(expected)}, but found null.");
            return And();
        }

        List<TValue>? missing = null;
        for (var i = 0; i < expected.Count; i++)
        {
            if (!TryFindValue(expected[i], out _, out _, out var scanned))
            {
                if (!scanned)
                {
                    return And();
                }

                (missing ??= []).Add(expected[i]);
            }
        }

        if (missing is not null && TryItems(out var items))
        {
            Fail.With($"Expected {Name} to contain values {Formatter.Format(expected)}, but it is missing {Formatter.Format(missing)}.{HasTail(items)}");
        }

        return And();
    }

    public AndConstraint<DictionaryAssertions<TKey, TValue>> NotContainValue(TValue unexpected)
    {
        if (IsNull)
        {
            Fail.With($"Expected {Name} not to contain value {Formatter.Format(unexpected)}, but found null.");
            return And();
        }

        if (TryFindValue(unexpected, out var key, out _, out _))
        {
            Fail.With($"Expected {Name} not to contain value {Formatter.Format(unexpected)}, but found it with key {Formatter.Format(key)}.");
        }

        return And();
    }

    public AndConstraint<DictionaryAssertions<TKey, TValue>> NotContainValues(params TValue[] unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected);
        return NotContainAnyValues(unexpected);
    }

    public AndConstraint<DictionaryAssertions<TKey, TValue>> NotContainValues(IEnumerable<TValue> unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected);
        return NotContainAnyValues(unexpected as IReadOnlyList<TValue> ?? unexpected.ToList());
    }

    private AndConstraint<DictionaryAssertions<TKey, TValue>> NotContainAnyValues(IReadOnlyList<TValue> unexpected)
    {
        if (IsNull)
        {
            Fail.With($"Expected {Name} not to contain values {Formatter.Format(unexpected)}, but found null.");
            return And();
        }

        List<TValue>? found = null;
        for (var i = 0; i < unexpected.Count; i++)
        {
            if (TryFindValue(unexpected[i], out _, out _, out _))
            {
                (found ??= []).Add(unexpected[i]);
            }
        }

        if (found is not null)
        {
            Fail.With($"Expected {Name} not to contain values {Formatter.Format(unexpected)}, but found {Formatter.Format(found)}.");
        }

        return And();
    }

    // Hides the collection Contain(T): the pair is matched by key (with the dictionary's own comparer), then by value.
    // A null key is simply not found.
    public new AndWhichConstraint<DictionaryAssertions<TKey, TValue>, KeyValuePair<TKey, TValue>> Contain(KeyValuePair<TKey, TValue> expected)
    {
        return ContainPair(expected.Key, expected.Value);
    }

    public AndConstraint<DictionaryAssertions<TKey, TValue>> Contain(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ContainPair(key, value);
        return And();
    }

    private AndWhichConstraint<DictionaryAssertions<TKey, TValue>, KeyValuePair<TKey, TValue>> ContainPair(TKey key, TValue value)
    {
        var pair = new KeyValuePair<TKey, TValue>(key, value);
        if (IsNull)
        {
            Fail.With($"Expected {Name} to contain {Formatter.Format(pair)}, but found null.");
            return new(this, pair);
        }

        if (!Prepare())
        {
            return new(this, pair);
        }

        TValue? actual;
        bool keyFound;
        bool matched;
        if (IsLookup)
        {
            keyFound = TryGetValue(key, out actual);
            matched = keyFound && EqualityComparer<TValue>.Default.Equals(actual, value);
        }
        else
        {
            matched = ScanPair(key, value, out keyFound, out actual);
        }

        if (matched)
        {
            return new(this, pair);
        }

        if (!keyFound)
        {
            if (TryItems(out var items))
            {
                Fail.With($"Expected {Name} to contain {Formatter.Format(pair)}, but the key was not found.{HasTail(items)}");
            }
        }
        else
        {
            Fail.With($"Expected {Name} to contain {Formatter.Format(pair)}, but found value {Formatter.Format(actual)} for that key.");
        }

        return new(this, pair);
    }

    public new AndConstraint<DictionaryAssertions<TKey, TValue>> NotContain(KeyValuePair<TKey, TValue> unexpected)
    {
        return NotContainPair(unexpected.Key, unexpected.Value);
    }

    public AndConstraint<DictionaryAssertions<TKey, TValue>> NotContain(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);
        return NotContainPair(key, value);
    }

    private AndConstraint<DictionaryAssertions<TKey, TValue>> NotContainPair(TKey key, TValue value)
    {
        var pair = new KeyValuePair<TKey, TValue>(key, value);
        if (IsNull)
        {
            Fail.With($"Expected {Name} not to contain {Formatter.Format(pair)}, but found null.");
            return And();
        }

        if (!Prepare())
        {
            return And();
        }

        var found = IsLookup
            ? TryGetValue(key, out var actual) && EqualityComparer<TValue>.Default.Equals(actual, value)
            : ScanPair(key, value, out _, out _);
        if (found)
        {
            Fail.With($"Expected {Name} not to contain {Formatter.Format(pair)}, but found it.");
        }

        return And();
    }

    // Scans a pair sequence for the whole pair across every entry. keyFound/first describe the first entry with that key.
    private bool ScanPair(TKey key, TValue value, out bool keyFound, out TValue? first)
    {
        keyFound = false;
        first = default;
        if (key is null)
        {
            return false;
        }

        var keys = EqualityComparer<TKey>.Default;
        var values = EqualityComparer<TValue>.Default;
        var items = Items;
        for (var i = 0; i < items.Count; i++)
        {
            if (!keys.Equals(items[i].Key, key))
            {
                continue;
            }

            if (values.Equals(items[i].Value, value))
            {
                return true;
            }

            if (!keyFound)
            {
                keyFound = true;
                first = items[i].Value;
            }
        }

        return false;
    }

    private bool IsLookup => Source is IDictionary<TKey, TValue> or IReadOnlyDictionary<TKey, TValue>;

    private IEqualityComparer<TKey> KeyComparer => Source switch
    {
        Dictionary<TKey, TValue> d => d.Comparer,
        ConcurrentDictionary<TKey, TValue> c => c.Comparer,
        ImmutableDictionary<TKey, TValue> i => i.KeyComparer,
        _ => EqualityComparer<TKey>.Default,
    };

    // A set that treats keys the way the subject's own lookup does.
    private ISet<TKey> NewKeySet() => Source switch
    {
        SortedDictionary<TKey, TValue> s => new SortedSet<TKey>(s.Comparer),
        SortedList<TKey, TValue> l => new SortedSet<TKey>(l.Comparer),
        ImmutableSortedDictionary<TKey, TValue> i => new SortedSet<TKey>(i.KeyComparer),
        _ => new HashSet<TKey>(KeyComparer),
    };

    private int SubjectCount() => Source switch
    {
        ICollection<KeyValuePair<TKey, TValue>> c => c.Count,
        IReadOnlyCollection<KeyValuePair<TKey, TValue>> r => r.Count,
        _ => TryItems(out var items) ? items.Count : -1,
    };

    private static string Describe(IEnumerable<KeyValuePair<TKey, TValue>> pairs) =>
        Formatter.Format(pairs as IReadOnlyList<KeyValuePair<TKey, TValue>> ?? pairs.ToList());

    // Hides the collection Equal, which is order sensitive: a dictionary has no order.
    public new AndConstraint<DictionaryAssertions<TKey, TValue>> Equal(IEnumerable<KeyValuePair<TKey, TValue>> expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        return EqualPairs(expected);
    }

    public new AndConstraint<DictionaryAssertions<TKey, TValue>> Equal(params KeyValuePair<TKey, TValue>[] expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        return EqualPairs(expected);
    }

    private AndConstraint<DictionaryAssertions<TKey, TValue>> EqualPairs(IEnumerable<KeyValuePair<TKey, TValue>> expected)
    {
        if (IsNull)
        {
            Fail.With($"Expected {Name} to equal {Describe(expected)}, but found null.");
            return And();
        }

        if (!Prepare())
        {
            return And();
        }

        var total = Diff(expected, firstOnly: false, validate: true, out var lines);
        if (total > 0)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("Expected ").Append(Name).Append(" to equal ").Append(Describe(expected)).Append(", but it differs:");
            if (lines is not null)
            {
                foreach (var line in lines)
                {
                    sb.Append("\n  ").Append(line);
                }
            }

            var shown = lines?.Count ?? 0;
            if (total > shown)
            {
                sb.Append("\n  ... (").Append(total - shown).Append(" more)");
            }

            Fail.With(sb.ToString());
        }

        return And();
    }

    public new AndConstraint<DictionaryAssertions<TKey, TValue>> NotEqual(IEnumerable<KeyValuePair<TKey, TValue>> unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected);
        if (IsNull)
        {
            Fail.With($"Expected {Name} not to equal {Describe(unexpected)}, but found null.");
            return And();
        }

        if (!Prepare())
        {
            return And();
        }

        if (Diff(unexpected, firstOnly: true, validate: false, out _) == 0)
        {
            Fail.With($"Expected {Name} not to equal {Describe(unexpected)}, but it did.");
        }

        return And();
    }

    private int Diff(IEnumerable<KeyValuePair<TKey, TValue>> expected, bool firstOnly, bool validate, out List<string>? lines)
    {
        if (expected is Dictionary<TKey, TValue> dictionary)
        {
            // Two dictionaries with the same comparer cannot hold a duplicate key, so no key set is needed.
            var unique = Source is Dictionary<TKey, TValue> subject && dictionary.Comparer.Equals(subject.Comparer);
            return Diff(dictionary.GetEnumerator(), expected, unique, firstOnly, validate, out lines);
        }

        return Diff(expected.GetEnumerator(), expected, false, firstOnly, validate, out lines);
    }

    // Same keys and values in any order, found through the subject's own lookup. Returns the number of differences
    // (with firstOnly it stops at the first one); lines holds at most Formatter.MaxItems descriptions, created on the first difference.
    private int Diff<TEnum>(TEnum pairs, IEnumerable<KeyValuePair<TKey, TValue>> expected, bool unique, bool firstOnly, bool validate, out List<string>? lines)
        where TEnum : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        lines = null;
        var count = 0;
        var distinct = 0;
        var values = EqualityComparer<TValue>.Default;
        ISet<TKey>? seen = null;
        HashSet<TKey>? declared = null;
        if (!unique)
        {
            seen = NewKeySet();
            declared = new HashSet<TKey>(expected is Dictionary<TKey, TValue> d ? d.Comparer : EqualityComparer<TKey>.Default);
        }

        try
        {
            while (pairs.MoveNext())
            {
                var pair = pairs.Current;
                string? line = null;
                if (pair.Key is null)
                {
                    count++;
                    line = $"missing key {Formatter.Format(pair.Key)} with value {Formatter.Format(pair.Value)}";
                }
                else
                {
                    if (declared is not null && !declared.Add(pair.Key))
                    {
                        if (validate)
                        {
                            throw new ArgumentException($"The expected pairs contain the key {Formatter.Format(pair.Key)} more than once, so they are not a valid dictionary.", nameof(expected));
                        }

                        continue;
                    }

                    distinct++;
                    seen?.Add(pair.Key);
                    if (!TryGetValue(pair.Key, out var actual))
                    {
                        count++;
                        line = $"missing key {Formatter.Format(pair.Key)} with value {Formatter.Format(pair.Value)}";
                    }
                    else if (!values.Equals(actual, pair.Value))
                    {
                        count++;
                        line = $"key {Formatter.Format(pair.Key)} has value {Formatter.Format(actual)}, expected {Formatter.Format(pair.Value)}";
                    }
                }

                if (line is not null)
                {
                    if (firstOnly)
                    {
                        return count;
                    }

                    lines ??= [];
                    if (lines.Count < Formatter.MaxItems)
                    {
                        lines.Add(line);
                    }
                }
            }
        }
        finally
        {
            pairs.Dispose();
        }

        var subjectCount = SubjectCount();
        if (subjectCount < 0)
        {
            return count;
        }

        var expectedCount = seen is null ? distinct : seen.Count;
        if (subjectCount == expectedCount)
        {
            return count;
        }

        if (firstOnly)
        {
            return count + 1;
        }

        if (seen is null)
        {
            seen = NewKeySet();
            foreach (var pair in expected)
            {
                seen.Add(pair.Key);
            }
        }

        var listed = 0;
        foreach (var pair in IsLookup ? Source! : Items)
        {
            if (!seen.Contains(pair.Key))
            {
                listed++;
                count++;
                lines ??= [];
                if (lines.Count < Formatter.MaxItems)
                {
                    lines.Add($"unexpected key {Formatter.Format(pair.Key)} with value {Formatter.Format(pair.Value)}");
                }
            }
        }

        if (listed == 0 && count == 0)
        {
            count++;
            (lines ??= []).Add($"it has {subjectCount} entries, but the expected pairs have {expectedCount} distinct keys");
        }

        return count;
    }

    // Looks a value up. scanned is false when the items could not be read (too many to materialise).
    // A Dictionary is walked with its own struct enumerator, so a hit allocates nothing.
    private bool TryFindValue(TValue value, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TKey key, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TValue found, out bool scanned)
    {
        var comparer = EqualityComparer<TValue>.Default;
        scanned = true;
        if (Source is Dictionary<TKey, TValue> dictionary)
        {
            foreach (var pair in dictionary)
            {
                if (comparer.Equals(pair.Value, value))
                {
                    key = pair.Key;
                    found = pair.Value;
                    return true;
                }
            }
        }
        else if (TryItems(out var items))
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (comparer.Equals(items[i].Value, value))
                {
                    key = items[i].Key;
                    found = items[i].Value;
                    return true;
                }
            }
        }
        else
        {
            scanned = false;
        }

        key = default;
        found = default;
        return false;
    }

    private bool Prepare() => IsLookup || TryItems(out _);

    private bool TryGetValue(TKey key, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TValue value)
    {
        if (key is null)
        {
            value = default;
            return false;
        }

        switch (Source)
        {
            case IDictionary<TKey, TValue> mutable:
                return mutable.TryGetValue(key, out value);
            case IReadOnlyDictionary<TKey, TValue> readOnly:
                return readOnly.TryGetValue(key, out value);
        }

        var comparer = EqualityComparer<TKey>.Default;
        foreach (var pair in Items)
        {
            if (comparer.Equals(pair.Key, key))
            {
                value = pair.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    public AndWhichConstraint<DictionaryAssertions<TKey, TValue>, TValue> ContainKey(TKey expected, string because)
    {
        Reason.Set(because);
        try
        {
            return ContainKey(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<DictionaryAssertions<TKey, TValue>> NotContainKey(TKey unexpected, string because)
    {
        Reason.Set(because);
        try
        {
            return NotContainKey(unexpected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<DictionaryAssertions<TKey, TValue>> NotContainKeys(TKey[] unexpected, string because)
    {
        Reason.Set(because);
        try
        {
            return NotContainKeys(unexpected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndWhichConstraint<DictionaryAssertions<TKey, TValue>, TValue> ContainValue(TValue expected, string because)
    {
        Reason.Set(because);
        try
        {
            return ContainValue(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<DictionaryAssertions<TKey, TValue>> ContainValues(TValue[] expected, string because)
    {
        Reason.Set(because);
        try
        {
            return ContainValues(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<DictionaryAssertions<TKey, TValue>> ContainValues(IEnumerable<TValue> expected, string because)
    {
        Reason.Set(because);
        try
        {
            return ContainValues(expected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<DictionaryAssertions<TKey, TValue>> NotContainValue(TValue unexpected, string because)
    {
        Reason.Set(because);
        try
        {
            return NotContainValue(unexpected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public AndConstraint<DictionaryAssertions<TKey, TValue>> NotContainValues(IEnumerable<TValue> unexpected, string because)
    {
        Reason.Set(because);
        try
        {
            return NotContainValues(unexpected);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public new AndWhichConstraint<DictionaryAssertions<TKey, TValue>, KeyValuePair<TKey, TValue>> Contain(KeyValuePair<TKey, TValue> expected, string because)
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

    public AndConstraint<DictionaryAssertions<TKey, TValue>> Contain(TKey key, TValue value, string because)
    {
        Reason.Set(because);
        try
        {
            return Contain(key, value);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public new AndConstraint<DictionaryAssertions<TKey, TValue>> NotContain(KeyValuePair<TKey, TValue> unexpected, string because)
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

    public AndConstraint<DictionaryAssertions<TKey, TValue>> NotContain(TKey key, TValue value, string because)
    {
        Reason.Set(because);
        try
        {
            return NotContain(key, value);
        }
        finally
        {
            Reason.Clear();
        }
    }

    public new AndConstraint<DictionaryAssertions<TKey, TValue>> Equal(IEnumerable<KeyValuePair<TKey, TValue>> expected, string because)
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

    public AndConstraint<DictionaryAssertions<TKey, TValue>> NotEqual(IEnumerable<KeyValuePair<TKey, TValue>> unexpected, string because)
    {
        Reason.Set(because);
        try
        {
            return NotEqual(unexpected);
        }
        finally
        {
            Reason.Clear();
        }
    }
}
