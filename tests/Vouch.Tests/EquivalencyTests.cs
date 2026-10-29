using System.Collections;
using System.Diagnostics;
using Xunit;

namespace Vouch.Tests;

public class EquivalencyTests
{
    public record Bundle(string Name, string[] Tests);

    public record struct Point(int X, int Y);

    public class EqPerson { public int Id { get; set; } public string? Name { get; set; } public int Extra { get; set; } }

    public class EqNode { public EqNode? Next { get; set; } public int Value { get; set; } }

    public class ThrowingGetter { public int Value => throw new InvalidOperationException("nope"); }

    public class WithIndexer
    {
        public int Id { get; set; }
        public int this[int i] => throw new InvalidOperationException("indexer read");
    }

    public class WithStatic
    {
        public static int Counter { get; set; }
        public int Id { get; set; }
    }

    public class Holder { public IEnumerable<int>? Items { get; set; } }

    public class Wrapper { public EqPerson? Inner { get; set; } }

    public class OtherWrapper { public object? Inner { get; set; } }

    public class Money : IEquatable<Money>
    {
        public Money(decimal amount) => Amount = amount;
        public decimal Amount { get; }
        public int Hidden { get; set; }
        public bool Equals(Money? other) => other is not null && other.Amount == Amount;
        public override bool Equals(object? obj) => Equals(obj as Money);
        public override int GetHashCode() => Amount.GetHashCode();
    }

    public enum Color { Red, Green }

    public enum Shade { Red, Green }

    public class Tagged { public Color Color { get; set; } public string? Tag { get; set; } }

    private static string Message(Action act) => Assert.Throws<VouchException>(act).Message;

    private static EqNode Chain(int n)
    {
        var head = new EqNode();
        var cur = head;
        for (var i = 0; i < n; i++) { cur.Next = new EqNode(); cur = cur.Next; }
        return head;
    }

    // from the brief

    [Fact]
    public void Records_compare_by_members_and_list_paths()
    {
        var package = new Bundle("Alpha", new[] { "BBB", "DDD" });
        var msg = Message(() => package.Should().BeEquivalentTo(new Bundle("Alpha basic", new[] { "BBB", "AAA" })));
        Assert.Equal("Expected package to be equivalent to the expected Bundle, but:\n  Name was \"Alpha\", expected \"Alpha basic\".\n  Tests[1] expected \"AAA\" was not found.", msg);
    }

    [Fact] public void Anonymous_expectation_against_class() => new EqPerson { Id = 1, Name = "A", Extra = 9 }.Should().BeEquivalentTo(new { Id = 1, Name = "A" });

    [Fact] public void Missing_member_is_a_difference() => Assert.Contains("has no member Age", Message(() => new EqPerson().Should().BeEquivalentTo(new { Age = 3 })));

    [Fact] public void Any_order_by_default() => new[] { 1, 2, 3 }.Should().BeEquivalentTo([3, 1, 2]);

    [Fact] public void Strict_ordering_fails_on_order() => Assert.Throws<VouchException>(() => new[] { 1, 2 }.Should().BeEquivalentTo(new[] { 2, 1 }, o => o.WithStrictOrdering()));

    [Fact] public void Params_form() => new List<string> { "a", "b" }.Should().BeEquivalentTo("b", "a");

    [Fact] public void Tuples_by_members() => (1, "a").Should().BeEquivalentTo((1, "a"));

    [Fact] public void Nested_collections_any_order() => new[] { new[] { 1, 2 } }.Should().BeEquivalentTo(new[] { new[] { 2, 1 } });

    [Fact]
    public void Null_on_either_side()
    {
        EqPerson? p = null;
        Assert.Throws<VouchException>(() => p.Should().BeEquivalentTo(new EqPerson()));
        Assert.Throws<VouchException>(() => new EqPerson().Should().BeEquivalentTo<EqPerson?>(null));
    }

    [Fact]
    public void Cycle_fails_with_message()
    {
        var a = new EqNode(); a.Next = a;
        var b = new EqNode(); b.Next = b;
        Assert.Contains("cyclic reference", Message(() => a.Should().BeEquivalentTo(b)));
    }

    [Fact] public void Depth_limit_fails_with_message() => Assert.Contains("deeper than 10 levels", Message(() => Chain(50).Should().BeEquivalentTo(Chain(50))));

    [Fact] public void Throwing_getter_is_a_difference() => Assert.Contains("Value threw InvalidOperationException", Message(() => new ThrowingGetter().Should().BeEquivalentTo(new { Value = 1 })));

    [Fact]
    public void Lists_at_most_ten_differences()
    {
        var a = Enumerable.Range(0, 30).ToArray();
        var b = Enumerable.Range(100, 30).ToArray();
        Assert.Contains("... and 20 more differences", Message(() => a.Should().BeEquivalentTo(b, o => o.WithStrictOrdering())));
    }

    [Fact]
    public void Large_any_order_list_is_quick()
    {
        var a = Enumerable.Range(0, 5_000).Select(i => new Row($"C{i}", i)).ToList();
        var b = a.AsEnumerable().Reverse().ToList();
        var sw = Stopwatch.StartNew();
        a.Should().BeEquivalentTo(b);
        Assert.True(sw.ElapsedMilliseconds < 2_000);
    }

    [Fact] public void NotContainEquivalentOf_fails_when_present() => Assert.Throws<VouchException>(() => new[] { new Row("A", 1) }.Should().NotContainEquivalentOf(new { Code = "A" }));

    // review focus: root and message shape

    [Fact] public void Root_scalar_message_has_empty_path() => Assert.Equal("Expected value to be equivalent to the expected Int32, but:\n  was 1, expected 2.", Message(() => new ObjectAssertions(1, null).BeEquivalentTo(2)));

