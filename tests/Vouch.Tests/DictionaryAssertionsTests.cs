using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using Xunit;

namespace Vouch.Tests;

public class DictionaryAssertionsTests
{
    private static string Message(Action act) => Assert.Throws<VouchException>(act).Message;

    private static Dictionary<string, int> Sample() => new() { ["a"] = 1, ["b"] = 2 };

    [Fact] public void Dictionary_resolves() => Assert.IsType<DictionaryAssertions<string, int>>(Sample().Should());
    [Fact] public void IDictionary_resolves() { IDictionary<string, int> d = Sample(); Assert.IsType<DictionaryAssertions<string, int>>(d.Should()); }
    [Fact] public void IReadOnlyDictionary_resolves() { IReadOnlyDictionary<string, int> d = Sample(); Assert.IsType<DictionaryAssertions<string, int>>(d.Should()); }

    [Fact] public void ContainKey_passes() => Sample().Should().ContainKey("a");
    [Fact] public void ContainKey_fails() => Assert.Throws<VouchException>(() => Sample().Should().ContainKey("z"));
    [Fact] public void ContainKey_message() { var d = Sample(); Assert.Equal("Expected d to contain key \"z\", but it did not. It has:\n  [0] [\"a\"] = 1\n  [1] [\"b\"] = 2", Message(() => d.Should().ContainKey("z"))); }
    [Fact] public void ContainKey_Which_is_the_value() => Assert.Equal(2, new Dictionary<string, int> { ["a"] = 2 }.Should().ContainKey("a").Which);
    [Fact] public void ContainKey_works_on_IDictionary_and_IReadOnlyDictionary() { IDictionary<string, int> d = Sample(); IReadOnlyDictionary<string, int> r = Sample(); Assert.Equal(2, d.Should().ContainKey("b").Which); Assert.Equal(1, r.Should().ContainKey("a").Which); Assert.Throws<VouchException>(() => r.Should().ContainKey("z")); }
    [Fact] public void ContainKey_null_key_throws() => Assert.Throws<ArgumentNullException>(() => Sample().Should().ContainKey(null!));
    [Fact] public void ContainKey_null_subject_fails() { Dictionary<string, int>? d = null; Assert.Equal("Expected d to contain key \"a\", but found null.", Message(() => d.Should().ContainKey("a"))); }

    [Fact] public void NotContainKey_passes() => Sample().Should().NotContainKey("z");
    [Fact] public void NotContainKey_fails() => Assert.Throws<VouchException>(() => Sample().Should().NotContainKey("a"));
    [Fact] public void NotContainKey_message() { var d = Sample(); Assert.Equal("Expected d not to contain key \"a\", but found it with value 1.", Message(() => d.Should().NotContainKey("a"))); }
    [Fact] public void NotContainKey_null_key_throws() => Assert.Throws<ArgumentNullException>(() => Sample().Should().NotContainKey(null!));

    [Fact] public void ContainKeys_passes() => Sample().Should().ContainKeys("a", "b");
    [Fact] public void ContainKeys_fails() => Assert.Throws<VouchException>(() => Sample().Should().ContainKeys("a", "z"));
    [Fact] public void ContainKeys_message() { var d = Sample(); Assert.Equal("Expected d to contain keys [\"a\", \"y\", \"z\"], but it is missing [\"y\", \"z\"]. It has:\n  [0] [\"a\"] = 1\n  [1] [\"b\"] = 2", Message(() => d.Should().ContainKeys("a", "y", "z"))); }
    [Fact] public void ContainKeys_null_array_throws() => Assert.Throws<ArgumentNullException>(() => Sample().Should().ContainKeys(null!));
    [Fact] public void ContainKeys_null_element_throws() => Assert.Throws<ArgumentNullException>(() => Sample().Should().ContainKeys("a", null!));

    [Fact] public void Dictionary_gets_collection_methods_too() => new Dictionary<string, int>().Should().BeEmpty();
    [Fact] public void Dictionary_HaveCount() => Sample().Should().HaveCount(2).And.ContainKey("a");
    [Fact] public void Dictionary_BeEmpty_message() { var d = Sample(); Assert.Equal("Expected d to be empty, but found 2 items:\n  [0] [\"a\"] = 1\n  [1] [\"b\"] = 2", Message(() => d.Should().BeEmpty())); }
    [Fact] public void Dictionary_Contain_pair() => Sample().Should().Contain(new KeyValuePair<string, int>("a", 1));
    [Fact] public void Dictionary_Contain_predicate_Which_is_the_pair() => Assert.Equal("b", Sample().Should().Contain(kv => kv.Value == 2).Which.Key);
    [Fact] public void Dictionary_null_BeNull_passes() { IDictionary<string, int>? d = null; d.Should().BeNull(); }
    [Fact] public void Dictionary_null_HaveCount_fails() { IReadOnlyDictionary<string, int>? d = null; Assert.Equal("Expected d to contain 1 item, but found null.", Message(() => d.Should().HaveCount(1))); }

