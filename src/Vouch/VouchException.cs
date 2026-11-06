using Vouch.Sdk;

namespace Vouch;

public class VouchException : Exception, IAssertionException
{
    public VouchException(string message) : base(message)
    {
    }

    protected VouchException(string message, Exception? innerException) : base(message, innerException)
    {
    }
}
