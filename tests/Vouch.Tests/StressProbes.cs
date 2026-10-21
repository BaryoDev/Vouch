using System.Diagnostics;
using System.Globalization;
using Xunit;

namespace Vouch.Tests;

public class StressProbes
{
    private static readonly TimeSpan Guard = TimeSpan.FromSeconds(5);
    private const int MaxMessage = 100 * 1024;

    // Runs the call on the thread pool and fails the test if it takes longer than the guard.
    private static T Bounded<T>(Func<T> call, TimeSpan? limit = null)
    {
        var task = Task.Run(call);
        if (!task.Wait(limit ?? Guard))
        {
            Assert.Fail($"Did not finish within {(limit ?? Guard).TotalSeconds} s.");
        }

        return task.Result;
    }

    // Returns the message of the VouchException thrown by the call. Fails if nothing or something else is thrown.
    private static string FailureMessage(Action call, TimeSpan? limit = null)
    {
        var thrown = Bounded<Exception?>(() =>
        {
            try { call(); return null; }
            catch (Exception ex) { return ex; }
        }, limit);
        Assert.NotNull(thrown);
        var vouch = Assert.IsType<VouchException>(thrown);
        return vouch.Message;
    }

    private sealed class Node
    {
        public int Value { get; set; }
        public Node? Next { get; set; }
    }

    private static Node Chain(int length)
    {
        var head = new Node { Value = 0 };
        var tail = head;
        for (var i = 1; i < length; i++)
        {
            tail.Next = new Node { Value = i };
            tail = tail.Next;
        }

        return head;
    }

    // ---------- Wildcard ----------

    [Fact]
    public void Wildcard_many_stars_against_long_nonmatching_string_finishes()
    {
        // Expect: fails fast with a VouchException (no match), within 5 s.
        var text = new string('a', 20_000);
        var pattern = string.Concat(Enumerable.Repeat("a*", 200)) + "b";
        var message = FailureMessage(() => text.Should().Match(pattern));
        Assert.True(message.Length < MaxMessage);
    }

    [Fact]
    public void Wildcard_star_question_mix_against_long_string_finishes()
    {
        // Expect: fails within 5 s. Quadratic worst case for the single-backtrack matcher.
        var text = new string('a', 30_000);
        var pattern = string.Concat(Enumerable.Repeat("*?", 5_000)) + "b";
        FailureMessage(() => text.Should().Match(pattern));
    }

    [Fact]
    public void Wildcard_huge_pattern_of_only_stars_matches()
    {
        // Expect: passes, within 5 s.
        var text = new string('a', 20_000);
        var pattern = new string('*', 20_000);
        Bounded(() => { text.Should().Match(pattern); return 0; });
    }

    [Fact]
    public void Wildcard_pattern_longer_than_text_with_no_match_finishes()
    {
        // Expect: fails within 5 s.
        var text = new string('a', 10_000);
        var pattern = new string('a', 10_001);
        FailureMessage(() => text.Should().Match(pattern));
    }

    [Fact]
    public void Regex_catastrophic_backtracking_is_cut_off_with_a_message()
    {
        // Expect: VouchException (timeout message), not a hang. The library sets a 1 s regex timeout.
        var text = new string('a', 50) + "!";
        var message = FailureMessage(() => text.Should().MatchRegex("^(a+)+$"));
        Assert.Contains("timed out", message);
    }

    // ---------- Formatting size and time ----------

    [Fact]
    public void Format_million_element_list_HaveCount_message_is_bounded()
    {
        // Expect: fails, message under 100 KB.
        var list = Enumerable.Range(0, 1_000_000).ToList();
        var message = FailureMessage(() => list.Should().HaveCount(5));
        Assert.True(message.Length < MaxMessage, $"Message length {message.Length}");
    }

    [Fact]
    public void Format_million_element_list_Equal_message_is_bounded()
    {
        // Expect: fails, message under 100 KB.
        var list = Enumerable.Range(0, 1_000_000).ToList();
        var other = Enumerable.Range(1, 1_000_000).ToList();
        var message = FailureMessage(() => list.Should().Equal(other));
        Assert.True(message.Length < MaxMessage, $"Message length {message.Length}");
    }

    [Fact]
    public void Format_million_element_list_Contain_missing_message_is_bounded()
    {
        // Expect: fails, message under 100 KB.
        var list = Enumerable.Range(0, 1_000_000).ToList();
        var message = FailureMessage(() => list.Should().Contain(-1));
        Assert.True(message.Length < MaxMessage, $"Message length {message.Length}");
    }