    [Fact] public void ConcurrentDictionary_compiles_and_works() => new ConcurrentDictionary<string, int>(Sample()).Should().ContainKey("a");
    [Fact] public void ImmutableDictionary_compiles_and_works() => Sample().ToImmutableDictionary().Should().ContainKey("a");
    [Fact] public void SortedDictionary_compiles_and_works() => new SortedDictionary<string, int>(Sample()).Should().ContainKey("a");
    [Fact] public void ReadOnlyDictionary_compiles_and_works() => new ReadOnlyDictionary<string, int>(Sample()).Should().ContainKey("a");
    [Fact] public void SortedList_compiles_and_works() => new SortedList<string, int>(Sample()).Should().ContainKey("a");
    [Fact] public void Plain_pair_sequence_works_by_scan() { IEnumerable<KeyValuePair<string, int>> pairs = Sample().ToList(); Assert.Equal(2, pairs.Should().ContainKey("b").Which); Assert.Throws<VouchException>(() => pairs.Should().ContainKey("z")); }
    [Fact] public void List_of_ints_still_resolves_to_collection_assertions() => Assert.IsType<GenericCollectionAssertions<int>>(new List<int>().Should());
    [Fact] public void ContainKey_message_on_empty() { var d = new Dictionary<string, int>(); Assert.Equal("Expected d to contain key \"a\", but it was empty.", Message(() => d.Should().ContainKey("a"))); }
    [Fact] public void ContainKeys_message_on_empty() { var d = new Dictionary<string, int>(); Assert.Equal("Expected d to contain keys [\"a\"], but it is missing [\"a\"].", Message(() => d.Should().ContainKeys("a"))); }
    [Fact] public void ContainKey_sees_the_live_dictionary() { var d = Sample(); var a = d.Should(); d["z"] = 9; a.ContainKey("z"); }

    [Fact] public void ContainKey_WhoseValue_is_the_value() => Assert.Equal(2, Sample().Should().ContainKey("b").WhoseValue);
    [Fact] public void ContainKey_WhoseValue_chains_into_an_assertion() => Sample().Should().ContainKey("b").WhoseValue.Should().Be(2);
    [Fact] public void ContainKey_WhoseValue_matches_Which() { var found = Sample().Should().ContainKey("a"); Assert.Equal(found.Which, found.WhoseValue); }

    private static List<KeyValuePair<string, int>> Pairs(params (string Key, int Value)[] pairs) => pairs.Select(p => new KeyValuePair<string, int>(p.Key, p.Value)).ToList();

    private const string Has = "\n  [0] [\"a\"] = 1\n  [1] [\"b\"] = 2";

    [Fact] public void NotContainKeys_passes() => Sample().Should().NotContainKeys("x", "y");
    [Fact] public void NotContainKeys_fails() => Assert.Throws<VouchException>(() => Sample().Should().NotContainKeys("x", "a"));
    [Fact] public void NotContainKeys_message() { var d = Sample(); Assert.Equal("Expected d not to contain keys [\"a\", \"z\", \"b\"], but found [\"a\", \"b\"].", Message(() => d.Should().NotContainKeys("a", "z", "b"))); }
    [Fact] public void NotContainKeys_null_subject_fails() { Dictionary<string, int>? d = null; Assert.Equal("Expected d not to contain keys [\"a\"], but found null.", Message(() => d.Should().NotContainKeys("a"))); }
    [Fact] public void NotContainKeys_null_array_throws() => Assert.Throws<ArgumentNullException>(() => Sample().Should().NotContainKeys(null!));
    [Fact] public void NotContainKeys_null_element_throws() => Assert.Throws<ArgumentNullException>(() => Sample().Should().NotContainKeys("a", null!));

