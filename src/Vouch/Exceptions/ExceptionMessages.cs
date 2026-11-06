namespace Vouch;

internal static class ExceptionMessages
{
    public static void ThrewWrongType(string name, Type expected, Exception actual, string verb = "to throw", string within = "") =>
        Fail.With($"Expected {name} {verb} {Formatter.Format(expected)}{within}, but it threw {Formatter.Format(actual.GetType())}:\n  {Formatter.Format(actual.Message)}");

    public static void DidNotThrow(string name, Type expected, string verb = "to throw", string within = "") =>
        Fail.With($"Expected {name} {verb} {Formatter.Format(expected)}{within}, but it did not throw.");

    public static void Threw(string name, Exception actual, string after = "")
    {
        var message = $"Expected {name} not to throw{after}, but it threw {Formatter.Format(actual.GetType())}:\n  {Formatter.Format(actual.Message)}";
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

    public static ExceptionAssertions<T> Check<T>(string name, Exception? thrown, bool exact = false, string within = "") where T : Exception
    {
        var verb = exact ? "to throw exactly" : "to throw";
        var matches = new List<T>();
        if (thrown is null)
        {
            DidNotThrow(name, typeof(T), verb, within);
            return new ExceptionAssertions<T>(matches, name);
        }

        if (thrown is AggregateException aggregate)
        {
            foreach (var inner in aggregate.Flatten().InnerExceptions)
            {
                if (inner is T unwrapped && (!exact || inner.GetType() == typeof(T)))
                {
                    matches.Add(unwrapped);
                }
            }

            if (matches.Count == 0 && thrown is T self && typeof(AggregateException).IsAssignableFrom(typeof(T))
                && (!exact || thrown.GetType() == typeof(T)))
            {
                matches.Add(self);
            }
        }
        else if (thrown is T match && (!exact || thrown.GetType() == typeof(T)))
        {
            matches.Add(match);
        }

        if (matches.Count == 0)
        {
            ThrewWrongType(name, typeof(T), thrown, verb, within);
        }

        return new ExceptionAssertions<T>(matches, name);
    }
}
