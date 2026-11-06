namespace Vouch;

internal static class ExceptionMessages
{
    public static void ThrewWrongType(string name, Type expected, Exception actual) =>
        Fail.With($"Expected {name} to throw {Formatter.Format(expected)}, but it threw {Formatter.Format(actual.GetType())}:\n  {Formatter.Format(actual.Message)}");

    public static void DidNotThrow(string name, Type expected) =>
        Fail.With($"Expected {name} to throw {Formatter.Format(expected)}, but it did not throw.");

    public static void Threw(string name, Exception actual)
    {
        var message = $"Expected {name} not to throw, but it threw {Formatter.Format(actual.GetType())}:\n  {Formatter.Format(actual.Message)}";
        var stack = actual.StackTrace;
        if (!string.IsNullOrEmpty(stack))
        {
            var end = stack.IndexOf('\n');
            var first = (end < 0 ? stack : stack[..end]).Trim();
            if (first.Length > 0)
            {
                message += "\n  " + first;
            }
        }

        Fail.With(message);
    }

    public static void NullDelegate(string name, string expectation) =>
        Fail.With($"Expected {name} {expectation}, but found null.");

    public static void NullTask(string name, string expectation) =>
        Fail.With($"Expected {name} {expectation}, but it returned a null Task.");

    public static ExceptionAssertions<T> Empty<T>(string name) where T : Exception
        => new(new List<T>(), name);

    public static ExceptionAssertions<T> Check<T>(string name, Exception? thrown) where T : Exception
    {
        var matches = new List<T>();
        if (thrown is null)
        {
            DidNotThrow(name, typeof(T));
            return new ExceptionAssertions<T>(matches, name);
        }

        if (thrown is AggregateException aggregate)
        {
            foreach (var inner in aggregate.Flatten().InnerExceptions)
            {
                if (inner is T unwrapped)
                {
                    matches.Add(unwrapped);
                }
            }

            if (matches.Count == 0 && thrown is T self && typeof(AggregateException).IsAssignableFrom(typeof(T)))
            {
                matches.Add(self);
            }
        }
        else if (thrown is T match)
        {
            matches.Add(match);
        }

        if (matches.Count == 0)
        {
            ThrewWrongType(name, typeof(T), thrown);
        }

        return new ExceptionAssertions<T>(matches, name);
    }
}
