using System.Diagnostics;

namespace Vouch;

/// <summary>Every wait and every measurement goes through this, so tests can run on fake time.</summary>
internal interface ITimeSource
{
    /// <summary>A monotonic reading. Only differences between two readings mean anything.</summary>
    TimeSpan Now { get; }

    /// <summary>Completes after the delay, or cancels when the token is cancelled.</summary>
    Task Delay(TimeSpan delay, CancellationToken cancellationToken);
}

internal sealed class StopwatchTimeSource : ITimeSource
{
    public static readonly StopwatchTimeSource Instance = new();

    private StopwatchTimeSource()
    {
    }

    public TimeSpan Now => Stopwatch.GetElapsedTime(0, Stopwatch.GetTimestamp());

    public Task Delay(TimeSpan delay, CancellationToken cancellationToken)
        => Task.Delay(delay, cancellationToken);
}

/// <summary>
/// The time source in use. The override is held in an AsyncLocal, so it follows the test that set it
/// and parallel tests never see each other's fake.
/// </summary>
internal static class Clock
{
    private static readonly AsyncLocal<ITimeSource?> Override = new();

    public static ITimeSource Current => Override.Value ?? StopwatchTimeSource.Instance;

    /// <summary>Uses the given source until the returned scope is disposed.</summary>
    public static IDisposable Use(ITimeSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var previous = Override.Value;
        Override.Value = source;
        return new Restore(previous);
    }

    private sealed class Restore(ITimeSource? previous) : IDisposable
    {
        public void Dispose() => Override.Value = previous;
    }
}
