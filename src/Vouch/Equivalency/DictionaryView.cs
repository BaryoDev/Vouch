using System.Diagnostics.CodeAnalysis;
using System.Collections;

namespace Vouch;

internal sealed class DictionaryView
{
    private readonly IDictionary? _dictionary;
    private readonly Dictionary<object, object?>? _snapshot;

    private DictionaryView(IDictionary dictionary) => _dictionary = dictionary;

    private DictionaryView(Dictionary<object, object?> snapshot) => _snapshot = snapshot;

    public int Count => _dictionary?.Count ?? _snapshot!.Count;

    [RequiresUnreferencedCode(TrimMessages.Equivalency)]
    public static DictionaryView Of(object dictionary)
    {
        if (dictionary is IDictionary typed)
        {
            return new DictionaryView(typed);
        }

        var snapshot = new Dictionary<object, object?>();
        var seen = 0;
        foreach (var pair in (IEnumerable)dictionary)
        {
            if (seen++ == Equivalency.MaxItems)
            {
                throw new TooManyItemsException();
            }

            var shape = MemberCache.ShapeOf(pair!.GetType());
            var key = shape.Find("Key")!.Get(pair);
            if (key is not null)
            {
                snapshot[key] = shape.Find("Value")!.Get(pair);
            }
        }

        return new DictionaryView(snapshot);
    }

    public bool TryGetValue(object key, out object? value)
    {
        if (_dictionary is not null)
        {
            if (_dictionary.Contains(key))
            {
                value = _dictionary[key];
                return true;
            }

            value = null;
            return false;
        }

        return _snapshot!.TryGetValue(key, out value);
    }

    public IEnumerable<KeyValuePair<object, object?>> Entries()
    {
        if (_dictionary is not null)
        {
            foreach (DictionaryEntry entry in _dictionary)
            {
                yield return new KeyValuePair<object, object?>(entry.Key, entry.Value);
            }

            yield break;
        }

        foreach (var pair in _snapshot!)
        {
            yield return pair;
        }
    }
}
