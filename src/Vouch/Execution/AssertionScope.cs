namespace Vouch;

/// <summary>
/// Collects assertion failures instead of throwing on the first one. Disposing the
/// outermost scope throws one <see cref="VouchException"/> that lists every failure.
/// </summary>
public sealed class AssertionScope : IDisposable
{
    private static readonly AsyncLocal<AssertionScope?> Current = new();

    private readonly AssertionScope? _parent;
    private readonly Action<VouchException>? _previousRecorder;
    private readonly List<string> _failures = new();
    private bool _disposed;

    public AssertionScope()
    {
        _parent = Current.Value;
        _previousRecorder = Fail.Recorder;
        Current.Value = this;
        Fail.Recorder = Record;
    }

    private void Record(VouchException failure)
    {
        lock (_failures)
        {
            _failures.Add(failure.Message);
        }
    }

    /// <summary>Returns the collected failure messages and clears them.</summary>
    public string[] Discard()
    {
        lock (_failures)
        {
            var messages = _failures.ToArray();
            _failures.Clear();
            return messages;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Current.Value = _parent;
        Fail.Recorder = _previousRecorder;

        var messages = Discard();
        if (messages.Length == 0)
        {
            return;
        }

        if (_parent is not null)
        {
            foreach (var message in messages)
            {
                _parent.Record(new VouchException(message));
            }

            return;
        }

        throw new VouchException(string.Join("\n\n", messages));
    }
}
