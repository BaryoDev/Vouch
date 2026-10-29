using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Vouch;

internal static class Equivalency
{
    internal const int MaxDepth = 10;
    internal const int MaxListed = 10;
    internal const int MaxItems = 1_000_000;

    public static bool AreEquivalent(object? subject, object? expectation, EquivalencyOptions options)
    {
        var run = new Comparison(options, collect: false, 0, stopOnFirst: true);
        run.CompareValues(subject, expectation);
        return run.Total == 0;
    }

    public static void Require<TSubject, TExpectation>(TSubject subject, TExpectation expectation, Type expectedType, EquivalencyOptions options, string name)
    {
        if (typeof(TSubject) == typeof(TExpectation)
            && ScalarType<TSubject>.Is
            && EqualityComparer<TSubject>.Default.Equals(subject, Unsafe.As<TExpectation, TSubject>(ref expectation)))
        {
            return;
        }

        Require((object?)subject, (object?)expectation, expectedType, options, name);
    }

    public static void Require(object? subject, object? expectation, Type expectedType, EquivalencyOptions options, string name)
    {
        if (subject is not null
            && expectation is not null
            && MemberCache.ShapeOf(expectation.GetType()).Kind == TypeKind.Scalar
            && Comparison.ScalarEquals(subject, expectation))
        {
            return;
        }

        var run = new Comparison(options, collect: true, MaxListed, stopOnFirst: false);
        run.CompareValues(subject, expectation);
        if (run.Total > 0)
        {
            Fail.With(Message(name, expectation?.GetType() ?? expectedType, run.Differences, run.Total));
        }
    }

    internal static object?[] Snapshot(IEnumerable sequence)
    {
        var items = new List<object?>();
        foreach (var item in sequence)
        {
            if (items.Count == MaxItems)
            {
                throw new TooManyItemsException();
            }

            items.Add(item);
        }

        return items.ToArray();
    }

    private static class ScalarType<T>
    {
        public static readonly bool Is = MemberCache.IsScalarType(typeof(T));
    }

    public static bool IsCollection(Type type) =>
        MemberCache.ShapeOf(type).Kind is TypeKind.Sequence or TypeKind.Dictionary;

    public static bool ElementsAreCollections(Type sequence)
    {
        foreach (var contract in sequence.GetInterfaces())
        {
            if (contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return IsCollection(contract.GetGenericArguments()[0]);
            }
        }

        return false;
    }

    public static bool IsDictionary(object value) =>
        MemberCache.ShapeOf(value.GetType()).Kind == TypeKind.Dictionary;

    public static EquivalencyOptions Options(Func<EquivalencyOptions, EquivalencyOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        return EquivalencyOptions.Resolve(configure);
    }

    private static string Message(string name, Type expectedType, List<Difference> differences, int total)
    {
        var sb = new StringBuilder("Expected ").Append(name).Append(" to be equivalent to the expected ")
            .Append(Formatter.IsAnonymous(expectedType) ? "anonymous type" : Formatter.TypeName(expectedType)).Append(", but:");
        for (var i = 0; i < differences.Count; i++)
        {
            sb.Append("\n  ");
            if (differences[i].Path.Length > 0)
            {
                sb.Append(differences[i].Path).Append(' ');
            }

            sb.Append(differences[i].Description).Append('.');
        }

        if (total > differences.Count)
        {
            var more = total - differences.Count;
            sb.Append("\n  ... and ").Append(more).Append(more == 1 ? " more difference" : " more differences");
        }

        return sb.ToString();
    }
}

internal sealed class TooManyItemsException : Exception
{
}

internal sealed class Comparison
{
    private enum Segment : byte
    {
        Member,
        Index,
        Key,
    }

    private enum Kind : byte
    {
        Mismatch,
        NotFound,
        Count,
        NoMember,
        NoKey,
        UnexpectedKey,
        Cycle,
        Depth,
        Threw,
        TooMany,
        NoMembers,
    }

    private struct Step
    {
        public Segment Kind;
        public string? Name;
        public int Index;
        public object? Key;
    }

