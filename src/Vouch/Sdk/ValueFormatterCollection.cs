namespace Vouch.Sdk;

/// <summary>
/// The registered <see cref="IValueFormatter"/>s. Copy on write, so it is safe to change while tests run in parallel.
/// The most recently added formatter is tried first, before the built-in rules.
/// </summary>
public sealed class ValueFormatterCollection
{
    private readonly object _gate = new();
    private IValueFormatter[] _items = [];

    internal ValueFormatterCollection()
    {
    }

    internal IValueFormatter[] Snapshot => Volatile.Read(ref _items);

    /// <summary>Number of registered formatters.</summary>
    public int Count => Snapshot.Length;

    /// <summary>Registers a formatter. It is tried before every formatter added earlier.</summary>
    public void Add(IValueFormatter formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        lock (_gate)
        {
            var old = _items;
            var next = new IValueFormatter[old.Length + 1];
            next[0] = formatter;
            Array.Copy(old, 0, next, 1, old.Length);
            Volatile.Write(ref _items, next);
        }
    }

    /// <summary>Removes a formatter. Returns false when it was not registered.</summary>
    public bool Remove(IValueFormatter formatter)
    {
        lock (_gate)
        {
            var index = Array.IndexOf(_items, formatter);
            if (index < 0)
            {
                return false;
            }

            var next = new IValueFormatter[_items.Length - 1];
            Array.Copy(_items, 0, next, 0, index);
            Array.Copy(_items, index + 1, next, index, next.Length - index);
            Volatile.Write(ref _items, next);
            return true;
        }
    }

    /// <summary>True when the formatter is registered.</summary>
    public bool Contains(IValueFormatter formatter) => Array.IndexOf(Snapshot, formatter) >= 0;

    /// <summary>Removes every registered formatter.</summary>
    public void Clear()
    {
        lock (_gate)
        {
            Volatile.Write(ref _items, Array.Empty<IValueFormatter>());
        }
    }
}
