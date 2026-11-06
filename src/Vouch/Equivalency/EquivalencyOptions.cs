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

    // The engine only looks at paths when this is true, so default options take the unchanged route.
    internal bool HasPathRules { get; private init; }

    internal EquivalencyOptions WithStrictOrdering() => Copy(strict: true);

    internal EquivalencyOptions WithExcluded(string path) => Copy(excluded: Excluded.Add(path));

    internal EquivalencyOptions WithIncluded(string path) => Copy(included: Included.Add(path));

    internal EquivalencyOptions WithSkipMissingMembers() => Copy(skipMissing: true);

    internal EquivalencyOptions WithMapping(string path, string subjectName) => Copy(mappings: Mappings.SetItem(path, subjectName));

    private EquivalencyOptions Copy(
        bool? strict = null,
        ImmutableArray<string>? excluded = null,
        ImmutableArray<string>? included = null,
        bool? skipMissing = null,
        ImmutableDictionary<string, string>? mappings = null)
    {
        var ex = excluded ?? Excluded;
        var inc = included ?? Included;
        var skip = skipMissing ?? SkipMissingMembers;
        var map = mappings ?? Mappings;
        return new EquivalencyOptions
        {
            StrictOrdering = strict ?? StrictOrdering,
            Excluded = ex,
            Included = inc,
            SkipMissingMembers = skip,
            Mappings = map,
            HasPathRules = ex.Length > 0 || inc.Length > 0 || map.Count > 0,
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