    [Fact]
    public void Format_million_element_list_BeEquivalentTo_message_is_bounded()
    {
        // Expect: fails (or hits the item cap) with a bounded message, within 5 s.
        var list = Enumerable.Range(0, 1_000_000).ToList();
        var other = Enumerable.Range(1, 1_000_000).ToList();
        var message = FailureMessage(() => list.Should().BeEquivalentTo(other));
        Assert.True(message.Length < MaxMessage, $"Message length {message.Length}");
    }

    [Fact]
    public void Format_lazy_million_element_sequence_message_is_bounded()
    {
        // Expect: fails, bounded message. The formatter counts the remainder of a non-ICollection up to 100,000.
        IEnumerable<int> Lazy() { for (var i = 0; i < 1_000_000; i++) yield return i; }
        var message = FailureMessage(() => Lazy().Should().BeEmpty());
        Assert.True(message.Length < MaxMessage, $"Message length {message.Length}");
    }

    [Fact]
    public void Format_10MB_string_Be_message_is_bounded()
    {
        // Expect: fails, message under 100 KB (strings are truncated at 200 chars).
        var a = new string('x', 10 * 1024 * 1024);
        var b = a + "y";
        var message = FailureMessage(() => a.Should().Be(b));
        Assert.True(message.Length < MaxMessage, $"Message length {message.Length}");
    }

    [Fact]
    public void Format_10MB_string_Contain_message_is_bounded()
    {
        // Expect: fails, message under 100 KB.
        var a = new string('x', 10 * 1024 * 1024);
        var message = FailureMessage(() => a.Should().Contain("needle"));
        Assert.True(message.Length < MaxMessage, $"Message length {message.Length}");
    }

    [Fact]
    public void Format_10MB_string_in_a_collection_message_is_bounded()
    {
        // Expect: fails, bounded message.
        var big = new string('x', 10 * 1024 * 1024);
        var list = new List<string> { big, big, big };
        var message = FailureMessage(() => list.Should().HaveCount(1));
        Assert.True(message.Length < MaxMessage, $"Message length {message.Length}");
    }

    [Fact]
    public void Format_10MB_string_Match_message_is_bounded()
    {
        // Expect: fails, bounded message.
        var a = new string('x', 10 * 1024 * 1024);
        var message = FailureMessage(() => a.Should().Match("*y"));
        Assert.True(message.Length < MaxMessage, $"Message length {message.Length}");
    }

    [Fact]
    public void Format_100k_dictionary_ContainKey_missing_message_is_bounded()
    {
        // Expect: fails, bounded message. The existing test shows ContainKey lists "It has:" entries, so this may list all 100k.
        IDictionary<int, int> dict = Enumerable.Range(0, 100_000).ToDictionary(i => i, i => i);
        var message = FailureMessage(() => dict.Should().ContainKey(-1));
        Assert.True(message.Length < MaxMessage, $"Message length {message.Length}");
    }

    [Fact]
    public void Format_100k_dictionary_HaveCount_message_is_bounded()
    {
        // Expect: fails, bounded message.
        IDictionary<int, int> dict = Enumerable.Range(0, 100_000).ToDictionary(i => i, i => i);
        var message = FailureMessage(() => dict.Should().HaveCount(3));
        Assert.True(message.Length < MaxMessage, $"Message length {message.Length}");
    }

    [Fact]
    public void Format_100k_dictionary_BeEquivalentTo_message_is_bounded()
    {
        // Expect: fails, bounded message.
        IDictionary<int, int> a = Enumerable.Range(0, 100_000).ToDictionary(i => i, i => i);
        IDictionary<int, int> b = Enumerable.Range(0, 100_000).ToDictionary(i => i, i => i + 1);
        var message = FailureMessage(() => a.Should().BeEquivalentTo(b));
        Assert.True(message.Length < MaxMessage, $"Message length {message.Length}");
    }

    // ---------- Equivalency: cycles and depth ----------

    [Fact]
    public void Equivalency_cyclic_graph_terminates()
    {
        // Expect: terminates. README says a self-referring graph fails with a message (or passes); no hang, no crash.
        var a = new Node { Value = 1 };
        var b = new Node { Value = 2 };
        a.Next = b;
        b.Next = a;
        var c = new Node { Value = 1 };
        var d = new Node { Value = 2 };
        c.Next = d;
        d.Next = c;
        var outcome = Bounded<string?>(() =>
        {
            try { a.Should().BeEquivalentTo(c); return null; }
            catch (VouchException ex) { return ex.Message; }
        });
        if (outcome is not null)
        {
            Assert.True(outcome.Length < MaxMessage);
            Assert.Contains("ycle", outcome);
        }
    }

