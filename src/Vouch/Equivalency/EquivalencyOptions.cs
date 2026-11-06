using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace Vouch;

internal sealed class EquivalencyOptions
{
    internal static readonly EquivalencyOptions Default = new();

    internal EquivalencyOptions()
    {
    }

    internal bool StrictOrdering { get; private init; }

    // Member paths are dot-separated member names, for example "Address.City". Collection and
    // dictionary steps are not part of a path, so x => x.Items[0].Name covers the Name of every item.
    internal ImmutableArray<string> Excluded { get; private init; } = ImmutableArray<string>.Empty;

    internal ImmutableArray<string> Included { get; private init; } = ImmutableArray<string>.Empty;

    internal bool SkipMissingMembers { get; private init; }

    // Expectation member path -> name of the subject member on the same level.
    internal ImmutableDictionary<string, string> Mappings { get; private init; } = ImmutableDictionary<string, string>.Empty;

    // Expectation member path -> comparer that replaces the structural comparison of that member.
    internal ImmutableDictionary<string, Func<object, object, bool>> PathComparers { get; private init; } =
        ImmutableDictionary<string, Func<object, object, bool>>.Empty;

    // Runtime type of the expectation value -> comparer. Nullable types are stored under their underlying type.
    internal ImmutableDictionary<Type, Func<object, object, bool>> TypeComparers { get; private init; } =
        ImmutableDictionary<Type, Func<object, object, bool>>.Empty;

    // Type -> true to compare by value (Equals), false to compare by members. One entry per type, so the last call wins.
    internal ImmutableDictionary<Type, bool> KindOverrides { get; private init; } = ImmutableDictionary<Type, bool>.Empty;

    internal bool IgnoreCycles { get; private init; }

    internal int MaxDepth { get; private init; } = Equivalency.MaxDepth;

    // The engine only looks at paths when this is true, so default options take the unchanged route.
    internal bool HasPathRules { get; private init; }

    // The engine only looks up types when this is true (type comparers or by-members / by-value types).
    internal bool HasTypeRules { get; private init; }

    internal EquivalencyOptions WithStrictOrdering(bool strict = true) => Copy(strict: strict);

    internal EquivalencyOptions WithPathComparer(string path, Func<object, object, bool> comparer) =>
        Copy(pathComparers: PathComparers.SetItem(path, comparer));

    internal EquivalencyOptions WithTypeComparer(Type type, Func<object, object, bool> comparer) =>
        Copy(typeComparers: TypeComparers.SetItem(type, comparer));

    internal EquivalencyOptions WithKindOverride(Type type, bool byValue) =>
        Copy(kindOverrides: KindOverrides.SetItem(type, byValue));

    internal EquivalencyOptions WithIgnoreCycles() => Copy(ignoreCycles: true);

    internal EquivalencyOptions WithMaxDepth(int depth) => Copy(maxDepth: depth);

    internal static Func<object, object, bool> Wrap<T>(IEqualityComparer<T> comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        return (subject, expectation) => subject is T s && expectation is T e && comparer.Equals(s, e);
    }

    internal static Type KeyOf(Type type) => Nullable.GetUnderlyingType(type) ?? type;

    internal EquivalencyOptions WithExcluded(string path) => Copy(excluded: Excluded.Add(path));

    internal EquivalencyOptions WithIncluded(string path) => Copy(included: Included.Add(path));

    internal EquivalencyOptions WithSkipMissingMembers() => Copy(skipMissing: true);

    internal EquivalencyOptions WithMapping(string path, string subjectName) => Copy(mappings: Mappings.SetItem(path, subjectName));

