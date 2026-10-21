using System.Text;
using Xunit;

namespace Vouch.Tests;

public class HuntRepros
{
    private static readonly TimeSpan Guard = TimeSpan.FromSeconds(10);

    // Runs the call on the thread pool and fails the test if it takes longer than the guard.
    private static T Bounded<T>(Func<T> call)
    {
        var task = Task.Run(call);
        if (!task.Wait(Guard))
        {
            Assert.Fail($"Did not finish within {Guard.TotalSeconds} s.");
        }

        return task.Result;
    }

    private static void BoundedRun(Action call) => Bounded<object?>(() => { call(); return null; });

    // ---------- F2: shared non-cyclic graph ----------

    private sealed class N
    {
        public N? A { get; set; }
        public N? B { get; set; }
        public N? C { get; set; }
        public N? D { get; set; }
        public N? E { get; set; }
    }

    private static N SharedGraph(int levels)
    {
        var node = new N();
        for (var i = 1; i < levels; i++)
        {
            node = new N { A = node, B = node, C = node, D = node, E = node };
        }

        return node;
    }

    [Fact]
    public void F2_equivalency_on_shared_graph_finishes()
    {
        var left = SharedGraph(11);
        var right = SharedGraph(11);
        BoundedRun(() => left.Should().BeEquivalentTo(right));
    }

    // ---------- F3: any-order matcher on equatable kinds ----------

    private sealed class Key : IEquatable<Key>
    {
        public Key(int id) => Id = id;
        public int Id { get; }
        public bool Equals(Key? other) => other is not null && other.Id == Id;
        public override bool Equals(object? obj) => Equals(obj as Key);
        public override int GetHashCode() => Id;
    }

    [Fact]
    public void F3_any_order_200k_uris_finishes()
    {
        var a = Enumerable.Range(0, 200_000).Select(i => new Uri($"http://example.com/{i}")).ToArray();
        var b = a.Reverse().ToArray();
        BoundedRun(() => a.Should().BeEquivalentTo(b));
    }

    [Fact]
    public void F3_any_order_200k_equals_overriding_class_finishes()
    {
        var a = Enumerable.Range(0, 200_000).Select(i => new Key(i)).ToArray();
        var b = a.Reverse().Select(k => new Key(k.Id)).ToArray();
        BoundedRun(() => a.Should().BeEquivalentTo(b));
    }

    // ---------- F4: Func<ValueTask> NotThrow ----------

    [Fact]
    public void F4_func_valuetask_not_throw_sees_the_exception()
    {
        Func<ValueTask> f = async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException();
        };

        Assert.Throws<VouchException>(() => f.Should().NotThrow());
    }

    [Fact]
    public void F4_func_valuetask_of_int_not_throw_sees_the_exception()
    {
        Func<ValueTask<int>> f = async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException();
        };

        Assert.Throws<VouchException>(() => f.Should().NotThrow());
    }

    // ---------- F5: formatter caps ----------

    [Fact]
    public void F5_failure_message_for_huge_tostring_is_capped()
    {
        var a = new StringBuilder().Append('x', 10 * 1024 * 1024);
        var b = new StringBuilder().Append('y', 10 * 1024 * 1024);
        var ex = Bounded<Exception?>(() =>
        {
            try { a.Should().Be(b); return null; }
            catch (Exception e) { return e; }
        });
        var vouch = Assert.IsType<VouchException>(ex);
        Assert.True(vouch.Message.Length < 100 * 1024, $"Message was {vouch.Message.Length} chars.");
    }

    private sealed class Counting : IEnumerable<int>
    {
        public int MoveNexts;

        public IEnumerator<int> GetEnumerator()
        {
            for (var i = 0; i < 100_000; i++)
            {
                MoveNexts++;
                yield return i;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Fact]
    public void F5_formatting_a_lazy_sequence_stops_early()
    {
        var gen = new Counting();
        Bounded(() => Formatter.Format(gen));
        Assert.True(gen.MoveNexts < 1_000, $"MoveNext ran {gen.MoveNexts} times.");
    }

    // ---------- F6: solicitation regex ----------

    [Fact]
    public void F6_solicitation_regex_does_not_match_errors_2()
    {
        // Same pattern as .github/workflows/flag-solicitation.yml:35, first alternative only.
        var regex = new System.Text.RegularExpressions.Regex(
            @"(\$|usd|eur|£|₱|inr|rs\.?)[\s]*[0-9]",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        Assert.False(regex.IsMatch("I see errors 2 times in the log"));
    }

    // ---------- F8: enums and DateTime Kind ----------

    private enum Color { Red = 0, Green = 1 }
    private enum Shape { Circle = 0, Square = 1 }
    private sealed class HasColor { public object? Value { get; set; } }

    [Fact]
    public void F8_enums_of_different_types_are_not_equivalent()
    {
        var left = new HasColor { Value = Color.Red };
        var right = new HasColor { Value = Shape.Circle };
        Assert.Throws<VouchException>(() => left.Should().BeEquivalentTo(right));
    }

    [Fact]
    public void F8_datetime_be_respects_kind()
    {
        var utc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var local = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Local);
        Assert.Throws<VouchException>(() => utc.Should().Be(local));
    }

    // ---------- F9: NaN ordering ----------

    [Fact]
    public void F9_half_nan_is_not_less_than_one()
    {
        Assert.Throws<VouchException>(() => Half.NaN.Should().BeLessThan(Half.One));
    }

    [Fact]
    public void F9_double_array_with_nan_is_not_ascending()
    {
        Assert.Throws<VouchException>(() => new[] { 1.0, double.NaN, 2.0 }.Should().BeInAscendingOrder());
    }

    // ---------- F10: wildcard trailing newline ----------

    // README says one trailing newline on the subject is ignored, so this is documented behaviour.
    // The test asserts the documented rule: one is ignored, two are not.
    [Fact]
    public void F10_match_ignores_exactly_one_trailing_newline()
    {
        "abc\n".Should().Match("abc");
        Assert.Throws<VouchException>(() => "abc\n\n".Should().Match("abc"));
    }
}
