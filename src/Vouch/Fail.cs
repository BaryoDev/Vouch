using System.Diagnostics.CodeAnalysis;
using Vouch.Sdk;

namespace Vouch;

internal static class Fail
{
    private static readonly AsyncLocal<Action<VouchException>?> Sink = new();

    // The exception most recently thrown by Throw on this thread. Catch sites that collect
    // inner failures use IsFailure to recognise it even when the factory made a custom type.
    [ThreadStatic]
    private static Exception? t_lastThrown;

    // Plug point for a recording sink. Null means throw, which is the default.
    internal static Action<VouchException>? Recorder
    {
        get => Sink.Value;
        set => Sink.Value = value;
    }

    public static void With(string message)
    {
        var failure = Reason.Apply(message);
        var recorder = Sink.Value;
        if (recorder is null)
        {
            Throw(failure.Message);
        }

        recorder(failure);
    }

    // Throws the factory's exception for the final message. Reads the factory only here.
    [DoesNotReturn]
    internal static void Throw(string message)
    {
        var ex = VouchConfiguration.ExceptionFactory(message) ?? new VouchException(message);
        t_lastThrown = ex;
        throw ex;
    }

    // True for a VouchException (including subclasses) or the exception Throw last threw.
    internal static bool IsFailure(Exception ex) =>
        ex is VouchException || ReferenceEquals(ex, t_lastThrown);

    public static string Name(string? expression) =>
        string.IsNullOrEmpty(expression) ? "value" : expression;
}
