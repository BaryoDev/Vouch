using Xunit;
using Xunit.Abstractions;

namespace Differential;

public class DifferentialTests
{
    private readonly ITestOutputHelper _output;

    public DifferentialTests(ITestOutputHelper output) => _output = output;

    public static TheoryData<string> Names()
    {
        var data = new TheoryData<string>();
        foreach (var name in AwesomeCases.Pass.Keys)
        {
            data.Add(name);
        }

        return data;
    }

    public static TheoryData<string> MessageNames()
    {
        var data = new TheoryData<string>();
        foreach (var name in AwesomeCases.Messages.Keys)
        {
            data.Add(name);
        }

        return data;
    }

    private static readonly Dictionary<string, (string Awesome, string Vouch, string Why)> Divergences = new()
    {
        ["match on null subject"] = ("throws NullReferenceException", "fails",
            "AwesomeAssertions crashes inside Match<T> on a null subject; both reject it, Vouch says why"),
        ["match on wrong type"] = ("throws InvalidCastException", "fails",
            "AwesomeAssertions crashes inside Match<T> on another type; both reject it, Vouch says why"),
        ["contain in consecutive order backtracking"] = ("fails", "passes",
            "[1, 1, 1, 2] holds 1, 1, 2 at positions 1 to 3; AwesomeAssertions does not restart after a partial match"),
    };

    [Fact]
    public void BothLibrariesDefineTheSameCases()
    {
        Assert.Equal(AwesomeCases.Pass.Keys.OrderBy(k => k), VouchCases.Pass.Keys.OrderBy(k => k));
        Assert.Equal(AwesomeCases.Messages.Keys.OrderBy(k => k), VouchCases.Messages.Keys.OrderBy(k => k));
    }

    [Theory]
    [MemberData(nameof(Names))]
    public void VouchAgreesWithAwesomeAssertions(string name)
    {
        var oracle = Oracle.AwesomeOutcome(AwesomeCases.Pass[name]);
        var vouch = Oracle.VouchOutcome(VouchCases.Pass[name]);

        if (Divergences.TryGetValue(name, out var known))
        {
            Assert.True(
                (oracle, vouch) == (known.Awesome, known.Vouch),
                $"{name}: pinned divergence changed ({known.Why}). AwesomeAssertions {oracle}, Vouch {vouch}");
            return;
        }

        Assert.True(oracle == vouch, $"{name}: AwesomeAssertions {oracle}, Vouch {vouch}");
    }

    [Theory]
    [MemberData(nameof(MessageNames))]
    public void BothLibrariesFailWithAMessage(string name)
    {
        var awesome = Oracle.AwesomeMessage(AwesomeCases.Messages[name]);
        var vouch = Oracle.VouchMessage(VouchCases.Messages[name]);
        _output.WriteLine($"[{name}]\nAwesome: {awesome}\nVouch:    {vouch}");

        Assert.False(string.IsNullOrWhiteSpace(awesome), $"{name}: AwesomeAssertions did not fail with a message");
        Assert.False(string.IsNullOrWhiteSpace(vouch), $"{name}: Vouch did not fail with a message");
    }
}