    [Fact]
    public void Equivalency_cyclic_graph_against_itself_terminates()
    {
        // Expect: terminates, pass or a clear cycle message.
        var a = new Node { Value = 1 };
        var b = new Node { Value = 2 };
        a.Next = b;
        b.Next = a;
        Bounded<string?>(() =>
        {
            try { a.Should().BeEquivalentTo(a); return null; }
            catch (VouchException ex) { return ex.Message; }
        });
    }

    [Fact]
    public void Equivalency_cyclic_graph_different_values_fails_with_message()
    {
        // Expect: VouchException naming the differing member.
        var a = new Node { Value = 1 };
        var b = new Node { Value = 2 };
        a.Next = b;
        b.Next = a;
        var c = new Node { Value = 1 };
        var d = new Node { Value = 99 };
        c.Next = d;
        d.Next = c;
        var message = FailureMessage(() => a.Should().BeEquivalentTo(c));
        Assert.Contains("Value", message);
    }

    [Fact]
    public void Format_cyclic_graph_in_failure_message_terminates()
    {
        // Expect: fails with a bounded message containing "cycle".
        var a = new Node { Value = 1 };
        var b = new Node { Value = 2 };
        a.Next = b;
        b.Next = a;
        var message = FailureMessage(() => a.Should().BeNull());
        Assert.True(message.Length < MaxMessage);
    }

    // Depth probes. Run each in its own test so a crash is attributable.
    // The 5,000 deep ones are expected to be safe (Equivalency.MaxDepth is 10, Formatter.MaxDepth is 3).
    // The 50,000 deep ones are the dangerous ones: a StackOverflowException would kill the test host.
    // If the host dies during a run, the last test name printed identifies the culprit.

    [Fact]
    public void Deep5000_Equivalency_identical_lists_does_not_crash()
    {
        // Expect: pass or a clear depth failure, no crash.
        var a = Chain(5_000);
        var b = Chain(5_000);
        Bounded<string?>(() =>
        {
            try { a.Should().BeEquivalentTo(b); return null; }
            catch (VouchException ex) { return ex.Message; }
        });
    }

    [Fact]
    public void Deep5000_Equivalency_differing_tail_fails_with_message()
    {
        // Expect: VouchException. Difference is beyond the depth limit, so it may also pass; must not crash.
        var a = Chain(5_000);
        var b = Chain(5_000);
        var tail = b;
        while (tail.Next is not null) tail = tail.Next;
        tail.Value = -1;
        Bounded<string?>(() =>
        {
            try { a.Should().BeEquivalentTo(b); return null; }
            catch (VouchException ex) { return ex.Message; }
        });
    }

    [Fact]
    public void Deep5000_Formatting_in_failure_message_is_bounded()
    {
        // Expect: fails with a bounded message.
        var a = Chain(5_000);
        var message = FailureMessage(() => a.Should().BeNull());
        Assert.True(message.Length < MaxMessage);
    }

    [Fact]
    public void Deep50000_Equivalency_identical_lists_does_not_crash()
    {
        // DANGEROUS if the comparison recurses without a depth cap. Expect: pass or a clear depth failure.
        var a = Chain(50_000);
        var b = Chain(50_000);
        Bounded<string?>(() =>
        {
            try { a.Should().BeEquivalentTo(b); return null; }
            catch (VouchException ex) { return ex.Message; }
        });
    }

    [Fact]
    public void Deep50000_Equivalency_differing_head_fails_with_message()
    {
        // Expect: VouchException naming Value.
        var a = Chain(50_000);
        var b = Chain(50_000);
        b.Value = -1;
        var message = FailureMessage(() => a.Should().BeEquivalentTo(b));
        Assert.Contains("Value", message);
    }

    [Fact]
    public void Deep50000_Formatting_in_failure_message_is_bounded()
    {
        // DANGEROUS if the formatter recurses without a depth cap. Expect: fails with a bounded message.
        var a = Chain(50_000);
        var message = FailureMessage(() => a.Should().BeNull());
        Assert.True(message.Length < MaxMessage);
    }

    [Fact]
    public void Deep50000_List_of_lists_formatting_does_not_crash()
    {
        // Expect: bounded message. Nested List<object> 50,000 deep.
        object inner = new List<object>();
        for (var i = 0; i < 50_000; i++) inner = new List<object> { inner };
        var message = FailureMessage(() => ((IEnumerable<object>)inner).Should().BeEmpty());
        Assert.True(message.Length < MaxMessage);
    }

    // ---------- Hostile members ----------

    private sealed class ThrowingGetter
    {
        public int Ok { get; set; } = 1;
        public int Bad => throw new InvalidOperationException("getter boom");
    }

