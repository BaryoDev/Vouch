namespace Vouch.Sdk;

/// <summary>
/// Formats values of chosen types for failure messages. Register one with
/// <see cref="VouchConfiguration.Formatters"/>. Formatters run only on the failure path.
/// </summary>
public interface IValueFormatter
{
    /// <summary>Returns true when this formatter should format <paramref name="value"/>. The value can be null.</summary>
    bool CanHandle(object? value);

    /// <summary>Returns the text for <paramref name="value"/>. Use <paramref name="context"/> to format nested values.</summary>
    string Format(object? value, FormattingContext context);
}

/// <summary>Lets an <see cref="IValueFormatter"/> format nested values with the configured rules and formatters.</summary>
public sealed class FormattingContext
{
    private readonly int _depth;
    private readonly HashSet<object> _path;

    internal FormattingContext(int depth, HashSet<object> path)
    {
        _depth = depth;
        _path = path;
    }

    /// <summary>How many levels deep the value being formatted is. The top-level value is 0.</summary>
    public int Depth => _depth;

    /// <summary>
    /// Formats a nested value one level deeper, so <see cref="FormattingOptions.MaxDepth"/>,
    /// <see cref="FormattingOptions.MaxItems"/> and cycle detection still apply.
    /// </summary>
    public string Format(object? value)
    {
        var sb = new System.Text.StringBuilder();
        Formatter.WriteNested(sb, value, _depth + 1, _path);
        return sb.ToString();
    }
}
