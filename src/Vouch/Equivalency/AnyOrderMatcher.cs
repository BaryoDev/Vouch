using System.Runtime.InteropServices;

namespace Vouch;

internal sealed class AnyOrderMatcher
{
    private const int IndexThreshold = 16;

    private readonly Comparison _run;
    private readonly object?[] _subject;
    private readonly bool[] _used;
    private HashIndex? _scalars;
    private HashIndex? _sequences;
    private HashIndex? _equatables;
    private bool _cheap;
    private Dictionary<Type, HashIndex?>? _byType;
    private int _firstUnused;
    private bool _exactNumbers;

    private AnyOrderMatcher(Comparison run, object?[] subject)
    {
        _run = run;
        _subject = subject;
        _used = new bool[subject.Length];
    }

    public static void Run(Comparison run, object?[] subject, object?[] expected)
    {
        var matcher = new AnyOrderMatcher(run, subject) { _exactNumbers = SingleNumericType(subject, expected) };
        for (var i = 0; i < expected.Length && !run.Stop; i++)
        {
            var item = expected[i];
            matcher._cheap = run.CapReached;
            var match = i < subject.Length && !matcher._used[i] && run.Probe(subject[i], item, i)
                ? i
                : matcher.Find(item, i);
            if (match < 0)
            {
                run.ReportNotFound(i, item);
            }
            else
            {
                matcher._used[match] = true;
            }
        }
    }