    private sealed class ThrowingToString
    {
        public override string ToString() => throw new InvalidOperationException("tostring boom");
    }

    private sealed class BlockingGetter
    {
        public int Ok { get; set; } = 1;
        public int Bad
        {
            get { Thread.Sleep(TimeSpan.FromSeconds(60)); return 1; }
        }
    }

    [Fact]
    public void Throwing_getter_Be_failure_has_a_message()
    {
        // Expect: VouchException whose message mentions the throw, not an escaped InvalidOperationException.
        var subject = new ThrowingGetter();
        var message = FailureMessage(() => subject.Should().BeNull());
        Assert.Contains("threw", message);
    }

    [Fact]
    public void Throwing_getter_BeEquivalentTo_has_a_message()
    {
        // Expect: VouchException, not the raw getter exception.
        var a = new ThrowingGetter();
        var b = new ThrowingGetter();
        var message = FailureMessage(() => a.Should().BeEquivalentTo(b));
        Assert.False(string.IsNullOrWhiteSpace(message));
    }

    [Fact]
    public void Throwing_ToString_failure_has_a_message()
    {
        // Expect: VouchException with "<threw InvalidOperationException>" in it.
        var subject = new ThrowingToString();
        var message = FailureMessage(() => subject.Should().BeNull());
        Assert.Contains("threw", message);
    }

    [Fact]
    public void Throwing_ToString_in_collection_failure_has_a_message()
    {
        // Expect: VouchException.
        var list = new List<ThrowingToString> { new(), new() };
        var message = FailureMessage(() => list.Should().HaveCount(5));
        Assert.Contains("threw", message);
    }

    [Fact]
    public void Throwing_ToString_Be_between_different_instances_has_a_message()
    {
        // Expect: VouchException.
        var a = new ThrowingToString();
        var b = new ThrowingToString();
        var message = FailureMessage(() => a.Should().Be(b));
        Assert.False(string.IsNullOrWhiteSpace(message));
    }

    [Fact]
    public void Blocking_getter_failure_finishes_within_guard()
    {
        // Vouch has no timeout of its own and the getter sleeps 60 s, so this is EXPECTED TO FAIL on the guard
        // unless the formatter avoids calling getters. A failure here shows the formatter can be hung by a member.
        var subject = new BlockingGetter();
        FailureMessage(() => subject.Should().BeNull());
    }

    [Fact]
    public void Blocking_getter_BeEquivalentTo_finishes_within_guard()
    {
        // Same as above through equivalency. EXPECTED TO FAIL on the guard.
        var a = new BlockingGetter();
        var b = new BlockingGetter();
        FailureMessage(() => a.Should().BeEquivalentTo(b));
    }

    // ---------- Parallel use ----------
    // Vouch has no scope or batch feature (no AssertionScope type in src/Vouch), so only plain parallel use is probed.

