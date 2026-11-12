using Xunit;

namespace Vouch.Tests;

public class CollectionAssertionsTests
{
    private static string Message(Action act) => Assert.Throws<VouchException>(act).Message;

    private static readonly Row[] Rows = [new("BBB", 1), new("AAA", 2)];

    // resolution and single pass

    [Fact] public void String_still_resolves_to_StringAssertions() { StringAssertions s = "abc".Should(); Assert.IsType<StringAssertions>("abc".Should()); s.Be("abc"); }
    [Fact] public void List_resolves_to_collection_assertions() => Assert.IsType<GenericCollectionAssertions<int>>(new List<int>().Should());
    [Fact] public void Array_resolves_to_collection_assertions() => Assert.IsType<GenericCollectionAssertions<int>>(new[] { 1 }.Should());

    [Fact]
    public void Subject_is_enumerated_once()
    {
        var calls = 0;
        IEnumerable<int> Gen() { calls++; yield return 1; }
        Gen().Should().HaveCount(1).And.Contain(1);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Single_pass_generator_failure_shows_the_items()
    {
        var calls = 0;
        IEnumerable<int> Gen() { if (++calls > 1) { throw new InvalidOperationException("twice"); } yield return 4; yield return 5; }
        var gen = Gen();
        Assert.Equal("Expected gen to contain 3 items, but found 2:\n  [0] 4\n  [1] 5", Message(() => gen.Should().HaveCount(3)));
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Single_pass_generator_works_with_Equal_and_Contain_and_ordering()
    {
        IEnumerable<int> Gen() { yield return 1; yield return 2; }
        var expectedCalls = 0;
        IEnumerable<int> Expected() { if (++expectedCalls > 1) { throw new InvalidOperationException("twice"); } yield return 1; yield return 3; }
        var gen = Gen();
        var e = Expected();
        Assert.Throws<VouchException>(() => gen.Should().Equal(e));
        Assert.Equal(1, expectedCalls);
    }

    // null arguments throw first, null subject fails

    [Fact] public void Null_expected_collection_throws_before_subject_check() { List<int>? xs = null; Assert.Throws<ArgumentNullException>(() => xs.Should().Equal((IEnumerable<int>)null!)); }
    [Fact] public void Null_params_array_throws() => Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().Equal((int[])null!));
    [Fact] public void Null_predicate_throws() { List<int>? xs = null; Assert.Throws<ArgumentNullException>(() => xs.Should().Contain((System.Linq.Expressions.Expression<Func<int, bool>>)null!)); }
    [Fact] public void Null_action_throws() => Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().AllSatisfy(null!));
    [Fact] public void Null_key_selector_throws() => Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().BeInAscendingOrder<int>(null!));
    [Fact] public void Null_other_collection_throws() { Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().BeSubsetOf(null!)); Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().NotIntersectWith(null!)); Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().NotContain((IEnumerable<int>)null!)); Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().NotEqual(null!)); Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().Contain((IEnumerable<int>)null!)); }

    [Fact] public void Null_list_HaveCount_fails() { List<int>? rows = null; Assert.Equal("Expected rows to contain 3 items, but found null.", Message(() => rows.Should().HaveCount(3))); }
    [Fact] public void Null_list_BeEmpty_fails() { List<int>? rows = null; Assert.Equal("Expected rows to be empty, but found null.", Message(() => rows.Should().BeEmpty())); }
    [Fact] public void Null_list_NotBeEmpty_fails() { List<int>? rows = null; Assert.Equal("Expected rows not to be empty, but found null.", Message(() => rows.Should().NotBeEmpty())); }
    [Fact] public void Null_list_BeNull_passes() { List<int>? rows = null; rows.Should().BeNull(); }
    [Fact] public void Null_list_NotBeNull_fails() { List<int>? rows = null; Assert.Equal("Expected rows not to be null, but it was.", Message(() => rows.Should().NotBeNull())); }
    [Fact]
    public void Null_list_never_throws_NullReference()
    {
        List<int>? rows = null;
        Assert.Throws<VouchException>(() => rows.Should().Contain(1));
        Assert.Throws<VouchException>(() => rows.Should().Contain(1, 2));
        Assert.Throws<VouchException>(() => rows.Should().Contain(x => x > 0));
        Assert.Throws<VouchException>(() => rows.Should().NotContain(1));
        Assert.Throws<VouchException>(() => rows.Should().ContainSingle());
        Assert.Throws<VouchException>(() => rows.Should().OnlyContain(x => x > 0));
        Assert.Throws<VouchException>(() => rows.Should().Equal(1));
        Assert.Throws<VouchException>(() => rows.Should().NotEqual(new[] { 1 }));
        Assert.Throws<VouchException>(() => rows.Should().OnlyHaveUniqueItems());
        Assert.Throws<VouchException>(() => rows.Should().BeInAscendingOrder());
        Assert.Throws<VouchException>(() => rows.Should().BeInDescendingOrder());
        Assert.Throws<VouchException>(() => rows.Should().AllSatisfy(x => { }));
        Assert.Throws<VouchException>(() => rows.Should().BeSubsetOf(new[] { 1 }));
        Assert.Throws<VouchException>(() => rows.Should().NotIntersectWith(new[] { 1 }));
        Assert.Throws<VouchException>(() => rows.Should().HaveCountGreaterThan(1));
        Assert.Throws<VouchException>(() => rows.Should().HaveCountGreaterThanOrEqualTo(1));
    }

    // BeEmpty / NotBeEmpty / BeNull / NotBeNull

    [Fact] public void BeEmpty_passes() => new List<int>().Should().BeEmpty();
    [Fact] public void BeEmpty_fails() => Assert.Throws<VouchException>(() => new[] { 1 }.Should().BeEmpty());
    [Fact] public void BeEmpty_message() { var xs = new[] { "BBB" }; Assert.Equal("Expected xs to be empty, but found 1 item:\n  [0] \"BBB\"", Message(() => xs.Should().BeEmpty())); }
    [Fact] public void BeEmpty_message_plural() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to be empty, but found 2 items:\n  [0] 1\n  [1] 2", Message(() => xs.Should().BeEmpty())); }
    [Fact] public void NotBeEmpty_passes() => new[] { 1 }.Should().NotBeEmpty();
    [Fact] public void NotBeEmpty_fails() => Assert.Throws<VouchException>(() => new int[0].Should().NotBeEmpty());
    [Fact] public void NotBeEmpty_message() { var xs = new int[0]; Assert.Equal("Expected xs not to be empty, but it was.", Message(() => xs.Should().NotBeEmpty())); }
    [Fact] public void BeNull_passes() { List<int>? xs = null; xs.Should().BeNull(); }
    [Fact] public void BeNull_fails() => Assert.Throws<VouchException>(() => new[] { 1 }.Should().BeNull());
    [Fact] public void BeNull_message() { var xs = new[] { 1 }; Assert.Equal("Expected xs to be null, but found [1].", Message(() => xs.Should().BeNull())); }
    [Fact] public void NotBeNull_passes() => new[] { 1 }.Should().NotBeNull();
    [Fact] public void NotBeNull_fails() { int[]? xs = null; Assert.Throws<VouchException>(() => xs.Should().NotBeNull()); }

    // counts

    [Fact] public void HaveCount_passes() => new[] { 1, 2 }.Should().HaveCount(2);
    [Fact] public void HaveCount_fails() => Assert.Throws<VouchException>(() => new[] { 1 }.Should().HaveCount(2));
    [Fact] public void HaveCount_message() { var xs = new[] { "a", "b" }; Assert.Equal("Expected xs to contain 3 items, but found 2:\n  [0] \"a\"\n  [1] \"b\"", Message(() => xs.Should().HaveCount(3))); }
    [Fact] public void HaveCount_singular_message() { var xs = new int[0]; Assert.Equal("Expected xs to contain 1 item, but found 0.", Message(() => xs.Should().HaveCount(1))); }
    [Fact]
    public void Message_lists_items_and_cuts_at_ten()
    {
        var rows = Enumerable.Range(0, 50).ToList();
        var msg = Message(() => rows.Should().HaveCount(3));
        Assert.Contains("  [9] 9", msg);
        Assert.DoesNotContain("  [10]", msg);
        Assert.EndsWith("\n  ... (40 more)", msg);
    }
    [Fact] public void HaveCountGreaterThan_passes() => new[] { 1, 2 }.Should().HaveCountGreaterThan(1);
    [Fact] public void HaveCountGreaterThan_fails_on_equal() => Assert.Throws<VouchException>(() => new[] { 1, 2 }.Should().HaveCountGreaterThan(2));
    [Fact] public void HaveCountGreaterThan_message() { var xs = new[] { 1 }; Assert.Equal("Expected xs to contain more than 1 item, but found 1:\n  [0] 1", Message(() => xs.Should().HaveCountGreaterThan(1))); }
    [Fact] public void HaveCountGreaterThanOrEqualTo_passes() => new[] { 1, 2 }.Should().HaveCountGreaterThanOrEqualTo(2);
    [Fact] public void HaveCountGreaterThanOrEqualTo_fails() => Assert.Throws<VouchException>(() => new[] { 1 }.Should().HaveCountGreaterThanOrEqualTo(2));
    [Fact] public void HaveCountGreaterThanOrEqualTo_message() { var xs = new[] { 1 }; Assert.Equal("Expected xs to contain at least 2 items, but found 1:\n  [0] 1", Message(() => xs.Should().HaveCountGreaterThanOrEqualTo(2))); }

    // Contain

    [Fact] public void Contain_item_passes() => new[] { 1, 2 }.Should().Contain(2);
    [Fact] public void Contain_item_fails() => Assert.Throws<VouchException>(() => new[] { 1 }.Should().Contain(2));
    [Fact] public void Contain_item_message() { var xs = new[] { "A", "B" }; Assert.Equal("Expected xs to contain \"BBB\", but it did not. It has:\n  [0] \"A\"\n  [1] \"B\"", Message(() => xs.Should().Contain("BBB"))); }
    [Fact] public void Contain_item_Which_is_the_item() => Assert.Equal(2, new[] { 1, 2 }.Should().Contain(2).Which);
    [Fact] public void Contain_item_allows_null_item() => new string?[] { "a", null }.Should().Contain((string?)null);
    [Fact] public void Contain_items_passes() => new[] { 1, 2, 3 }.Should().Contain(new List<int> { 1, 3 });
    [Fact] public void Contain_items_fails() => Assert.Throws<VouchException>(() => new[] { 1 }.Should().Contain(new List<int> { 1, 3 }));
    [Fact] public void Contain_items_message() { var xs = new[] { 1 }; Assert.Equal("Expected xs to contain all of [1, 3, 4], but it is missing [3, 4]. It has:\n  [0] 1", Message(() => xs.Should().Contain(new List<int> { 1, 3, 4 }))); }
    [Fact] public void Contain_params_passes() => new[] { 1, 2, 3 }.Should().Contain(1, 3);
    [Fact] public void Contain_params_fails() => Assert.Throws<VouchException>(() => new[] { 1 }.Should().Contain(1, 3));
    [Fact] public void Contain_params_message() { var xs = new[] { 1 }; Assert.Equal("Expected xs to contain all of [1, 3], but it is missing [3]. It has:\n  [0] 1", Message(() => xs.Should().Contain(1, 3))); }
    [Fact] public void Contain_predicate_passes() => new[] { 1, 2 }.Should().Contain(x => x > 1);
    [Fact] public void Contain_predicate_fails() => Assert.Throws<VouchException>(() => new[] { 1 }.Should().Contain(x => x > 1));
    [Fact] public void Contain_predicate_message() { var xs = new[] { 1 }; Assert.Equal("Expected xs to contain an item matching (x > 1), but it did not. It has:\n  [0] 1", Message(() => xs.Should().Contain(x => x > 1))); }
    [Fact] public void Contain_predicate_Which_is_first_match() => Assert.Equal(new Row("AAA", 2), Rows.Should().Contain(r => r.Qty > 1).Which);

    // NotContain

    [Fact] public void NotContain_item_passes() => new[] { 1 }.Should().NotContain(2);
    [Fact] public void NotContain_item_fails() => Assert.Throws<VouchException>(() => new[] { 1 }.Should().NotContain(1));
    [Fact] public void NotContain_item_message() { var xs = new[] { 5, 6 }; Assert.Equal("Expected xs not to contain 6, but found it at index 1.", Message(() => xs.Should().NotContain(6))); }
    [Fact] public void NotContain_items_passes() => new[] { 1 }.Should().NotContain(new List<int> { 2, 3 });
    [Fact] public void NotContain_items_fails() => Assert.Throws<VouchException>(() => new[] { 1, 2 }.Should().NotContain(new List<int> { 2, 3 }));
    [Fact] public void NotContain_items_message() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs not to contain any of [2, 3], but found [2].", Message(() => xs.Should().NotContain(new List<int> { 2, 3 }))); }
    [Fact] public void NotContain_predicate_passes() => new[] { 1 }.Should().NotContain(x => x > 1);
    [Fact] public void NotContain_predicate_fails() => Assert.Throws<VouchException>(() => new[] { 1, 2 }.Should().NotContain(x => x > 1));
    [Fact] public void NotContain_predicate_message() { var xs = new[] { 1, 2, 3 }; Assert.Equal("Expected xs not to contain any item matching (x > 1), but found:\n  [1] 2\n  [2] 3", Message(() => xs.Should().NotContain(x => x > 1))); }

    // ContainSingle

    [Fact] public void ContainSingle_passes() => new[] { 1 }.Should().ContainSingle();
    [Fact] public void ContainSingle_fails() => Assert.Throws<VouchException>(() => new[] { 1, 2 }.Should().ContainSingle());
    [Fact] public void ContainSingle_message() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to contain a single item, but found 2:\n  [0] 1\n  [1] 2", Message(() => xs.Should().ContainSingle())); }
    [Fact] public void ContainSingle_message_on_empty() { var xs = new int[0]; Assert.Equal("Expected xs to contain a single item, but found 0.", Message(() => xs.Should().ContainSingle())); }
    [Fact] public void ContainSingle_Which_is_the_item() => Assert.Equal("BBB", new[] { new Row("BBB", 1) }.Should().ContainSingle().Which.Code);
    [Fact] public void ContainSingle_Subject_is_the_item() => Assert.Equal(1, new[] { 1 }.Should().ContainSingle().Subject);
    [Fact] public void ContainSingle_predicate_passes() => Assert.Equal("AAA", Rows.Should().ContainSingle(r => r.Qty == 2).Which.Code);
    [Fact] public void ContainSingle_predicate_fails_on_two() => Assert.Throws<VouchException>(() => new[] { 1, 1 }.Should().ContainSingle(x => x == 1));
    [Fact] public void ContainSingle_predicate_fails_on_none() => Assert.Throws<VouchException>(() => new[] { 1 }.Should().ContainSingle(x => x == 2));
    [Fact] public void ContainSingle_predicate_message() { var xs = new[] { new Row("BBB", 1), new Row("AAA", 2), new Row("BBB", 3) }; Assert.Equal("Expected xs to contain a single item matching (x.Code == \"BBB\"), but found 2:\n  [0] Row { Code = \"BBB\", Qty = 1 }\n  [2] Row { Code = \"BBB\", Qty = 3 }", Message(() => xs.Should().ContainSingle(x => x.Code == "BBB"))); }
    [Fact] public void ContainSingle_predicate_message_on_none() { var xs = new[] { 1 }; Assert.Equal("Expected xs to contain a single item matching (x == 2), but found 0. It has:\n  [0] 1", Message(() => xs.Should().ContainSingle(x => x == 2))); }

    // OnlyContain

    [Fact] public void OnlyContain_passes() => new[] { 1, 2 }.Should().OnlyContain(x => x > 0);
    [Fact] public void OnlyContain_fails() => Assert.Throws<VouchException>(() => new[] { 1, -2 }.Should().OnlyContain(x => x > 0));
    [Fact] public void OnlyContain_message() { var xs = new[] { 1, -2, 0 }; Assert.Equal("Expected xs to only contain items matching (x > 0), but these did not:\n  [1] -2\n  [2] 0", Message(() => xs.Should().OnlyContain(x => x > 0))); }

    // Equal / NotEqual

    [Fact] public void Equal_passes() => new[] { 1, 2 }.Should().Equal(new List<int> { 1, 2 });
    [Fact] public void Equal_params_form() => new List<string> { "a", "b" }.Should().Equal("a", "b");
    [Fact] public void Equal_is_ordered() => Assert.Throws<VouchException>(() => new[] { 2, 1 }.Should().Equal(1, 2));
    [Fact] public void Equal_message_differs_at_index() { var xs = new[] { "BBB", "CCC", "DDD" }; Assert.Equal("Expected xs to equal the expected items, but they differ at index 2: expected \"AAA\", found \"DDD\".", Message(() => xs.Should().Equal("BBB", "CCC", "AAA"))); }
    [Fact] public void Equal_message_subject_longer() { var xs = new[] { 1, 2, 9, 4 }; Assert.Equal("Expected xs to equal the expected items, but it has 4 items, expected 3.\n  They differ at index 2: expected 3, found 9.", Message(() => xs.Should().Equal(1, 2, 3))); }
    [Fact] public void Equal_message_subject_shorter_prefix_matches() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to equal the expected items, but it has 2 items, expected 3.\n  They differ at index 2: expected 3, found nothing.", Message(() => xs.Should().Equal(1, 2, 3))); }
    [Fact] public void Equal_message_subject_longer_prefix_matches() { var xs = new[] { 1, 2, 3 }; Assert.Equal("Expected xs to equal the expected items, but it has 3 items, expected 2.\n  They differ at index 2: expected nothing, found 3.", Message(() => xs.Should().Equal(1, 2))); }
    [Fact] public void Equal_handles_null_items() => new string?[] { "a", null }.Should().Equal("a", null);
    [Fact] public void NotEqual_passes() => new[] { 1, 2 }.Should().NotEqual(new[] { 2, 1 });
    [Fact] public void NotEqual_fails() => Assert.Throws<VouchException>(() => new[] { 1, 2 }.Should().NotEqual(new[] { 1, 2 }));
    [Fact] public void NotEqual_message() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs not to equal [1, 2], but it did.", Message(() => xs.Should().NotEqual(new[] { 1, 2 }))); }

    // uniqueness

    [Fact] public void OnlyHaveUniqueItems_passes() => new[] { 1, 2 }.Should().OnlyHaveUniqueItems();
    [Fact] public void OnlyHaveUniqueItems_fails() => Assert.Throws<VouchException>(() => new[] { 1, 1 }.Should().OnlyHaveUniqueItems());
    [Fact] public void OnlyHaveUniqueItems_message() { var xs = new[] { "BBB", "CCC", "BBB" }; Assert.Equal("Expected xs to have unique items, but \"BBB\" appears 2 times.", Message(() => xs.Should().OnlyHaveUniqueItems())); }
    [Fact] public void OnlyHaveUniqueItems_handles_null_items() { Assert.Throws<VouchException>(() => new string?[] { null, null }.Should().OnlyHaveUniqueItems()); new string?[] { null, "a" }.Should().OnlyHaveUniqueItems(); }

    // ordering

    [Fact] public void Strings_in_an_object_array_order_ordinally() { var saved = System.Globalization.CultureInfo.CurrentCulture; try { System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("sv-SE"); new object[] { "B", "a" }.Should().BeInAscendingOrder(); } finally { System.Globalization.CultureInfo.CurrentCulture = saved; } }

    [Fact]
    public void String_ordering_is_ordinal_in_every_culture()
    {
        var saved = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            foreach (var name in new[] { "en-US", "sv-SE", "tr-TR", "" })
            {
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(name);
                new[] { "B", "a" }.Should().BeInAscendingOrder();
                new[] { "a", "B" }.Should().BeInDescendingOrder();
                Assert.Throws<VouchException>(() => new[] { "a", "Z", "B" }.Should().BeInAscendingOrder());
                new[] { "B", "a" }.Should().BeInAscendingOrder(x => x);
                new[] { "a", "B" }.Should().BeInDescendingOrder(x => x);
                new[] { (1, "B"), (1, "a") }.Should().BeInAscendingOrder(x => x.Item1).ThenBeInAscendingOrder(x => x.Item2);
            }
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = saved;
        }
    }

    [Fact] public void BeInAscendingOrder_passes() => new[] { 1, 2, 2, 3 }.Should().BeInAscendingOrder();
    [Fact] public void BeInAscendingOrder_fails() => Assert.Throws<VouchException>(() => new[] { 2, 1 }.Should().BeInAscendingOrder());
    [Fact] public void BeInAscendingOrder_message() { var xs = new[] { 1, 3, 7, 5 }; Assert.Equal("Expected xs to be in ascending order, but [3] 5 comes after [2] 7.", Message(() => xs.Should().BeInAscendingOrder())); }
    [Fact] public void BeInAscendingOrder_key_passes() => Rows.Should().BeInAscendingOrder(r => r.Qty);
    [Fact] public void BeInAscendingOrder_key_fails() => Assert.Throws<VouchException>(() => Rows.Should().BeInAscendingOrder(r => r.Code));
    [Fact] public void BeInAscendingOrder_key_message() { Assert.Equal("Expected Rows to be in ascending order, but [1] Row { Code = \"AAA\", Qty = 2 } comes after [0] Row { Code = \"BBB\", Qty = 1 }.", Message(() => Rows.Should().BeInAscendingOrder(r => r.Code))); }
    [Fact] public void BeInDescendingOrder_passes() => new[] { 3, 2, 2, 1 }.Should().BeInDescendingOrder();
    [Fact] public void BeInDescendingOrder_fails() => Assert.Throws<VouchException>(() => new[] { 1, 2 }.Should().BeInDescendingOrder());
    [Fact] public void BeInDescendingOrder_message() { var xs = new[] { 9, 7, 8 }; Assert.Equal("Expected xs to be in descending order, but [2] 8 comes after [1] 7.", Message(() => xs.Should().BeInDescendingOrder())); }
    [Fact] public void BeInDescendingOrder_key_passes() => Rows.Should().BeInDescendingOrder(r => r.Code);
    [Fact] public void BeInDescendingOrder_key_fails() => Assert.Throws<VouchException>(() => Rows.Should().BeInDescendingOrder(r => r.Qty));
    [Fact] public void BeInDescendingOrder_key_message() { Assert.Equal("Expected Rows to be in descending order, but [1] Row { Code = \"AAA\", Qty = 2 } comes after [0] Row { Code = \"BBB\", Qty = 1 }.", Message(() => Rows.Should().BeInDescendingOrder(r => r.Qty))); }
    [Fact] public void Ordering_passes_on_empty_and_single() { new int[0].Should().BeInAscendingOrder(); new[] { 1 }.Should().BeInDescendingOrder(); }

    // AllSatisfy

    [Fact] public void AllSatisfy_passes() => new[] { 1, 2 }.Should().AllSatisfy(x => x.Should().BePositive());
    [Fact] public void AllSatisfy_fails() => Assert.Throws<VouchException>(() => new[] { 1, -1 }.Should().AllSatisfy(x => x.Should().BePositive()));
    [Fact]
    public void AllSatisfy_reports_inner_failure()
    {
        var xs = new[] { 1, -1 };
        var msg = Message(() => xs.Should().AllSatisfy(x => x.Should().BePositive()));
        Assert.Contains("[1]", msg);
    }
    [Fact]
    public void AllSatisfy_message()
    {
        var xs = new[] { 1, -1, 5, 0 };
        Assert.Equal("Expected xs to have all items satisfy the assertion, but 2 did not:\n  [1] Expected x to be positive, but found -1.\n  [3] Expected x to be positive, but found 0.",
            Message(() => xs.Should().AllSatisfy(x => x.Should().BePositive())));
    }
    [Fact]
    public void AllSatisfy_cuts_at_ten()
    {
        var xs = Enumerable.Repeat(-1, 15).ToArray();
        var msg = Message(() => xs.Should().AllSatisfy(x => x.Should().BePositive()));
        Assert.Contains("but 15 did not:", msg);
        Assert.EndsWith("\n  ... (5 more)", msg);
    }
    [Fact] public void AllSatisfy_does_not_swallow_other_exceptions() => Assert.Throws<InvalidOperationException>(() => new[] { 1 }.Should().AllSatisfy(_ => throw new InvalidOperationException()));

    // sets

    [Fact] public void NotIntersectWith_passes() => new[] { 1, 2 }.Should().NotIntersectWith(new[] { 3 });
    [Fact] public void NotIntersectWith_fails() => Assert.Throws<VouchException>(() => new[] { 1, 2 }.Should().NotIntersectWith(new[] { 2 }));
    [Fact] public void NotIntersectWith_message() { var xs = new[] { 1, 2, 3 }; Assert.Equal("Expected xs not to intersect with [2, 3, 4], but found common items [2, 3].", Message(() => xs.Should().NotIntersectWith(new[] { 2, 3, 4 }))); }
    [Fact] public void BeSubsetOf_passes() => new[] { 1, 2 }.Should().BeSubsetOf(new[] { 1, 2, 3 });
    [Fact] public void BeSubsetOf_fails() => Assert.Throws<VouchException>(() => new[] { 1, 4 }.Should().BeSubsetOf(new[] { 1, 2, 3 }));
    [Fact] public void BeSubsetOf_message() { var xs = new[] { 1, 4, 5 }; Assert.Equal("Expected xs to be a subset of [1, 2, 3], but [4, 5] are not in it.", Message(() => xs.Should().BeSubsetOf(new[] { 1, 2, 3 }))); }
    [Fact] public void BeSubsetOf_single_pass_other() { IEnumerable<int> Other() { yield return 1; yield return 2; } new[] { 1 }.Should().BeSubsetOf(Other()); }

    // chaining

    [Fact] public void And_chains_back_to_the_assertions() => new[] { 1, 2 }.Should().HaveCount(2).And.Contain(1).And.NotContain(3);

    // empty subjects end with a period

    [Fact] public void Contain_item_on_empty_message() { var xs = new int[0]; Assert.Equal("Expected xs to contain 5, but it was empty.", Message(() => xs.Should().Contain(5))); }
    [Fact] public void Contain_predicate_on_empty_message() { var xs = new int[0]; Assert.Equal("Expected xs to contain an item matching (x > 1), but it was empty.", Message(() => xs.Should().Contain(x => x > 1))); }
    [Fact] public void Contain_items_on_empty_message() { var xs = new int[0]; Assert.Equal("Expected xs to contain all of [1, 2], but it is missing [1, 2].", Message(() => xs.Should().Contain(1, 2))); }
    [Fact] public void HaveCountGreaterThan_on_empty_message() { var xs = new int[0]; Assert.Equal("Expected xs to contain more than 1 item, but found 0.", Message(() => xs.Should().HaveCountGreaterThan(1))); }
    [Fact] public void ContainSingle_predicate_on_empty_message() { var xs = new int[0]; Assert.Equal("Expected xs to contain a single item matching (x == 2), but found 0.", Message(() => xs.Should().ContainSingle(x => x == 2))); }

    // lazy materialisation

    [Fact]
    public void Failure_on_lazy_set_shows_items_and_enumerates_once()
    {
        var calls = 0;
        IEnumerable<int> Gen() { calls++; yield return 1; }
        var gen = Gen();
        var a = gen.Should();
        Assert.Equal("Expected gen to contain 2 items, but found 1:\n  [0] 1", Message(() => a.HaveCount(2)));
        a.Contain(1);
        Assert.Equal(1, calls);
    }

    // non-comparable items

    private sealed class Opaque { }

    [Fact] public void BeInAscendingOrder_non_comparable_fails_cleanly() { var xs = new[] { new Opaque(), new Opaque() }; Assert.Equal("Expected xs to be in ascending order, but Opaque is not comparable.", Message(() => xs.Should().BeInAscendingOrder())); }
    [Fact] public void BeInDescendingOrder_non_comparable_fails_cleanly() { var xs = new[] { new Opaque(), new Opaque() }; Assert.Equal("Expected xs to be in descending order, but Opaque is not comparable.", Message(() => xs.Should().BeInDescendingOrder())); }
    [Fact] public void BeInAscendingOrder_non_comparable_key_fails_cleanly() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to be in ascending order, but Opaque is not comparable.", Message(() => xs.Should().BeInAscendingOrder(_ => new Opaque()))); }
    [Fact] public void BeInDescendingOrder_non_comparable_key_fails_cleanly() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to be in descending order, but Opaque is not comparable.", Message(() => xs.Should().BeInDescendingOrder(_ => new Opaque()))); }

    [Fact] public void BeSameAs_passes_on_the_same_instance() { var xs = new List<int> { 1 }; xs.Should().BeSameAs(xs); }
    [Fact] public void BeSameAs_fails_on_an_equal_copy() { var xs = new List<int> { 1 }; Assert.Throws<VouchException>(() => xs.Should().BeSameAs(new List<int> { 1 })); }
    [Fact] public void BeSameAs_message() { var xs = new List<int> { 1 }; var ys = new List<int> { 2 }; Assert.Equal("Expected xs to refer to [2], but found another collection: [1].", Message(() => xs.Should().BeSameAs(ys))); }
    [Fact] public void BeSameAs_fails_on_a_null_subject() { List<int>? xs = null; Assert.Equal("Expected xs to refer to [2], but found null.", Message(() => xs.Should().BeSameAs(new List<int> { 2 }))); }
    [Fact] public void BeSameAs_null_expected_throws_ArgumentNullException() { var xs = new List<int> { 1 }; Assert.Throws<ArgumentNullException>(() => xs.Should().BeSameAs(null!)); }

    [Fact] public void EndWith_passes_on_the_trailing_items() { new[] { 1, 2, 3 }.Should().EndWith(new[] { 2, 3 }); }
    [Fact] public void EndWith_passes_on_the_whole_sequence_and_on_none() { var xs = new[] { 1, 2 }; xs.Should().EndWith(new[] { 1, 2 }); xs.Should().EndWith(Array.Empty<int>()); }
    [Fact] public void EndWith_fails_on_a_different_tail() { var xs = new[] { 1, 2, 3 }; Assert.Throws<VouchException>(() => xs.Should().EndWith(new[] { 1, 3 })); }
    [Fact] public void EndWith_fails_when_the_expected_is_longer() { var xs = new[] { 2, 3 }; Assert.Throws<VouchException>(() => xs.Should().EndWith(new[] { 1, 2, 3 })); }
    [Fact] public void EndWith_message() { var xs = new[] { 1, 2, 3 }; Assert.Equal("Expected xs to end with [1, 3], but found [1, 2, 3].", Message(() => xs.Should().EndWith(new[] { 1, 3 }))); }
    [Fact] public void EndWith_fails_on_a_null_subject() { int[]? xs = null; Assert.Equal("Expected xs to end with [1], but found null.", Message(() => xs.Should().EndWith(new[] { 1 }))); }
    [Fact] public void EndWith_null_expected_throws_ArgumentNullException() { var xs = new[] { 1 }; Assert.Throws<ArgumentNullException>(() => xs.Should().EndWith(null!)); }
    [Fact] public void EndWith_reads_a_one_shot_sequence_once() { var reads = 0; IEnumerable<int> Tail() { reads++; yield return 3; } new[] { 1, 2, 3 }.Should().EndWith(Tail()); Assert.Equal(1, reads); }

    [Fact] public void BeNullOrEmpty_passes_on_null_and_empty() { List<int>? none = null; none.Should().BeNullOrEmpty(); new List<int>().Should().BeNullOrEmpty(); }
    [Fact] public void BeNullOrEmpty_fails_on_items() { var xs = new List<int> { 1 }; Assert.Throws<VouchException>(() => xs.Should().BeNullOrEmpty()); }
    [Fact] public void BeNullOrEmpty_message() { var xs = new List<int> { 1, 2 }; Assert.Equal("Expected xs to be null or empty, but found 2 items:\n  [0] 1\n  [1] 2", Message(() => xs.Should().BeNullOrEmpty())); }
    [Fact] public void BeNullOrEmpty_works_on_a_dictionary() { Dictionary<string, int>? none = null; none.Should().BeNullOrEmpty(); new Dictionary<string, int>().Should().BeNullOrEmpty(); Assert.Throws<VouchException>(() => new Dictionary<string, int> { ["a"] = 1 }.Should().BeNullOrEmpty()); }

    [Fact] public void BeInAscendingOrder_by_comparer_passes() { new[] { "a", "B", "c" }.Should().BeInAscendingOrder(StringComparer.OrdinalIgnoreCase); }
    [Fact] public void BeInAscendingOrder_by_comparer_uses_the_comparer() { var xs = new[] { "a", "B", "c" }; Assert.Throws<VouchException>(() => xs.Should().BeInAscendingOrder(StringComparer.Ordinal)); }
    [Fact] public void BeInAscendingOrder_by_comparer_message() { var xs = new[] { "a", "B", "c" }; Assert.Equal("Expected xs to be in ascending order, but [1] \"B\" comes after [0] \"a\".", Message(() => xs.Should().BeInAscendingOrder(StringComparer.Ordinal))); }
    [Fact] public void BeInAscendingOrder_by_comparer_chains() { new[] { "a", "b" }.Should().BeInAscendingOrder(StringComparer.Ordinal).And.OnlyHaveUniqueItems(); }
    [Fact] public void BeInAscendingOrder_by_comparer_null_throws_ArgumentNullException() { Assert.Throws<ArgumentNullException>(() => new[] { "a" }.Should().BeInAscendingOrder((IComparer<string>)null!)); }
    [Fact] public void BeInDescendingOrder_by_comparer_passes_and_fails() { new[] { "c", "B", "a" }.Should().BeInDescendingOrder(StringComparer.OrdinalIgnoreCase); var xs = new[] { "a", "b" }; Assert.Equal("Expected xs to be in descending order, but [1] \"b\" comes after [0] \"a\".", Message(() => xs.Should().BeInDescendingOrder(StringComparer.Ordinal))); }
    [Fact] public void BeInDescendingOrder_by_comparer_null_throws_ArgumentNullException() { Assert.Throws<ArgumentNullException>(() => new[] { "a" }.Should().BeInDescendingOrder((IComparer<string>)null!)); }

    // AllBeEquivalentTo

    [Fact] public void AllBeEquivalentTo_passes() => new[] { 1, 1 }.Should().AllBeEquivalentTo(1);
    [Fact] public void AllBeEquivalentTo_passes_on_empty() => new int[0].Should().AllBeEquivalentTo(1);
    [Fact] public void AllBeEquivalentTo_fails() => Assert.Throws<VouchException>(() => new[] { 1, 2 }.Should().AllBeEquivalentTo(1));
    [Fact] public void AllBeEquivalentTo_message() { var xs = new[] { 1, 2, 1, 3 }; Assert.Equal("Expected xs to have all items equivalent to 1, but 2 did not:\n  [1] 2\n  [3] 3", Message(() => xs.Should().AllBeEquivalentTo(1))); }
    [Fact]
    public void AllBeEquivalentTo_cuts_at_ten()
    {
        var xs = Enumerable.Repeat(2, 15).ToArray();
        var msg = Message(() => xs.Should().AllBeEquivalentTo(1));
        Assert.Contains("but 15 did not:", msg);
        Assert.EndsWith("\n  ... (5 more)", msg);
    }
    [Fact] public void AllBeEquivalentTo_fails_on_null() { List<int>? xs = null; Assert.Equal("Expected xs to have all items equivalent to 1, but found null.", Message(() => xs.Should().AllBeEquivalentTo(1))); }
    [Fact] public void AllBeEquivalentTo_with_options_passes() { var rows = new[] { new Row("A", 1), new Row("A", 2) }; rows.Should().AllBeEquivalentTo(new Row("A", 9), o => o.Excluding(r => r.Qty)); }
    [Fact] public void AllBeEquivalentTo_with_options_fails() { var rows = new[] { new Row("A", 1), new Row("B", 2) }; Assert.Throws<VouchException>(() => rows.Should().AllBeEquivalentTo(new Row("A", 9), o => o.Excluding(r => r.Qty))); }
    [Fact] public void AllBeEquivalentTo_with_options_message() { var rows = new[] { new Row("A", 1), new Row("B", 2) }; Assert.Equal("Expected rows to have all items equivalent to Row { Code = \"A\", Qty = 9 }, but 1 did not:\n  [1] Row { Code = \"B\", Qty = 2 }", Message(() => rows.Should().AllBeEquivalentTo(new Row("A", 9), o => o.Excluding(r => r.Qty)))); }
    [Fact] public void AllBeEquivalentTo_with_options_fails_on_null() { Row[]? rows = null; Assert.Equal("Expected rows to have all items equivalent to Row { Code = \"A\", Qty = 9 }, but found null.", Message(() => rows.Should().AllBeEquivalentTo(new Row("A", 9), o => o.Excluding(r => r.Qty)))); }
    [Fact] public void AllBeEquivalentTo_takes_a_reason() { var xs = new[] { 2 }; Assert.Contains("Because why.", Message(() => xs.Should().AllBeEquivalentTo(1, "why"))); }

    // ContainInOrder

    [Fact] public void ContainInOrder_passes() => new[] { 1, 2, 3, 4 }.Should().ContainInOrder(1, 3, 4);
    [Fact] public void ContainInOrder_passes_with_a_sequence() => new[] { 1, 2, 3, 4 }.Should().ContainInOrder(new List<int> { 2, 4 });
    [Fact] public void ContainInOrder_passes_for_an_empty_expectation() => new[] { 1 }.Should().ContainInOrder();
    [Fact] public void ContainInOrder_fails_when_the_order_is_wrong() => Assert.Throws<VouchException>(() => new[] { 1, 2, 3 }.Should().ContainInOrder(3, 1));
    [Fact] public void ContainInOrder_message_for_a_missing_item() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to contain [5] in order, but 5 (item 0 of the expected items) was not found at all. It has:\n  [0] 1\n  [1] 2", Message(() => xs.Should().ContainInOrder(5))); }
    [Fact] public void ContainInOrder_message_for_a_wrong_order() { var xs = new[] { 1, 2, 3 }; Assert.Equal("Expected xs to contain [1, 3, 2] in order, but 2 (item 2 of the expected items) was not found after index 2. It has:\n  [0] 1\n  [1] 2\n  [2] 3", Message(() => xs.Should().ContainInOrder(1, 3, 2))); }
    [Fact] public void ContainInOrder_fails_on_null() { List<int>? xs = null; Assert.Equal("Expected xs to contain [1] in order, but found null.", Message(() => xs.Should().ContainInOrder(1))); }
    [Fact] public void ContainInOrder_takes_a_reason() { var xs = new int[0]; Assert.Equal("Expected xs to contain [1] in order, but 1 (item 0 of the expected items) was not found at all. Because why.", Message(() => xs.Should().ContainInOrder(new List<int> { 1 }, "why"))); }
    [Fact] public void ContainInOrder_null_expectation_throws() { var xs = new[] { 1 }; Assert.Throws<ArgumentNullException>(() => xs.Should().ContainInOrder((IEnumerable<int>)null!)); }

    // ContainInConsecutiveOrder

    [Fact] public void ContainInConsecutiveOrder_passes() => new[] { 1, 2, 3, 4 }.Should().ContainInConsecutiveOrder(2, 3);
    [Fact] public void ContainInConsecutiveOrder_passes_with_a_sequence() => new[] { 1, 2, 3, 4 }.Should().ContainInConsecutiveOrder(new List<int> { 3, 4 });
    [Fact] public void ContainInConsecutiveOrder_passes_for_an_empty_expectation() => new[] { 1 }.Should().ContainInConsecutiveOrder();
    [Fact] public void ContainInConsecutiveOrder_fails_when_items_are_apart() => Assert.Throws<VouchException>(() => new[] { 1, 2, 3 }.Should().ContainInConsecutiveOrder(1, 3));
    [Fact] public void ContainInConsecutiveOrder_message() { var xs = new[] { 1, 2, 3 }; Assert.Equal("Expected xs to contain [1, 3] in consecutive order, but the longest run in order was 1 of 2, broken at 3 (item 1 of the expected items). It has:\n  [0] 1\n  [1] 2\n  [2] 3", Message(() => xs.Should().ContainInConsecutiveOrder(1, 3))); }
    [Fact] public void ContainInConsecutiveOrder_fails_on_null() { List<int>? xs = null; Assert.Equal("Expected xs to contain [1] in consecutive order, but found null.", Message(() => xs.Should().ContainInConsecutiveOrder(1))); }
    [Fact] public void ContainInConsecutiveOrder_takes_a_reason() { var xs = new int[0]; Assert.Equal("Expected xs to contain [1] in consecutive order, but the longest run in order was 0 of 1, broken at 1 (item 0 of the expected items). Because why.", Message(() => xs.Should().ContainInConsecutiveOrder(new List<int> { 1 }, "why"))); }
    [Fact] public void ContainInConsecutiveOrder_null_expectation_throws() { var xs = new[] { 1 }; Assert.Throws<ArgumentNullException>(() => xs.Should().ContainInConsecutiveOrder((IEnumerable<int>)null!)); }

    // Fix round 1

    private static int _lazyEnumerations;

    private static IEnumerable<int> LazyOnce(params int[] values)
    {
        _lazyEnumerations++;
        foreach (var v in values)
        {
            yield return v;
        }
    }

    [Fact] public void ContainInOrder_repeated_expected_item_needs_two_occurrences() => Assert.Throws<VouchException>(() => new[] { 1 }.Should().ContainInOrder(1, 1));
    [Fact] public void ContainInOrder_repeated_expected_item_passes_with_two_occurrences() => new[] { 1, 2, 1 }.Should().ContainInOrder(1, 1);
    [Fact] public void ContainInOrder_handles_null_items() => new string[] { null!, "a", null! }.Should().ContainInOrder(new string[] { null!, null! });
    [Fact] public void ContainInOrder_fails_on_missing_null_item() => Assert.Throws<VouchException>(() => new string[] { "a" }.Should().ContainInOrder(new string[] { null! }));
    [Fact]
    public void ContainInOrder_enumerates_a_lazy_subject_once()
    {
        _lazyEnumerations = 0;
        LazyOnce(1, 2, 3).Should().ContainInOrder(1, 3);
        Assert.Equal(1, _lazyEnumerations);
        _lazyEnumerations = 0;
        Assert.Throws<VouchException>(() => LazyOnce(1, 2, 3).Should().ContainInOrder(3, 1));
        Assert.Equal(1, _lazyEnumerations);
    }

    [Fact]
    public void ContainInConsecutiveOrder_enumerates_a_lazy_subject_once()
    {
        _lazyEnumerations = 0;
        LazyOnce(1, 2, 3).Should().ContainInConsecutiveOrder(2, 3);
        Assert.Equal(1, _lazyEnumerations);
    }

    [Fact] public void ContainInConsecutiveOrder_backtracks_after_a_partial_match() => new[] { 1, 1, 1, 2 }.Should().ContainInConsecutiveOrder(1, 1, 2);
    [Fact] public void ContainInConsecutiveOrder_handles_null_items() => new string[] { "a", null!, "b" }.Should().ContainInConsecutiveOrder(new string[] { null!, "b" });
    [Fact] public void ContainInConsecutiveOrder_message_reports_the_longest_run() { var xs = new[] { 1, 2, 9, 1, 2, 3 }; Assert.Equal("Expected xs to contain [1, 2, 4] in consecutive order, but the longest run in order was 2 of 3, broken at 4 (item 2 of the expected items). It has:\n  [0] 1\n  [1] 2\n  [2] 9\n  [3] 1\n  [4] 2\n  [5] 3", Message(() => xs.Should().ContainInConsecutiveOrder(1, 2, 4))); }
    [Fact] public void ContainInConsecutiveOrder_message_when_the_expectation_is_longer_than_the_subject() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to contain [1, 2, 3] in consecutive order, but the longest run in order was 2 of 3, broken at 3 (item 2 of the expected items). It has:\n  [0] 1\n  [1] 2", Message(() => xs.Should().ContainInConsecutiveOrder(1, 2, 3))); }
    [Fact] public void ContainInOrder_array_overload_takes_a_reason() { var xs = new int[0]; Assert.Contains("Because why.", Message(() => xs.Should().ContainInOrder(new[] { 1 }, "why"))); }
    [Fact] public void ContainInConsecutiveOrder_array_overload_takes_a_reason() { var xs = new int[0]; Assert.Contains("Because why.", Message(() => xs.Should().ContainInConsecutiveOrder(new[] { 1 }, "why"))); }

    [Fact]
    public void ContainInOrder_accepts_an_expected_sequence_over_the_item_cap()
    {
        var xs = new[] { 0, 1, 2 };
        var expected = Enumerable.Range(0, Equivalency.MaxItems + 1);
        var msg = Message(() => xs.Should().ContainInOrder(expected));
        Assert.StartsWith("Expected xs to contain [", msg);
        Assert.Contains("(item 3 of the expected items) was not found after index 2", msg);
    }

    [Fact] public void AllBeEquivalentTo_passes_with_null_elements() => new string[] { null!, null! }.Should().AllBeEquivalentTo<string>(null!);
    [Fact] public void AllBeEquivalentTo_fails_when_one_element_is_null() => Assert.Throws<VouchException>(() => new string[] { null!, "a" }.Should().AllBeEquivalentTo("a"));
    [Fact] public void AllBeEquivalentTo_passes_on_an_empty_subject_with_options() => new Row[0].Should().AllBeEquivalentTo(new Row("A", 1), o => o.Excluding(r => r.Qty));
    [Fact] public void AllBeEquivalentTo_with_options_takes_a_reason() { var rows = new[] { new Row("B", 2) }; Assert.Contains("Because why.", Message(() => rows.Should().AllBeEquivalentTo(new Row("A", 9), o => o.Excluding(r => r.Qty), "why"))); }
    // issue #15: resolution

    [Fact] public void ImmutableArray_resolves_to_collection_assertions() => Assert.IsType<ImmutableArrayAssertions<int>>(System.Collections.Immutable.ImmutableArray.Create(1, 2).Should());
    [Fact] public void ImmutableArray_assertions_see_the_items() { var arr = System.Collections.Immutable.ImmutableArray.Create(1, 2); arr.Should().HaveCount(2).And.Contain(2); Assert.Equal("Expected arr to contain 3 items, but found 2:\n  [0] 1\n  [1] 2", Message(() => arr.Should().HaveCount(3))); }
    [Fact] public void Default_ImmutableArray_is_treated_as_null() { System.Collections.Immutable.ImmutableArray<int> arr = default; Assert.Equal("Expected arr to contain 1 item, but found null.", Message(() => arr.Should().HaveCount(1))); arr.Should().BeNullOrEmpty(); }
    [Fact] public void IReadOnlyList_resolves_to_collection_assertions() { IReadOnlyList<int> xs = new List<int> { 1 }; Assert.IsType<GenericCollectionAssertions<int>>(xs.Should()); }
    [Fact] public void String_array_resolves_to_collection_assertions() => Assert.IsType<GenericCollectionAssertions<string>>(new[] { "a" }.Should());
    [Fact] public void ArrayList_resolves_to_object_collection_assertions() { var xs = new System.Collections.ArrayList { 1, "a" }; Assert.IsType<NonGenericCollectionAssertions>(xs.Should()); xs.Should().HaveCount(2).And.Contain("a"); }
    [Fact] public void Non_generic_IEnumerable_resolves_to_object_collection_assertions() { System.Collections.IEnumerable xs = new System.Collections.ArrayList { 1 }; Assert.IsType<NonGenericCollectionAssertions>(xs.Should()); }
    [Fact] public void Non_generic_one_shot_sequence_is_enumerated_for_assertions() { System.Collections.IEnumerable Gen() { yield return 1; yield return 2; } Gen().Should().HaveCount(2).And.Contain(2); }
    [Fact] public void Null_non_generic_IEnumerable_is_a_null_subject() { System.Collections.IEnumerable? xs = null; Assert.Equal("Expected xs to contain 1 item, but found null.", Message(() => xs.Should().HaveCount(1))); }
    [Fact] public void Non_generic_ArrayList_failure_message() { var xs = new System.Collections.ArrayList { 1, 2 }; Assert.Equal("Expected xs to contain 3 items, but found 2:\n  [0] 1\n  [1] 2", Message(() => xs.Should().HaveCount(3))); }

    // an empty string is a valid element of a collection (StringAssertions.Contain("") is covered in StringAssertionsTests)

    [Fact] public void Contain_accepts_an_empty_string_element() { new[] { "", "a" }.Should().Contain(""); new List<string> { "" }.Should().Contain(""); }
    [Fact] public void NotContain_accepts_an_empty_string_element() { new[] { "a" }.Should().NotContain(""); }
    [Fact] public void Contain_empty_string_fails_on_a_collection_without_it() { var xs = new[] { "a" }; Assert.Throws<VouchException>(() => xs.Should().Contain("")); }
    [Fact] public void NotContain_empty_string_fails_on_a_collection_with_it() { var xs = new[] { "", "a" }; Assert.Throws<VouchException>(() => xs.Should().NotContain("")); }

    // HaveElementPreceding / HaveElementSucceeding

    [Fact] public void HaveElementPreceding_passes_and_returns_the_element() { var which = new[] { 1, 2, 3 }.Should().HaveElementPreceding(2, 1); Assert.Equal(1, which.Which); }
    [Fact] public void HaveElementPreceding_message_for_another_element() { var xs = new[] { 1, 2, 3 }; Assert.Equal("Expected xs to have element 3 preceding 2, but found 1.", Message(() => xs.Should().HaveElementPreceding(2, 3))); }
    [Fact] public void HaveElementPreceding_message_when_successor_is_first() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to have element 3 preceding 1, but it is the first item and has nothing before it.", Message(() => xs.Should().HaveElementPreceding(1, 3))); }
    [Fact] public void HaveElementPreceding_message_when_successor_is_missing() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to have element 1 preceding 9, but 9 was not found. It has:\n  [0] 1\n  [1] 2", Message(() => xs.Should().HaveElementPreceding(9, 1))); }
    [Fact] public void HaveElementPreceding_message_on_an_empty_collection() { var xs = new int[0]; Assert.Equal("Expected xs to have element 1 preceding 2, but 2 was not found.", Message(() => xs.Should().HaveElementPreceding(2, 1))); }
    [Fact] public void HaveElementPreceding_null_subject_fails() { int[]? xs = null; Assert.Equal("Expected xs to have element 1 preceding 2, but found null.", Message(() => xs.Should().HaveElementPreceding(2, 1))); }
    [Fact] public void HaveElementPreceding_with_reason() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to have element 3 preceding 2, but found 1. Because order matters.", Message(() => xs.Should().HaveElementPreceding(2, 3, "order matters"))); }
    [Fact] public void HaveElementSucceeding_passes_and_returns_the_element() { var which = new[] { 1, 2, 3 }.Should().HaveElementSucceeding(2, 3); Assert.Equal(3, which.Which); }
    [Fact] public void HaveElementSucceeding_message_for_another_element() { var xs = new[] { 1, 2, 3 }; Assert.Equal("Expected xs to have element 1 succeeding 2, but found 3.", Message(() => xs.Should().HaveElementSucceeding(2, 1))); }
    [Fact] public void HaveElementSucceeding_message_when_predecessor_is_last() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to have element 3 succeeding 2, but it is the last item and has nothing after it.", Message(() => xs.Should().HaveElementSucceeding(2, 3))); }
    [Fact] public void HaveElementSucceeding_message_when_predecessor_is_missing() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to have element 1 succeeding 9, but 9 was not found. It has:\n  [0] 1\n  [1] 2", Message(() => xs.Should().HaveElementSucceeding(9, 1))); }
    [Fact] public void HaveElementSucceeding_null_subject_fails() { int[]? xs = null; Assert.Equal("Expected xs to have element 1 succeeding 2, but found null.", Message(() => xs.Should().HaveElementSucceeding(2, 1))); }

    // ContainEquivalentOf

    [Fact] public void ContainEquivalentOf_passes_and_returns_the_matched_item() { var which = new List<Row> { new("A", 1), new("B", 2) }.Should().ContainEquivalentOf(new Row("B", 2)); Assert.Equal(new Row("B", 2), which.Which); }
    [Fact] public void ContainEquivalentOf_matches_by_members_of_another_type() { new List<Row> { new("A", 1) }.Should().ContainEquivalentOf(new { Code = "A", Qty = 1 }); }
    [Fact] public void ContainEquivalentOf_with_options_passes() { new List<Row> { new("A", 1) }.Should().ContainEquivalentOf(new Row("A", 9), o => o.Excluding(r => r.Qty)); }
    [Fact] public void ContainEquivalentOf_message_on_an_empty_collection() { var xs = new List<int>(); Assert.Equal("Expected xs to contain an item equivalent to 5, but it was empty.", Message(() => xs.Should().ContainEquivalentOf(5))); }
    [Fact] public void ContainEquivalentOf_message_lists_the_items() { var xs = new List<int> { 1, 2 }; Assert.Equal("Expected xs to contain an item equivalent to 5, but it did not. It has:\n  [0] 1\n  [1] 2", Message(() => xs.Should().ContainEquivalentOf(5))); }
    [Fact] public void ContainEquivalentOf_null_subject_fails() { List<int>? xs = null; Assert.Equal("Expected xs to contain an item equivalent to 5, but found null.", Message(() => xs.Should().ContainEquivalentOf(5))); }
    [Fact] public void ContainEquivalentOf_with_reason() { var xs = new List<int>(); Assert.Equal("Expected xs to contain an item equivalent to 5, but it was empty. Because we need it.", Message(() => xs.Should().ContainEquivalentOf(5, "we need it"))); }

    // SatisfyRespectively

    [Fact] public void SatisfyRespectively_passes() { new[] { 1, 2 }.Should().SatisfyRespectively(x => x.Should().Be(1), x => x.Should().Be(2)); }
    [Fact] public void SatisfyRespectively_message_for_a_wrong_count() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to contain 1 item to satisfy the assertions respectively, but found 2:\n  [0] 1\n  [1] 2", Message(() => xs.Should().SatisfyRespectively(x => x.Should().Be(1)))); }
    [Fact] public void SatisfyRespectively_collects_the_failures_per_item() { var xs = new[] { 1, 2, 3 }; Assert.Equal("Expected xs to satisfy the assertions respectively, but 2 did not:\n  [0] Expected x to be 9, but found 1.\n  [2] Expected x to be 8, but found 3.", Message(() => xs.Should().SatisfyRespectively(x => x.Should().Be(9), x => x.Should().Be(2), x => x.Should().Be(8)))); }
    [Fact] public void SatisfyRespectively_null_subject_fails() { int[]? xs = null; Assert.Equal("Expected xs to satisfy the assertions respectively, but found null.", Message(() => xs.Should().SatisfyRespectively(x => x.Should().Be(1)))); }
    [Fact] public void SatisfyRespectively_null_assertions_throw_ArgumentNullException() { Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().SatisfyRespectively(null!)); }

    // Satisfy

    [Fact] public void Satisfy_matches_each_predicate_to_a_distinct_item_in_any_order() { new[] { 1, 2, 3 }.Should().Satisfy(x => x == 3, x => x > 0, x => x == 1); }
    [Fact] public void Satisfy_does_not_let_a_loose_predicate_starve_a_strict_one() { new[] { 1, 2 }.Should().Satisfy(x => x > 0, x => x == 1); }
    [Fact] public void Satisfy_message_for_a_wrong_count() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to contain 1 item to satisfy the predicates, but found 2:\n  [0] 1\n  [1] 2", Message(() => xs.Should().Satisfy(x => x > 0))); }
    [Fact] public void Satisfy_message_names_the_unmatched_predicate() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to satisfy the predicates with a distinct item each, but no unused item matched:\n  (x > 5)", Message(() => xs.Should().Satisfy(x => x > 5, x => x > 0))); }
    [Fact] public void Satisfy_fails_when_two_predicates_need_the_same_item() { var xs = new[] { 1, 2 }; Assert.Throws<VouchException>(() => xs.Should().Satisfy(x => x == 1, x => x == 1)); }
    [Fact] public void Satisfy_null_subject_fails() { int[]? xs = null; Assert.Equal("Expected xs to satisfy the predicates, but found null.", Message(() => xs.Should().Satisfy(x => x > 0))); }

    // AllBe / AllBeOfType / AllBeAssignableTo

    [Fact] public void AllBe_passes() { new[] { 2, 2 }.Should().AllBe(2); new int[0].Should().AllBe(2); }
    [Fact] public void AllBe_message_lists_the_differing_items() { var xs = new[] { 2, 3, 2, 4 }; Assert.Equal("Expected xs to have all items be 2, but found 2 that differ:\n  [1] 3\n  [3] 4", Message(() => xs.Should().AllBe(2))); }
    [Fact] public void AllBe_null_subject_fails() { int[]? xs = null; Assert.Equal("Expected xs to have all items be 2, but found null.", Message(() => xs.Should().AllBe(2))); }
    [Fact] public void AllBe_with_reason() { var xs = new[] { 3 }; Assert.Equal("Expected xs to have all items be 2, but found 1 that differ:\n  [0] 3\nBecause they match.", Message(() => xs.Should().AllBe(2, "they match"))); }
    [Fact] public void AllBeOfType_passes_and_returns_the_cast_items() { var which = new object[] { 1, 2 }.Should().AllBeOfType<int>(); Assert.Equal(new[] { 1, 2 }, which.Which); }
    [Fact] public void AllBeOfType_rejects_a_derived_type() { var xs = new object[] { new ArgumentException(), new Exception() }; Assert.Throws<VouchException>(() => xs.Should().AllBeOfType<ArgumentException>()); Assert.Throws<VouchException>(() => new object[] { new ArgumentException() }.Should().AllBeOfType<Exception>()); }
    [Fact] public void AllBeOfType_message() { var xs = new object[] { 1, "x" }; Assert.Equal("Expected xs to have all items be of type Int32, but found 1 that are not:\n  [1] \"x\"", Message(() => xs.Should().AllBeOfType<int>())); }
    [Fact] public void AllBeOfType_null_subject_fails() { object[]? xs = null; Assert.Equal("Expected xs to have all items be of type Int32, but found null.", Message(() => xs.Should().AllBeOfType<int>())); }
    [Fact] public void AllBeAssignableTo_passes_and_returns_the_cast_items() { var which = new object[] { "a", "b" }.Should().AllBeAssignableTo<IComparable>(); Assert.Equal(2, which.Which.Count); Assert.Equal("a", which.Which[0]); }
    [Fact] public void AllBeAssignableTo_accepts_a_derived_type() { new object[] { new ArgumentException() }.Should().AllBeAssignableTo<Exception>(); }
    [Fact] public void AllBeAssignableTo_message() { var xs = new object[] { "a", 1 }; Assert.Equal("Expected xs to have all items be assignable to String, but found 1 that are not:\n  [1] 1", Message(() => xs.Should().AllBeAssignableTo<string>())); }
    [Fact] public void AllBeAssignableTo_null_subject_fails() { object[]? xs = null; Assert.Equal("Expected xs to have all items be assignable to String, but found null.", Message(() => xs.Should().AllBeAssignableTo<string>())); }

    // counts

    [Fact] public void NotHaveCount_passes() { new[] { 1, 2 }.Should().NotHaveCount(3); }
    [Fact] public void NotHaveCount_message() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs not to contain 2 items, but it did:\n  [0] 1\n  [1] 2", Message(() => xs.Should().NotHaveCount(2))); }
    [Fact] public void NotHaveCount_null_subject_fails() { int[]? xs = null; Assert.Equal("Expected xs not to contain 2 items, but found null.", Message(() => xs.Should().NotHaveCount(2))); }
    [Fact] public void NotHaveCount_with_reason() { var xs = new int[0]; Assert.Equal("Expected xs not to contain 0 items, but it did. Because it must have some.", Message(() => xs.Should().NotHaveCount(0, "it must have some"))); }
    [Fact] public void HaveCountLessThan_passes() { new[] { 1, 2 }.Should().HaveCountLessThan(3); }
    [Fact] public void HaveCountLessThan_message() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to contain fewer than 2 items, but found 2:\n  [0] 1\n  [1] 2", Message(() => xs.Should().HaveCountLessThan(2))); }
    [Fact] public void HaveCountLessThan_null_subject_fails() { int[]? xs = null; Assert.Equal("Expected xs to contain fewer than 2 items, but found null.", Message(() => xs.Should().HaveCountLessThan(2))); }
    [Fact] public void HaveCountLessThanOrEqualTo_passes() { new[] { 1, 2 }.Should().HaveCountLessThanOrEqualTo(2); }
    [Fact] public void HaveCountLessThanOrEqualTo_message() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to contain at most 1 item, but found 2:\n  [0] 1\n  [1] 2", Message(() => xs.Should().HaveCountLessThanOrEqualTo(1))); }
    [Fact] public void HaveCountLessThanOrEqualTo_null_subject_fails() { int[]? xs = null; Assert.Equal("Expected xs to contain at most 2 items, but found null.", Message(() => xs.Should().HaveCountLessThanOrEqualTo(2))); }
    [Fact] public void HaveSameCount_passes_across_element_types() { new[] { 1, 2 }.Should().HaveSameCount(new[] { "a", "b" }); }
    [Fact] public void HaveSameCount_counts_a_one_shot_sequence() { IEnumerable<string> Gen() { yield return "a"; yield return "b"; } new[] { 1, 2 }.Should().HaveSameCount(Gen()); }
    [Fact] public void HaveSameCount_message() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to contain the same number of items as the other collection (1 item), but found 2:\n  [0] 1\n  [1] 2", Message(() => xs.Should().HaveSameCount(new[] { "a" }))); }
    [Fact] public void HaveSameCount_null_subject_fails() { int[]? xs = null; Assert.Equal("Expected xs to contain the same number of items as the other collection, but found null.", Message(() => xs.Should().HaveSameCount(new[] { "a" }))); }
    [Fact] public void HaveSameCount_null_other_throws_ArgumentNullException() { Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Should().HaveSameCount<int>(null!)); }

    // NotBeEquivalentTo

    [Fact] public void NotBeEquivalentTo_passes() { new[] { 1, 2 }.Should().NotBeEquivalentTo(new[] { 3, 4 }); }
    [Fact] public void NotBeEquivalentTo_params_passes() { new[] { 1, 2 }.Should().NotBeEquivalentTo(3, 4); }
    [Fact] public void NotBeEquivalentTo_message_ignores_order() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs not to be equivalent to [2, 1], but it was.", Message(() => xs.Should().NotBeEquivalentTo(new[] { 2, 1 }))); }
    [Fact] public void NotBeEquivalentTo_with_options_can_make_a_difference_equal() { var xs = new List<Row> { new("A", 1) }; Assert.Throws<VouchException>(() => xs.Should().NotBeEquivalentTo(new[] { new Row("A", 9) }, o => o.Excluding(r => r.Qty))); xs.Should().NotBeEquivalentTo(new[] { new Row("A", 9) }); }
    [Fact] public void NotBeEquivalentTo_null_subject_fails() { int[]? xs = null; Assert.Equal("Expected xs not to be equivalent to [1], but found null.", Message(() => xs.Should().NotBeEquivalentTo(new[] { 1 }))); }
    [Fact] public void NotBeEquivalentTo_with_reason() { var xs = new[] { 1 }; Assert.Equal("Expected xs not to be equivalent to [1], but it was. Because it differs.", Message(() => xs.Should().NotBeEquivalentTo(new[] { 1 }, "it differs"))); }

    // IntersectWith / NotIntersectWith

    [Fact] public void IntersectWith_passes() { new[] { 1, 2 }.Should().IntersectWith(new[] { 2, 3 }); }
    [Fact] public void IntersectWith_passes_on_large_inputs() { new[] { 1, 99999 }.Should().IntersectWith(Enumerable.Range(50000, 60000).ToArray()); }
    [Fact] public void IntersectWith_message() { var xs = new[] { 1, 2 }; Assert.Equal("Expected xs to intersect with [3, 4], but they have no common items.", Message(() => xs.Should().IntersectWith(new[] { 3, 4 }))); }
    [Fact] public void IntersectWith_null_subject_fails() { int[]? xs = null; Assert.Equal("Expected xs to intersect with [3], but found null.", Message(() => xs.Should().IntersectWith(new[] { 3 }))); }
    [Fact] public void IntersectWith_with_reason() { var xs = new[] { 1 }; Assert.Equal("Expected xs to intersect with [3], but they have no common items. Because overlap is needed.", Message(() => xs.Should().IntersectWith(new[] { 3 }, "overlap is needed"))); }
    [Fact] public void NotIntersectWith_passes_and_has_a_reason_overload() { new[] { 1, 2 }.Should().NotIntersectWith(new[] { 3 }, "disjoint"); }
    [Fact] public void NotIntersectWith_with_reason_fails() { var xs = new[] { 1, 2 }; Assert.Throws<VouchException>(() => xs.Should().NotIntersectWith(new[] { 2 }, "disjoint")); }
    [Fact] public void NotIntersectWith_null_subject_fails() { int[]? xs = null; Assert.Throws<VouchException>(() => xs.Should().NotIntersectWith(new[] { 2 }, "disjoint")); }

    // NotContainNulls

    [Fact] public void NotContainNulls_passes() { new[] { "a", "" }.Should().NotContainNulls(); new string?[0].Should().NotContainNulls(); }
    [Fact] public void NotContainNulls_message() { var xs = new string?[] { "a", null, "b", null }; Assert.Equal("Expected xs not to contain nulls, but found 2:\n  [1] null\n  [3] null", Message(() => xs.Should().NotContainNulls())); }
    [Fact] public void NotContainNulls_null_subject_fails() { string?[]? xs = null; Assert.Equal("Expected xs not to contain nulls, but found null.", Message(() => xs.Should().NotContainNulls())); }
    [Fact] public void NotContainNulls_with_reason() { var xs = new string?[] { null }; Assert.Throws<VouchException>(() => xs.Should().NotContainNulls("nulls are bugs")); }

    // ThenBeInAscendingOrder / ThenBeInDescendingOrder

    [Fact] public void ThenBeInAscendingOrder_passes() { new[] { "a1", "a2", "b0" }.Should().BeInAscendingOrder(s => s[0]).ThenBeInAscendingOrder(s => s[1]); }
    [Fact] public void ThenBeInAscendingOrder_only_compares_items_that_tied_on_earlier_keys() { new[] { "a9", "b0" }.Should().BeInAscendingOrder(s => s[0]).ThenBeInAscendingOrder(s => s[1]); }
    [Fact] public void ThenBeInAscendingOrder_message() { var xs = new[] { "ab", "aa" }; Assert.Equal("Expected xs to be in ascending order, but [1] \"aa\" comes after [0] \"ab\", among items with equal earlier keys.", Message(() => xs.Should().BeInAscendingOrder(s => s[0]).ThenBeInAscendingOrder(s => s[1]))); }
    [Fact] public void ThenBeInDescendingOrder_passes() { new[] { "b2", "b1", "a5" }.Should().BeInDescendingOrder(s => s[0]).ThenBeInDescendingOrder(s => s[1]); }
    [Fact] public void ThenBeInDescendingOrder_message() { var xs = new[] { "b1", "b2" }; Assert.Equal("Expected xs to be in descending order, but [1] \"b2\" comes after [0] \"b1\", among items with equal earlier keys.", Message(() => xs.Should().BeInDescendingOrder(s => s[0]).ThenBeInDescendingOrder(s => s[1]))); }
    [Fact] public void ThenBe_can_mix_directions_and_chain_a_third_key() { new[] { "a2x", "a1x", "a1y", "b0a" }.Should().BeInAscendingOrder(s => s[0]).ThenBeInDescendingOrder(s => s[1]).ThenBeInAscendingOrder(s => s[2]); }
    [Fact] public void ThenBe_third_key_only_compares_items_tied_on_both_earlier_keys() { var xs = new[] { "a1y", "a1x" }; Assert.Throws<VouchException>(() => xs.Should().BeInAscendingOrder(s => s[0]).ThenBeInAscendingOrder(s => s[1]).ThenBeInAscendingOrder(s => s[2])); new[] { "a1y", "a2x" }.Should().BeInAscendingOrder(s => s[0]).ThenBeInAscendingOrder(s => s[1]).ThenBeInAscendingOrder(s => s[2]); }
    [Fact] public void ThenBe_And_returns_the_assertions() { var ordered = new[] { "a1", "a2" }.Should().BeInAscendingOrder(s => s[0]).ThenBeInAscendingOrder(s => s[1]); ordered.And.HaveCount(2); }
    [Fact] public void BeInAscendingOrder_by_key_And_returns_the_assertions() { new[] { "a", "b" }.Should().BeInAscendingOrder(s => s).And.HaveCount(2); }
    [Fact] public void ThenBe_null_key_selector_throws_ArgumentNullException() { var ordered = new[] { "a" }.Should().BeInAscendingOrder(s => s); Assert.Throws<ArgumentNullException>(() => ordered.ThenBeInAscendingOrder<char>(null!)); }
    [Fact]
    public void ThenBe_on_a_null_subject_records_only_the_null_failure()
    {
        string[]? xs = null;
        var ex = Assert.Throws<VouchException>(() =>
        {
            using var scope = new AssertionScope();
            xs.Should().BeInAscendingOrder(s => s[0]).ThenBeInAscendingOrder(s => s[1]);
        });
        Assert.Contains("found null", ex.Message);
        Assert.DoesNotContain("\n\n", ex.Message);
    }

    [Fact]
    public void ThenBe_keys_are_read_only_for_tied_groups()
    {
        var xs = new[] { "a", "b", "c" };
        xs.Should().BeInAscendingOrder(s => s)
            .ThenBeInAscendingOrder<int>(s => throw new InvalidOperationException("second key read for an untied pair"));
    }

    [Fact]
    public void Failing_ThenBe_inside_a_scope_is_recorded()
    {
        var xs = new[] { "a2", "a1" };
        var ex = Assert.Throws<VouchException>(() =>
        {
            using var scope = new AssertionScope();
            xs.Should().BeInAscendingOrder(s => s[0]).ThenBeInAscendingOrder(s => s[1]);
        });
        Assert.NotEmpty(ex.Message);
    }

    [Fact]
    public void Failing_AllBe_inside_a_scope_is_recorded()
    {
        var xs = new[] { 1, 2 };
        var ex = Assert.Throws<VouchException>(() =>
        {
            using var scope = new AssertionScope();
            xs.Should().AllBe(1);
        });
        Assert.NotEmpty(ex.Message);
    }

    [Fact]
    public void Failing_Satisfy_inside_a_scope_is_recorded()
    {
        var xs = new[] { 1, 2 };
        var ex = Assert.Throws<VouchException>(() =>
        {
            using var scope = new AssertionScope();
            xs.Should().Satisfy(x => x > 5, x => x > 6);
        });
        Assert.NotEmpty(ex.Message);
    }

    [Fact]
    public void Ordering_result_converts_to_AndConstraint()
    {
        var xs = new[] { "a", "b" };
        AndConstraint<GenericCollectionAssertions<string>> c = xs.Should().BeInAscendingOrder(s => s);
        c.And.HaveCount(2);
    }

    [Fact]
    public void AllBeOfType_exact_mode_unwraps_nullable()
    {
        new object[] { 1, 2 }.Should().AllBeOfType<int?>();
        new object[] { 1, 2 }.Should().AllBeAssignableTo<int>();
    }

    [Fact]
    public void HaveSameCount_null_subject_does_not_enumerate_other()
    {
        IEnumerable<int>? xs = null;
        var enumerated = false;
        IEnumerable<int> Lazy() { enumerated = true; yield return 1; }
        var ex = Assert.Throws<VouchException>(() => xs.Should().HaveSameCount(Lazy()));
        Assert.Contains("found null", ex.Message);
        Assert.False(enumerated);
    }

    [Fact]
    public void HaveSameCount_counts_a_lazy_other()
    {
        IEnumerable<int> Lazy() { yield return 1; yield return 2; }
        new[] { 5, 6 }.Should().HaveSameCount(Lazy());
        Assert.Throws<VouchException>(() => new[] { 5 }.Should().HaveSameCount(Lazy()));
    }

    [Fact]
    public void HaveSameCount_caps_the_other_collection()
    {
        // Enumerable.Range knows its count, so use an iterator to reach the capped path.
        static IEnumerable<int> Endless(int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return i;
            }
        }

        var ex = Assert.Throws<VouchException>(() => new[] { 1 }.Should().HaveSameCount(Endless(Equivalency.MaxItems + 1)));
        Assert.Contains("more than", ex.Message);
    }

    [Fact]
    public void Count_checks_past_the_item_cap_on_a_lazy_sequence()
    {
        var over = Equivalency.MaxItems + 1;
        Enumerable.Range(0, over).Should().NotHaveCount(5);
        Enumerable.Range(0, over).Should().HaveCountLessThan(over + 1);
        Enumerable.Range(0, over).Should().HaveCountLessThanOrEqualTo(over);
        Assert.Throws<VouchException>(() => Enumerable.Range(0, over).Should().NotHaveCount(over));
        Assert.Throws<VouchException>(() => Enumerable.Range(0, over).Should().HaveCountLessThan(5));
        Assert.Throws<VouchException>(() => Enumerable.Range(0, over).Should().HaveCountLessThanOrEqualTo(5));
    }

    [Fact] public void Satisfy_finds_a_matching_that_needs_reassignment() => new[] { 6, 1 }.Should().Satisfy(x => x > 0, x => x > 5);

    [Fact]
    public void Satisfy_refuses_more_than_the_size_guard()
    {
        var items = Enumerable.Range(0, 10_001).ToArray();
        var predicates = Enumerable.Repeat<System.Linq.Expressions.Expression<Func<int, bool>>>(_ => true, 10_001).ToArray();
        var ex = Assert.Throws<VouchException>(() => items.Should().Satisfy(predicates));
        Assert.Contains("10000", ex.Message.Replace(",", "").Replace("_", ""));
    }

    [Fact] public void ThenBe_on_a_null_subject_fails_at_the_first_ordering() { string[]? xs = null; Assert.Throws<VouchException>(() => xs.Should().BeInAscendingOrder(s => s[0]).ThenBeInAscendingOrder(s => s[1])); }
}
