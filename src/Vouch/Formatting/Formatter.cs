using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Vouch.Sdk;

namespace Vouch;

internal static class Formatter
{
    private const int HardDepthLimit = FormattingOptions.HardMaxDepth;

    public static int MaxDepth => Math.Min(VouchConfiguration.Formatting.MaxDepth, HardDepthLimit);
    public static int MaxItems => VouchConfiguration.Formatting.MaxItems;
    public const int MaxStringLength = 200;

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public static string FormatList<T>(T[] values) =>
        "[" + string.Join(", ", values.Select(v => Format(v))) + "]";

    public static string Format(object? value)
    {
        var sb = new StringBuilder();
        try
        {
            Write(sb, value, 0, new FormatState());
        }
        catch (Exception ex)
        {
            return $"<threw {ex.GetType().Name}>";
        }

        return LimitLines(sb.ToString());
    }

    internal static void WriteNested(StringBuilder sb, object? value, int depth, FormatState state) =>
        Write(sb, value, depth, state);

    /// <summary>Per top-level Format call: cycle path, values with a running custom formatter, and a call budget.</summary>
    internal sealed class FormatState
    {
        private const int CustomCallBudget = 10_000;

        public HashSet<object> Path { get; } = new(ReferenceEqualityComparer.Instance);
        public HashSet<object> Active { get; } = new(ReferenceEqualityComparer.Instance);
        public int Budget = CustomCallBudget;
    }

    private static string LimitLines(string text)
    {
        var max = VouchConfiguration.Formatting.MaxLines;
        if (max == int.MaxValue)
        {
            return text;
        }

        var breaks = 0;
        foreach (var ch in text)
        {
            if (ch == '\n')
            {
                breaks++;
            }
        }

        // A trailing newline ends the last line, it does not start another one. "\r\n" counts once.
        var total = text.EndsWith('\n') ? breaks : breaks + 1;
        if (total <= max)
        {
            return text;
        }

        var seen = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n' && ++seen == max)
            {
                var cut = i > 0 && text[i - 1] == '\r' ? i - 1 : i;
                return text.Substring(0, cut) + "\n... (" + (total - max) + " more lines)";
            }
        }