    private EquivalencyOptions Copy(
        bool? strict = null,
        ImmutableArray<string>? excluded = null,
        ImmutableArray<string>? included = null,
        bool? skipMissing = null,
        ImmutableDictionary<string, string>? mappings = null,
        ImmutableDictionary<string, Func<object, object, bool>>? pathComparers = null,
        ImmutableDictionary<Type, Func<object, object, bool>>? typeComparers = null,
        ImmutableDictionary<Type, bool>? kindOverrides = null,
        bool? ignoreCycles = null,
        int? maxDepth = null)
    {
        var ex = excluded ?? Excluded;
        var inc = included ?? Included;
        var skip = skipMissing ?? SkipMissingMembers;
        var map = mappings ?? Mappings;
        var pathCmp = pathComparers ?? PathComparers;
        var typeCmp = typeComparers ?? TypeComparers;
        var kinds = kindOverrides ?? KindOverrides;
        return new EquivalencyOptions
        {
            StrictOrdering = strict ?? StrictOrdering,
            Excluded = ex,
            Included = inc,
            SkipMissingMembers = skip,
            Mappings = map,
            PathComparers = pathCmp,
            TypeComparers = typeCmp,
            KindOverrides = kinds,
            IgnoreCycles = ignoreCycles ?? IgnoreCycles,
            MaxDepth = maxDepth ?? MaxDepth,
            HasPathRules = ex.Length > 0 || inc.Length > 0 || map.Count > 0 || pathCmp.Count > 0,
            HasTypeRules = typeCmp.Count > 0 || kinds.Count > 0,
        };
    }

    // True when the member at this path takes part in the comparison.
    internal bool IsSelected(string path)
    {
        foreach (var excluded in Excluded)
        {
            if (IsSameOrBelow(path, excluded))
            {
                return false;
            }
        }

        if (Included.Length == 0)
        {
            return true;
        }

        foreach (var included in Included)
        {
            // Inside an included member, or a parent that leads to one.
            if (IsSameOrBelow(path, included) || IsSameOrBelow(included, path))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSameOrBelow(string path, string root) =>
        path.StartsWith(root, StringComparison.Ordinal)
        && (path.Length == root.Length || path[root.Length] == '.');

    internal static string PathOf(LambdaExpression expression, string option)
    {
        ArgumentNullException.ThrowIfNull(expression);
        var segments = new List<string>();
        var node = expression.Body;
        while (true)
        {
            switch (node)
            {
                case UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked or ExpressionType.TypeAs } cast:
                    node = cast.Operand;
                    continue;
                case MemberExpression { Member: PropertyInfo or FieldInfo } member when member.Expression is not null:
                    if (member.Member is PropertyInfo { GetMethod.IsStatic: true } || member.Member is FieldInfo { IsStatic: true })
                    {
                        throw Bad(expression, option);
                    }

                    segments.Add(member.Member.Name);
                    node = member.Expression;
                    continue;
                case BinaryExpression { NodeType: ExpressionType.ArrayIndex } element:
                    node = element.Left;
                    continue;
                case MethodCallExpression { Method.Name: "get_Item", Object: not null, Arguments.Count: 1 } call:
                    // An index such as Items[0] stands for every element: paths skip collection steps.
                    node = call.Object;
                    continue;
                case ParameterExpression when segments.Count > 0:
                    segments.Reverse();
                    return string.Join('.', segments);
                default:
                    throw Bad(expression, option);
            }
        }
    }

    private static ArgumentException Bad(LambdaExpression expression, string option) =>
        new($"{option} needs a member access such as x => x.Name or x => x.Address.City, but got '{expression}'.", "member");
}

public sealed class EquivalencyOptions<TExpectation>
{
    internal EquivalencyOptions(EquivalencyOptions state)
    {
        State = state;
    }

    internal EquivalencyOptions State { get; }

    public EquivalencyOptions<TExpectation> WithStrictOrdering() => new(State.WithStrictOrdering());

    /// <summary>Undoes <see cref="WithStrictOrdering"/>: collections are matched without regard to order again.</summary>
    public EquivalencyOptions<TExpectation> WithoutStrictOrdering() => new(State.WithStrictOrdering(false));

    /// <summary>
    /// Compares every value whose runtime type is <typeparamref name="TMember"/> (a nullable type counts as its underlying type)
    /// with <paramref name="comparer"/> instead of the structural rules. Subtypes are not matched.
    /// A later call for the same type replaces the earlier comparer.
    /// </summary>
    public EquivalencyOptions<TExpectation> Using<TMember>(IEqualityComparer<TMember> comparer) =>
        new(State.WithTypeComparer(EquivalencyOptions.KeyOf(typeof(TMember)), EquivalencyOptions.Wrap(comparer)));

