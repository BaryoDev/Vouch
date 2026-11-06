namespace Vouch;

internal static class Fail
{
    private static readonly AsyncLocal<Action<VouchException>?> Sink = new();

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
            throw failure;
        }

        recorder(failure);
    }

    public static string Name(string? expression) =>
        string.IsNullOrEmpty(expression) ? "value" : expression;
}