    [Fact]
    public void Parallel_64_tasks_mixed_assertions_never_leak()
    {
        // Expect: every failing assertion throws VouchException naming its own task id,
        // no passing assertion throws, no message carries another task's marker.
        const int tasks = 64;
        const int rounds = 1_000;
        var errors = new System.Collections.Concurrent.ConcurrentBag<string>();
        var work = Enumerable.Range(0, tasks).Select(id => Task.Run(() =>
        {
            var marker = $"task-{id}-marker";
            var other = $"task-{id}-other";
            for (var i = 0; i < rounds; i++)
            {
                try { marker.Should().Be(marker); }
                catch (Exception ex) { errors.Add($"{id}: passing string threw {ex.GetType().Name}"); }

                try { (id + i).Should().Be(id + i); }
                catch (Exception ex) { errors.Add($"{id}: passing int threw {ex.GetType().Name}"); }

                try { new[] { id, i }.Should().BeEquivalentTo(new[] { i, id }); }
                catch (Exception ex) { errors.Add($"{id}: passing equivalency threw {ex.GetType().Name}"); }

                Check(id, () => marker.Should().Be(other), marker, other, errors);
                Check(id, () => (id + i).Should().Be(id + i + 1), null, null, errors);
                Check(id, () => new[] { id, i }.Should().BeEquivalentTo(new[] { i, id + 1 }), null, null, errors);
                Check(id, () => marker.Should().Match(other + "*"), marker, other, errors);
            }
        })).ToArray();

        Assert.True(Task.WaitAll(work, TimeSpan.FromSeconds(60)), "Parallel work did not finish in 60 s.");
        Assert.True(errors.IsEmpty, string.Join("\n", errors.Take(20)));

        static void Check(int id, Action call, string? mustContain, string? alsoContains, System.Collections.Concurrent.ConcurrentBag<string> errors)
        {
            try
            {
                call();
                errors.Add($"{id}: failing assertion did not throw");
            }
            catch (VouchException ex)
            {
                if (mustContain is not null && !ex.Message.Contains(mustContain)) errors.Add($"{id}: message lacks own marker: {ex.Message}");
                for (var other = 0; other < 64; other++)
                {
                    if (other != id && ex.Message.Contains($"task-{other}-")) { errors.Add($"{id}: message leaked task {other}: {ex.Message}"); break; }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{id}: failing assertion threw {ex.GetType().Name}");
            }
        }
    }

    [Fact]
    public async Task Parallel_64_tasks_async_assertions_never_leak()
    {
        // Expect: ThrowAsync passes for the matching type, fails for the wrong type, per task, 1,000 times.
        const int tasks = 64;
        const int rounds = 1_000;
        var errors = new System.Collections.Concurrent.ConcurrentBag<string>();
        var work = Enumerable.Range(0, tasks).Select(id => Task.Run(async () =>
        {
            for (var i = 0; i < rounds; i++)
            {
                Func<Task> throwsInvalid = async () => { await Task.Yield(); throw new InvalidOperationException($"id-{id}"); };
                try { await throwsInvalid.Should().ThrowAsync<InvalidOperationException>(); }
                catch (Exception ex) { errors.Add($"{id}: passing ThrowAsync threw {ex.GetType().Name}"); }

                try
                {
                    await throwsInvalid.Should().ThrowAsync<ArgumentException>();
                    errors.Add($"{id}: wrong type did not fail");
                }
                catch (VouchException) { }
                catch (Exception ex) { errors.Add($"{id}: wrong type threw {ex.GetType().Name}"); }
            }
        })).ToArray();

        var all = Task.WhenAll(work);
        var finished = await Task.WhenAny(all, Task.Delay(TimeSpan.FromSeconds(60)));
        Assert.Same(all, finished);
        await all;
        Assert.True(errors.IsEmpty, string.Join("\n", errors.Take(20)));
    }

    // ---------- False passes ----------

    [Fact]
    public void NaN_does_not_equal_one()
    {
        // Expect: fails.
        FailureMessage(() => double.NaN.Should().Be(1.0));
        FailureMessage(() => 1.0.Should().Be(double.NaN));
    }

    [Fact]
    public void NaN_Be_NaN_matches_documented_behaviour()
    {
        // Numeric Be uses T.Equals, where NaN.Equals(NaN) is true (as in FluentAssertions). Expect: passes.
        double.NaN.Should().Be(double.NaN);
        float.NaN.Should().Be(float.NaN);
    }

    [Fact]
    public void NaN_Approximately_NaN_does_not_pass_silently()
    {
        // Expect: fails. |NaN - NaN| is NaN and NaN <= precision is false. If this passes, a NaN result hides in tolerance checks.
        FailureMessage(() => double.NaN.Should().BeApproximately(double.NaN, 0.1));
    }

    [Fact]
    public void NaN_Approximately_number_fails_and_number_Approximately_NaN_fails()
    {
        // Expect: both fail, even with a huge precision.
        FailureMessage(() => double.NaN.Should().BeApproximately(1.0, double.MaxValue));
        FailureMessage(() => 1.0.Should().BeApproximately(double.NaN, double.MaxValue));
        FailureMessage(() => double.NaN.Should().BeApproximately(1.0, double.PositiveInfinity));
    }

    [Fact]
    public void Precision_of_NaN_never_passes()
    {
        // Expect: fails. A NaN tolerance must not accept everything.
        FailureMessage(() => 1.0.Should().BeApproximately(2.0, double.NaN));
    }

    [Fact]
    public void Epsilon_is_not_zero()
    {
        // Expect: Be fails between 0 and Epsilon; Approximately passes with precision Epsilon and fails with precision 0.
        FailureMessage(() => 0.0.Should().Be(double.Epsilon));
        0.0.Should().BeApproximately(double.Epsilon, double.Epsilon);
        FailureMessage(() => 0.0.Should().BeApproximately(double.Epsilon, 0.0));
    }

    [Fact]
    public void Negative_zero_equals_zero()
    {
        // Expect: passes, -0.0 == 0.0 in IEEE 754 and Double.Equals(0.0, -0.0) is true.
        (-0.0).Should().Be(0.0);
        0.0.Should().Be(-0.0);
        (-0.0).Should().BeApproximately(0.0, 0.0);
    }

    [Fact]
    public void Infinity_within_precision_semantics()
    {
        // Expect: +inf equals +inf; +inf vs -inf fails; +inf vs a finite value fails with a finite precision.
        double.PositiveInfinity.Should().BeApproximately(double.PositiveInfinity, 0.1);
        double.NegativeInfinity.Should().BeApproximately(double.NegativeInfinity, 0.0);
        FailureMessage(() => double.PositiveInfinity.Should().BeApproximately(double.NegativeInfinity, double.MaxValue));
        FailureMessage(() => double.PositiveInfinity.Should().BeApproximately(1.0, double.MaxValue));
    }

    [Fact]
    public void Infinity_with_infinite_precision_does_not_accept_a_finite_number()
    {
        // Expect: fails. |inf - 1| = inf, and inf <= inf is true in the implementation, so this probe may expose a false pass.
        FailureMessage(() => double.PositiveInfinity.Should().BeApproximately(1.0, double.PositiveInfinity));
    }

    [Fact]
    public void Float_NaN_and_infinity_behave_like_double()
    {
        // Expect: fails for NaN vs number; infinity vs finite with finite precision fails.
        FailureMessage(() => float.NaN.Should().Be(1f));
        FailureMessage(() => float.NaN.Should().BeApproximately(float.NaN, 0.1f));
        FailureMessage(() => float.PositiveInfinity.Should().BeApproximately(1f, 1f));
    }

    [Fact]
    public void Decimal_approximation_overflow_fails_with_a_message()
    {
        // Expect: VouchException, not OverflowException.
        FailureMessage(() => decimal.MaxValue.Should().BeApproximately(decimal.MinValue, 1m));
    }

    [Fact]
    public void Equivalency_NaN_vs_number_fails_and_NaN_vs_NaN_is_stable()
    {
        // Expect: NaN vs 1.0 fails; NaN vs NaN passes (README: float NaN equals double NaN).
        double nan = double.NaN;
        FailureMessage(() => nan.Should().BeEquivalentTo(1.0));
        nan.Should().BeEquivalentTo(double.NaN);
    }

    [Fact]
    public void DateTime_Local_and_Utc_with_same_ticks_are_different_instants()
    {
        // Expect: Be fails, because the two values are different instants when the local offset is not zero.
        // DateTime == ignores Kind, so this probe may expose a false pass. Skipped when local time is UTC.
        if (TimeZoneInfo.Local.BaseUtcOffset == TimeSpan.Zero && !TimeZoneInfo.Local.SupportsDaylightSavingTime)
        {
            return;
        }

        var ticks = new DateTime(2026, 1, 15, 12, 0, 0).Ticks;
        var local = new DateTime(ticks, DateTimeKind.Local);
        var utc = new DateTime(ticks, DateTimeKind.Utc);
        FailureMessage(() => local.Should().Be(utc));
    }

    [Fact]
    public void DateTime_Utc_vs_Local_BeCloseTo_zero_precision_fails_when_offset_nonzero()
    {
        // Expect: fails with zero precision for same ticks across kinds when the offset is not zero.
        if (TimeZoneInfo.Local.BaseUtcOffset == TimeSpan.Zero && !TimeZoneInfo.Local.SupportsDaylightSavingTime)
        {
            return;
        }

        var ticks = new DateTime(2026, 1, 15, 12, 0, 0).Ticks;
        var local = new DateTime(ticks, DateTimeKind.Local);
        var utc = new DateTime(ticks, DateTimeKind.Utc);
        FailureMessage(() => local.Should().BeCloseTo(utc, TimeSpan.Zero));
    }

    [Fact]
    public void DateTimeOffset_different_offsets_same_instant_are_equal()
    {
        // Expect: passes for Be and BeCloseTo(zero); NotBe fails; BeBefore/BeAfter fail.
        var a = new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);
        var b = new DateTimeOffset(2026, 1, 15, 20, 0, 0, TimeSpan.FromHours(8));
        a.Should().Be(b);
        a.Should().BeCloseTo(b, TimeSpan.Zero);
        FailureMessage(() => a.Should().NotBe(b));
        FailureMessage(() => a.Should().BeBefore(b));
        FailureMessage(() => a.Should().BeAfter(b));
    }

