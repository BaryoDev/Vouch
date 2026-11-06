namespace Vouch;

public static class ArgumentExceptionExtensions
{
    public static ExceptionAssertions<T> WithParameterName<T>(this ExceptionAssertions<T> assertions, string paramName)
        where T : ArgumentException
    {
        ArgumentNullException.ThrowIfNull(assertions);
        ArgumentNullException.ThrowIfNull(paramName);
        var which = assertions.Which;
        if (which is null)
        {
            return assertions;
        }

        if (!string.Equals(which.ParamName, paramName, StringComparison.Ordinal))
        {
            Fail.With($"Expected {assertions.Name}'s exception parameter name to be {Formatter.Format(paramName)}, but found {Formatter.Format(which.ParamName)}.");
        }

        return assertions;
    }

    public static Task<ExceptionAssertions<T>> WithParameterName<T>(this Task<ExceptionAssertions<T>> task, string paramName)
        where T : ArgumentException
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(paramName);
        return WithParameterNameCore(task, paramName);
    }

    private static async Task<ExceptionAssertions<T>> WithParameterNameCore<T>(Task<ExceptionAssertions<T>> task, string paramName)
        where T : ArgumentException
    {
        var assertions = await task.ConfigureAwait(false);
        return assertions.WithParameterName(paramName);
    }
}
