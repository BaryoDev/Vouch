namespace Vouch.Tests;

/// <summary>
/// A time source the test drives by hand. In manual mode a delay stays pending until Advance reaches it
/// or its token is cancelled. In auto-advance mode a delay moves the clock forward and finishes at once.
/// </summary>
internal sealed class FakeTime : ITimeSource
{
    private readonly object _gate = new();
    private readonly List<Pending> _pending = new();

    public TimeSpan Now { get; set; }

    public bool AutoAdvance { get; init; }

    /// <summary>How many delays are waiting and not yet finished or cancelled.</summary>
    public int PendingCount
    {
        get
        {
            lock (_gate)
            {
                return _pending.Count;
            }
        }
    }

    public Task Delay(TimeSpan delay, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        if (AutoAdvance)
        {
            Now += delay;
            return Task.CompletedTask;
        }

        var entry = new Pending(Now + delay, new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));
        lock (_gate)
        {
            _pending.Add(entry);
        }

        entry.Registration = cancellationToken.Register(() =>
        {
            lock (_gate)
            {
                _pending.Remove(entry);
            }

            entry.Source.TrySetCanceled(cancellationToken);
        });
        return entry.Source.Task;
    }

    public void Advance(TimeSpan by)
    {
        List<Pending> due;
        lock (_gate)
        {
            Now += by;
            due = _pending.Where(p => p.Due <= Now).ToList();
            foreach (var p in due)
            {
                _pending.Remove(p);
            }
        }

        foreach (var p in due)
        {
            p.Registration.Dispose();
            p.Source.TrySetResult();
        }
    }

    private sealed class Pending(TimeSpan due, TaskCompletionSource source)
    {
        public TimeSpan Due { get; } = due;

        public TaskCompletionSource Source { get; } = source;

        public CancellationTokenRegistration Registration { get; set; }
    }
}
