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
            if (TryGetValue(key, out _))
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
    public new AndWhichConstraint<DictionaryAssertions<TKey, TValue>, KeyValuePair<TKey, TValue>> Contain(KeyValuePair<TKey, TValue> expected)
    {
        ArgumentNullException.ThrowIfNull(expected.Key, nameof(expected));
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

        if (!TryGetValue(key, out var actual))
        {
            if (TryItems(out var items))
            {
                Fail.With($"Expected {Name} to contain {Formatter.Format(pair)}, but the key was not found.{HasTail(items)}");
            }
        }
        else if (!EqualityComparer<TValue>.Default.Equals(actual, value))
        {
            Fail.With($"Expected {Name} to contain {Formatter.Format(pair)}, but found value {Formatter.Format(actual)} for that key.");
        }

        return new(this, pair);
    }

    public new AndConstraint<DictionaryAssertions<TKey, TValue>> NotContain(KeyValuePair<TKey, TValue> unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected.Key, nameof(unexpected));
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

        if (TryGetValue(key, out var actual) && EqualityComparer<TValue>.Default.Equals(actual, value))
        {
            Fail.With($"Expected {Name} not to contain {Formatter.Format(pair)}, but found it.");
        }

        return And();
    }

    // Hides the collection Equal, which is order sensitive: a dictionary has no order.
    public new AndConstraint<DictionaryAssertions<TKey, TValue>> Equal(IEnumerable<KeyValuePair<TKey, TValue>> expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        return EqualPairs(expected as IReadOnlyList<KeyValuePair<TKey, TValue>> ?? expected.ToList());
    }

    public new AndConstraint<DictionaryAssertions<TKey, TValue>> Equal(params KeyValuePair<TKey, TValue>[] expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        return EqualPairs(expected);
    }

    private AndConstraint<DictionaryAssertions<TKey, TValue>> EqualPairs(IReadOnlyList<KeyValuePair<TKey, TValue>> expected)
    {
        if (IsNull)
        {
            Fail.With($"Expected {Name} to equal {Formatter.Format(expected)}, but found null.");
            return And();
        }

        if (!TryItems(out var items))
        {
            return And();
        }

        var differences = Differences(items, expected, out var total, firstOnly: false);
        if (total > 0)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("Expected ").Append(Name).Append(" to equal ").Append(Formatter.Format(expected)).Append(", but it differs:");
            foreach (var line in differences)
            {
                sb.Append("\n  ").Append(line);
            }

            if (total > differences.Count)
            {
                sb.Append("\n  ... (").Append(total - differences.Count).Append(" more)");
            }

            Fail.With(sb.ToString());
        }

        return And();
    }

    public new AndConstraint<DictionaryAssertions<TKey, TValue>> NotEqual(IEnumerable<KeyValuePair<TKey, TValue>> unexpected)
    {
        ArgumentNullException.ThrowIfNull(unexpected);
        var other = unexpected as IReadOnlyList<KeyValuePair<TKey, TValue>> ?? unexpected.ToList();
        if (IsNull)
        {
            Fail.With($"Expected {Name} not to equal {Formatter.Format(other)}, but found null.");
            return And();
        }

        if (!TryItems(out var items))
        {
            return And();
        }

        Differences(items, other, out var total, firstOnly: true);
        if (total == 0)
        {
            Fail.With($"Expected {Name} not to equal {Formatter.Format(other)}, but it did.");
        }

        return And();
    }

    // Same keys and values in any order. Returns at most Formatter.MaxItems lines; total is the full number of differences
    // (with firstOnly it stops at the first one).
    private List<string> Differences(IReadOnlyList<KeyValuePair<TKey, TValue>> items, IReadOnlyList<KeyValuePair<TKey, TValue>> expected, out int total, bool firstOnly)
    {
        var lines = new List<string>();
        var count = 0;
        var comparer = Source is Dictionary<TKey, TValue> dictionary ? dictionary.Comparer : EqualityComparer<TKey>.Default;
        var expectedKeys = new HashSet<TKey>(comparer);
        var values = EqualityComparer<TValue>.Default;

        foreach (var pair in expected)
        {
            expectedKeys.Add(pair.Key);
            if (!TryGetValue(pair.Key, out var actual))
            {
                count++;
                if (lines.Count < Formatter.MaxItems)
                {
                    lines.Add($"missing key {Formatter.Format(pair.Key)} with value {Formatter.Format(pair.Value)}");
                }
            }
            else if (!values.Equals(actual, pair.Value))
            {
                count++;
                if (lines.Count < Formatter.MaxItems)
                {
                    lines.Add($"key {Formatter.Format(pair.Key)} has value {Formatter.Format(actual)}, expected {Formatter.Format(pair.Value)}");
                }
            }

            if (firstOnly && count > 0)
            {
                total = count;
                return lines;
            }
        }

        foreach (var pair in items)
        {
            if (!expectedKeys.Contains(pair.Key))
            {
                count++;
                if (lines.Count < Formatter.MaxItems)
                {
                    lines.Add($"unexpected key {Formatter.Format(pair.Key)} with value {Formatter.Format(pair.Value)}");
                }

                if (firstOnly)
                {
                    break;
                }
            }
        }

        total = count;
        return lines;
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

    private bool Prepare() => Source is IDictionary<TKey, TValue> or IReadOnlyDictionary<TKey, TValue> || TryItems(out _);

    private bool TryGetValue(TKey key, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TValue value)
    {
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
