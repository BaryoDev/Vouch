namespace Vouch;

/// <summary>
/// Collects assertion failures instead of throwing on the first one. Disposing the
/// outermost scope throws one <see cref="VouchException"/> that lists every failure.
/// </summary>
/// <remarks>
/// Scopes must be disposed in reverse order of creation, which is what a <c>using</c>
/// statement does. Work started under a scope must finish before the scope is disposed;
/// a failure recorded after disposal is thrown directly. If user code throws inside a
/// scope that has collected failures, the exception from Dispose replaces it, as with
/// any exception thrown from Dispose.
/// </remarks>
public sealed class AssertionScope : IDisposable
{
    private static readonly AsyncLocal<AssertionScope?> Current = new();

    private readonly AssertionScope? _previousScope;
    private readonly Action<VouchException>? _previousRecorder;
    private readonly List<string> _failures = new();
    private int _disposed;

    public AssertionScope()
    {
        _previousScope = Current.Value;
        _previousRecorder = Fail.Recorder;
        Current.Value = this;
        Fail.Recorder = Record;
    }

    private void Record(VouchException failure)
    {
        lock (_failures)
        {
            if (Volatile.Read(ref _disposed) == 0)
            {
                _failures.Add(failure.Message);
                return;
            }
        }

        throw failure;
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
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        if (Current.Value == this)
        {
            Current.Value = _previousScope;
            Fail.Recorder = _previousRecorder;
        }

        var messages = Discard();
        if (messages.Length == 0)
        {
            return;
        }

        if (_previousRecorder is not null)
        {
            foreach (var message in messages)
            {
                _previousRecorder(new VouchException(message));
            }

            return;
        }

        throw new VouchException(string.Join("\n\n", messages));
    }
}
