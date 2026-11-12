using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vouch.Sdk;

namespace Vouch;

internal static class Fail
{
    private static readonly AsyncLocal<Action<VouchException>?> Sink = new();

    // Marks each thrown failure with the original Vouch message. The marker travels with the
    // exception, so it survives other threads, rethrows and later failures.
    private static readonly ConditionalWeakTable<Exception, string> Failures = new();

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
        Exception? ex;
        try
        {
            ex = VouchConfiguration.ExceptionFactory(message);
        }
        catch (Exception)
        {
            ex = new VouchException(message);
        }

        ex ??= new VouchException(message);
        Failures.AddOrUpdate(ex, message);
        throw ex;
    }

    // True for a VouchException (including subclasses) or an exception made by Throw.
    internal static bool IsFailure(Exception ex) =>
        ex is VouchException || Failures.TryGetValue(ex, out _);

    // The original Vouch message, without any decoration the factory added.
    internal static string FailureMessage(Exception ex) =>
        Failures.TryGetValue(ex, out var message) ? message : ex.Message;

    public static string Name(string? expression) =>
        string.IsNullOrEmpty(expression) ? "value" : expression;
}
