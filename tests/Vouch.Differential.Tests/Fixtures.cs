namespace Differential;

public record Person(string Name, int Age);

public record OtherPerson(string Name, int Age);

public record Titled(string Title, int Age);

public class Loop
{
    public Loop? Next { get; set; }

    public int Value { get; set; }
}

public record struct Point(int X, int Y);

public record Node(string Name, string? Note);

public enum Color { Red = 1, Green = 2 }

public enum Tint { Red = 1, Green = 2 }

public class AlwaysEqual
{
    public int Value { get; init; }

    public override bool Equals(object? obj) => true;

    public override int GetHashCode() => 0;
}

public class NeverEqual
{
    public int Value { get; init; }

    public override bool Equals(object? obj) => false;

    public override int GetHashCode() => 0;
}

// Does not override Equals, so by default it is compared by members and by value means reference equality.
public class RefOnly
{
    public int Value { get; init; }
}

public class Wrapper
{
    public List<List<int>> Rows { get; init; } = new();
}

public sealed class BoxedLong
{
    public long Value { get; init; }
}

public sealed class ColorHolder
{
    public Color Value { get; init; }
}

public sealed class IntHolder
{
    public int Value { get; init; }
}

public static class Throws
{
    public static Action Of(Exception exception) => () => throw exception;

    public static Action Nothing() => () => { };
}
