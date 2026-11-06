namespace Vouch.Sdk;

/// <summary>Limits applied when values are written into failure messages.</summary>
public sealed class FormattingOptions
{
    internal const int DefaultMaxDepth = 3;
    internal const int DefaultMaxItems = 10;
    internal const int HardMaxDepth = 100;

    private volatile int _maxDepth = DefaultMaxDepth;
    private volatile int _maxItems = DefaultMaxItems;
    private volatile int _maxLines = int.MaxValue;

    internal FormattingOptions()
    {
    }

    /// <summary>
    /// How many levels of nested objects and collections are written. Default 3. At least 1.
    /// Values above 100 are clamped to 100, so a deep structure cannot overflow the stack.
    /// The limit also applies to custom formatters: below this depth they are not called.
    /// </summary>
    public int MaxDepth
    {
        get => _maxDepth;
        set => _maxDepth = Math.Min(AtLeastOne(value), HardMaxDepth);
    }

    /// <summary>
    /// How many items of a collection are written. Default 10. At least 1.
    /// This also caps how many failures AllSatisfy and similar assertions list.
    /// </summary>
    public int MaxItems
    {
        get => _maxItems;
        set => _maxItems = AtLeastOne(value);
    }

    /// <summary>
    /// How many lines one formatted value may have. Extra lines are replaced by a "... (n more lines)" line.
    /// The built-in rules write one line, so this matters for multi-line text from custom formatters and ToString.
    /// A trailing newline does not count as a line, and "\r\n" counts as one break.
    /// Default is no limit. At least 1.
    /// </summary>
    public int MaxLines
    {
        get => _maxLines;
        set => _maxLines = AtLeastOne(value);
    }

    /// <summary>Restores every option to its default.</summary>
    public void Reset()
    {
        _maxDepth = DefaultMaxDepth;
        _maxItems = DefaultMaxItems;
        _maxLines = int.MaxValue;
    }

    private static int AtLeastOne(int value) =>
        value >= 1 ? value : throw new ArgumentOutOfRangeException(nameof(value), value, "Must be at least 1.");
}