    [Fact] public void ContainValue_passes() => Sample().Should().ContainValue(2);
    [Fact] public void ContainValue_Which_is_the_value() => Assert.Equal(2, Sample().Should().ContainValue(2).Which);
    [Fact] public void ContainValue_fails() => Assert.Throws<VouchException>(() => Sample().Should().ContainValue(5));
    [Fact] public void ContainValue_message() { var d = Sample(); Assert.Equal("Expected d to contain value 5, but it did not. It has:" + Has, Message(() => d.Should().ContainValue(5))); }
    [Fact] public void ContainValue_message_on_empty() { var d = new Dictionary<string, int>(); Assert.Equal("Expected d to contain value 5, but it was empty.", Message(() => d.Should().ContainValue(5))); }
    [Fact] public void ContainValue_null_subject_fails() { Dictionary<string, int>? d = null; Assert.Equal("Expected d to contain value 5, but found null.", Message(() => d.Should().ContainValue(5))); }
    [Fact] public void ContainValue_with_because_message() { var d = Sample(); Assert.Equal("Expected d to contain value 5, but it did not. It has:" + Has + "\nBecause it matters.", Message(() => d.Should().ContainValue(5, "it matters"))); }
    [Fact] public void ContainValue_allows_a_null_value() => new Dictionary<string, string?> { ["a"] = null }.Should().ContainValue(null);

    [Fact] public void ContainValues_passes() => Sample().Should().ContainValues(1, 2);
    [Fact] public void ContainValues_enumerable_passes() => Sample().Should().ContainValues(new List<int> { 2 });
    [Fact] public void ContainValues_fails() => Assert.Throws<VouchException>(() => Sample().Should().ContainValues(1, 7));
    [Fact] public void ContainValues_message() { var d = Sample(); Assert.Equal("Expected d to contain values [1, 7, 8], but it is missing [7, 8]. It has:" + Has, Message(() => d.Should().ContainValues(1, 7, 8))); }
    [Fact] public void ContainValues_null_subject_fails() { Dictionary<string, int>? d = null; Assert.Equal("Expected d to contain values [1], but found null.", Message(() => d.Should().ContainValues(1))); }
    [Fact] public void ContainValues_null_array_throws() => Assert.Throws<ArgumentNullException>(() => Sample().Should().ContainValues((int[])null!));

    [Fact] public void NotContainValue_passes() => Sample().Should().NotContainValue(5);
    [Fact] public void NotContainValue_fails() => Assert.Throws<VouchException>(() => Sample().Should().NotContainValue(1));
    [Fact] public void NotContainValue_message() { var d = Sample(); Assert.Equal("Expected d not to contain value 1, but found it with key \"a\".", Message(() => d.Should().NotContainValue(1))); }
    [Fact] public void NotContainValue_null_subject_fails() { Dictionary<string, int>? d = null; Assert.Equal("Expected d not to contain value 1, but found null.", Message(() => d.Should().NotContainValue(1))); }

    [Fact] public void NotContainValues_passes() => Sample().Should().NotContainValues(5, 6);
    [Fact] public void NotContainValues_enumerable_passes() => Sample().Should().NotContainValues(new List<int> { 5 });
    [Fact] public void NotContainValues_fails() => Assert.Throws<VouchException>(() => Sample().Should().NotContainValues(5, 1));
    [Fact] public void NotContainValues_message() { var d = Sample(); Assert.Equal("Expected d not to contain values [1, 5, 2], but found [1, 2].", Message(() => d.Should().NotContainValues(1, 5, 2))); }
    [Fact] public void NotContainValues_null_subject_fails() { Dictionary<string, int>? d = null; Assert.Equal("Expected d not to contain values [1], but found null.", Message(() => d.Should().NotContainValues(1))); }
    [Fact] public void NotContainValues_null_array_throws() => Assert.Throws<ArgumentNullException>(() => Sample().Should().NotContainValues((int[])null!));