        return text;
    }

    private static void AppendCapped(StringBuilder sb, string text)
    {
        if (text.Length > MaxStringLength)
        {
            var cut = CutLength(text);
            sb.Append(text, 0, cut).Append("... (").Append(text.Length - cut).Append(" more chars)");
        }
        else
        {
            sb.Append(text);
        }
    }

    private static bool TryWriteCustom(StringBuilder sb, object? value, int depth, FormatState state)
    {
        var formatters = VouchConfiguration.Formatters.Snapshot;
        if (formatters.Length == 0 || depth > MaxDepth || state.Budget <= 0)
        {
            return false;
        }

        // A formatter that formats its own value again must not recurse forever: the second visit skips custom formatters.
        // This set is separate from the cycle path, so the built-in rules do not mistake it for a real cycle.
        var tracked = value is not null && !value.GetType().IsValueType;
        if (tracked && !state.Active.Add(value!))
        {
            return false;
        }

        try
        {
            foreach (var formatter in formatters)
            {
                try
                {
                    if (!formatter.CanHandle(value))
                    {
                        continue;
                    }
                }
                catch (Exception ex) when (ex is not OutOfMemoryException)
                {
                    continue;
                }

                state.Budget--;
                try
                {
                    AppendCapped(sb, formatter.Format(value, new FormattingContext(depth, state)) ?? "null");
                }
                catch (Exception ex) when (ex is not OutOfMemoryException)
                {
                    sb.Append("<threw ").Append(ex.GetType().Name).Append('>');
                }

                return true;
            }

            return false;
        }
        finally
        {
            if (tracked)
            {
                state.Active.Remove(value!);
            }
        }
    }

    private static void Write(StringBuilder sb, object? value, int depth, FormatState state)
    {
        if (TryWriteCustom(sb, value, depth, state))
        {
            return;
        }

        switch (value)
        {
            case null: sb.Append("null"); return;
            case string s: WriteString(sb, s); return;
            case bool b: sb.Append(b ? "true" : "false"); return;
            case char c: sb.Append('\'').Append(c == '\'' ? "\\'" : Escape(c.ToString())).Append('\''); return;
            case decimal m: sb.Append(m.ToString(Invariant)).Append('m'); return;
            case double d: sb.Append(d.ToString("R", Invariant)); return;
            case float f: sb.Append(f.ToString("R", Invariant)).Append('f'); return;
            case long l: sb.Append(l.ToString(Invariant)).Append('L'); return;
            case int or short or byte or sbyte or ushort or uint or ulong:
                sb.Append(((IFormattable)value).ToString(null, Invariant)); return;
            case DateTime dt: sb.Append(dt.ToString("o", Invariant)); return;
            case DateTimeOffset dto: sb.Append(dto.ToString("o", Invariant)); return;
            case DateOnly dateOnly: sb.Append(dateOnly.ToString("yyyy-MM-dd", Invariant)); return;
            case TimeOnly timeOnly: sb.Append(timeOnly.ToString("o", Invariant)); return;
            case TimeSpan ts: sb.Append(ts.ToString("c", Invariant)); return;
            case Guid g: sb.Append(g.ToString("D", Invariant)); return;
            case Enum e: sb.Append(TypeName(e.GetType())).Append('.').Append(e.ToString()); return;
            case Type t: sb.Append(TypeName(t)); return;
        }

        var type = value.GetType();
        var isReference = !type.IsValueType;
        if (isReference && !state.Path.Add(value))
        {
            sb.Append("<cycle ").Append(TypeName(type)).Append('>');
            return;
        }

        try
        {
            if (value is IDictionary dictionary)
            {
                WriteDictionary(sb, dictionary, depth, state);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                WriteKeyValuePair(sb, value, type, depth, state);
            }
            else if (value is IEnumerable enumerable)
            {
                WriteSequence(sb, enumerable, depth, state);
            }
            else if (SafeMembers(type) is { } safe)
            {
                WriteObject(sb, value, type, depth, state, safe);
            }
            else if (!IsRecord(type) && !IsAnonymous(type) && OverridesToString(type))
            {
                WriteToString(sb, value);
            }
            else
            {
                WriteObject(sb, value, type, depth, state);
            }
        }
        finally
        {
            if (isReference)
            {
                state.Path.Remove(value);
            }
        }
    }

    private static void WriteString(StringBuilder sb, string s)
    {
        sb.Append('"');
        if (s.Length > MaxStringLength)
        {
            var cut = CutLength(s);
            sb.Append(Escape(s.Substring(0, cut)));
            sb.Append("... (").Append(s.Length - cut).Append(" more chars)");
        }
        else
        {
            sb.Append(Escape(s));
        }

        sb.Append('"');
    }

    private static int CutLength(string s) =>
        char.IsHighSurrogate(s[MaxStringLength - 1]) ? MaxStringLength - 1 : MaxStringLength;

    private static string Escape(string s)
    {
        StringBuilder? sb = null;
        for (var i = 0; i < s.Length; i++)
        {
            var replacement = s[i] switch
            {
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                '"' => "\\\"",
                '\\' => "\\\\",
                _ => null,
            };

            if (replacement is null)
            {
                sb?.Append(s[i]);
                continue;
            }

            sb ??= new StringBuilder(s.Length + 8).Append(s, 0, i);
            sb.Append(replacement);
        }

        return sb?.ToString() ?? s;
    }

    private static void WriteSequence(StringBuilder sb, IEnumerable sequence, int depth, FormatState state)
    {
        if (depth >= MaxDepth)
        {
            sb.Append("[...]");
            return;
        }

        sb.Append('[');
        try
        {
            var enumerator = sequence.GetEnumerator();
            try
            {
                var count = 0;
                while (count < MaxItems && enumerator.MoveNext())
                {
                    if (count > 0)
                    {
                        sb.Append(", ");
                    }

                    Write(sb, enumerator.Current, depth + 1, state);
                    count++;
                }

                WriteRemainder(sb, enumerator, sequence, count);
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            sb.Append("<threw ").Append(ex.GetType().Name).Append('>');
        }

        sb.Append(']');
    }

    private static void WriteDictionary(StringBuilder sb, IDictionary dictionary, int depth, FormatState state)
    {
        if (depth >= MaxDepth)
        {
            sb.Append("{...}");
            return;
        }

        sb.Append('{');
        try
        {
            var enumerator = dictionary.GetEnumerator();
            try
            {
                var count = 0;
                while (count < MaxItems && enumerator.MoveNext())
                {
                    if (count > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append('[');
                    Write(sb, enumerator.Key, depth + 1, state);
                    sb.Append("] = ");
                    Write(sb, enumerator.Value, depth + 1, state);
                    count++;
                }

                WriteRemainder(sb, enumerator, dictionary, count);
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            sb.Append("<threw ").Append(ex.GetType().Name).Append('>');
        }

        sb.Append('}');
    }

    private static void WriteKeyValuePair(StringBuilder sb, object pair, Type type, int depth, FormatState state)
    {
        sb.Append('[');
        Write(sb, type.GetProperty("Key")!.GetValue(pair), depth + 1, state);
        sb.Append("] = ");
        Write(sb, type.GetProperty("Value")!.GetValue(pair), depth + 1, state);
    }

    private static void WriteRemainder(StringBuilder sb, IEnumerator enumerator, object source, int shown)
    {
        if (shown < MaxItems)
        {
            return;
        }

        if (KnownCount(source) is { } total)
        {
            if (total > shown)
            {
                sb.Append(", ... (").Append(total - shown).Append(" more)");
            }

            return;
        }

        if (enumerator.MoveNext())
        {
            sb.Append(", ... (more)");
        }
    }

    // Only collections that already know their size are counted. Anything else is not walked to find out.
    private static int? KnownCount(object source)
    {
        if (source is ICollection collection)
        {
            return collection.Count;
        }

        foreach (var contract in source.GetType().GetInterfaces())
        {
            if (contract.IsGenericType
                && contract.GetGenericTypeDefinition() is var definition
                && (definition == typeof(IReadOnlyCollection<>) || definition == typeof(ICollection<>))
                && contract.GetProperty("Count")?.GetValue(source) is int count)
            {
                return count;
            }
        }

        return null;
    }

    private static void WriteToString(StringBuilder sb, object value)
    {
        try
        {
            var text = value.ToString() ?? "null";
            if (text.Length > MaxStringLength)
            {
                var cut = CutLength(text);
                sb.Append(text, 0, cut).Append("... (").Append(text.Length - cut).Append(" more chars)");
            }
            else
            {
                sb.Append(text);
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            sb.Append("<threw ").Append(ex.GetType().Name).Append('>');
        }
    }

    private static void WriteObject(StringBuilder sb, object value, Type type, int depth, FormatState state, MemberGetter[]? only = null)
    {
        var anonymous = IsAnonymous(type);
        if (!anonymous)
        {
            sb.Append(TypeName(type)).Append(' ');
        }

        if (depth >= MaxDepth)
        {
            sb.Append("{ ... }");
            return;
        }

        var members = only ?? MemberCache.For(type);
        if (members.Length == 0)
        {
            sb.Append("{ }");
            return;
        }

        sb.Append("{ ");
        for (var i = 0; i < members.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            sb.Append(members[i].Name).Append(" = ");
            object? memberValue;
            try
            {
                memberValue = members[i].Get(value);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                var inner = ex is TargetInvocationException { InnerException: { } cause } ? cause : ex;
                sb.Append("<threw ").Append(inner.GetType().Name).Append('>');
                continue;
            }

            if (only is not null && memberValue is Enum enumValue)
            {
                sb.Append(enumValue.ToString());
                continue;
            }

            Write(sb, memberValue, depth + 1, state);
        }

        sb.Append(" }");
    }

    private static MemberGetter[]? SafeMembers(Type type)
    {
        string? name = null;
        if (typeof(Task).IsAssignableFrom(type))
        {
            name = nameof(Task.Status);
        }
        else if (type.Namespace == "System.Threading.Tasks" && type.Name.StartsWith("ValueTask", StringComparison.Ordinal))
        {
            name = nameof(Task.IsCompleted);
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Lazy<>))
        {
            name = "IsValueCreated";
        }

        var property = name is null ? null : type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        return property is null ? null : [new MemberGetter(property)];
    }

    private static bool OverridesToString(Type type)
    {
        var declaring = type.GetMethod(nameof(ToString), Type.EmptyTypes)?.DeclaringType;
        return declaring is not null && declaring != typeof(object) && declaring != typeof(ValueType);
    }

    internal static bool IsSafeOnly(Type type) => SafeMembers(type) is not null;

    internal static bool IsAnonymous(Type type) =>
        type.IsDefined(typeof(CompilerGeneratedAttribute), false) && type.Name.Contains("AnonymousType", StringComparison.Ordinal);

    internal static bool IsRecord(Type type)
    {
        const BindingFlags all = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        return type.GetProperty("EqualityContract", all) is not null
            || type.GetMethod("<Clone>$", all) is not null
            || (type.IsValueType && type.GetMethod("PrintMembers", all, null, [typeof(StringBuilder)], null) is not null);
    }

    internal static string TypeName(Type type)
    {
        if (type.IsArray)
        {
            return TypeName(type.GetElementType()!) + "[]";
        }

        var name = type.Name;
        if (!type.IsGenericType)
        {
            return name;
        }

        var tick = name.IndexOf('`');
        if (tick >= 0)
        {
            name = name.Substring(0, tick);
        }

        return name + "<" + string.Join(", ", type.GetGenericArguments().Select(TypeName)) + ">";
    }
}
