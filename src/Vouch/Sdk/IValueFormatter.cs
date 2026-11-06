namespace Vouch.Sdk;

/// <summary>
/// Formats values of chosen types for failure messages. Register one with
/// <see cref="VouchConfiguration.Formatters"/>. Formatters run only on the failure path.
/// Registered formatters run before the built-in rules, so a formatter whose CanHandle accepts
/// object or an interface handles every value it accepts. Output longer than 200 characters is cut.
/// If CanHandle throws, the formatter is skipped. If Format throws, the text shows as "&lt;threw ...&gt;".
/// A null result is written as "null".
/// </summary>
public interface IValueFormatter
{
    /// <summary>Returns true when this formatter should format <paramref name="value"/>. The value can be null.</summary>
    bool CanHandle(object? value);

    /// <summary>Returns the text for <paramref name="value"/>. Use <paramref name="context"/> to format nested values.</summary>
    string Format(object? value, FormattingContext context);
}

/// <summary>
/// Lets an <see cref="IValueFormatter"/> format nested values with the configured rules and formatters.
/// It is valid only during the Format call that received it and is not thread-safe. Do not keep it.
/// </summary>
public sealed class FormattingContext
{
    private readonly int _depth;
    private readonly Formatter.FormatState _state;

    internal FormattingContext(int depth, Formatter.FormatState state)
    {
        _depth = depth;
        _state = state;
    }

    /// <summary>How many levels deep the value being formatted is. The top-level value is 0.</summary>
    public int Depth => _depth;

    /// <summary>
    /// Formats a nested value one level deeper, so <see cref="FormattingOptions.MaxDepth"/>,
    /// <see cref="FormattingOptions.MaxItems"/> and cycle detection still apply.
    /// The total number of custom formatter calls for one top-level value is limited.
    /// </summary>
    public string Format(object? value)
    {
        var sb = new System.Text.StringBuilder();
        Formatter.WriteNested(sb, value, _depth + 1, _state);
        return sb.ToString();
    }
}