    [Fact] public void Contain_pair_passes() => Sample().Should().Contain(new KeyValuePair<string, int>("a", 1));
    [Fact] public void Contain_pair_fails() => Assert.Throws<VouchException>(() => Sample().Should().Contain(new KeyValuePair<string, int>("a", 5)));
    [Fact] public void Contain_pair_missing_key_message() { var d = Sample(); Assert.Equal("Expected d to contain [\"z\"] = 1, but the key was not found. It has:" + Has, Message(() => d.Should().Contain(new KeyValuePair<string, int>("z", 1)))); }
    [Fact] public void Contain_pair_wrong_value_message() { var d = Sample(); Assert.Equal("Expected d to contain [\"a\"] = 5, but found value 1 for that key.", Message(() => d.Should().Contain(new KeyValuePair<string, int>("a", 5)))); }
    [Fact] public void Contain_pair_null_subject_fails() { Dictionary<string, int>? d = null; Assert.Equal("Expected d to contain [\"a\"] = 1, but found null.", Message(() => d.Should().Contain(new KeyValuePair<string, int>("a", 1)))); }
    [Fact] public void Contain_pair_uses_the_dictionary_key_comparer() => new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["a"] = 1 }.Should().Contain(new KeyValuePair<string, int>("A", 1));
    [Fact] public void Contain_pair_Which_is_the_pair() => Assert.Equal(1, Sample().Should().Contain(new KeyValuePair<string, int>("a", 1)).Which.Value);

    [Fact] public void Contain_key_and_value_passes() => Sample().Should().Contain("a", 1).And.Contain("b", 2);
    [Fact] public void Contain_key_and_value_fails() => Assert.Throws<VouchException>(() => Sample().Should().Contain("a", 5));
    [Fact] public void Contain_key_and_value_missing_key_message() { var d = Sample(); Assert.Equal("Expected d to contain [\"z\"] = 1, but the key was not found. It has:" + Has, Message(() => d.Should().Contain("z", 1))); }
    [Fact] public void Contain_key_and_value_wrong_value_message() { var d = Sample(); Assert.Equal("Expected d to contain [\"a\"] = 5, but found value 1 for that key.", Message(() => d.Should().Contain("a", 5))); }
    [Fact] public void Contain_key_and_value_null_subject_fails() { Dictionary<string, int>? d = null; Assert.Equal("Expected d to contain [\"a\"] = 1, but found null.", Message(() => d.Should().Contain("a", 1))); }
    [Fact] public void Contain_key_and_value_null_key_throws() => Assert.Throws<ArgumentNullException>(() => Sample().Should().Contain(null!, 1));
    [Fact] public void Contain_key_and_value_with_because_message() { var d = Sample(); Assert.Equal("Expected d to contain [\"a\"] = 5, but found value 1 for that key. Because it matters.", Message(() => d.Should().Contain("a", 5, "it matters"))); }

    [Fact] public void NotContain_pair_passes() { Sample().Should().NotContain(new KeyValuePair<string, int>("a", 5)); Sample().Should().NotContain(new KeyValuePair<string, int>("z", 1)); }
    [Fact] public void NotContain_pair_fails() => Assert.Throws<VouchException>(() => Sample().Should().NotContain(new KeyValuePair<string, int>("a", 1)));
    [Fact] public void NotContain_pair_message() { var d = Sample(); Assert.Equal("Expected d not to contain [\"a\"] = 1, but found it.", Message(() => d.Should().NotContain(new KeyValuePair<string, int>("a", 1)))); }
    [Fact] public void NotContain_pair_null_subject_fails() { Dictionary<string, int>? d = null; Assert.Equal("Expected d not to contain [\"a\"] = 1, but found null.", Message(() => d.Should().NotContain(new KeyValuePair<string, int>("a", 1)))); }

    [Fact] public void NotContain_key_and_value_passes() { Sample().Should().NotContain("a", 5); Sample().Should().NotContain("z", 1); }
    [Fact] public void NotContain_key_and_value_fails() => Assert.Throws<VouchException>(() => Sample().Should().NotContain("a", 1));
    [Fact] public void NotContain_key_and_value_message() { var d = Sample(); Assert.Equal("Expected d not to contain [\"b\"] = 2, but found it.", Message(() => d.Should().NotContain("b", 2))); }
    [Fact] public void NotContain_key_and_value_null_subject_fails() { Dictionary<string, int>? d = null; Assert.Equal("Expected d not to contain [\"a\"] = 1, but found null.", Message(() => d.Should().NotContain("a", 1))); }
    [Fact] public void NotContain_key_and_value_null_key_throws() => Assert.Throws<ArgumentNullException>(() => Sample().Should().NotContain(null!, 1));

    [Fact] public void Equal_passes_in_any_order() => Sample().Should().Equal(Pairs(("b", 2), ("a", 1)));
    [Fact] public void Equal_params_passes_in_any_order() => Sample().Should().Equal(new KeyValuePair<string, int>("b", 2), new KeyValuePair<string, int>("a", 1));
    [Fact] public void Equal_accepts_another_dictionary() => Sample().Should().Equal(new Dictionary<string, int> { ["b"] = 2, ["a"] = 1 });
    [Fact] public void Equal_fails() => Assert.Throws<VouchException>(() => Sample().Should().Equal(Pairs(("a", 1))));
    [Fact] public void Equal_message() { var d = Sample(); Assert.Equal("Expected d to equal [[\"a\"] = 1, [\"b\"] = 3, [\"c\"] = 4], but it differs:\n  key \"b\" has value 2, expected 3\n  missing key \"c\" with value 4", Message(() => d.Should().Equal(Pairs(("a", 1), ("b", 3), ("c", 4))))); }
    [Fact] public void Equal_message_with_extra_key() { var d = Sample(); Assert.Equal("Expected d to equal [[\"a\"] = 1], but it differs:\n  unexpected key \"b\" with value 2", Message(() => d.Should().Equal(Pairs(("a", 1))))); }
    [Fact] public void Equal_duplicate_expected_keys_do_not_hide_an_extra_key() => Assert.Throws<VouchException>(() => Sample().Should().Equal(Pairs(("a", 1), ("a", 1))));
    [Fact] public void Equal_null_subject_fails() { Dictionary<string, int>? d = null; Assert.Equal("Expected d to equal [[\"a\"] = 1], but found null.", Message(() => d.Should().Equal(Pairs(("a", 1))))); }
    [Fact] public void Equal_null_expected_throws() => Assert.Throws<ArgumentNullException>(() => Sample().Should().Equal((IEnumerable<KeyValuePair<string, int>>)null!));
    [Fact] public void Equal_uses_the_dictionary_key_comparer() => new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["a"] = 1 }.Should().Equal(Pairs(("A", 1)));
    [Fact] public void Equal_with_because_message() { var d = Sample(); Assert.Equal("Expected d to equal [[\"a\"] = 1], but it differs:\n  unexpected key \"b\" with value 2\nBecause it matters.", Message(() => d.Should().Equal(Pairs(("a", 1)), "it matters"))); }

    [Fact] public void NotEqual_passes() { Sample().Should().NotEqual(Pairs(("a", 1))); Sample().Should().NotEqual(Pairs(("a", 1), ("b", 3))); Sample().Should().NotEqual(Pairs(("a", 1), ("c", 2))); }
    [Fact] public void NotEqual_fails() => Assert.Throws<VouchException>(() => Sample().Should().NotEqual(Pairs(("b", 2), ("a", 1))));
    [Fact] public void NotEqual_message() { var d = Sample(); Assert.Equal("Expected d not to equal [[\"b\"] = 2, [\"a\"] = 1], but it did.", Message(() => d.Should().NotEqual(Pairs(("b", 2), ("a", 1))))); }
    [Fact] public void NotEqual_null_subject_fails() { Dictionary<string, int>? d = null; Assert.Equal("Expected d not to equal [[\"a\"] = 1], but found null.", Message(() => d.Should().NotEqual(Pairs(("a", 1))))); }
    [Fact] public void NotEqual_null_expected_throws() => Assert.Throws<ArgumentNullException>(() => Sample().Should().NotEqual(null!));

    [Fact] public void BeEquivalentTo_dictionary_with_options_is_inherited() => Sample().Should().BeEquivalentTo(new Dictionary<string, int> { ["b"] = 2, ["a"] = 1 }, o => o);
    [Fact] public void BeEquivalentTo_dictionary_with_options_fails_on_a_different_value() => Assert.Throws<VouchException>(() => Sample().Should().BeEquivalentTo(new Dictionary<string, int> { ["b"] = 3, ["a"] = 1 }, o => o));

    [Fact]
    public void New_methods_resolve_without_ambiguity_on_Dictionary_IDictionary_and_IReadOnlyDictionary()
    {
        IDictionary<string, int> mutable = Sample();
        IReadOnlyDictionary<string, int> readOnly = Sample();
        var pair = new KeyValuePair<string, int>("a", 1);
        var absent = new KeyValuePair<string, int>("z", 9);
        var same = Pairs(("a", 1), ("b", 2));
        var other = Pairs(("a", 1));

        Sample().Should().ContainValue(1).And.ContainValues(1, 2).And.NotContainValue(9).And.NotContainValues(8, 9)
            .And.Contain(pair).And.Contain("a", 1).And.NotContain(absent).And.NotContain("z", 9)
            .And.Equal(same).And.NotEqual(other).And.NotContainKeys("z");
        mutable.Should().ContainValue(1).And.ContainValues(1, 2).And.NotContainValue(9).And.NotContainValues(8, 9)
            .And.Contain(pair).And.Contain("a", 1).And.NotContain(absent).And.NotContain("z", 9)
            .And.Equal(same).And.NotEqual(other).And.NotContainKeys("z");
        readOnly.Should().ContainValue(1).And.ContainValues(1, 2).And.NotContainValue(9).And.NotContainValues(8, 9)
            .And.Contain(pair).And.Contain("a", 1).And.NotContain(absent).And.NotContain("z", 9)
            .And.Equal(same).And.NotEqual(other).And.NotContainKeys("z");
        Assert.Equal(1, mutable.Should().ContainValue(1).WhoseValue);
        Assert.Equal(2, readOnly.Should().ContainKey("b").WhoseValue);
    }

    private static IEnumerable<KeyValuePair<string, int>> Lazy(params (string Key, int Value)[] items)
    {
        foreach (var (key, value) in items)
        {
            yield return new KeyValuePair<string, int>(key, value);
        }
    }

    [Fact] public void Pair_sequence_subject_works_by_scan() => Lazy(("a", 1), ("b", 2)).Should().ContainValue(2).And.Contain("a", 1).And.Equal(Pairs(("b", 2), ("a", 1)));

    [Fact]
    public void Lazy_pair_sequence_Contain_matches_the_whole_pair_across_entries()
    {
        Lazy(("a", 1), ("a", 2)).Should().Contain("a", 2).And.Contain(new KeyValuePair<string, int>("a", 1));
        Assert.Throws<VouchException>(() => Lazy(("a", 1), ("a", 2)).Should().NotContain("a", 2));
        Lazy(("a", 1), ("a", 2)).Should().NotContain("a", 3);
    }

    [Fact]
    public void Equal_works_with_a_case_insensitive_SortedDictionary()
    {
        var subject = new SortedDictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["a"] = 1, ["b"] = 2 };
        subject.Should().Equal(Pairs(("A", 1), ("B", 2)));
        Assert.Throws<VouchException>(() => subject.Should().NotEqual(Pairs(("A", 1), ("B", 2))));
        Assert.Throws<VouchException>(() => subject.Should().Equal(Pairs(("A", 1))));
        Assert.Throws<VouchException>(() => subject.Should().Equal(Pairs(("A", 1), ("B", 3))));
    }

    [Fact]
    public void Equal_works_with_a_case_insensitive_ImmutableDictionary()
    {
        var subject = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 }.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
        subject.Should().Equal(Pairs(("A", 1), ("B", 2)));
    }

    [Fact]
    public void Equal_reports_a_null_key_as_missing()
    {
        var subject = new Dictionary<string, int> { ["a"] = 1 };
        var message = Message(() => subject.Should().Equal(new KeyValuePair<string, int>[] { new(null!, 1), new("a", 1) }));
        Assert.Contains("missing key null with value 1", message);
    }

    [Fact]
    public void Contain_and_NotContain_pair_with_a_null_key()
    {
        var subject = Sample();
        var pair = new KeyValuePair<string, int>(null!, 1);
        Assert.Throws<VouchException>(() => subject.Should().Contain(pair));
        subject.Should().NotContain(pair);
    }

    [Fact]
    public void Equal_with_a_duplicate_expected_key_throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => Sample().Should().Equal(Pairs(("a", 1), ("b", 2), ("a", 1))));
        Assert.Contains("\"a\"", ex.Message);
    }

    [Fact]
    public void NotContainKeys_reports_each_found_key_once()
    {
        var message = Message(() => Sample().Should().NotContainKeys("a", "a", "z"));
        Assert.Contains("but found [\"a\"].", message);
    }

    [Fact]
    public void NotContainKeys_and_ContainValues_with_because()
    {
        var keys = new[] { "z" };
        var values = new[] { 1 };
        Sample().Should().NotContainKeys(keys, "why").And.ContainValues(values, "why");
        Assert.Contains("because why", Message(() => Sample().Should().NotContainKeys(new[] { "a" }, "why")));
        Assert.Contains("because why", Message(() => Sample().Should().ContainValues(new[] { 9 }, "why")));
    }
}
