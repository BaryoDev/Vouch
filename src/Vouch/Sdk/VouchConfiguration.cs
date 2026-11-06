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
}
