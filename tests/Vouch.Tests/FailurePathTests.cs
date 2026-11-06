using Xunit;

namespace Vouch.Tests;

public class FailurePathTests
{
    private sealed class Person
    {
        public string? Name { get; set; }
    }

    private static List<string> Record(Action act)
    {
        var recorded = new List<string>();
        var previous = Fail.Recorder;
        Fail.Recorder = ex => recorded.Add(ex.Message);
        try
        {
            act();
        }
        finally
        {
            Fail.Recorder = previous;
        }

        return recorded;
    }

    private static async Task<List<string>> RecordAsync(Func<Task> act)
    {
        var recorded = new List<string>();
        var previous = Fail.Recorder;
        Fail.Recorder = ex => recorded.Add(ex.Message);
        try
        {
            await act();
        }
        finally
        {
            Fail.Recorder = previous;
        }

        return recorded;
    }

    // The failing call must return normally under the recorder, record one failure,
    // and that failure must read exactly like the one the throwing path raises.
    private static void Same(Action act)
    {
        var thrown = Assert.Throws<VouchException>(act).Message;
        var recorded = Record(act);
        Assert.Equal(new[] { thrown }, recorded);
    }

    private static async Task SameAsync(Func<Task> act)
    {
        var thrown = (await Assert.ThrowsAsync<VouchException>(act)).Message;
        var recorded = await RecordAsync(act);
        Assert.Equal(new[] { thrown }, recorded);
    }

    [Fact]
    public void Recorder_is_off_by_default() => Assert.Null(Fail.Recorder);

    [Fact]
    public void Recorder_is_restored_after_recording()
    {
        Record(() => true.Should().BeFalse());
        Assert.Null(Fail.Recorder);
    }

    [Fact]
    public void Passing_assertion_records_nothing() => Assert.Empty(Record(() => true.Should().BeTrue()));

    [Fact] public void Bool() => Same(() => true.Should().BeFalse());
    [Fact] public void Bool_with_reason() => Same(() => true.Should().BeFalse("it matters"));
    [Fact] public void Nullable_bool_null() => Same(() => ((bool?)null).Should().BeTrue());
    [Fact] public void Nullable_bool_value() => Same(() => ((bool?)false).Should().BeTrue());

    [Fact] public void Object_null_subject() => Same(() => ((object?)null).Should().NotBeNull());
    [Fact] public void Object_not_null() => Same(() => new object().Should().BeNull());
    [Fact] public void Object_be() => Same(() => ((object)1).Should().Be(2));
    [Fact] public void Object_be_of_type_null() => Same(() => ((object?)null).Should().BeOfType<string>());
    [Fact] public void Object_be_assignable_null() => Same(() => ((object?)null).Should().BeAssignableTo<string>());

    [Fact] public void String_null_subject_start() => Same(() => ((string?)null).Should().StartWith("a"));
    [Fact] public void String_null_subject_contain() => Same(() => ((string?)null).Should().Contain("a"));
    [Fact] public void String_null_subject_not_be_empty() => Same(() => ((string?)null).Should().NotBeEmpty());
    [Fact] public void String_null_subject_have_length() => Same(() => ((string?)null).Should().HaveLength(3));
    [Fact] public void String_be() => Same(() => "abc".Should().Be("abd"));
    [Fact] public void String_be_with_reason() => Same(() => "abc".Should().Be("abd", "they must match"));
    [Fact] public void String_end_with() => Same(() => "abc".Should().EndWith("x"));

    [Fact] public void Numeric_int() => Same(() => 1.Should().Be(2));
    [Fact] public void Numeric_int_with_reason() => Same(() => 1.Should().BeGreaterThan(5, "five is the floor"));
    [Fact] public void Numeric_double() => Same(() => 1.5.Should().BeApproximately(3.0, 0.1));

    [Fact] public void Nullable_int_null() => Same(() => ((int?)null).Should().Be(1));
    [Fact] public void Nullable_int_has_value() => Same(() => ((int?)null).Should().NotBeNull());
    [Fact] public void Nullable_int_have_no_value() => Same(() => ((int?)4).Should().BeNull());

