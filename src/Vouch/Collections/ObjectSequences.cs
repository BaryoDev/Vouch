using System.Collections;

namespace Vouch;

// Views of a non-generic sequence as IEnumerable<object>, so Should() on an ArrayList or another IEnumerable
// reaches the generic collection assertions. A list keeps its index access and count; nothing is copied up front.
internal sealed class ObjectList(IList source) : IReadOnlyList<object>
{
    public int Count => source.Count;

    public object this[int index] => source[index]!;

    public IEnumerator<object> GetEnumerator()
    {
        foreach (var item in source)
        {
            yield return item!;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal sealed class ObjectSequence(IEnumerable source) : IEnumerable<object>
{
    public IEnumerator<object> GetEnumerator()
    {
        foreach (var item in source)
        {
            yield return item!;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