    [Fact] public void Both_null_passes() { object? a = null; a.Should().BeEquivalentTo<object?>(null); }

    [Fact]
    public void Null_subject_message()
    {
        EqPerson? p = null;
        Assert.Equal("Expected p to be equivalent to the expected EqPerson, but:\n  was null, expected EqPerson { Id = 0, Name = null, Extra = 0 }.", Message(() => p.Should().BeEquivalentTo(new EqPerson())));
    }

    [Fact]
    public void Null_expectation_message()
    {
        var p = new EqPerson();
        Assert.Equal("Expected p to be equivalent to the expected EqPerson, but:\n  was EqPerson { Id = 0, Name = null, Extra = 0 }, expected null.", Message(() => p.Should().BeEquivalentTo<EqPerson?>(null)));
    }

    [Fact] public void Same_instance_passes() { var n = new EqNode(); n.Next = n; n.Should().BeEquivalentTo(n); }

    [Fact] public void Value_type_subject_passes_and_fails() { 5.Should().BeEquivalentTo(5); Assert.Throws<VouchException>(() => 5.Should().BeEquivalentTo(6)); }

    [Fact] public void Nullable_subject_passes_and_fails() { int? n = 5; n.Should().BeEquivalentTo(5); Assert.Throws<VouchException>(() => n.Should().BeEquivalentTo(6)); }

    [Fact]
    public void Numeric_and_date_subjects_pass_and_fail()
    {
        var d = new DateTime(2026, 1, 1);
        d.Should().BeEquivalentTo(d);
        Assert.Throws<VouchException>(() => d.Should().BeEquivalentTo(d.AddDays(1)));
        var o = new DateOnly(2026, 1, 1);
        o.Should().BeEquivalentTo(o);
        Assert.Throws<VouchException>(() => o.Should().BeEquivalentTo(o.AddDays(1)));
        var dto = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        dto.Should().BeEquivalentTo(dto);
        Assert.Throws<VouchException>(() => dto.Should().BeEquivalentTo(dto.AddDays(1)));
        DateTime? nd = d;
        nd.Should().BeEquivalentTo(d);
        Assert.Throws<VouchException>(() => nd.Should().BeEquivalentTo(d.AddDays(1)));
        DateTimeOffset? ndto = dto;
        ndto.Should().BeEquivalentTo(dto);
        Assert.Throws<VouchException>(() => ndto.Should().BeEquivalentTo(dto.AddDays(1)));
        DateOnly? nDate = o;
        nDate.Should().BeEquivalentTo(o);
        Assert.Throws<VouchException>(() => nDate.Should().BeEquivalentTo(o.AddDays(1)));
        long l = 3;
        l.Should().BeEquivalentTo(3L);
        Assert.Throws<VouchException>(() => l.Should().BeEquivalentTo(4L));
        long? nl = 3;
        nl.Should().BeEquivalentTo(3L);
        Assert.Throws<VouchException>(() => nl.Should().BeEquivalentTo(4L));
    }

    [Fact]
    public void Configure_overload_on_object_value_and_numeric()
    {
        object o = new[] { 1, 2 };
        o.Should().BeEquivalentTo(new[] { 1, 2 }, x => x.WithStrictOrdering());
        Assert.Throws<VouchException>(() => o.Should().BeEquivalentTo(new[] { 2, 1 }, x => x.WithStrictOrdering()));
        1.Should().BeEquivalentTo(1, x => x.WithStrictOrdering());
        Assert.Throws<VouchException>(() => 1.Should().BeEquivalentTo(2, x => x.WithStrictOrdering()));
        var point = new Point(1, 2);
        point.Should().BeEquivalentTo(new Point(1, 2), x => x);
    }

