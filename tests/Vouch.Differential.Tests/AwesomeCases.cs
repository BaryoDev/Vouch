using AwesomeAssertions;

namespace Differential;

public static class AwesomeCases
{
    private static Loop SelfLoop(int value)
    {
        var node = new Loop { Value = value };
        node.Next = node;
        return node;
    }

    private static Action WithMessage(string message, string pattern) => () =>
    {
        Action act = Throws.Of(new InvalidOperationException(message));
        act.Should().Throw<InvalidOperationException>().WithMessage(pattern);
    };

    private static Action StringMatch(string subject, string pattern) => () => subject.Should().Match(pattern);

    private static readonly object[] GridValues =
    {
        0.1f, 0.1m, 0.1d, 9007199254740993L, 9007199254740993m, 9007199254740992d, decimal.MaxValue,
        (double)decimal.MaxValue, float.NaN, double.NaN, 1, 1L, 1.0, 1.0f, 1m, ulong.MaxValue,
        1.8446744073709552E19, long.MaxValue, 9.2233720368547758E18d, double.PositiveInfinity,
        (byte)1, 123456792f, 123456792, 0.5f, 0.5m, 0.5d, -1, -1m, -1.0d,
    };

    private static Dictionary<string, Action> BuildGrid()
    {
        var rows = new Dictionary<string, Action>();
        for (var i = 0; i < GridValues.Length; i++)
        {
            for (var j = 0; j < GridValues.Length; j++)
            {
                var a = GridValues[i];
                var b = GridValues[j];
                var key = $"{a.GetType().Name} {a} against {b.GetType().Name} {b} ({i},{j})";
                rows[$"grid be {key}"] = () => a.Should().Be(b);
                rows[$"grid notbe {key}"] = () => a.Should().NotBe(b);
                rows[$"grid equivalent {key}"] = () => a.Should().BeEquivalentTo(b);
                rows[$"grid oneof {key}"] = () => a.Should().BeOneOf(b, "other");
            }
        }

        return rows;
    }

    private static Action AggregateThrow<T>(params Exception[] inner) where T : Exception => () =>
    {
        Action act = Throws.Of(new AggregateException(inner));
        act.Should().Throw<T>();
    };

    private static readonly Lazy<IReadOnlyDictionary<string, Action>> PassRows = new(BuildPass);

    private static IReadOnlyDictionary<string, Action> BuildPass() => Hand.Concat(BuildGrid()).ToDictionary(p => p.Key, p => p.Value);

    public static IReadOnlyDictionary<string, Action> Pass => PassRows.Value;

