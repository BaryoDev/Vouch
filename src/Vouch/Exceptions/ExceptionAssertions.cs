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
                Fail.With("More than one exception was thrown. Vouch cannot tell which one was meant.");
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
}