    private readonly struct PairKey : IEquatable<PairKey>
    {
        private readonly object _subject;
        private readonly object _expectation;
        private readonly EquivalencyOptions _options;

        public PairKey(object subject, object expectation, EquivalencyOptions options)
        {
            _subject = subject;
            _expectation = expectation;
            _options = options;
        }

        public bool Equals(PairKey other) =>
            ReferenceEquals(_subject, other._subject) && ReferenceEquals(_expectation, other._expectation) && ReferenceEquals(_options, other._options);

        public override bool Equals(object? obj) => obj is PairKey other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(RuntimeHelpers.GetHashCode(_subject), RuntimeHelpers.GetHashCode(_expectation), RuntimeHelpers.GetHashCode(_options));
    }

    // A pair already walked to the end: no differences (Failures 0) or a known difference count.
    private readonly record struct Settled(int Depth, int Failures);

    private static readonly List<Difference> NoDifferences = new();

    private readonly bool _collect;
    private readonly int _maxListed;
    private List<Difference>? _differences;
    private Dictionary<object, object?[]>? _arrays;
    private Dictionary<object, DictionaryView>? _views;
    private readonly List<Step> _path = new();
    private readonly List<object> _pairs = new();
    private Dictionary<PairKey, Settled>? _settled;

    private bool _quiet;
    private bool _stopOnFirst;

    public Comparison(EquivalencyOptions options, bool collect, int maxListed, bool stopOnFirst)
    {
        Options = options;
        _collect = collect;
        _maxListed = maxListed;
        _stopOnFirst = stopOnFirst;
        _quiet = !collect;
    }

    public List<Difference> Differences => _differences ?? NoDifferences;

    public bool CapReached => !_quiet && _differences is not null && _differences.Count >= _maxListed;

    public object?[] Elements(object sequence)
    {
        if (sequence is object?[] direct)
        {
            return direct;
        }

        if (sequence is ICollection collection)
        {
            if (collection.Count > Equivalency.MaxItems)
            {
                throw new TooManyItemsException();
            }

            var copy = new object?[collection.Count];
            collection.CopyTo(copy, 0);
            return copy;
        }

        _arrays ??= new Dictionary<object, object?[]>(ReferenceEqualityComparer.Instance);
        if (!_arrays.TryGetValue(sequence, out var items))
        {
            items = Equivalency.Snapshot((IEnumerable)sequence);
            _arrays[sequence] = items;
        }

        return items;
    }

    private DictionaryView View(object dictionary)
    {
        if (dictionary is IDictionary)
        {
            return DictionaryView.Of(dictionary);
        }

        _views ??= new Dictionary<object, DictionaryView>(ReferenceEqualityComparer.Instance);
        if (!_views.TryGetValue(dictionary, out var view))
        {
            view = DictionaryView.Of(dictionary);
            _views[dictionary] = view;
        }

        return view;
    }

    public EquivalencyOptions Options { get; }

    public int Total { get; private set; }

    public bool Stop => _stopOnFirst && Total > 0;

    private bool Counting => !_stopOnFirst && (_quiet || (_differences is not null && _differences.Count >= _maxListed));

