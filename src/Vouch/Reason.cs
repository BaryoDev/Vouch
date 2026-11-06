namespace Vouch;

internal static class Reason
{
    [ThreadStatic]
    private static string? t_because;

    public static void Set(string? because) => t_because = because;

    public static void Clear() => t_because = null;

    public static string? Current => t_because;

    public static VouchException Apply(string message)
    {
        var because = t_because;
        if (string.IsNullOrWhiteSpace(because))
        {
            return new VouchException(message);
        }

        var reason = because.Trim();
        if (!reason.EndsWith('.'))
        {
            reason += ".";
        }

        var separator = message.Contains('\n') ? "\n" : " ";
        return new VouchException($"{message}{separator}Because {reason}");
    }
}