    private static bool SingleNumericType(object?[] subject, object?[] expected)
    {
        Type? type = null;
        foreach (var items in new[] { subject, expected })
        {
            foreach (var item in items)
            {
                if (item is null || !(item is Enum || Numbers.IsNumber(item)))
                {
                    continue;
                }

                var current = item.GetType();
                if (type is null)
                {
                    type = current;
                }
                else if (type != current)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static int TopLevelHash(object value, bool exactNumbers)
    {
        if (!exactNumbers)
        {
            return ScalarHash(value);
        }

        switch (value)
        {
            case double d:
                return double.IsNaN(d) ? 1 : d == 0 ? 0 : d.GetHashCode();
            case float f:
                return float.IsNaN(f) ? 1 : f == 0 ? 0 : f.GetHashCode();
            default:
                return value.GetHashCode();
        }
    }

    private static bool TrySequenceHash(Comparison run, object? item, out int hash)
    {
        hash = 0;
        if (item is null)
        {
            return false;
        }

        try
        {
            if (MemberCache.ShapeOf(item.GetType()).Kind != TypeKind.Sequence)
            {
                return false;
            }

            var elements = run.Elements(item);
            var sum = 0;
            foreach (var element in elements)
            {
                if (element is not null && MemberCache.IsScalarType(element.GetType()))
                {
                    sum = unchecked(sum + ScalarHash(element));
                }
            }

            hash = HashCode.Combine(elements.Length, sum);
            return true;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return false;
        }
    }

    private static bool TryHash(object? item, MemberGetter[] keys, out int hash)
    {
        hash = 0;
        if (item is null)
        {
            return false;
        }

        var shape = MemberCache.ShapeOf(item.GetType());
        if (shape.Kind == TypeKind.Scalar)
        {
            return false;
        }

        var code = new HashCode();
        try
        {
            for (var i = 0; i < keys.Length; i++)
            {
                var member = shape.Find(keys[i].Name);
                if (member is null)
                {
                    return false;
                }

                var value = member.Get(item);
                if (value is null)
                {
                    code.Add(0);
                }
                else if (MemberCache.IsScalarType(value.GetType()))
                {
                    code.Add(ScalarHash(value));
                }
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return false;
        }

        hash = code.ToHashCode();
        return true;
    }

    private static bool TryHashEquatable(object? item, out int hash)
    {
        hash = 0;
        if (item is null)
        {
            return false;
        }

        try
        {
            if (MemberCache.ShapeOf(item.GetType()).Kind != TypeKind.Equatable)
            {
                return false;
            }

            hash = item.GetHashCode();
            return true;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return false;
        }
    }

    private static int ScalarHash(object value)
    {
        if (value is Enum e)
        {
            var bits = ((IConvertible)e).GetTypeCode() == TypeCode.UInt64 ? unchecked((long)Convert.ToUInt64(e)) : Convert.ToInt64(e);
            return bits.GetHashCode();
        }

        return Numbers.IsNumber(value) ? Numbers.Hash(value) : value.GetHashCode();
    }

    private int Find(object? item, int index)
    {
        HashIndex? hashIndex = null;
        var hash = 0;
        var equatable = false;
        if (item is not null && _subject.Length >= IndexThreshold)
        {
            TypeShape? shape = null;
            try
            {
                shape = MemberCache.ShapeOf(item.GetType());
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
            }

            if (shape is null)
            {
                return _cheap ? -1 : Scan(item, index);
            }

            if (shape.Kind == TypeKind.Sequence)
            {
                if (TrySequenceHash(_run, item, out hash))
                {
                    hashIndex = _sequences ??= HashIndex.Build(_run, _subject, null, sequences: true, false);
                }
            }
            else if (shape.Kind == TypeKind.Scalar)
            {
                hash = TopLevelHash(item, _exactNumbers);
                hashIndex = _scalars ??= HashIndex.Build(_run, _subject, null, sequences: false, _exactNumbers);
            }
            else if (shape.Kind == TypeKind.Equatable && TryHashEquatable(item, out hash))
            {
                equatable = true;
                hashIndex = _equatables ??= HashIndex.Build(_run, _subject, null, sequences: false, false, equatables: true);
            }
            else if (shape.Kind == TypeKind.Members && shape.Members.Length > 0 && TryHash(item, shape.Members, out hash))
            {
                _byType ??= new Dictionary<Type, HashIndex?>();
                var type = item.GetType();
                if (!_byType.TryGetValue(type, out hashIndex))
                {
                    hashIndex = HashIndex.Build(_run, _subject, shape.Members, sequences: false, false);
                    _byType[type] = hashIndex;
                }
            }
        }

        if (hashIndex is null)
        {
            return _cheap && _subject.Length >= IndexThreshold ? -1 : Scan(item, index);
        }

        if (hashIndex.Buckets.TryGetValue(hash, out var bucket))
        {
            var match = Walk(ref hashIndex.Heads[bucket], hashIndex.Next, item, index);
            if (match >= 0)
            {
                return match;
            }
        }

        if (_cheap)
        {
            return -1;
        }

        var wild = Walk(ref hashIndex.WildcardHead, hashIndex.Next, item, index);
        return wild >= 0 || !equatable ? wild : Scan(item, index);
    }

    private int Walk(ref int head, int[] next, object? item, int index)
    {
        var previous = -1;
        var node = head;
        while (node >= 0)
        {
            var following = next[node];
            if (_used[node])
            {
                if (previous < 0)
                {
                    head = following;
                }
                else
                {
                    next[previous] = following;
                }
            }
            else
            {
                if (_run.Probe(_subject[node], item, index))
                {
                    return node;
                }

                previous = node;
            }

            node = following;
        }

        return -1;
    }

    private int Scan(object? item, int index)
    {
        while (_firstUnused < _subject.Length && _used[_firstUnused])
        {
            _firstUnused++;
        }

        for (var j = _firstUnused; j < _subject.Length; j++)
        {
            if (!_used[j] && _run.Probe(_subject[j], item, index))
            {
                return j;
            }
        }

        return -1;
    }

    private sealed class HashIndex
    {
        public readonly Dictionary<int, int> Buckets = new();
        public int[] Heads = [];
        public int[] Next = [];
        public int WildcardHead = -1;

        public static HashIndex Build(Comparison run, object?[] subject, MemberGetter[]? keys, bool sequences, bool exactNumbers, bool equatables = false)
        {
            var index = new HashIndex { Next = new int[subject.Length], Heads = new int[subject.Length] };
            var buckets = 0;
            for (var j = subject.Length - 1; j >= 0; j--)
            {
                int hash;
                var hashed = sequences ? TrySequenceHash(run, subject[j], out hash)
                    : equatables ? TryHashEquatable(subject[j], out hash)
                    : keys is null ? TryHashScalar(subject[j], exactNumbers, out hash) : TryHash(subject[j], keys, out hash);
                if (!hashed)
                {
                    index.Next[j] = index.WildcardHead;
                    index.WildcardHead = j;
                    continue;
                }

                ref var bucket = ref CollectionsMarshal.GetValueRefOrAddDefault(index.Buckets, hash, out var exists);
                if (!exists)
                {
                    bucket = buckets++;
                    index.Heads[bucket] = -1;
                }

                index.Next[j] = index.Heads[bucket];
                index.Heads[bucket] = j;
            }

            return index;
        }

        private static bool TryHashScalar(object? item, bool exactNumbers, out int hash)
        {
            if (item is not null && MemberCache.IsScalarType(item.GetType()))
            {
                hash = TopLevelHash(item, exactNumbers);
                return true;
            }

            hash = 0;
            return false;
        }
    }
}