    public void CompareValues(object? subject, object? expectation)
    {
        if (expectation is null)
        {
            if (subject is not null)
            {
                Add(Kind.Mismatch, subject, expectation);
            }

            return;
        }

        if (subject is null)
        {
            Add(Kind.Mismatch, subject, expectation);
            return;
        }

        if (ReferenceEquals(subject, expectation))
        {
            return;
        }

        try
        {
            var shape = MemberCache.ShapeOf(expectation.GetType());
            switch (shape.Kind)
            {
                case TypeKind.Scalar:
                    if (!ScalarEquals(subject, expectation))
                    {
                        Add(Kind.Mismatch, subject, expectation);
                    }

                    return;
                case TypeKind.Equatable:
                    CompareEquatable(subject, expectation);
                    return;
                default:
                    if (!Compatible(subject, shape.Kind))
                    {
                        Add(Kind.Mismatch, subject, expectation);
                        return;
                    }

                    CompareStructure(subject, expectation, shape);
                    return;
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            AddThrew(ex);
        }
    }

    public bool Probe(object? subject, object? expectation, int index)
    {
        var total = Total;
        var quiet = _quiet;
        var stop = _stopOnFirst;
        var pathCount = _path.Count;
        var pairCount = _pairs.Count;
        _quiet = true;
        _stopOnFirst = true;
        Total = 0;
        Push(index);
        CompareValues(subject, expectation);
        var equivalent = Total == 0;
        Restore(pathCount, pairCount);
        Total = total;
        _quiet = quiet;
        _stopOnFirst = stop;
        return equivalent;
    }

    public void ReportNotFound(int index, object? expected)
    {
        Push(index);
        Add(Kind.NotFound, null, expected);
        _path.RemoveAt(_path.Count - 1);
    }

    private static bool Compatible(object subject, TypeKind expected)
    {
        var actual = MemberCache.ShapeOf(subject.GetType()).Kind;
        return expected switch
        {
            TypeKind.Dictionary => actual == TypeKind.Dictionary,
            TypeKind.Sequence => actual == TypeKind.Sequence,
            _ => actual != TypeKind.Scalar,
        };
    }

    private void CompareEquatable(object subject, object expectation)
    {
        try
        {
            if (!subject.Equals(expectation))
            {
                Add(Kind.Mismatch, subject, expectation);
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            AddThrew(ex);
        }
    }

    private void CompareStructure(object subject, object expectation, TypeShape shape)
    {
        var pathCount = _path.Count;
        var pairCount = _pairs.Count;
        try
        {
            if (pathCount > Equivalency.MaxDepth)
            {
                Add(Kind.Depth, null, null);
                return;
            }

            for (var i = 0; i < pairCount; i += 2)
            {
                if (ReferenceEquals(_pairs[i], subject) && ReferenceEquals(_pairs[i + 1], expectation))
                {
                    Add(Kind.Cycle, null, null);
                    return;
                }
            }

            // The root pair is compared once, so it is not worth a dictionary.
            var memoize = pathCount > 0
                && !subject.GetType().IsValueType && !expectation.GetType().IsValueType;
            var key = default(PairKey);
            if (memoize)
            {
                key = new PairKey(subject, expectation, Options);
                if (_settled is not null && _settled.TryGetValue(key, out var known))
                {
                    if (known.Failures == 0 && pathCount <= known.Depth)
                    {
                        return;
                    }

                    if (known.Failures > 0 && pathCount == known.Depth && Counting)
                    {
                        Total = (int)Math.Min((long)Total + known.Failures, int.MaxValue);
                        return;
                    }
                }
            }

            var before = Total;
            _pairs.Add(subject);
            _pairs.Add(expectation);
            switch (shape.Kind)
            {
                case TypeKind.Dictionary:
                    CompareDictionaries(subject, expectation);
                    break;
                case TypeKind.Sequence:
                    CompareSequences(subject, expectation);
                    break;
                default:
                    CompareMembers(subject, expectation, shape);
                    break;
            }

            if (memoize && (Total == before || !_stopOnFirst))
            {
                Settle(key, pathCount, Total - before);
            }
        }
        catch (TooManyItemsException)
        {
            Restore(pathCount, pairCount);
            Add(Kind.TooMany, null, null);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            Restore(pathCount, pairCount);
            AddThrew(ex);
        }
        finally
        {
            if (_pairs.Count > pairCount)
            {
                _pairs.RemoveRange(pairCount, _pairs.Count - pairCount);
            }
        }
    }

    private void Settle(PairKey key, int depth, int failures)
    {
        _settled ??= new Dictionary<PairKey, Settled>();
        if (_settled.TryGetValue(key, out var old) && old.Failures == 0)
        {
            if (failures == 0 && depth > old.Depth)
            {
                _settled[key] = new Settled(depth, 0);
            }

            return;
        }

        _settled[key] = new Settled(depth, failures);
    }

    private void CompareMembers(object subject, object expectation, TypeShape shape)
    {
        var subjectShape = MemberCache.ShapeOf(subject.GetType());
        var members = shape.Members;
        if (members.Length == 0)
        {
            Add(Kind.NoMembers, null, null);
            return;
        }

        for (var i = 0; i < members.Length && !Stop; i++)
        {
            var member = members[i];
            var counterpart = subjectShape.Find(member.Name);
            if (counterpart is null)
            {
                Add(Kind.NoMember, null, null, member.Name);
                continue;
            }

            var pathCount = _path.Count;
            _path.Add(new Step { Kind = Segment.Member, Name = member.Name });
            try
            {
                var actual = counterpart.Get(subject);
                var expected = member.Get(expectation);
                CompareValues(actual, expected);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                AddThrew(ex);
            }

            _path.RemoveRange(pathCount, _path.Count - pathCount);
        }
    }

    private void CompareSequences(object subject, object expectation)
    {
        var actual = Elements(subject);
        var expected = Elements(expectation);
        if (actual.Length != expected.Length)
        {
            Add(Kind.Count, actual.Length, expected.Length);
            return;
        }

        if (Options.StrictOrdering)
        {
            for (var i = 0; i < expected.Length && !Stop; i++)
            {
                var pathCount = _path.Count;
                Push(i);
                CompareValues(actual[i], expected[i]);
                _path.RemoveRange(pathCount, _path.Count - pathCount);
            }

            return;
        }

        AnyOrderMatcher.Run(this, actual, expected);
    }

    private void CompareDictionaries(object subject, object expectation)
    {
        var actual = View(subject);
        var expected = View(expectation);
        var missing = false;
        foreach (var entry in expected.Entries())
        {
            if (Stop)
            {
                return;
            }

            if (!actual.TryGetValue(entry.Key, out var value))
            {
                missing = true;
                Add(Kind.NoKey, entry.Key, null);
                continue;
            }

            var pathCount = _path.Count;
            _path.Add(new Step { Kind = Segment.Key, Key = entry.Key });
            CompareValues(value, entry.Value);
            _path.RemoveRange(pathCount, _path.Count - pathCount);
        }

        if (missing || actual.Count > expected.Count)
        {
            foreach (var entry in actual.Entries())
            {
                if (Stop)
                {
                    return;
                }

                if (!expected.TryGetValue(entry.Key, out _))
                {
                    Add(Kind.UnexpectedKey, entry.Key, null);
                }
            }
        }
    }

    internal static bool ScalarEquals(object subject, object expectation)
    {
        if (subject.Equals(expectation) || Numbers.Equal(subject, expectation))
        {
            return true;
        }

        if (subject is Enum a && expectation is Enum b && a.GetType() != b.GetType())
        {
            return Convert.ToDecimal(a) == Convert.ToDecimal(b);
        }

        return false;
    }

    private void Push(int index) => _path.Add(new Step { Kind = Segment.Index, Index = index });

    private void Restore(int pathCount, int pairCount)
    {
        if (_path.Count > pathCount)
        {
            _path.RemoveRange(pathCount, _path.Count - pathCount);
        }

        if (_pairs.Count > pairCount)
        {
            _pairs.RemoveRange(pairCount, _pairs.Count - pairCount);
        }
    }

    private void AddThrew(Exception ex) => Add(Kind.Threw, ex is TargetInvocationException { InnerException: { } inner } ? inner : ex, null);

    private void Add(Kind kind, object? actual, object? expected, string? name = null)
    {
        Total++;
        if (_quiet || !_collect || (_differences is not null && _differences.Count >= _maxListed))
        {
            return;
        }

        (_differences ??= new List<Difference>()).Add(new Difference(PathText(), Describe(kind, actual, expected, name)));
    }

    private static string Describe(Kind kind, object? actual, object? expected, string? name) => kind switch
    {
        Kind.Mismatch => $"was {Formatter.Format(actual)}, expected {Formatter.Format(expected)}",
        Kind.NotFound => $"expected {Formatter.Format(expected)} was not found",
        Kind.Count => $"has {Items((int)actual!)}, expected {Items((int)expected!)}",
        Kind.NoMember => $"has no member {name}",
        Kind.NoKey => $"has no key {Formatter.Format(actual)}",
        Kind.UnexpectedKey => $"has unexpected key {Formatter.Format(actual)}",
        Kind.Cycle => "is a cyclic reference",
        Kind.Depth => $"is nested deeper than {Equivalency.MaxDepth} levels",
        Kind.TooMany => $"has more than {Equivalency.MaxItems} items, not compared",
        Kind.NoMembers => "has no members to compare",
        _ => $"threw {actual!.GetType().Name}: {((Exception)actual).Message}",
    };

    private static string Items(int count) => count == 1 ? "1 item" : $"{count} items";

    private string PathText()
    {
        if (_path.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        foreach (var step in _path)
        {
            switch (step.Kind)
            {
                case Segment.Member:
                    if (sb.Length > 0)
                    {
                        sb.Append('.');
                    }

                    sb.Append(step.Name);
                    break;
                case Segment.Index:
                    sb.Append('[').Append(step.Index).Append(']');
                    break;
                default:
                    sb.Append('[').Append(Formatter.Format(step.Key)).Append(']');
                    break;
            }
        }

        return sb.ToString();
    }
}