    [Fact]
    public void DateTimeOffset_same_wall_clock_different_offsets_are_not_equal()
    {
        // Expect: Be fails. Same wall clock, different instants.
        var a = new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);
        var b = new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.FromHours(8));
        FailureMessage(() => a.Should().Be(b));
    }

    private static T WithCulture<T>(string name, Func<T> call)
    {
        var saved = CultureInfo.CurrentCulture;
        var savedUi = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(name);
            CultureInfo.CurrentUICulture = new CultureInfo(name);
            return call();
        }
        finally
        {
            CultureInfo.CurrentCulture = saved;
            CultureInfo.CurrentUICulture = savedUi;
        }
    }

    [Fact]
    public void Turkish_culture_string_Be_is_ordinal()
    {
        // Expect: "I" is not "i" and "I" is not dotless "ı" under tr-TR. Be fails for both. Runs on the calling thread.
        WithCulture("tr-TR", () =>
        {
            Assert.Throws<VouchException>(() => "I".Should().Be("i"));
            Assert.Throws<VouchException>(() => "I".Should().Be("ı"));
            "TITLE".Should().Be("TITLE");
            return 0;
        });
    }

    [Fact]
    public void Turkish_culture_string_BeEquivalentTo_does_not_fold_dotless_i()
    {
        // Expect: OrdinalIgnoreCase, so "I" equals "i" but does not equal "ı" or "İ".
        WithCulture("tr-TR", () =>
        {
            "I".Should().BeEquivalentTo("i");
            "TITLE".Should().BeEquivalentTo("title");
            Assert.Throws<VouchException>(() => "I".Should().BeEquivalentTo("ı"));
            Assert.Throws<VouchException>(() => "i".Should().BeEquivalentTo("İ"));
            return 0;
        });
    }

    [Fact]
    public void Turkish_culture_Contain_StartWith_EndWith_are_ordinal()
    {
        // Expect: all fail for case or dotless differences.
        WithCulture("tr-TR", () =>
        {
            Assert.Throws<VouchException>(() => "FILE".Should().Contain("fi"));
            Assert.Throws<VouchException>(() => "FILE".Should().StartWith("fi"));
            Assert.Throws<VouchException>(() => "FILE".Should().EndWith("le"));
            "file".Should().Contain("il");
            return 0;
        });
    }

    [Fact]
    public void Turkish_culture_Match_wildcard_ignores_case_only_for_messages()
    {
        // Expect: string Match is case sensitive, so "FILE" does not match "fi*".
        WithCulture("tr-TR", () =>
        {
            Assert.Throws<VouchException>(() => "FILE".Should().Match("fi*"));
            "FILE".Should().Match("FI*");
            return 0;
        });
    }

    [Fact]
    public void Turkish_culture_does_not_change_formatted_numbers_and_dates()
    {
        // Expect: failure messages use the invariant culture: "1.5", not "1,5".
        WithCulture("tr-TR", () =>
        {
            var message = FailureMessage(() => 1.5.Should().Be(2.5));
            Assert.Contains("1.5", message);
            Assert.Contains("2.5", message);
            return 0;
        });
    }

    [Fact]
    public void NotThrow_on_method_that_throws_fails()
    {
        // Expect: VouchException saying it threw.
        Action act = () => throw new InvalidOperationException("boom");
        var message = FailureMessage(() => act.Should().NotThrow());
        Assert.Contains("InvalidOperationException", message);
    }

    [Fact]
    public void NotThrow_on_function_that_throws_fails()
    {
        // Expect: VouchException.
        Func<int> fn = () => throw new InvalidOperationException("boom");
        FailureMessage(() => fn.Should().NotThrow());
    }

    [Fact]
    public async Task NotThrowAsync_on_task_that_faults_fails()
    {
        // Expect: VouchException.
        Func<Task> fn = async () => { await Task.Yield(); throw new InvalidOperationException("boom"); };
        await Assert.ThrowsAsync<VouchException>(() => fn.Should().NotThrowAsync());
    }

    [Fact]
    public async Task NotThrowAsync_on_synchronous_throw_before_task_returns_fails()
    {
        // Expect: VouchException. The delegate throws instead of returning a task.
        Func<Task> fn = () => throw new InvalidOperationException("boom");
        await Assert.ThrowsAsync<VouchException>(() => fn.Should().NotThrowAsync());
    }

    [Fact]
    public async Task ThrowAsync_on_task_that_never_throws_fails()
    {
        // Expect: VouchException "did not throw". Task completes without awaiting anything.
        Func<Task> fn = () => Task.CompletedTask;
        await Assert.ThrowsAsync<VouchException>(() => fn.Should().ThrowAsync<InvalidOperationException>());
    }

    [Fact]
    public async Task ThrowAsync_on_task_that_throws_only_after_completion_of_returned_task_fails()
    {
        // Expect: VouchException. The exception is thrown on a fire-and-forget task that the returned task does not await.
        Func<Task> fn = () =>
        {
            _ = Task.Run(() => throw new InvalidOperationException("late"));
            return Task.CompletedTask;
        };
        await Assert.ThrowsAsync<VouchException>(() => fn.Should().ThrowAsync<InvalidOperationException>());
    }

    [Fact]
    public async Task ThrowAsync_on_wrong_exception_type_fails()
    {
        // Expect: VouchException.
        Func<Task> fn = async () => { await Task.Yield(); throw new ArgumentException("x"); };
        await Assert.ThrowsAsync<VouchException>(() => fn.Should().ThrowAsync<InvalidOperationException>());
    }

    [Fact]
    public async Task ThrowAsync_on_null_returning_delegate_fails_with_a_message()
    {
        // Expect: VouchException, not NullReferenceException.
        Func<Task> fn = () => null!;
        await Assert.ThrowsAsync<VouchException>(() => fn.Should().ThrowAsync<InvalidOperationException>());
    }

    [Fact]
    public void Empty_collection_is_not_equivalent_to_collection_of_nulls()
    {
        // Expect: both directions fail.
        var empty = new List<string?>();
        var nulls = new List<string?> { null, null };
        FailureMessage(() => empty.Should().BeEquivalentTo(nulls));
        FailureMessage(() => nulls.Should().BeEquivalentTo(empty));
        FailureMessage(() => empty.Should().Equal(nulls));
        FailureMessage(() => nulls.Should().BeEmpty());
    }

    [Fact]
    public void Collection_with_one_null_is_not_equivalent_to_empty_or_to_two_nulls()
    {
        // Expect: fails both ways.
        var one = new List<string?> { null };
        FailureMessage(() => one.Should().BeEquivalentTo(new List<string?>()));
        FailureMessage(() => one.Should().BeEquivalentTo(new List<string?> { null, null }));
        one.Should().BeEquivalentTo(new List<string?> { null });
    }

    [Fact]
    public void Null_collection_is_not_equivalent_to_empty_collection()
    {
        // Expect: fails.
        List<int>? missing = null;
        FailureMessage(() => missing.Should().BeEquivalentTo(new List<int>()));
    }

    [Fact]
    public void Duplicates_one_one_two_vs_one_two_two_are_not_equivalent()
    {
        // Expect: fails both ways, even though the sets are equal and the lengths match.
        var a = new[] { 1, 1, 2 };
        var b = new[] { 1, 2, 2 };
        FailureMessage(() => a.Should().BeEquivalentTo(b));
        FailureMessage(() => b.Should().BeEquivalentTo(a));
    }

    [Fact]
    public void Duplicates_in_any_order_still_match_when_counts_agree()
    {
        // Expect: passes.
        new[] { 1, 2, 1 }.Should().BeEquivalentTo(new[] { 1, 1, 2 });
    }

    [Fact]
    public void Duplicates_of_objects_one_one_two_vs_one_two_two_are_not_equivalent()
    {
        // Expect: fails both ways. Objects go through the any-order matcher rather than the scalar path.
        var a = new[] { new Node { Value = 1 }, new Node { Value = 1 }, new Node { Value = 2 } };
        var b = new[] { new Node { Value = 1 }, new Node { Value = 2 }, new Node { Value = 2 } };
        FailureMessage(() => a.Should().BeEquivalentTo(b));
        FailureMessage(() => b.Should().BeEquivalentTo(a));
    }

    [Fact]
    public void Duplicates_with_mixed_numeric_types_are_still_counted()
    {
        // Expect: fails. 1 and 1L are equal by README, but the counts of 1 and 2 differ.
        var a = new object[] { 1, 1L, 2 };
        var b = new object[] { 1, 2, 2L };
        FailureMessage(() => a.Should().BeEquivalentTo(b));
    }

    [Fact]
    public void Strict_ordering_rejects_reordered_collections()
    {
        // Expect: fails.
        FailureMessage(() => new[] { 1, 2, 3 }.Should().BeEquivalentTo(new[] { 3, 2, 1 }, o => o.WithStrictOrdering()));
    }

    [Fact]
    public void Equivalency_with_a_million_distinct_items_finishes()
    {
        // Expect: any order matching over 1,000,000 shuffled ints passes or fails in time (not quadratic).
        var a = Enumerable.Range(0, 1_000_000).ToArray();
        var b = a.Reverse().ToArray();
        Bounded(() => { a.Should().BeEquivalentTo(b); return 0; }, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void Equivalency_any_order_with_all_equal_objects_finishes()
    {
        // Expect: 5,000 identical-looking objects against 5,000 objects with one different finishes (matcher must not be quadratic in a way that hangs).
        var a = Enumerable.Range(0, 5_000).Select(_ => new Node { Value = 1 }).ToArray();
        var b = Enumerable.Range(0, 5_000).Select(i => new Node { Value = i == 4_999 ? 2 : 1 }).ToArray();
        FailureMessage(() => a.Should().BeEquivalentTo(b));
    }
}