    private static readonly Dictionary<string, Action> Hand = new()
    {
        ["throw exception on empty aggregate"] = AggregateThrow<Exception>(),
        ["throw aggregate on empty aggregate"] = AggregateThrow<AggregateException>(),
        ["throw exception on nested empty aggregate"] = () =>
        {
            Action act = Throws.Of(new AggregateException(new AggregateException()));
            act.Should().Throw<Exception>();
        },
        ["throw aggregate on aggregate of argument"] = AggregateThrow<AggregateException>(new ArgumentException("a")),
        ["withmessage any of several inners"] = () =>
        {
            Action act = Throws.Of(new AggregateException(new InvalidOperationException("x"), new ArgumentException("inner msg")));
            act.Should().Throw<Exception>().WithMessage("inner*");
        },
        ["withmessage first of several inners"] = () =>
        {
            Action act = Throws.Of(new AggregateException(new InvalidOperationException("x"), new ArgumentException("inner msg")));
            act.Should().Throw<Exception>().WithMessage("x");
        },
        ["withmessage none of several inners"] = () =>
        {
            Action act = Throws.Of(new AggregateException(new InvalidOperationException("x"), new ArgumentException("inner msg")));
            act.Should().Throw<Exception>().WithMessage("zzz");
        },
        ["withmessage outer aggregate text on unwrapped throw"] = () =>
        {
            Action act = Throws.Of(new AggregateException(new InvalidOperationException("x")));
            act.Should().Throw<Exception>().WithMessage("One or more*");
        },
        ["decimal against double beyond 2^53"] = () => ((object)9007199254740993m).Should().Be(9007199254740992d),
        ["decimal max against its double"] = () => ((object)decimal.MaxValue).Should().Be((double)decimal.MaxValue),
        ["float against decimal"] = () => ((object)0.1f).Should().Be(0.1m),
        ["float nan against double nan"] = () => ((object)float.NaN).Should().Be(double.NaN),
        ["float nan against double nan notbe"] = () => ((object)float.NaN).Should().NotBe(double.NaN),
        ["float nan against double nan equivalent"] = () => ((object)float.NaN).Should().BeEquivalentTo(double.NaN),
        ["oneof int against longs"] = () => ((object)1).Should().BeOneOf(5L, 1L),
        ["oneof int against ints"] = () => ((object)1).Should().BeOneOf(5, 1),
        ["oneof float against double"] = () => ((object)0.5f).Should().BeOneOf(0.5d),
        ["oneof byte against sbyte"] = () => ((object)(byte)5).Should().BeOneOf((sbyte)5),
        ["equivalent decimal member against double member"] = () => new[] { new object[] { 9007199254740993m } }.Should().BeEquivalentTo(new[] { new object[] { 9007199254740992d } }),
        ["equivalent two hundred decimals against longs reversed"] = () => Enumerable.Range(0, 200).Select(i => (object)(9007199254740993m + i)).ToArray()
            .Should().BeEquivalentTo(Enumerable.Range(0, 200).Select(i => (object)(9007199254740993L + i)).Reverse().ToArray()),
        ["withmessage exact"] = WithMessage("abc", "abc"),
        ["withmessage star suffix"] = WithMessage("abcdef", "abc*"),
        ["withmessage star prefix"] = WithMessage("abcdef", "*def"),
        ["withmessage question mark"] = WithMessage("abc", "a?c"),
        ["withmessage question mark too short"] = WithMessage("ac", "a?c"),
        ["withmessage question mark too long"] = WithMessage("abbc", "a?c"),
        ["withmessage different case"] = WithMessage("ABC", "abc"),
        ["withmessage different case with wildcard"] = WithMessage("Hello World", "hello*"),
        ["withmessage trailing newline on message"] = WithMessage("abc\n", "abc"),
        ["withmessage trailing newline on pattern"] = WithMessage("abc", "abc\n"),
        ["withmessage crlf message lf pattern"] = WithMessage("a\r\nb", "a\nb"),
        ["withmessage lf message crlf pattern"] = WithMessage("a\nb", "a\r\nb"),
        ["withmessage star spans newline"] = WithMessage("a\nb", "a*b"),
        ["withmessage question mark on newline"] = WithMessage("a\nb", "a?b"),
        ["withmessage empty pattern empty message"] = WithMessage("", ""),
        ["withmessage empty pattern text message"] = WithMessage("abc", ""),
        ["withmessage star alone"] = WithMessage("abc", "*"),
        ["withmessage star alone empty message"] = WithMessage("", "*"),
        ["withmessage regex dot is literal"] = WithMessage("abc", "a.c"),
        ["withmessage brackets are literal"] = WithMessage("a[b]c", "a[b]c"),
        ["withmessage leading space"] = WithMessage(" abc", "abc"),
        ["withmessage trailing space"] = WithMessage("abc ", "abc"),
        ["withmessage mismatch"] = WithMessage("abc", "xyz"),

        ["string match exact"] = StringMatch("abc", "abc"),
        ["string match star"] = StringMatch("abcdef", "abc*"),
        ["string match question mark"] = StringMatch("abc", "a?c"),
        ["string match different case"] = StringMatch("ABC", "abc"),
        ["string match trailing newline on subject"] = StringMatch("abc\n", "abc"),
        ["string match trailing newline on pattern"] = StringMatch("abc", "abc\n"),
        ["string match crlf against lf"] = StringMatch("a\r\nb", "a\nb"),
        ["string match empty pattern empty subject"] = StringMatch("", ""),
        ["string match empty pattern text subject"] = StringMatch("abc", ""),
        ["string match star alone"] = StringMatch("abc", "*"),
        ["string match star alone empty subject"] = StringMatch("", "*"),
        ["string match null subject"] = () => ((string?)null).Should().Match("*"),

        ["equivalent record same"] = () => new Person("a", 1).Should().BeEquivalentTo(new Person("a", 1)),
        ["equivalent record one member changed"] = () => new Person("a", 1).Should().BeEquivalentTo(new Person("a", 2)),
        ["equivalent excluding changed member"] = () => new Person("a", 1).Should().BeEquivalentTo(new Person("a", 2), o => o.Excluding(x => x.Age)),
        ["equivalent including only unchanged member"] = () => new Person("a", 1).Should().BeEquivalentTo(new Person("b", 1), o => o.Including(x => x.Age)),
        ["equivalent excluding missing members"] = () => new { Name = "a" }.Should().BeEquivalentTo(new Person("a", 1), o => o.ExcludingMissingMembers()),
        ["equivalent with mapping to renamed member"] = () => new Titled("a", 1).Should().BeEquivalentTo(new Person("a", 1), o => o.WithMapping("Name", "Title")),
        ["equivalent using comparer same"] = () => new Person("A", 1).Should().BeEquivalentTo(new Person("a", 1), o => o.Using<string>(StringComparer.OrdinalIgnoreCase)),
        ["equivalent using comparer differs"] = () => new Person("A", 1).Should().BeEquivalentTo(new Person("b", 1), o => o.Using<string>(StringComparer.OrdinalIgnoreCase)),
        ["equivalent comparing by members passes where equals says unequal"] = () => new NeverEqual { Value = 1 }.Should().BeEquivalentTo(new NeverEqual { Value = 1 }, o => o.ComparingByMembers<NeverEqual>()),
        ["equivalent comparing by members fails where equals says equal"] = () => new AlwaysEqual { Value = 1 }.Should().BeEquivalentTo(new AlwaysEqual { Value = 2 }, o => o.ComparingByMembers<AlwaysEqual>()),
        ["equivalent comparing by value passes where members differ"] = () => { var same = new RefOnly { Value = 1 }; same.Should().BeEquivalentTo(same, o => o.ComparingByValue<RefOnly>()); },
        ["equivalent comparing by value fails where equals says unequal"] = () => new RefOnly { Value = 1 }.Should().BeEquivalentTo(new RefOnly { Value = 1 }, o => o.ComparingByValue<RefOnly>()),
        ["equivalent without strict ordering after strict ordering"] = () => new[] { 1, 2 }.Should().BeEquivalentTo(new[] { 2, 1 }, o => o.WithStrictOrdering().WithoutStrictOrdering()),
        ["equivalent ignoring cyclic references"] = () => SelfLoop(1).Should().BeEquivalentTo(SelfLoop(1), o => o.IgnoringCyclicReferences()),
        ["equivalent ignoring cyclic references still fails on a difference"] = () => SelfLoop(1).Should().BeEquivalentTo(SelfLoop(2), o => o.IgnoringCyclicReferences()),
        ["equivalent respecting runtime types same"] = () => new Person("a", 1).Should().BeEquivalentTo(new Person("a", 1), o => o.PreferringRuntimeMemberTypes()),
        ["equivalent respecting runtime types changed"] = () => new Person("a", 1).Should().BeEquivalentTo(new Person("a", 2), o => o.PreferringRuntimeMemberTypes()),
        ["equivalent record against anonymous"] = () => new Person("a", 1).Should().BeEquivalentTo(new { Name = "a", Age = 1 }),
        ["equivalent record against anonymous changed"] = () => new Person("a", 1).Should().BeEquivalentTo(new { Name = "a", Age = 2 }),
        ["equivalent record against other record type"] = () => new Person("a", 1).Should().BeEquivalentTo(new OtherPerson("a", 1)),
        ["equivalent record struct same"] = () => new Point(1, 2).Should().BeEquivalentTo(new Point(1, 2)),
        ["equivalent record struct changed"] = () => new Point(1, 2).Should().BeEquivalentTo(new Point(1, 3)),
        ["equivalent value tuple same"] = () => (1, "a").Should().BeEquivalentTo((1, "a")),
        ["equivalent value tuple changed"] = () => (1, "a").Should().BeEquivalentTo((1, "b")),
        ["equivalent tuple same"] = () => Tuple.Create(1, "a").Should().BeEquivalentTo(Tuple.Create(1, "a")),
        ["equivalent tuple changed"] = () => Tuple.Create(1, "a").Should().BeEquivalentTo(Tuple.Create(1, "b")),
        ["equivalent equals says equal members differ"] = () => new AlwaysEqual { Value = 1 }.Should().BeEquivalentTo(new AlwaysEqual { Value = 2 }),
        ["equivalent equals says different members same"] = () => new NeverEqual { Value = 1 }.Should().BeEquivalentTo(new NeverEqual { Value = 1 }),
        ["equivalent enum against other enum same value"] = () => ((object)Color.Red).Should().BeEquivalentTo(Tint.Red),
        ["equivalent enum against other enum different value"] = () => ((object)Color.Red).Should().BeEquivalentTo(Tint.Green),
        ["equivalent enum against int"] = () => ((object)Color.Red).Should().BeEquivalentTo(1),
        ["equivalent enum against wrong int"] = () => ((object)Color.Red).Should().BeEquivalentTo(2),
        ["equivalent enum member against int member"] = () => new ColorHolder { Value = Color.Red }.Should().BeEquivalentTo(new IntHolder { Value = 1 }),
        ["equivalent int against long"] = () => ((object)1).Should().BeEquivalentTo(1L),
        ["equivalent int against different long"] = () => ((object)1).Should().BeEquivalentTo(2L),
        ["equivalent boxed int against long"] = () => ((object)1).Should().BeEquivalentTo(1L),
        ["equivalent int member against long member"] = () => new IntHolder { Value = 1 }.Should().BeEquivalentTo(new BoxedLong { Value = 1 }),
        ["equivalent int member against different long member"] = () => new IntHolder { Value = 1 }.Should().BeEquivalentTo(new BoxedLong { Value = 2 }),
        ["equivalent list against array"] = () => new List<string> { "a", "b" }.Should().BeEquivalentTo(new[] { "a", "b" }),
        ["equivalent list against array other order"] = () => new List<string> { "a", "b" }.Should().BeEquivalentTo(new[] { "b", "a" }),
        ["equivalent nested lists other order"] = () => new Wrapper { Rows = { new() { 1, 2 }, new() { 3 } } }
            .Should().BeEquivalentTo(new Wrapper { Rows = { new() { 3 }, new() { 2, 1 } } }),
        ["equivalent nested lists different content"] = () => new Wrapper { Rows = { new() { 1, 2 }, new() { 3 } } }
            .Should().BeEquivalentTo(new Wrapper { Rows = { new() { 3 }, new() { 2, 4 } } }),
        ["equivalent dictionary other key order"] = () => new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 }
            .Should().BeEquivalentTo(new Dictionary<string, int> { ["b"] = 2, ["a"] = 1 }),
        ["equivalent dictionary different value"] = () => new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 }
            .Should().BeEquivalentTo(new Dictionary<string, int> { ["b"] = 3, ["a"] = 1 }),
        ["equivalent date time offset other offset"] = () => ((object)new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero))
            .Should().BeEquivalentTo(new DateTimeOffset(2020, 1, 1, 2, 0, 0, TimeSpan.FromHours(2))),
        ["equivalent null member against missing member"] = () => new Node("a", null).Should().BeEquivalentTo(new { Name = "a" }),
        ["equivalent missing member against null member"] = () => new { Name = "a" }.Should().BeEquivalentTo(new Node("a", null)),
        ["equivalent null subject against value"] = () => ((Person?)null).Should().BeEquivalentTo(new Person("a", 1)),
        ["equivalent value against null expectation"] = () => new Person("a", 1).Should().BeEquivalentTo((Person?)null),

        ["string equivalent null subject null expected"] = () => ((string?)null).Should().BeEquivalentTo(null!),
        ["string equivalent text subject null expected"] = () => "a".Should().BeEquivalentTo(null!),
        ["string equivalent null subject text expected"] = () => ((string?)null).Should().BeEquivalentTo("a"),
        ["string equivalent other case"] = () => "ABC".Should().BeEquivalentTo("abc"),
        ["string not contain equivalent of other case"] = () => "Hello".Should().NotContainEquivalentOf("hello"),
        ["string not contain equivalent of absent"] = () => "Hello".Should().NotContainEquivalentOf("xyz"),
        ["string not contain equivalent of null subject"] = () => ((string?)null).Should().NotContainEquivalentOf("a"),
        ["string contain occurrence exactly twice met"] = () => "banana".Should().Contain("an", Exactly.Twice()),
        ["string contain occurrence exactly once not met"] = () => "banana".Should().Contain("an", Exactly.Once()),
        ["string contain occurrence at least"] = () => "banana".Should().Contain("a", AtLeast.Times(3)),
        ["string contain occurrence at most not met"] = () => "banana".Should().Contain("a", AtMost.Twice()),
        ["string contain occurrence more than not met"] = () => "banana".Should().Contain("a", MoreThan.Times(3)),
        ["string contain occurrence less than"] = () => "banana".Should().Contain("a", LessThan.Times(4)),
        ["string start with equivalent of other case"] = () => "abc".Should().StartWithEquivalentOf("AB"),
        ["string start with equivalent of absent"] = () => "abc".Should().StartWithEquivalentOf("x"),
        ["string not start with equivalent of other case"] = () => "abc".Should().NotStartWithEquivalentOf("AB"),
        ["string not start with equivalent of absent"] = () => "abc".Should().NotStartWithEquivalentOf("x"),
        ["string end with equivalent of other case"] = () => "abc".Should().EndWithEquivalentOf("BC"),
        ["string end with equivalent of absent"] = () => "abc".Should().EndWithEquivalentOf("x"),
        ["string not end with equivalent of other case"] = () => "abc".Should().NotEndWithEquivalentOf("BC"),
        ["string not end with equivalent of absent"] = () => "abc".Should().NotEndWithEquivalentOf("x"),
        ["string match equivalent of other case"] = () => "abc".Should().MatchEquivalentOf("A?C"),
        ["string match equivalent of absent"] = () => "abc".Should().MatchEquivalentOf("x*"),
        ["string not match equivalent of other case"] = () => "abc".Should().NotMatchEquivalentOf("A*"),
        ["string not match equivalent of absent"] = () => "abc".Should().NotMatchEquivalentOf("x*"),
        ["string not match matching"] = () => "abc".Should().NotMatch("a*"),
        ["string not match other case"] = () => "abc".Should().NotMatch("A*"),
        ["string not match absent"] = () => "abc".Should().NotMatch("x*"),
        ["string be upper cased upper"] = () => "ABC".Should().BeUpperCased(),
        ["string be upper cased mixed"] = () => "Abc".Should().BeUpperCased(),
        ["string not be upper cased upper"] = () => "ABC".Should().NotBeUpperCased(),
        ["string not be upper cased mixed"] = () => "Abc".Should().NotBeUpperCased(),
        ["string be lower cased lower"] = () => "abc".Should().BeLowerCased(),
        ["string be lower cased mixed"] = () => "aBc".Should().BeLowerCased(),
        ["string not be lower cased lower"] = () => "abc".Should().NotBeLowerCased(),
        ["string not be lower cased mixed"] = () => "aBc".Should().NotBeLowerCased(),
        ["string be null or white space null"] = () => ((string?)null).Should().BeNullOrWhiteSpace(),
        ["string be null or white space blank"] = () => "  ".Should().BeNullOrWhiteSpace(),
        ["string be null or white space text"] = () => "a".Should().BeNullOrWhiteSpace(),
        ["string match regex object matching"] = () => "abc".Should().MatchRegex(new System.Text.RegularExpressions.Regex("^a")),
        ["string match regex object absent"] = () => "abc".Should().MatchRegex(new System.Text.RegularExpressions.Regex("^b")),
        ["string match regex occurrence met"] = () => "a1b2".Should().MatchRegex("[0-9]", Exactly.Twice()),
        ["string match regex occurrence not met"] = () => "a1b2".Should().MatchRegex("[0-9]", Exactly.Once()),
        ["string match regex object occurrence met"] = () => "a1b2".Should().MatchRegex(new System.Text.RegularExpressions.Regex("[a-z]"), AtLeast.Twice()),
        ["string match regex object occurrence not met"] = () => "a1b2".Should().MatchRegex(new System.Text.RegularExpressions.Regex("[a-z]"), MoreThan.Twice()),
        ["string not match regex matching"] = () => "abc".Should().NotMatchRegex("^a"),
        ["string not match regex absent"] = () => "abc".Should().NotMatchRegex("^b"),
        ["string not match regex object matching"] = () => "abc".Should().NotMatchRegex(new System.Text.RegularExpressions.Regex("^a")),
        ["string not match regex object absent"] = () => "abc".Should().NotMatchRegex(new System.Text.RegularExpressions.Regex("^b")),

        ["be reference equal arrays"] = () => ((object)new[] { 1 }).Should().Be(new[] { 1 }),
        ["be same array"] = () =>
        {
            var array = new[] { 1 };
            ((object)array).Should().Be(array);
        },
        ["be date time offset other offset"] = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)
            .Should().Be(new DateTimeOffset(2020, 1, 1, 2, 0, 0, TimeSpan.FromHours(2))),
        ["be boxed int against long"] = () => ((object)1).Should().Be(1L),
        ["be boxed int against different long"] = () => ((object)1).Should().Be(2L),
        ["be boxed int against double"] = () => ((object)1).Should().Be(1.0),
        ["be boxed int against fractional double"] = () => ((object)2).Should().Be(1.5),
        ["be boxed decimal against double"] = () => ((object)0.1m).Should().Be(0.1),
        ["be boxed float against double"] = () => ((object)0.1f).Should().Be(0.1),
        ["be boxed negative int against ulong"] = () => ((object)(-1)).Should().Be(ulong.MaxValue),
        ["be boxed int against enum"] = () => ((object)1).Should().Be(DayOfWeek.Monday),
        ["be boxed int against char"] = () => ((object)97).Should().Be('a'),
        ["equivalent items int against long"] = () => new[] { 1, 2 }.Should().BeEquivalentTo(new long[] { 2, 1 }),
        ["equivalent boxed int against fractional double"] = () => ((object)2).Should().BeEquivalentTo(1.5),
        ["equivalent boxed int against whole double"] = () => ((object)1).Should().BeEquivalentTo(1.0),

        ["throw aggregate not unwrapped"] = () =>
        {
            Action act = Throws.Of(new AggregateException(new ArgumentException("x")));
            act.Should().Throw<ArgumentException>();
        },
        ["throw aggregate as aggregate"] = () =>
        {
            Action act = Throws.Of(new AggregateException(new ArgumentException("x")));
            act.Should().Throw<AggregateException>();
        },
        ["throw aggregate nested"] = () =>
        {
            Action act = Throws.Of(new AggregateException(new AggregateException(new ArgumentException("x"))));
            act.Should().Throw<ArgumentException>();
        },
        ["throw aggregate two inners"] = () =>
        {
            Action act = Throws.Of(new AggregateException(new FormatException("x"), new ArgumentException("y")));
            act.Should().Throw<ArgumentException>();
        },
        ["throw aggregate wrong inner"] = () =>
        {
            Action act = Throws.Of(new AggregateException(new FormatException("x")));
            act.Should().Throw<ArgumentException>();
        },
        ["throw aggregate empty"] = () =>
        {
            Action act = Throws.Of(new AggregateException());
            act.Should().Throw<ArgumentException>();
        },
        ["throw aggregate then inner message"] = () =>
        {
            Action act = Throws.Of(new AggregateException("outer", new ArgumentException("inner")));
            act.Should().Throw<ArgumentException>().WithMessage("inner");
        },
        ["throw aggregate then outer message"] = () =>
        {
            Action act = Throws.Of(new AggregateException("outer", new ArgumentException("inner")));
            act.Should().Throw<ArgumentException>().WithMessage("outer*");
        },
        ["throw derived type"] = () =>
        {
            Action act = Throws.Of(new ArgumentNullException("x"));
            act.Should().Throw<ArgumentException>();
        },
        ["throw wrong type"] = () =>
        {
            Action act = Throws.Of(new InvalidOperationException("x"));
            act.Should().Throw<ArgumentException>();
        },
        ["throw nothing thrown"] = () =>
        {
            Action act = Throws.Nothing();
            act.Should().Throw<ArgumentException>();
        },
        ["not throw when thrown"] = () =>
        {
            Action act = Throws.Of(new InvalidOperationException("x"));
            act.Should().NotThrow();
        },

        ["contain single empty"] = () => new List<int>().Should().ContainSingle(),
        ["contain single one item"] = () => new List<int> { 1 }.Should().ContainSingle(),
        ["contain single two items"] = () => new List<int> { 1, 2 }.Should().ContainSingle(),
        ["be empty on null collection"] = () => ((List<int>?)null).Should().BeEmpty(),
        ["be empty on null string"] = () => ((string?)null).Should().BeEmpty(),
        ["be empty on empty collection"] = () => new List<int>().Should().BeEmpty(),

        ["match on null subject"] = () => ((object?)null).Should().Match<string>(s => s.Length > 0),
        ["match on wrong type"] = () => ((object?)1).Should().Match<string>(s => s.Length > 0),
        ["match on right type true"] = () => ((object?)"a").Should().Match<string>(s => s.Length > 0),
        ["match on right type false"] = () => ((object?)"").Should().Match<string>(s => s.Length > 0),
        ["not be of type on null subject"] = () => ((object?)null).Should().NotBeOfType<string>(),
        ["not be assignable to on null subject"] = () => ((object?)null).Should().NotBeAssignableTo<string>(),
        ["not be of type on other type"] = () => ((object?)1).Should().NotBeOfType<string>(),
        ["not be of type on same type"] = () => ((object?)"a").Should().NotBeOfType<string>(),
        ["not be assignable to on assignable"] = () => ((object?)"a").Should().NotBeAssignableTo<object>(),

        ["contain with reason present"] = () => new List<string> { "a" }.Should().Contain("a", "b"),
        ["contain with reason both present"] = () => new List<string> { "a", "b" }.Should().Contain("a", "b"),
        ["contain with reason absent"] = () => new List<string> { "x" }.Should().Contain("a", "b"),
    };

    public static IReadOnlyDictionary<string, Action> Messages { get; } = new Dictionary<string, Action>
    {
        ["be on int"] = () => 1.Should().Be(2),
        ["be on string"] = () => "abc".Should().Be("abd"),
        ["be on int with reason"] = () => 1.Should().Be(2, "the answer is two"),
        ["equivalent with one member changed"] = () => new Person("a", 1).Should().BeEquivalentTo(new Person("a", 2)),
        ["throw wrong type"] = () =>
        {
            Action act = Throws.Of(new InvalidOperationException("x"));
            act.Should().Throw<ArgumentException>();
        },
        ["withmessage mismatch"] = WithMessage("abc", "xyz"),
        ["contain absent item"] = () => new List<int> { 1, 2 }.Should().Contain(3),
        ["be null on text"] = () => "a".Should().BeNull(),
    };
}
