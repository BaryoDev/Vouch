using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Vouch;

internal sealed class MemberGetter
{
    private readonly PropertyInfo? _property;
    private readonly FieldInfo? _field;

    public MemberGetter(PropertyInfo property)
    {
        _property = property;
        Name = property.Name;
        Type = property.PropertyType;
    }

    public MemberGetter(FieldInfo field)
    {
        _field = field;
        Name = field.Name;
        Type = field.FieldType;
    }

    public string Name { get; }

    public Type Type { get; }

    public object? Get(object target) => _property is not null ? _property.GetValue(target) : _field!.GetValue(target);
}

internal enum TypeKind
{
    Scalar,
    Dictionary,
    Sequence,
    Members,
    Equatable,
}

internal sealed class TypeShape
{
    private const int LinearLookupLimit = 8;

    private Dictionary<string, MemberGetter>? _byName;

    [RequiresUnreferencedCode(TrimMessages.Members)]
    public TypeShape(Type type, TypeKind? forced = null)
    {
        Kind = forced ?? MemberCache.Classify(type);
        Members = MemberCache.Read(type);
    }

    public TypeKind Kind { get; }

    public MemberGetter[] Members { get; }

    public MemberGetter? Find(string name)
    {
        var members = Members;
        if (members.Length <= LinearLookupLimit)
        {
            for (var i = 0; i < members.Length; i++)
            {
                if (members[i].Name == name)
                {
                    return members[i];
                }
            }

            return null;
        }

        var map = _byName;
        if (map is null)
        {
            map = new Dictionary<string, MemberGetter>(members.Length);
            for (var i = 0; i < members.Length; i++)
            {
                map.TryAdd(members[i].Name, members[i]);
            }

            _byName = map;
        }

        return map.GetValueOrDefault(name);
    }
}

internal static class MemberCache
{
    private static readonly ConcurrentDictionary<Type, TypeShape> Shapes = new();

    private static readonly ConcurrentDictionary<(Type, TypeKind), TypeShape> Forced = new();

    // The shape of a type whose kind was chosen by an option instead of Classify.
    [RequiresUnreferencedCode(TrimMessages.Members)]
    public static TypeShape ShapeOf(Type type, TypeKind forced) =>
        Forced.TryGetValue((type, forced), out var shape) ? shape : Forced.GetOrAdd((type, forced), Build(type, forced));

    [RequiresUnreferencedCode(TrimMessages.Members)]
    public static MemberGetter[] For(Type type) => ShapeOf(type).Members;

    [RequiresUnreferencedCode(TrimMessages.Members)]
    public static TypeShape ShapeOf(Type type) =>
        Shapes.TryGetValue(type, out var shape) ? shape : Shapes.GetOrAdd(type, Build(type, null));

    [RequiresUnreferencedCode(TrimMessages.Members)]
    private static TypeShape Build(Type type, TypeKind? forced) => new(type, forced);

    public static bool IsScalarType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(DateOnly)
            || type == typeof(TimeOnly)
            || type == typeof(TimeSpan)
            || type == typeof(Guid);
    }

    [RequiresUnreferencedCode(TrimMessages.Members)]
    internal static TypeKind Classify(Type type)
    {
        if (IsScalarType(type))
        {
            return TypeKind.Scalar;
        }

        if (Formatter.IsSafeOnly(type))
        {
            return TypeKind.Equatable;
        }

        if (typeof(IDictionary).IsAssignableFrom(type) || ImplementsReadOnlyDictionary(type))
        {
            return TypeKind.Dictionary;
        }

        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            return TypeKind.Sequence;
        }

        if (Formatter.IsRecord(type) || Formatter.IsAnonymous(type) || IsTuple(type))
        {
            return TypeKind.Members;
        }

        return OverridesEquals(type) ? TypeKind.Equatable : TypeKind.Members;
    }

    [RequiresUnreferencedCode(TrimMessages.Members)]
    internal static MemberGetter[] Read(Type type)
    {
        var members = new List<MemberGetter>();
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetMethod is not null && property.GetIndexParameters().Length == 0)
            {
                members.Add(new MemberGetter(property));
            }
        }

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            members.Add(new MemberGetter(field));
        }

        return members.ToArray();
    }

    [RequiresUnreferencedCode(TrimMessages.Equivalency)]
    private static bool ImplementsReadOnlyDictionary(Type type)
    {
        foreach (var contract in type.GetInterfaces())
        {
            if (contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsTuple(Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        var definition = type.GetGenericTypeDefinition();
        var name = definition.FullName;
        return name is not null
            && (name.StartsWith("System.ValueTuple`", StringComparison.Ordinal) || name.StartsWith("System.Tuple`", StringComparison.Ordinal));
    }

    [RequiresUnreferencedCode(TrimMessages.Equivalency)]
    private static bool OverridesEquals(Type type)
    {
        var declaring = type.GetMethod(nameof(object.Equals), [typeof(object)])?.DeclaringType;
        return declaring is not null && declaring != typeof(object) && declaring != typeof(ValueType);
    }
}
