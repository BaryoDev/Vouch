namespace Vouch.Sdk;

/// <summary>Global settings for Vouch. Adapter packages use this to change the thrown exception type.</summary>
public static class VouchConfiguration
{
    private static readonly Func<string, Exception> DefaultFactory = static message => new VouchException(message);

    private static volatile Func<string, Exception>? s_factory;

    /// <summary>
    /// Creates the exception thrown to the caller when an assertion fails, given the final failure message.
    /// The default creates a <see cref="VouchException"/>. Setting null restores the default.
    /// The factory runs only on the failure path.
    /// </summary>
    public static Func<string, Exception> ExceptionFactory
    {
        get => s_factory ?? DefaultFactory;
        set => s_factory = value;
    }

    private static readonly Dictionary<System.Reflection.Assembly, List<IValueFormatter>> Scanned = new();

    /// <summary>
    /// Custom formatters, tried before the built-in rules, most recently added first.
    /// Thread-safe. Formatters run only on the failure path.
    /// </summary>
    public static ValueFormatterCollection Formatters { get; } = new();

    /// <summary>Limits for values written into failure messages.</summary>
    public static FormattingOptions Formatting { get; } = new();

    /// <summary>
    /// Registers every public static method marked <see cref="ValueFormatterAttribute"/> in the assembly.
    /// The assembly is scanned once; calling again only re-registers formatters that were removed.
    /// Scanning is explicit, never over every loaded assembly, so startup cost and surprises stay with the caller.
    /// Returns how many formatters were newly registered.
    /// </summary>
    public static int AddFormattersFrom(System.Reflection.Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        List<IValueFormatter> found;
        lock (Scanned)
        {
            if (!Scanned.TryGetValue(assembly, out found!))
            {
                Scanned[assembly] = found = MethodValueFormatter.Scan(assembly);
            }
        }

        var added = 0;
        foreach (var formatter in found)
        {
            if (!Formatters.Contains(formatter))
            {
                Formatters.Add(formatter);
                added++;
            }
        }

        return added;
    }
}
