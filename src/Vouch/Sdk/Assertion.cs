namespace Vouch.Sdk;

/// <summary>
/// The entry point for custom assertions. A failure reported here behaves like a built-in failure:
/// an <see cref="AssertionScope"/> collects it, and otherwise it is thrown through
/// <see cref="VouchConfiguration.ExceptionFactory"/>.
/// </summary>
public static class Assertion
{
    /// <summary>
    /// Reports a failed assertion. When <paramref name="because"/> is null or whitespace, the failure is
    /// reported directly and the reason is left alone, so an outer reason that is already set still applies.
    /// When it is given, the current reason is saved, replaced by <paramref name="because"/> (added to the
    /// message as "Because ..."), the failure is reported, and the saved reason is restored in a finally.
    /// </summary>
    public static void Fail(string message, string? because = null)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (string.IsNullOrWhiteSpace(because))
        {
            Vouch.Fail.With(message);
            return;
        }

        var previous = Reason.Current;
        Reason.Set(because);
        try
        {
            Vouch.Fail.With(message);
        }
        finally
        {
            Reason.Set(previous);
        }
    }

    /// <summary>
    /// Returns the name to show for a subject: the expression text, or "value" when it is null or empty.
    /// </summary>
    public static string Name(string? expression) => Vouch.Fail.Name(expression);
}