    [Fact]
    public void Configure_null_throws_up_front()
    {
        Assert.Throws<ArgumentNullException>(() => 1.Should().BeEquivalentTo(1, null!));
        Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().BeEquivalentTo(new[] { 1 }, (Func<EquivalencyOptions, EquivalencyOptions>)null!));
        Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().BeEquivalentTo((IEnumerable<int>)null!));
        Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().BeEquivalentTo((int[])null!));
    }

    // type mismatch

    [Fact] public void Root_type_mismatch_is_reported() => Assert.Equal("Expected value to be equivalent to the expected String, but:\n  was 1, expected \"1\".", Message(() => new ObjectAssertions(1, null).BeEquivalentTo("1")));

    [Fact]
    public void Member_type_mismatch_is_reported()
    {
        var msg = Message(() => new OtherWrapper { Inner = 5 }.Should().BeEquivalentTo(new OtherWrapper { Inner = "5" }));
        Assert.Contains("  Inner was 5, expected \"5\".", msg);
    }

    [Fact] public void Scalar_subject_against_object_expectation_is_a_mismatch() => Assert.Contains("  was 1, expected { Id = 1 }.", Message(() => new ObjectAssertions(1, null).BeEquivalentTo(new { Id = 1 })));

    [Fact] public void Object_subject_against_collection_expectation_is_a_mismatch() => Assert.Contains("Inner was EqPerson", Message(() => new OtherWrapper { Inner = new EqPerson() }.Should().BeEquivalentTo(new OtherWrapper { Inner = new[] { 1 } })));

    [Fact] public void Collection_subject_against_dictionary_expectation_is_a_mismatch() => Assert.Throws<VouchException>(() => new OtherWrapper { Inner = new[] { 1 } }.Should().BeEquivalentTo(new OtherWrapper { Inner = new Dictionary<int, int> { [1] = 1 } }));

    private sealed class IntBox { public int Value { get; init; } }
    private sealed class LongBox { public long Value { get; init; } }
    private sealed class DoubleBox { public double Value { get; init; } }

    [Fact] public void Numbers_of_different_types_compare_by_value() { object o = 1; o.Should().BeEquivalentTo(1L); Assert.Throws<VouchException>(() => o.Should().BeEquivalentTo(2L)); }
    [Fact] public void A_number_and_a_fractional_value_differ() { object o = 2; Assert.Throws<VouchException>(() => o.Should().BeEquivalentTo(1.5)); }
    [Fact] public void A_typed_int_subject_matches_a_long_expectation() { 1.Should().BeEquivalentTo(1L); Assert.Throws<VouchException>(() => 1.Should().BeEquivalentTo(2L)); }
    [Fact]
    public void Number_members_of_different_types_compare_by_value()
    {
        new IntBox { Value = 1 }.Should().BeEquivalentTo(new LongBox { Value = 1 });
        new IntBox { Value = 1 }.Should().BeEquivalentTo(new DoubleBox { Value = 1.0 });
        Assert.Throws<VouchException>(() => new IntBox { Value = 2 }.Should().BeEquivalentTo(new DoubleBox { Value = 1.5 }));
    }
    [Fact] public void Items_of_different_numeric_types_compare_by_value() { new[] { 1, 2 }.Should().BeEquivalentTo(new long[] { 2, 1 }); }
    private sealed class DecimalBox { public decimal Value { get; init; } }
    [Fact] public void A_decimal_member_does_not_equal_a_double_it_only_rounds_to() { Assert.Throws<VouchException>(() => new DecimalBox { Value = 9007199254740993m }.Should().BeEquivalentTo(new DoubleBox { Value = 9007199254740992d })); }
    [Fact] public void A_decimal_does_not_equal_a_double_in_a_nested_array() { Assert.Throws<VouchException>(() => new[] { new object[] { 9007199254740993m } }.Should().BeEquivalentTo(new[] { new object[] { 9007199254740992d } })); }
    [Fact] public void Float_nan_equals_double_nan() { object f = float.NaN; f.Should().BeEquivalentTo(double.NaN); }
    [Fact]
    public void Two_hundred_big_decimals_match_longs_in_reverse_order()
    {
        var subject = Enumerable.Range(0, 200).Select(i => (object)(9007199254740993m + i)).ToArray();
        var expected = Enumerable.Range(0, 200).Select(i => (object)(9007199254740993L + i)).Reverse().ToArray();
        subject.Should().BeEquivalentTo(expected);
    }
    [Fact]
    public void Two_hundred_small_mixed_numbers_match_in_reverse_order()
    {
        var subject = Enumerable.Range(0, 200).Select(i => (object)(decimal)i).ToArray();
        var expected = Enumerable.Range(0, 200).Select(i => (object)(double)i).Reverse().ToArray();
        subject.Should().BeEquivalentTo(expected);
    }
    [Fact] public void An_enum_does_not_equal_an_int() { object o = Color.Green; Assert.Throws<VouchException>(() => o.Should().BeEquivalentTo(1)); }

    [Fact] public void Enums_of_different_types_compare_by_value() { Color.Green.Should().BeEquivalentTo(Shade.Green); Assert.Throws<VouchException>(() => Color.Green.Should().BeEquivalentTo(Shade.Red)); }

    // records, anonymous types and tuples compare by members

    [Fact] public void Record_with_equal_members_passes() => new Row("A", 1).Should().BeEquivalentTo(new Row("A", 1));

    [Fact] public void Record_struct_compares_by_members() { new Point(1, 2).Should().BeEquivalentTo(new Point(1, 2)); Assert.Contains("Y was 2, expected 3.", Message(() => new Point(1, 2).Should().BeEquivalentTo(new Point(1, 3)))); }

    [Fact] public void Record_array_member_is_compared_by_content_not_reference() => new Bundle("A", new[] { "x" }).Should().BeEquivalentTo(new Bundle("A", new[] { "x" }));

    [Fact] public void Anonymous_types_compare_by_members() { new { A = 1, B = "x" }.Should().BeEquivalentTo(new { A = 1, B = "x" }); Assert.Contains("B was \"x\", expected \"y\".", Message(() => new { A = 1, B = "x" }.Should().BeEquivalentTo(new { A = 1, B = "y" }))); }

    [Fact] public void Tuple_class_and_value_tuple_compare_by_members() { Tuple.Create(1, "a").Should().BeEquivalentTo(Tuple.Create(1, "a")); Assert.Contains("Item2 was \"a\", expected \"b\".", Message(() => (1, "a").Should().BeEquivalentTo((1, "b")))); }

    [Fact] public void Extra_subject_members_are_ignored_and_expectation_members_drive() { new Row("A", 1).Should().BeEquivalentTo(new { Code = "A" }); }

    [Fact]
    public void Type_overriding_Equals_uses_Equals_not_members()
    {
        new Money(1m) { Hidden = 1 }.Should().BeEquivalentTo(new Money(1m) { Hidden = 2 });
        Assert.Throws<VouchException>(() => new Money(1m).Should().BeEquivalentTo(new Money(2m)));
    }

    [Fact]
    public void Enum_member_compares_and_reports()
    {
        new Tagged { Color = Color.Red, Tag = "t" }.Should().BeEquivalentTo(new Tagged { Color = Color.Red, Tag = "t" });
        Assert.Contains("Color was Color.Red, expected Color.Green.", Message(() => new Tagged { Color = Color.Red }.Should().BeEquivalentTo(new Tagged { Color = Color.Green })));
    }

    [Fact] public void Nested_member_path() => Assert.Contains("  Inner.Name was \"a\", expected \"b\".", Message(() => new Wrapper { Inner = new EqPerson { Name = "a" } }.Should().BeEquivalentTo(new Wrapper { Inner = new EqPerson { Name = "b" } })));

    [Fact]
    public void Null_member_on_either_side_is_a_difference()
    {
        Assert.Contains("  Inner was null, expected EqPerson", Message(() => new Wrapper().Should().BeEquivalentTo(new Wrapper { Inner = new EqPerson() })));
        Assert.Contains("  Inner was EqPerson", Message(() => new Wrapper { Inner = new EqPerson() }.Should().BeEquivalentTo(new Wrapper())));
    }

    // indexers and statics

    [Fact] public void Indexers_are_skipped() => new WithIndexer { Id = 1 }.Should().BeEquivalentTo(new WithIndexer { Id = 1 });

    [Fact]
    public void Static_members_are_skipped()
    {
        WithStatic.Counter = 1;
        var a = new WithStatic { Id = 1 };
        WithStatic.Counter = 2;
        a.Should().BeEquivalentTo(new WithStatic { Id = 1 });
        Assert.Contains("has no member Counter", Message(() => a.Should().BeEquivalentTo(new { Counter = 2 })));
    }

    // throwing getters

    [Fact] public void Throwing_getter_on_expectation_side_is_a_difference() => Assert.Contains("Value threw InvalidOperationException: nope", Message(() => new { Value = 1 }.Should().BeEquivalentTo(new ThrowingGetter())));

    [Fact] public void Throwing_getter_does_not_stop_other_differences() { var msg = Message(() => new ThrowingGetter().Should().BeEquivalentTo(new { Value = 1, Missing = 2 })); Assert.Contains("Value threw", msg); Assert.Contains("has no member Missing", msg); }

    [Fact]
    public void Throwing_enumerator_is_a_difference()
    {
        IEnumerable<int> Boom() { yield return 1; throw new InvalidOperationException("enum"); }
        Assert.Contains("Items threw InvalidOperationException: enum", Message(() => new Holder { Items = Boom() }.Should().BeEquivalentTo(new Holder { Items = new[] { 1, 2 } })));
    }

    [Fact] public void Throwing_Equals_is_a_difference() => Assert.Contains("threw", Message(() => new ThrowingEquals().Should().BeEquivalentTo(new ThrowingEquals())));

    public class ThrowingEquals { public override bool Equals(object? obj) => throw new InvalidOperationException("eq"); public override int GetHashCode() => 0; }

    // cycles and depth

    [Fact]
    public void Cycle_reports_path_and_does_not_follow()
    {
        var a = new EqNode(); a.Next = a;
        var b = new EqNode(); b.Next = b;
        Assert.Equal("Expected a to be equivalent to the expected EqNode, but:\n  Next is a cyclic reference.", Message(() => a.Should().BeEquivalentTo(b)));
    }

    [Fact]
    public void Two_step_cycle_is_reported()
    {
        EqNode Ring() { var x = new EqNode(); var y = new EqNode { Next = x }; x.Next = y; return x; }
        Assert.Contains("cyclic reference", Message(() => Ring().Should().BeEquivalentTo(Ring())));
    }

    [Fact]
    public void Shared_non_cyclic_references_are_fine()
    {
        var shared = new EqPerson { Id = 1 };
        new[] { shared, shared }.Should().BeEquivalentTo(new[] { new EqPerson { Id = 1 }, new EqPerson { Id = 1 } });
    }

    [Fact] public void Chain_at_the_depth_limit_passes() => Chain(10).Should().BeEquivalentTo(Chain(10));

    [Fact] public void Chain_just_over_the_depth_limit_fails() => Assert.Contains("Next.Next.Next.Next.Next.Next.Next.Next.Next.Next.Next is nested deeper than 10 levels.", Message(() => Chain(12).Should().BeEquivalentTo(Chain(12))));

    // difference cap

    [Fact]
    public void Ten_differences_are_listed_with_the_rest_counted()
    {
        var msg = Message(() => Enumerable.Range(0, 30).ToArray().Should().BeEquivalentTo(Enumerable.Range(100, 30).ToArray(), o => o.WithStrictOrdering()));
        var lines = msg.Split('\n');
        Assert.Equal(1 + 10 + 1, lines.Length);
        Assert.Equal("  [0] was 0, expected 100.", lines[1]);
        Assert.Equal("  ... and 20 more differences", lines[^1]);
    }

    [Fact] public void Exactly_ten_differences_have_no_tail() => Assert.DoesNotContain("more differences", Message(() => Enumerable.Range(0, 10).ToArray().Should().BeEquivalentTo(Enumerable.Range(100, 10).ToArray(), o => o.WithStrictOrdering())));

    // single pass and collections

    [Fact]
    public void Single_pass_member_is_enumerated_once()
    {
        var calls = 0;
        IEnumerable<int> Gen() { calls++; yield return 1; yield return 2; }
        new Holder { Items = Gen() }.Should().BeEquivalentTo(new Holder { Items = new[] { 2, 1 } });
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Single_pass_collection_subject_and_expectation_are_enumerated_once()
    {
        var a = 0; var b = 0;
        IEnumerable<int> A() { a++; yield return 1; yield return 2; }
        IEnumerable<int> B() { b++; yield return 2; yield return 1; }
        A().Should().BeEquivalentTo(B());
        Assert.Equal(1, a);
        Assert.Equal(1, b);
    }

    [Fact] public void Count_mismatch_is_reported() => Assert.Equal("Expected a to be equivalent to the expected Int32[], but:\n  has 2 items, expected 3 items.", Message(() => { var a = new[] { 1, 2 }; a.Should().BeEquivalentTo(new[] { 1, 2, 3 }); }));

    [Fact] public void Single_item_count_wording() => Assert.Contains("has 1 item, expected 2 items.", Message(() => new[] { 1 }.Should().BeEquivalentTo(new[] { 1, 2 })));

    [Fact] public void Null_collection_subject_is_a_difference() { int[]? a = null; Assert.Contains("was null, expected [1]", Message(() => a.Should().BeEquivalentTo(new[] { 1 }))); }

    [Fact] public void Empty_collections_are_equivalent() => Array.Empty<int>().Should().BeEquivalentTo(Array.Empty<int>());

    [Fact]
    public void Strict_ordering_message_lists_positions()
    {
        var msg = Message(() => new[] { 1, 2 }.Should().BeEquivalentTo(new[] { 2, 1 }, o => o.WithStrictOrdering()));
        Assert.Equal("Expected value to be equivalent to the expected Int32[], but:\n  [0] was 1, expected 2.\n  [1] was 2, expected 1.".Replace("value", "new[] { 1, 2 }"), msg);
    }

    [Fact] public void Strict_ordering_passes_in_order() => new[] { 1, 2 }.Should().BeEquivalentTo(new[] { 1, 2 }, o => o.WithStrictOrdering());

    [Fact] public void Strict_ordering_is_inherited_by_nested_collections() => Assert.Throws<VouchException>(() => new[] { new[] { 1, 2 } }.Should().BeEquivalentTo(new[] { new[] { 2, 1 } }, o => o.WithStrictOrdering()));

    [Fact] public void Any_order_with_duplicates_matches_each_item_once() => new[] { 1, 1, 2 }.Should().BeEquivalentTo(new[] { 1, 2, 1 });

    [Fact]
    public void Any_order_duplicates_with_different_multiplicity_fail()
    {
        Assert.Contains("[1] expected 2 was not found.", Message(() => new[] { 1, 1, 3 }.Should().BeEquivalentTo(new[] { 1, 2, 1 })));
        Assert.Contains("[2] expected 1 was not found.", Message(() => new[] { 1, 2, 2 }.Should().BeEquivalentTo(new[] { 2, 1, 1 })));
    }

    [Fact]
    public void Any_order_duplicates_of_records()
    {
        new[] { new Row("A", 1), new Row("A", 1), new Row("B", 2) }.Should().BeEquivalentTo(new[] { new Row("B", 2), new Row("A", 1), new Row("A", 1) });
        Assert.Throws<VouchException>(() => new[] { new Row("A", 1), new Row("A", 1) }.Should().BeEquivalentTo(new[] { new Row("A", 1), new Row("A", 2) }));
    }

    [Fact] public void Any_order_with_nulls() { new string?[] { null, "a" }.Should().BeEquivalentTo(new string?[] { "a", null }); Assert.Throws<VouchException>(() => new string?[] { null, "a" }.Should().BeEquivalentTo(new string?[] { null, null })); }

    [Fact] public void Any_order_matches_anonymous_expectations_against_records() => new[] { new Row("A", 1), new Row("B", 2) }.Should().BeEquivalentTo(new[] { new { Code = "B" }, new { Code = "A" } });

    [Fact] public void Any_order_with_a_mixed_subject_falls_back_to_scanning() => new object[] { "x", 1, new Row("A", 1) }.Should().BeEquivalentTo(new object[] { new Row("A", 1), "x", 1 });

    [Fact] public void Any_order_tuples() => new[] { (1, "a"), (2, "b") }.Should().BeEquivalentTo(new[] { (2, "b"), (1, "a") });

    [Fact] public void Any_order_where_all_keys_collide_still_matches() => Enumerable.Range(0, 500).Select(i => new Row("same", i)).ToList().Should().BeEquivalentTo(Enumerable.Range(0, 500).Reverse().Select(i => new Row("same", i)).ToList());

    public record Loose(string Code, object? Tag);

    [Fact]
    public void Any_order_with_every_key_colliding_stays_within_pairwise_cost()
    {
        var a = Enumerable.Range(0, 1_000).Select(i => new Loose("same", new[] { i })).ToList();
        var b = a.AsEnumerable().Reverse().ToList();
        var sw = Stopwatch.StartNew();
        a.Should().BeEquivalentTo(b);
        Assert.True(sw.ElapsedMilliseconds < 10_000);
        Assert.Throws<VouchException>(() => a.Should().BeEquivalentTo(b.Take(999).Append(new Loose("same", new[] { -1 })).ToList()));
    }

    [Fact]
    public void Collection_of_constants_any_order_scales()
    {
        var a = Enumerable.Range(0, 50_000).ToArray();
        var b = a.Reverse().ToArray();
        var sw = Stopwatch.StartNew();
        a.Should().BeEquivalentTo(b);
        Assert.True(sw.ElapsedMilliseconds < 2_000);
    }

    [Fact]
    public void Large_mismatch_any_order_is_quick()
    {
        var a = Enumerable.Range(0, 5_000).Select(i => new Row($"C{i}", i)).ToList();
        var b = Enumerable.Range(0, 5_000).Select(i => new Row($"D{i}", i)).ToList();
        var sw = Stopwatch.StartNew();
        var msg = Message(() => a.Should().BeEquivalentTo(b));
        Assert.True(sw.ElapsedMilliseconds < 5_000);
        Assert.Contains("... and 4990 more differences", msg);
    }

    [Fact] public void Hash_set_and_enumerable_sources_work() => new HashSet<int> { 1, 2, 3 }.Should().BeEquivalentTo(new Queue<int>(new[] { 3, 2, 1 }));

    [Fact] public void Non_generic_list_of_mixed_numbers() { var l = new ArrayList { 1, "a" }; l.Should().BeEquivalentTo(new object[] { "a", 1 }); }

    [Fact]
    public void List_of_lists_passed_as_one_argument_is_the_expected_sequence()
    {
        var actual = new List<List<int>> { new() { 1, 2 }, new() { 3 } };
        var expected = new List<List<int>> { new() { 3 }, new() { 2, 1 } };
        actual.Should().BeEquivalentTo(expected);
        Assert.Throws<VouchException>(() => actual.Should().BeEquivalentTo(new List<List<int>> { new() { 3 }, new() { 9 } }));
    }

    [Fact]
    public void A_single_list_stays_one_item_when_subject_items_are_lists()
    {
        var actual = new List<List<int>> { new() { 1, 2 } };
        actual.Should().BeEquivalentTo(new List<int> { 2, 1 });
        Assert.Throws<VouchException>(() => actual.Should().BeEquivalentTo(new List<int> { 2, 3 }));
    }

    // dictionaries

    [Fact] public void Dictionaries_with_the_same_content_pass() => new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 }.Should().BeEquivalentTo(new Dictionary<string, int> { ["b"] = 2, ["a"] = 1 });

    [Fact] public void Dictionary_value_difference_has_key_path() => Assert.Contains("  [\"a\"] was 1, expected 5.", Message(() => new Dictionary<string, int> { ["a"] = 1 }.Should().BeEquivalentTo(new Dictionary<string, int> { ["a"] = 5 })));

    public class Holder2 { public Dictionary<string, int>? Map { get; set; } }

    [Fact] public void Dictionary_path_under_a_member() => Assert.Contains("  Map[\"a\"] was 1, expected 5.", Message(() => new Holder2 { Map = new() { ["a"] = 1 } }.Should().BeEquivalentTo(new Holder2 { Map = new() { ["a"] = 5 } })));

    [Fact]
    public void Dictionary_missing_and_unexpected_keys()
    {
        var msg = Message(() => new Dictionary<string, int> { ["a"] = 1, ["x"] = 9 }.Should().BeEquivalentTo(new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 }));
        Assert.Contains("has no key \"b\"", msg);
        Assert.Contains("has unexpected key \"x\"", msg);
    }

    [Fact]
    public void Dictionary_typed_as_enumerable_of_pairs_is_compared_as_a_dictionary()
    {
        IEnumerable<KeyValuePair<string, int>> expected = new Dictionary<string, int> { ["a"] = 1 };
        new Dictionary<string, int> { ["a"] = 1 }.Should().BeEquivalentTo(expected);
        Assert.Throws<VouchException>(() => new Dictionary<string, int> { ["a"] = 2 }.Should().BeEquivalentTo(expected));
    }

    [Fact] public void Dictionary_values_compare_structurally() => new Dictionary<int, Row> { [1] = new Row("A", 1) }.Should().BeEquivalentTo(new Dictionary<int, object> { [1] = new { Code = "A" } });

    [Fact] public void Read_only_dictionary_only_types_work() => new ReadOnlyOnlyDictionary(new() { ["a"] = 1 }).Should().BeEquivalentTo(new Dictionary<string, int> { ["a"] = 1 });

    [Fact] public void Read_only_dictionary_only_subject_difference() => Assert.Contains("[\"a\"] was 1, expected 2.", Message(() => new Dictionary<string, int> { ["a"] = 1 }.Should().BeEquivalentTo(new ReadOnlyOnlyDictionary(new() { ["a"] = 2 }))));

    [Fact] public void Dictionary_with_different_count_reports_keys() => Assert.Throws<VouchException>(() => new Dictionary<string, int>().Should().BeEquivalentTo(new Dictionary<string, int> { ["a"] = 1 }));

    // NotContainEquivalentOf

    [Fact] public void NotContainEquivalentOf_passes_when_absent() => new[] { new Row("A", 1) }.Should().NotContainEquivalentOf(new { Code = "B" });

    [Fact] public void NotContainEquivalentOf_message() => Assert.Equal("Expected rows not to contain an item equivalent to { Code = \"A\" }, but found one at index 1.", Message(() => { var rows = new[] { new Row("B", 1), new Row("A", 1) }; rows.Should().NotContainEquivalentOf(new { Code = "A" }); }));

    [Fact] public void NotContainEquivalentOf_fails_on_null_subject() { Row[]? rows = null; Assert.Equal("Expected rows not to contain an item equivalent to { Code = \"A\" }, but found null.", Message(() => rows.Should().NotContainEquivalentOf(new { Code = "A" }))); }

    [Fact]
    public void NotContainEquivalentOf_enumerates_a_single_pass_source_once()
    {
        var calls = 0;
        IEnumerable<int> Gen() { calls++; yield return 1; yield return 2; }
        Gen().Should().NotContainEquivalentOf(3);
        Assert.Equal(1, calls);
    }

    [Fact] public void NotContainEquivalentOf_with_null_expectation() { new string?[] { "a" }.Should().NotContainEquivalentOf<string?>(null); Assert.Throws<VouchException>(() => new string?[] { null }.Should().NotContainEquivalentOf<string?>(null)); }

    // no NullReferenceException

    [Fact]
    public void Null_in_every_position_never_throws_NRE()
    {
        new object?[] { null, null }.Should().BeEquivalentTo(new object?[] { null, null });
        Assert.Throws<VouchException>(() => new object?[] { null }.Should().BeEquivalentTo(new object?[] { new object() }));
        Assert.Throws<VouchException>(() => new Dictionary<string, object?> { ["a"] = null }.Should().BeEquivalentTo(new Dictionary<string, object?> { ["a"] = new object() }));
    }

    // fix round 1

    public sealed class OneShotSequence : IEnumerable<int>
    {
        private readonly int[] _items;
        private bool _used;

        public OneShotSequence(params int[] items) => _items = items;

        public IEnumerator<int> GetEnumerator()
        {
            if (_used)
            {
                throw new InvalidOperationException("enumerated twice");
            }

            _used = true;
            return ((IEnumerable<int>)_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private static Holder Shot(params int[] items) => new() { Items = new OneShotSequence(items) };

    [Fact] public void Nested_one_shot_members_that_differ_fail_in_any_order_mode() => Assert.Throws<VouchException>(() => new[] { Shot(1, 2) }.Should().BeEquivalentTo(new[] { Shot(3, 4) }));

    [Fact]
    public void Swapped_items_with_one_shot_members_compare_by_content()
    {
        new[] { Shot(1, 2), Shot(3, 4) }.Should().BeEquivalentTo(new[] { Shot(3, 4), Shot(1, 2) });
        Assert.Throws<VouchException>(() => new[] { Shot(1, 2), Shot(3, 4) }.Should().BeEquivalentTo(new[] { Shot(3, 4), Shot(9, 9) }));
    }

    [Fact] public void One_shot_member_that_throws_on_a_second_pass_is_read_once() => new[] { Shot(1, 2), Shot(3, 4), Shot(5, 6) }.Should().BeEquivalentTo(new[] { Shot(5, 6), Shot(3, 4), Shot(1, 2) });

    [Fact] public void One_shot_members_of_a_large_list_are_read_once() => Enumerable.Range(0, 200).Select(i => Shot(i, i + 1)).ToList().Should().BeEquivalentTo(Enumerable.Range(0, 200).Reverse().Select(i => Shot(i, i + 1)).ToList());

    public class Empty { }

    public class Nested { public Empty? X { get; set; } }

    [Fact]
    public void Expectation_with_no_members_is_a_difference()
    {
        Assert.Equal("Expected value to be equivalent to the expected Empty, but:\n  has no members to compare.", Message(() => new ObjectAssertions(new EqPerson { Id = 1 }, null).BeEquivalentTo(new Empty())));
        Assert.Contains("has no members to compare", Message(() => new EqPerson { Id = 1 }.Should().BeEquivalentTo(new object())));
        Assert.Contains("has no members to compare", Message(() => new EqPerson { Id = 1 }.Should().BeEquivalentTo(new { })));
    }

    [Fact] public void Nested_expectation_with_no_members_reports_its_path() => Assert.Contains("\n  Inner has no members to compare.", Message(() => new OtherWrapper { Inner = new EqPerson { Id = 1 } }.Should().BeEquivalentTo(new OtherWrapper { Inner = new Empty() })));

    [Fact]
    public void Empty_expectation_still_passes_for_scalar_equatable_and_collection_kinds()
    {
        Array.Empty<int>().Should().BeEquivalentTo(Array.Empty<int>());
        new Money(1m).Should().BeEquivalentTo(new Money(1m));
        new Dictionary<string, int>().Should().BeEquivalentTo(new Dictionary<string, int>());
    }

    [Fact]
    public void Collection_overloads_bind_as_pinned()
    {
        var list = new List<string> { "a", "b" };
        Func<string[], AndConstraint<GenericCollectionAssertions<string>>> items = list.Should().BeEquivalentTo;
        Func<IEnumerable<string>, AndConstraint<GenericCollectionAssertions<string>>> sequence = list.Should().BeEquivalentTo;
        items(new[] { "b", "a" });
        sequence(new List<string> { "b", "a" });
    }

    [Fact]
    public void List_of_strings_binds_the_sequence_array_and_params_forms()
    {
        var list = new List<string> { "a", "b" };
        list.Should().BeEquivalentTo(new List<string> { "b", "a" });
        list.Should().BeEquivalentTo("b", "a");
        list.Should().BeEquivalentTo(new[] { "b", "a" });
        Assert.Throws<VouchException>(() => list.Should().BeEquivalentTo(new List<string> { "b", "c" }));
        Assert.Throws<VouchException>(() => list.Should().BeEquivalentTo("a"));
    }

    [Fact]
    public void A_list_of_lists_takes_one_list_as_one_item_and_an_enumerable_as_the_sequence()
    {
        var subject = new List<List<int>> { new() { 1, 2 } };
        subject.Should().BeEquivalentTo(new List<int> { 1, 2 });
        Assert.Throws<VouchException>(() => subject.Should().BeEquivalentTo((IEnumerable<int>)new List<int> { 1, 2 }));
    }

    [Fact]
    public void Array_with_configure_uses_the_instance_overload()
    {
        Func<int[], Func<EquivalencyOptions, EquivalencyOptions>, AndConstraint<GenericCollectionAssertions<int>>> f = new[] { 1 }.Should().BeEquivalentTo;
        f(new[] { 1 }, o => o.WithStrictOrdering());
    }

    private static IEnumerable<int> Endless() { var i = 0; while (true) { yield return i++; } }

    public class Endlessly { public IEnumerable<int> Items => Endless(); }

    private static string MessageWithin(Action act)
    {
        var task = Task.Run(() => Message(act));
        Assert.True(task.Wait(TimeSpan.FromSeconds(30)), "timed out");
        return task.Result;
    }

    [Fact] public void Endless_member_is_capped() => Assert.Contains("Items has more than 1000000 items, not compared.", MessageWithin(() => new Endlessly().Should().BeEquivalentTo(new Holder { Items = new[] { 1 } })));

    [Fact] public void Endless_collection_subject_is_capped() => Assert.Contains("has more than 1000000 items, not compared", MessageWithin(() => Endless().Should().HaveCount(1)));

    [Fact] public void Endless_dictionary_snapshot_is_capped() => Assert.Contains("has more than 1000000 items, not compared", MessageWithin(() => new Dictionary<int, int>().Should().BeEquivalentTo(new EndlessDictionary())));

    public sealed class EndlessDictionary : IReadOnlyDictionary<int, int>
    {
        public int this[int key] => key;
        public IEnumerable<int> Keys => Endless();
        public IEnumerable<int> Values => Endless();
        public int Count => int.MaxValue;
        public bool ContainsKey(int key) => true;
        public bool TryGetValue(int key, out int value) { value = key; return true; }
        public IEnumerator<KeyValuePair<int, int>> GetEnumerator() { var i = 0; while (true) { yield return new(i, i); i++; } }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Fact]
    public void Large_nested_lists_with_one_mismatch_finish_quickly()
    {
        var a = Enumerable.Range(0, 5_000).Select(i => new List<int> { i, i + 1, i + 2 }).ToList();
        var b = a.AsEnumerable().Reverse().Select(l => l.ToList()).ToList();
        b[100] = new List<int> { -1, -1, -1 };
        var sw = Stopwatch.StartNew();
        var msg = Message(() => a.Should().BeEquivalentTo(b));
        Assert.True(sw.ElapsedMilliseconds < 3_000, sw.ElapsedMilliseconds.ToString());
        Assert.Contains("[100] expected [-1, -1, -1] was not found.", msg);
    }

    [Fact]
    public void One_more_difference_is_singular()
    {
        var msg = Message(() => Enumerable.Range(0, 11).ToArray().Should().BeEquivalentTo(Enumerable.Range(100, 11).ToArray(), o => o.WithStrictOrdering()));
        Assert.EndsWith("\n  ... and 1 more difference", msg);
    }

    private static readonly TimeSpan Bound = TimeSpan.FromSeconds(2);

    // Runs the call on the thread pool and fails the test if it takes longer than the bound.
    private static T Bounded<T>(Func<T> call)
    {
        var task = Task.Run(call);
        if (!task.Wait(Bound))
        {
            Assert.Fail($"Did not finish within {Bound.TotalSeconds} s.");
        }

        return task.Result;
    }

    public class SharedNode
    {
        public int Value { get; set; }
        public SharedNode? A { get; set; }
        public SharedNode? B { get; set; }
        public SharedNode? C { get; set; }
        public SharedNode? D { get; set; }
        public SharedNode? E { get; set; }
    }

    private static SharedNode SharedGraph(int levels, int leaf)
    {
        var node = new SharedNode { Value = leaf };
        for (var i = 1; i < levels; i++)
        {
            node = new SharedNode { A = node, B = node, C = node, D = node, E = node };
        }

        return node;
    }

    public sealed class EqKey : IEquatable<EqKey>
    {
        public EqKey(int id) => Id = id;
        public int Id { get; }
        public bool Equals(EqKey? other) => other is not null && other.Id == Id;
        public override bool Equals(object? obj) => Equals(obj as EqKey);
        public override int GetHashCode() => Id;
    }

    public sealed class DriftingKey : IEquatable<DriftingKey>
    {
        private static int _next;
        public DriftingKey(int id) => Id = id;
        public int Id { get; }
        public bool Equals(DriftingKey? other) => other is not null && other.Id == Id;
        public override bool Equals(object? obj) => Equals(obj as DriftingKey);
        public override int GetHashCode() => Interlocked.Increment(ref _next);
    }

    [Fact]
    public void Shared_graph_of_11_levels_is_equivalent_within_2_seconds()
    {
        var left = SharedGraph(11, 1);
        var right = SharedGraph(11, 1);
        Bounded(() => { left.Should().BeEquivalentTo(right); return 0; });
    }

    [Fact]
    public void Shared_graph_mismatch_names_the_first_differing_path()
    {
        var left = SharedGraph(11, 1);
        var right = SharedGraph(11, 2);
        var msg = Bounded(() => Message(() => left.Should().BeEquivalentTo(right)));
        Assert.Contains("\n  A.A.A.A.A.A.A.A.A.A.Value was 1, expected 2.", msg);
    }

    [Fact]
    public void Shared_graph_pair_proven_equal_does_not_hide_a_difference_elsewhere()
    {
        var shared = new SharedNode { Value = 1 };
        var other = new SharedNode { Value = 2 };
        var left = new SharedNode { A = shared, B = shared };
        var right = new SharedNode { A = new SharedNode { Value = 1 }, B = other };
        var msg = Message(() => left.Should().BeEquivalentTo(right));
        Assert.Contains("B.Value was 1, expected 2.", msg);
    }

    [Fact]
    public void Any_order_200000_reversed_uris_match_within_2_seconds()
    {
        var a = Enumerable.Range(0, 200_000).Select(i => new Uri($"http://example.com/{i}")).ToArray();
        var b = a.Reverse().ToArray();
        Bounded(() => { a.Should().BeEquivalentTo(b); return 0; });
    }

    [Fact]
    public void Any_order_200000_reversed_equatable_objects_match_within_2_seconds()
    {
        var a = Enumerable.Range(0, 200_000).Select(i => new EqKey(i)).ToArray();
        var b = a.Reverse().Select(k => new EqKey(k.Id)).ToArray();
        Bounded(() => { a.Should().BeEquivalentTo(b); return 0; });
    }

    [Fact]
    public void Any_order_matches_equatable_objects_whose_hash_code_disagrees_with_equals()
    {
        var a = Enumerable.Range(0, 100).Select(i => new DriftingKey(i)).ToArray();
        var b = a.Reverse().Select(k => new DriftingKey(k.Id)).ToArray();
        a.Should().BeEquivalentTo(b);
    }

    public sealed class ReadOnlyOnlyDictionary : IReadOnlyDictionary<string, int>
    {
        private readonly Dictionary<string, int> _inner;
        public ReadOnlyOnlyDictionary(Dictionary<string, int> inner) => _inner = inner;
        public int this[string key] => _inner[key];
        public IEnumerable<string> Keys => _inner.Keys;
        public IEnumerable<int> Values => _inner.Values;
        public int Count => _inner.Count;
        public bool ContainsKey(string key) => _inner.ContainsKey(key);
        public bool TryGetValue(string key, out int value) => _inner.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<string, int>> GetEnumerator() => _inner.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