    [Fact] public void Date_time() => Same(() => new DateTime(2020, 1, 1).Should().BeAfter(new DateTime(2021, 1, 1)));
    [Fact] public void Date_time_nullable_null() => Same(() => ((DateTime?)null).Should().BeAfter(new DateTime(2021, 1, 1)));
    [Fact] public void Date_time_offset_nullable_null() => Same(() => ((DateTimeOffset?)null).Should().BeAfter(new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero)));
    [Fact] public void Date_only() => Same(() => new DateOnly(2020, 1, 1).Should().BeAfter(new DateOnly(2021, 1, 1)));
    [Fact] public void Date_only_nullable_null() => Same(() => ((DateOnly?)null).Should().BeAfter(new DateOnly(2021, 1, 1)));

    [Fact] public void Collection_null_be_empty() => Same(() => ((IEnumerable<int>?)null).Should().BeEmpty());
    [Fact] public void Collection_null_not_be_empty() => Same(() => ((IEnumerable<int>?)null).Should().NotBeEmpty());
    [Fact] public void Collection_null_have_count() => Same(() => ((IEnumerable<int>?)null).Should().HaveCount(1));
    [Fact] public void Collection_null_contain() => Same(() => ((IEnumerable<int>?)null).Should().Contain(1));
    [Fact] public void Collection_null_contain_predicate() => Same(() => ((IEnumerable<int>?)null).Should().Contain(x => x > 1));
    [Fact] public void Collection_null_contain_single() => Same(() => ((IEnumerable<int>?)null).Should().ContainSingle());
    [Fact] public void Collection_null_contain_single_predicate() => Same(() => ((IEnumerable<int>?)null).Should().ContainSingle(x => x > 1));
    [Fact] public void Collection_null_all_satisfy() => Same(() => ((IEnumerable<int>?)null).Should().AllSatisfy(x => { }));
    [Fact] public void Collection_contain_missing() => Same(() => new[] { 1, 2 }.Should().Contain(5));
    [Fact] public void Collection_contain_missing_with_reason() => Same(() => new[] { 1, 2 }.Should().Contain(5, "five is required"));
    [Fact] public void Collection_contain_single_many() => Same(() => new[] { 1, 2 }.Should().ContainSingle());
    [Fact] public void Collection_contain_single_none() => Same(() => Array.Empty<int>().Should().ContainSingle());
    [Fact] public void Collection_contain_predicate_none() => Same(() => new[] { 1, 2 }.Should().Contain(x => x > 5));
    [Fact] public void Collection_have_count() => Same(() => new[] { 1, 2 }.Should().HaveCount(3));

    [Fact] public void Dictionary_null_contain_key() => Same(() => ((Dictionary<string, int>?)null).Should().ContainKey("a"));
    [Fact] public void Dictionary_null_not_contain_key() => Same(() => ((Dictionary<string, int>?)null).Should().NotContainKey("a"));
    [Fact] public void Dictionary_null_contain_keys() => Same(() => ((Dictionary<string, int>?)null).Should().ContainKeys("a", "b"));
    [Fact] public void Dictionary_contain_key_missing() => Same(() => new Dictionary<string, int> { ["a"] = 1 }.Should().ContainKey("b"));
    [Fact] public void Dictionary_not_contain_key_present() => Same(() => new Dictionary<string, int> { ["a"] = 1 }.Should().NotContainKey("a"));

    [Fact] public void Action_null_throw() => Same(() => ((Action?)null).Should().Throw<InvalidOperationException>());
    [Fact] public void Action_null_not_throw() => Same(() => ((Action?)null).Should().NotThrow());
    [Fact] public void Action_does_not_throw() => Same(() => ((Action)(() => { })).Should().Throw<InvalidOperationException>());
    [Fact] public void Action_does_not_throw_with_reason() => Same(() => ((Action)(() => { })).Should().Throw<InvalidOperationException>("it must fail"));
    [Fact] public void Action_wrong_type() => Same(() => ((Action)(() => throw new ArgumentException("x"))).Should().Throw<InvalidOperationException>());
    [Fact] public void Action_throws_when_not_expected() => Same(() => ((Action)(() => throw new ArgumentException("x"))).Should().NotThrow());
    [Fact] public void Func_null_throw() => Same(() => ((Func<int>?)null).Should().Throw<InvalidOperationException>());
    [Fact] public void Func_null_not_throw() => Same(() => ((Func<int>?)null).Should().NotThrow());
    [Fact] public void Func_does_not_throw() => Same(() => ((Func<int>)(() => 1)).Should().Throw<InvalidOperationException>());

    [Fact]
    public void Which_on_missing_exception_is_default()
    {
        ExceptionAssertions<InvalidOperationException> result = default!;
        var recorded = Record(() => result = ((Action)(() => { })).Should().Throw<InvalidOperationException>());
        Assert.Single(recorded);
        Assert.Null(result.Which);
    }

    [Fact]
    public void Chained_message_check_on_missing_exception_adds_no_second_failure()
    {
        var recorded = Record(() => ((Action)(() => { })).Should().Throw<InvalidOperationException>().WithMessage("x*"));
        Assert.Single(recorded);
    }

    [Fact]
    public void Chained_parameter_name_check_on_missing_exception_adds_no_second_failure()
    {
        var recorded = Record(() => ((Action)(() => { })).Should().Throw<ArgumentException>().WithParameterName("p"));
        Assert.Single(recorded);
    }

    [Fact] public Task Async_null_throw() => SameAsync(async () => await ((Func<Task>?)null).Should().ThrowAsync<InvalidOperationException>());
    [Fact] public Task Async_null_not_throw() => SameAsync(() => ((Func<Task>?)null).Should().NotThrowAsync());
    [Fact] public Task Async_does_not_throw() => SameAsync(async () => await ((Func<Task>)(() => Task.CompletedTask)).Should().ThrowAsync<InvalidOperationException>());
    [Fact] public Task Async_does_not_throw_with_reason() => SameAsync(async () => await ((Func<Task>)(() => Task.CompletedTask)).Should().ThrowAsync<InvalidOperationException>("it must fail"));
    [Fact] public Task Async_wrong_type() => SameAsync(async () => await ((Func<Task>)(async () => { await Task.Yield(); throw new ArgumentException("x"); })).Should().ThrowAsync<InvalidOperationException>());
    [Fact] public Task Async_throws_when_not_expected() => SameAsync(() => ((Func<Task>)(async () => { await Task.Yield(); throw new ArgumentException("x"); })).Should().NotThrowAsync("it was fine"));
    [Fact] public Task Async_returns_null_task() => SameAsync(async () => await ((Func<Task>)(() => null!)).Should().ThrowAsync<InvalidOperationException>());
    [Fact] public Task Async_typed_null_throw() => SameAsync(async () => await ((Func<Task<int>>?)null).Should().ThrowAsync<InvalidOperationException>());
    [Fact] public Task Async_typed_null_not_throw() => SameAsync(async () => await ((Func<Task<int>>?)null).Should().NotThrowAsync());
    [Fact] public Task Value_task_null_throw() => SameAsync(async () => await ((Func<ValueTask>?)null).Should().ThrowAsync<InvalidOperationException>());
    [Fact] public Task Value_task_does_not_throw() => SameAsync(async () => await ((Func<ValueTask>)(() => default)).Should().ThrowAsync<InvalidOperationException>());

    [Fact]
    public async Task Async_reason_survives_the_await()
    {
        var recorded = await RecordAsync(async () => await ((Func<Task>)(async () => await Task.Delay(5))).Should().ThrowAsync<InvalidOperationException>("it must fail"));
        Assert.Single(recorded);
        Assert.EndsWith("Because it must fail.", recorded[0]);
    }

    [Fact]
    public void Reason_reaches_the_recorded_failure()
    {
        var recorded = Record(() => 1.Should().Be(2, "numbers matter"));
        Assert.Single(recorded);
        Assert.EndsWith("Because numbers matter.", recorded[0]);
    }

    [Fact]
    public void Reason_is_cleared_after_a_recorded_failure()
    {
        var recorded = Record(() =>
        {
            1.Should().Be(2, "first reason");
            1.Should().Be(3);
        });
        Assert.Equal(2, recorded.Count);
        Assert.DoesNotContain("first reason", recorded[1]);
    }

    [Fact]
    public void Nested_assertion_keeps_its_own_reason_and_does_not_leak()
    {
        var withReason = Assert.Throws<VouchException>(() => new[] { 1 }.Should().AllSatisfy(x => x.Should().Be(2, "inner reason"))).Message;
        Assert.Contains("inner reason", withReason);

        var recorded = Record(() =>
        {
            new[] { 1 }.Should().AllSatisfy(x => x.Should().Be(2, "inner reason"));
            1.Should().Be(3);
        });
        Assert.Equal(2, recorded.Count);
        Assert.Contains("inner reason", recorded[0]);
        Assert.DoesNotContain("inner reason", recorded[1]);
    }

    [Fact]
    public void Nested_failure_in_all_satisfy_records_once_and_matches_throwing_path()
        => Same(() => new[] { 1, 2 }.Should().AllSatisfy(x => x.Should().Be(0)));

    // After a recorded failure the returned constraint must be usable: chaining another
    // failing assertion records a second failure that names the original subject.
    private static void Chains(string subjectName, Action act)
    {
        var recorded = Record(act);
        Assert.Equal(2, recorded.Count);
        Assert.Contains(subjectName, recorded[1]);
    }

    [Fact] public void Chain_object_be_of_type() { object? subjectObject = null; Chains("subjectObject", () => subjectObject.Should().BeOfType<string>().And.NotBeNull()); }
    [Fact] public void Chain_object_be_assignable_null() { object? subjectObject = null; Chains("subjectObject", () => subjectObject.Should().BeAssignableTo<string>().And.NotBeNull()); }
    [Fact] public void Chain_object_be_assignable_wrong_type() { object? subjectObject = 1; Chains("subjectObject", () => subjectObject.Should().BeAssignableTo<string>().And.BeNull()); }
    [Fact] public void Chain_object_match_null() { object? subjectObject = null; Chains("subjectObject", () => subjectObject.Should().Match<string>(s => s.Length == 1).And.NotBeNull()); }
    [Fact] public void Chain_object_match_wrong_type() { object? subjectObject = 1; Chains("subjectObject", () => subjectObject.Should().Match<string>(s => s.Length == 1).And.BeNull()); }

    [Fact] public void Chain_collection_contain_null() { IEnumerable<int>? subjectItems = null; Chains("subjectItems", () => subjectItems.Should().Contain(1).And.NotBeNull()); }
    [Fact] public void Chain_collection_contain_missing() { var subjectItems = new[] { 1, 2 }; Chains("subjectItems", () => subjectItems.Should().Contain(5).And.BeEmpty()); }
    [Fact] public void Chain_collection_contain_predicate_null() { IEnumerable<int>? subjectItems = null; Chains("subjectItems", () => subjectItems.Should().Contain(x => x > 1).And.NotBeNull()); }
    [Fact] public void Chain_collection_contain_predicate_missing() { var subjectItems = new[] { 1, 2 }; Chains("subjectItems", () => subjectItems.Should().Contain(x => x > 5).And.BeEmpty()); }
    [Fact] public void Chain_collection_contain_single_null() { IEnumerable<int>? subjectItems = null; Chains("subjectItems", () => subjectItems.Should().ContainSingle().And.NotBeNull()); }
    [Fact] public void Chain_collection_contain_single_many() { var subjectItems = new[] { 1, 2 }; Chains("subjectItems", () => subjectItems.Should().ContainSingle().And.BeEmpty()); }
    [Fact] public void Chain_collection_contain_single_predicate_null() { IEnumerable<int>? subjectItems = null; Chains("subjectItems", () => subjectItems.Should().ContainSingle(x => x > 1).And.NotBeNull()); }
    [Fact] public void Chain_collection_contain_single_predicate_none() { var subjectItems = new[] { 1, 2 }; Chains("subjectItems", () => subjectItems.Should().ContainSingle(x => x > 5).And.BeEmpty()); }
    [Fact] public void Chain_collection_contain_single_predicate_many() { var subjectItems = new[] { 1, 2 }; Chains("subjectItems", () => subjectItems.Should().ContainSingle(x => x > 0).And.BeEmpty()); }

    [Fact] public void Chain_dictionary_contain_key_null() { Dictionary<string, int>? subjectMap = null; Chains("subjectMap", () => subjectMap.Should().ContainKey("a").And.NotBeNull()); }
    [Fact] public void Chain_dictionary_contain_key_missing() { var subjectMap = new Dictionary<string, int> { ["a"] = 1 }; Chains("subjectMap", () => subjectMap.Should().ContainKey("b").And.BeEmpty()); }

    [Fact] public void Chain_nullable_int_not_be_null() { int? subjectNumber = null; Chains("subjectNumber", () => subjectNumber.Should().NotBeNull().And.NotBeNull()); }
    [Fact] public void Chain_nullable_date_time_not_be_null() { DateTime? subjectDate = null; Chains("subjectDate", () => subjectDate.Should().NotBeNull().And.NotBeNull()); }
    [Fact] public void Chain_nullable_date_time_offset_not_be_null() { DateTimeOffset? subjectDate = null; Chains("subjectDate", () => subjectDate.Should().NotBeNull().And.NotBeNull()); }
    [Fact] public void Chain_nullable_date_only_not_be_null() { DateOnly? subjectDate = null; Chains("subjectDate", () => subjectDate.Should().NotBeNull().And.NotBeNull()); }
    [Fact] public void Chain_nullable_bool_not_be_null() { bool? subjectFlag = null; Chains("subjectFlag", () => subjectFlag.Should().NotBeNull().And.NotBeNull()); }
    [Fact] public void Chain_nullable_value_not_be_null() { Guid? subjectId = null; Chains("subjectId", () => subjectId.Should().NotBeNull().And.NotBeNull()); }

    [Fact]
    public void All_satisfy_callback_that_chains_after_a_failure_throws_the_assertion_message()
    {
        var items = new string?[] { null };
        var ex = Assert.Throws<VouchException>(() => items.Should().AllSatisfy(s => { s.Should().NotBeNull(); s!.Length.Should().Be(1); }));
        Assert.Contains("to have all items satisfy the assertion, but 1 did not:", ex.Message);
        Assert.Contains("[0] Expected s not to be null, but it was.", ex.Message);
    }

    [Fact]
    public void All_satisfy_callback_that_chains_after_a_failure_records_once()
    {
        var items = new string?[] { null };
        var recorded = Record(() => items.Should().AllSatisfy(s => { s.Should().NotBeNull(); s!.Length.Should().Be(1); }));
        Assert.Single(recorded);
        Assert.Contains("[0] Expected s not to be null, but it was.", recorded[0]);
    }

    private static IEnumerable<int> Lazy(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return i;
        }
    }

    [Fact]
    public void Over_cap_collection_records_exactly_one_failure()
    {
        var over = Equivalency.MaxItems + 1;
        Assert.Single(Record(() => Lazy(over).Should().HaveCount(1)));
        Assert.Single(Record(() => Lazy(over).Should().BeEmpty()));
        Assert.Single(Record(() => Lazy(over).Should().Contain(1)));
        Assert.Single(Record(() => Lazy(over).Should().ContainSingle()));
        Assert.Single(Record(() => Lazy(over).Should().Equal(1, 2)));
        Assert.Single(Record(() => Lazy(over).Should().NotContain(1)));
        Assert.Throws<VouchException>(() => Lazy(over).Should().HaveCount(1));
    }

    [Fact]
    public void Over_cap_counted_collection_records_exactly_one_failure()
    {
        var over = Equivalency.MaxItems + 1;
        Assert.Single(Record(() => new Queue<int>(Enumerable.Range(0, over)).Should().HaveCount(1)));
        Assert.Single(Record(() => new Queue<int>(Enumerable.Range(0, over)).Should().BeEmpty()));
        Assert.Single(Record(() => new Queue<int>(Enumerable.Range(0, over)).Should().HaveCountGreaterThan(Equivalency.MaxItems + 5)));
        Assert.Single(Record(() => new Queue<int>(Enumerable.Range(0, over)).Should().BeNullOrEmpty()));
    }

    private static IEnumerable<KeyValuePair<int, int>> LazyPairs(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return new KeyValuePair<int, int>(i, i);
        }
    }

    [Fact]
    public void Over_cap_lazy_dictionary_records_exactly_one_failure()
    {
        var over = Equivalency.MaxItems + 1;
        Assert.Single(Record(() => LazyPairs(over).Should().ContainKey(-1)));
        Assert.Single(Record(() => LazyPairs(over).Should().ContainKeys(-1, -2)));
    }

    [Fact]
    public void Over_cap_dictionary_records_exactly_one_failure()
    {
        var big = new Dictionary<int, int>(LazyPairs(Equivalency.MaxItems + 1));
        Assert.Single(Record(() => big.Should().ContainKey(-1)));
        Assert.Single(Record(() => big.Should().ContainKeys(-1, -2)));
    }

    [Fact] public void Equivalency_failure() => Same(() => new Person { Name = "A" }.Should().BeEquivalentTo(new { Name = "B" }));
    [Fact] public void Equivalency_null_subject() => Same(() => ((Person?)null).Should().BeEquivalentTo(new Person { Name = "B" }));
    [Fact] public void Equivalency_collection() => Same(() => new[] { 1, 2 }.Should().BeEquivalentTo(new[] { 1, 3 }));
    [Fact] public void Equivalency_null_collection() => Same(() => ((IEnumerable<int>?)null).Should().BeEquivalentTo(new[] { 1 }));
}
