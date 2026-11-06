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
            return default;
        }

        if (!TryGetValue(expected, out var value))
        {
            Fail.With($"Expected {Name} to contain key {Formatter.Format(expected)}, {NotFound(Items)}");
            return default;
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

        List<TKey>? missing = null;
        foreach (var key in expected)
        {
            if (!TryGetValue(key, out _))
            {
                (missing ??= []).Add(key);
            }
        }

        if (missing is not null)
        {
            Fail.With($"Expected {Name} to contain keys {Formatter.Format(expected)}, but it is missing {Formatter.Format(missing)}.{HasTail(Items)}");
        }

        return And();
    }

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
}