    /// <summary>Compares one member of the expectation, or a nested member such as <c>x => x.Address.City</c>, with <paramref name="comparer"/>.</summary>
    public EquivalencyOptions<TExpectation> Using<TMember>(Expression<Func<TExpectation, TMember>> member, IEqualityComparer<TMember> comparer)
    {
        var path = EquivalencyOptions.PathOf(member, nameof(Using));
        return new(State.WithPathComparer(path, EquivalencyOptions.Wrap(comparer)));
    }

    /// <summary>
    /// Compares <typeparamref name="T"/> member by member, even if it overrides Equals.
    /// If this and <see cref="ComparingByValue{T}"/> are both called for the same type, the last call wins.
    /// </summary>
    public EquivalencyOptions<TExpectation> ComparingByMembers<T>() => new(State.WithKindOverride(EquivalencyOptions.KeyOf(typeof(T)), byValue: false));

    /// <summary>
    /// Compares <typeparamref name="T"/> with its Equals, even if it has members.
    /// If this and <see cref="ComparingByMembers{T}"/> are both called for the same type, the last call wins.
    /// </summary>
    public EquivalencyOptions<TExpectation> ComparingByValue<T>() => new(State.WithKindOverride(EquivalencyOptions.KeyOf(typeof(T)), byValue: true));

    /// <summary>Treats a pair of objects that is already being compared further up as equal, instead of reporting a cyclic reference.</summary>
    public EquivalencyOptions<TExpectation> IgnoringCyclicReferences() => new(State.WithIgnoreCycles());

    /// <summary>Replaces the default limit on how deeply nested the compared graph may be.</summary>
    public EquivalencyOptions<TExpectation> WithMaxDepth(int depth)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(depth, 1);
        return new(State.WithMaxDepth(depth));
    }

    /// <summary>
    /// Takes members from the runtime type of each object rather than its declared type.
    /// Vouch always does this, so the call changes nothing; it exists so such configuration reads the same as elsewhere.
    /// </summary>
    public EquivalencyOptions<TExpectation> RespectingRuntimeTypes() => new(State);

    /// <summary>Skips a member of the expectation, or a nested member such as <c>x => x.Address.City</c>.</summary>
    public EquivalencyOptions<TExpectation> Excluding(Expression<Func<TExpectation, object?>> member) =>
        new(State.WithExcluded(EquivalencyOptions.PathOf(member, nameof(Excluding))));

    /// <summary>Compares only the included members. Naming a nested member keeps its parents so it can be reached.</summary>
    public EquivalencyOptions<TExpectation> Including(Expression<Func<TExpectation, object?>> member) =>
        new(State.WithIncluded(EquivalencyOptions.PathOf(member, nameof(Including))));

    /// <summary>Skips expectation members the subject does not have instead of reporting them.</summary>
    public EquivalencyOptions<TExpectation> ExcludingMissingMembers() => new(State.WithSkipMissingMembers());

    /// <summary>Compares an expectation member with the subject member of another name on the same object.</summary>
    public EquivalencyOptions<TExpectation> WithMapping(Expression<Func<TExpectation, object?>> expectationMember, string subjectMemberName)
    {
        var path = EquivalencyOptions.PathOf(expectationMember, nameof(WithMapping));
        ArgumentException.ThrowIfNullOrEmpty(subjectMemberName);
        return new(State.WithMapping(path, subjectMemberName));
    }

    internal static EquivalencyOptions Resolve(Func<EquivalencyOptions<TExpectation>, EquivalencyOptions<TExpectation>>? configure)
    {
        if (configure is null)
        {
            return EquivalencyOptions.Default;
        }

        var result = configure(new EquivalencyOptions<TExpectation>(EquivalencyOptions.Default));
        return (result ?? throw new InvalidOperationException("The configure callback returned null.")).State;
    }
}
