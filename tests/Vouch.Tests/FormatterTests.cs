using System.Diagnostics;
using System.Text;
using Xunit;

namespace Vouch.Tests;

public record Row(string Code, int Qty);
public record struct Tick(int N);
public class Node { public Node? Next { get; set; } }
public class Pair { public Node? A { get; set; } public Node? B { get; set; } }
public class Bad { public int Value => throw new InvalidOperationException(); }
public class BadToString { public override string ToString() => throw new InvalidOperationException(); }
public sealed class ReadOnlyOnly(int count) : IReadOnlyCollection<int>
{
    public int Count => count;
    public IEnumerator<int> GetEnumerator() => Enumerable.Range(0, count).GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

public class Named { public override string ToString() => "named!"; }
public class Chain { public Chain? Inner { get; set; } }
public enum Status { Active, Closed, Draft }

public class FormatterTests
{
    private static Chain MakeChain(int n)
    {
        var head = new Chain();
        var cur = head;
        for (var i = 0; i < n; i++) { cur.Inner = new Chain(); cur = cur.Inner; }
        return head;
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData("Juan", "\"Juan\"")]
    [InlineData("a\"b\n", "\"a\\\"b\\n\"")]
    [InlineData(true, "true")]
    [InlineData('x', "'x'")]
    [InlineData(42L, "42L")]
    [InlineData(1.5, "1.5")]
    [InlineData(2.5f, "2.5f")]
    public void Formats_simple_values(object? value, string expected) => Assert.Equal(expected, Formatter.Format(value));

    [Fact] public void Key_value_pair_uses_dictionary_style() => Assert.Equal("[\"a\"] = 1", Formatter.Format(new KeyValuePair<string, int>("a", 1)));
    [Fact] public void Key_value_pair_with_null_value() => Assert.Equal("[\"a\"] = null", Formatter.Format(new KeyValuePair<string, string?>("a", null)));

    [Fact] public void Escapes_backslash_and_tab() => Assert.Equal("\"a\\\\b\\t\\r\"", Formatter.Format("a\\b\t\r"));
    [Fact] public void Double_nan() => Assert.Equal("NaN", Formatter.Format(double.NaN));
    [Fact] public void Decimal_has_m_suffix() => Assert.Equal("100m", Formatter.Format(100m));
    [Fact] public void Decimal_keeps_scale() => Assert.Equal("12.50m", Formatter.Format(12.50m));
    [Fact] public void Date_is_iso() => Assert.Equal("2026-10-19T08:30:00.0000000Z", Formatter.Format(new DateTime(2026, 10, 19, 8, 30, 0, DateTimeKind.Utc)));
    [Fact] public void DateTimeOffset_is_iso() => Assert.Equal("2026-10-19T08:30:00.0000000+08:00", Formatter.Format(new DateTimeOffset(2026, 10, 19, 8, 30, 0, TimeSpan.FromHours(8))));
    [Fact] public void DateOnly_is_iso() => Assert.Equal("2026-10-19", Formatter.Format(new DateOnly(2026, 10, 19)));
    [Fact] public void TimeSpan_is_constant_format() => Assert.Equal("01:02:03", Formatter.Format(new TimeSpan(1, 2, 3)));
    [Fact] public void Guid_is_D() => Assert.Equal("00000000-0000-0000-0000-000000000001", Formatter.Format(Guid.Parse("00000000-0000-0000-0000-000000000001")));
    [Fact] public void Enum_has_type_name() => Assert.Equal("Status.Active", Formatter.Format(Status.Active));
    [Fact] public void Type_is_csharp_name() => Assert.Equal("Dictionary<String, List<Int32>>", Formatter.Format(typeof(Dictionary<string, List<int>>)));
    [Fact] public void List_shows_items() => Assert.Equal("[1, 2, 3]", Formatter.Format(new List<int> { 1, 2, 3 }));
    [Fact] public void Empty_list() => Assert.Equal("[]", Formatter.Format(new List<int>()));
    [Fact] public void Dictionary_shows_pairs() => Assert.Equal("{[\"a\"] = 1, [\"b\"] = 2}", Formatter.Format(new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 }));
    [Fact] public void ToString_override_is_used() => Assert.Equal("named!", Formatter.Format(new Named()));
    [Fact] public void Record_shows_members() => Assert.Equal("Row { Code = \"BBB\", Qty = 2 }", Formatter.Format(new Row("BBB", 2)));
    [Fact] public void Record_struct_shows_members() => Assert.Equal("Tick { N = 3 }", Formatter.Format(new Tick(3)));
    [Fact] public void Anonymous_shows_members() => Assert.Equal("{ Id = 1 }", Formatter.Format(new { Id = 1 }));
    [Fact] public void List_is_cut_after_ten() => Assert.Equal("[0, 1, 2, 3, 4, 5, 6, 7, 8, 9, ... (5 more)]", Formatter.Format(Enumerable.Range(0, 15).ToList()));
    [Fact] public void Exactly_ten_is_not_cut() => Assert.Equal("[0, 1, 2, 3, 4, 5, 6, 7, 8, 9]", Formatter.Format(Enumerable.Range(0, 10).ToList()));
    [Fact] public void Cycle_is_named_not_followed() { var n = new Node(); n.Next = n; Assert.Equal("Node { Next = <cycle Node> }", Formatter.Format(n)); }
    [Fact] public void Throwing_getter_is_shown() => Assert.Equal("Bad { Value = <threw InvalidOperationException> }", Formatter.Format(new Bad()));
    [Fact] public void Throwing_ToString_is_shown() => Assert.Equal("<threw InvalidOperationException>", Formatter.Format(new BadToString()));
    [Fact] public void Deep_graph_stops_at_max_depth() => Assert.Contains("{ ... }", Formatter.Format(MakeChain(20)));
    [Fact] public void Long_string_is_cut() => Assert.EndsWith("... (300 more chars)\"", Formatter.Format(new string('a', 500)));

