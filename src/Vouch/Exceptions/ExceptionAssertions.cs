namespace Vouch;

public sealed class ExceptionAssertions<TException> where TException : Exception
{
    private readonly IReadOnlyList<TException> _matches;
    private readonly string _name;

    internal ExceptionAssertions(IReadOnlyList<TException> matches, string name)
    {
        _matches = matches;
        _name = name;
    }

    internal string Name => _name;

    private bool _reported;

    /// <summary>
    /// The matched exception. Inside an <see cref="AssertionScope"/>, Which is null after a
    /// failure was recorded (no exception, or more than one); the failure is recorded once
    /// per instance.
    /// </summary>
    public TException Which
    {
        get
        {
            if (_matches.Count == 0)
            {
                return default!;
            }

            if (_matches.Count > 1)
            {
                if (!_reported)
                {
                    Fail.With("More than one exception was thrown. Vouch cannot tell which one was meant.");
                    _reported = true;
                }

                return default!;
            }

            return _matches[0];
        }
    }

    public TException Subject => Which;

    public ExceptionAssertions<TException> And => this;

    public ExceptionAssertions<TException> WithMessage(string pattern)
    {
        Wildcard.RequirePattern(pattern, nameof(pattern));
        if (_matches.Count == 0)
        {
            return this;
        }

        foreach (var match in _matches)
        {
            if (Wildcard.MatchesMessage(match.Message, pattern))
            {
                return this;
            }
        }

        var found = _matches.Count == 1 ? _matches[0].Message : string.Join(" | ", _matches.Select(m => m.Message));
        Fail.With($"Expected {_name}'s exception message to match {Formatter.Format(pattern)}, but found {Formatter.Format(found)}.");
        return this;
    }

    /// <summary>Asserts the exception has an inner exception of type <typeparamref name="TInner"/> or a subtype.</summary>
    /// <remarks>If several exceptions matched, it passes when any of them has such an inner exception, as WithMessage does, and the result holds those inner exceptions.</remarks>
    public ExceptionAssertions<TInner> WithInnerException<TInner>() where TInner : Exception => Inner<TInner>(exactly: false);

    /// <summary>Asserts the exception has an inner exception of exactly type <typeparamref name="TInner"/>.</summary>
    /// <remarks>If several exceptions matched, it passes when any of them has such an inner exception, as WithMessage does, and the result holds those inner exceptions.</remarks>
    public ExceptionAssertions<TInner> WithInnerExceptionExactly<TInner>() where TInner : Exception => Inner<TInner>(exactly: true);

    private ExceptionAssertions<TInner> Inner<TInner>(bool exactly) where TInner : Exception
    {
        var innerName = $"{_name}'s inner exception";
        if (_matches.Count == 0)
        {
            return new ExceptionAssertions<TInner>([], innerName);
        }

        List<TInner>? found = null;
        foreach (var match in _matches)
        {
            if (match.InnerException is TInner inner && (!exactly || inner.GetType() == typeof(TInner)))
            {
                (found ??= []).Add(inner);
            }
        }

        if (found is not null)
        {
            return new ExceptionAssertions<TInner>(found, innerName);
        }

        string Describe(Exception match) => match.InnerException is { } other
            ? _matches.Count == 1
                ? $"{Formatter.TypeName(other.GetType())}:\n  {Formatter.Format(other.Message)}"
                : $"{Formatter.TypeName(other.GetType())} {Formatter.Format(other.Message)}"
            : "no inner exception";

        var actual = _matches.Count == 1 ? Describe(_matches[0]) : string.Join(" | ", _matches.Select(Describe));
        Fail.With($"Expected {_name}'s exception to have an inner exception of {(exactly ? "exactly " : "")}type {Formatter.TypeName(typeof(TInner))}, but found {actual}.");
        return new ExceptionAssertions<TInner>([], innerName);
    }
}
