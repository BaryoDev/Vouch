namespace Vouch.Sdk;

/// <summary>
/// The entry point for custom assertions. A failure reported here behaves like a built-in failure:
/// an <see cref="AssertionScope"/> collects it, and otherwise it is thrown through
/// <see cref="VouchConfiguration.ExceptionFactory"/>.
/// </summary>
public static class Assertion
{
    /// <summary>Reports a failed assertion with the given message.</summary>
    public static void Fail(string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        Vouch.Fail.With(message);
    }

    /// <summary>
    /// Reports a failed assertion. A non-blank <paramref name="because"/> is added to the message
    /// as "Because ...", the same way built-in assertions do it.
    /// </summary>
    public static void Fail(string message, string? because)
    {
        ArgumentNullException.ThrowIfNull(message);
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