    private static int _moves;

    private static IEnumerable<int> Counting(int n)
    {
        for (var i = 0; i < n; i++) { _moves++; yield return i; }
    }

    [Fact]
    public void Huge_sequence_is_cut_while_enumerating()
    {
        _moves = 0;
        var text = Formatter.Format(Counting(1_000_000));
        Assert.True(_moves < 1_000, $"enumerated {_moves}");
        Assert.EndsWith(", ... (more)]", text);
    }

    [Fact]
    public void Sequence_of_exactly_ten_has_no_tail()
    {
        _moves = 0;
        Assert.Equal("[0, 1, 2, 3, 4, 5, 6, 7, 8, 9]", Formatter.Format(Counting(10)));
    }

    [Fact] public void Read_only_collection_reports_exact_count() => Assert.EndsWith("... (90 more)]", Formatter.Format(new ReadOnlyOnly(100)));
    [Fact] public void Generic_collection_reports_exact_count() => Assert.EndsWith("... (90 more)]", Formatter.Format(new HashSet<int>(Enumerable.Range(0, 100))));
    [Fact] public void Array_reports_exact_count() => Assert.EndsWith("... (90 more)]", Formatter.Format(Enumerable.Range(0, 100).ToArray()));
    [Fact] public void Huge_ToString_is_cut() => Assert.Equal(200 + "... (9999800 more chars)".Length, Formatter.Format(new StringBuilder().Append('x', 10_000_000)).Length);
    [Fact] public void Short_ToString_is_not_cut() => Assert.Equal("named!", Formatter.Format(new Named()));

    [Fact]
    public void Failure_message_for_huge_ToString_is_capped()
    {
        var a = new StringBuilder().Append('a', 10_000_000);
        var b = new StringBuilder().Append('b', 10_000_000);
        var ex = Assert.Throws<VouchException>(() => a.Should().Be(b));
        Assert.True(ex.Message.Length < 100 * 1024, $"message was {ex.Message.Length} chars");
    }

    [Fact] public void Big_list_reports_exact_count() => Assert.EndsWith("... (100000 more)]", Formatter.Format(Enumerable.Range(0, 100_010).ToList()));
    [Fact] public void Big_dictionary_reports_exact_count() => Assert.EndsWith("... (140 more)}", Formatter.Format(Enumerable.Range(0, 150).ToDictionary(i => i, i => i)));

    [Fact]
    public async Task Unfinished_task_does_not_block()
    {
        var format = Task.Run(() => Formatter.Format(new TaskCompletionSource<int>().Task));
        var finished = await Task.WhenAny(format, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.Same(format, finished);
        Assert.Equal("Task<Int32> { Status = WaitingForActivation }", await format);
    }

    [Fact]
    public void Cut_does_not_split_a_surrogate_pair()
    {
        var text = new string('a', 199) + "\U0001F600" + "more text";
        var output = Formatter.Format(text);
        var quoted = output.Substring(1, output.IndexOf("...", StringComparison.Ordinal) - 1);
        Assert.False(char.IsHighSurrogate(quoted[^1]));
        Assert.Equal(199, quoted.Length);
    }

    [Fact]
    public void ToString_cut_does_not_split_a_surrogate_pair()
    {
        var output = Formatter.Format(new Texty(new string('a', 199) + "\U0001F600" + "more text"));
        var kept = output.Substring(0, output.IndexOf("...", StringComparison.Ordinal));
        Assert.False(char.IsHighSurrogate(kept[^1]));
    }

    private sealed class Texty(string text)
    {
        public override string ToString() => text;
    }

    [Fact] public void Lazy_is_not_evaluated() => Assert.Equal("Lazy<Int32> { IsValueCreated = false }", Formatter.Format(new Lazy<int>(() => throw new InvalidOperationException())));

    [Fact]
    public void Shared_child_is_not_a_cycle()
    {
        var shared = new Node();
        var text = Formatter.Format(new Pair { A = shared, B = shared });
        Assert.DoesNotContain("<cycle", text);
        Assert.Equal("Pair { A = Node { Next = null }, B = Node { Next = null } }", text);
    }

    [Fact]
    public void Throwing_enumeration_does_not_escape()
    {
        static IEnumerable<int> Boom() { yield return 1; throw new InvalidOperationException(); }
        Assert.Equal("[1<threw InvalidOperationException>]", Formatter.Format(Boom()));
    }
}
