using System.Runtime.CompilerServices;
using Vouch.Sdk;
using Xunit;

namespace Vouch.Tests;

public sealed record Money(decimal Amount, string Currency);

public static class MoneyShouldExtensions
{
    public static MoneyAssertions Should(this Money? subject,
        [CallerArgumentExpression(nameof(subject))] string? expression = null)
        => new(subject, expression);
}

public readonly struct MoneyAssertions
{
    internal MoneyAssertions(Money? subject, string? expression)
    {
        Subject = subject;
        Expression = expression;
    }

    public Money? Subject { get; }

    public string? Expression { get; }

    public AndConstraint<MoneyAssertions> BeIn(string currency, string? because = null)
    {
        if (Subject?.Currency != currency)
        {
            Assertion.Fail(
                $"Expected {Assertion.Name(Expression)} to be in {currency}, but found {Subject?.Currency ?? "null"}.",
                because);
        }

        return new(this);
    }
}

public static class ObjectAssertionsCustomExtensions
{
    public static AndConstraint<ObjectAssertions> BeEven(this ObjectAssertions assertions, string? because = null)
    {
        if (assertions.Subject is not int n || n % 2 != 0)
        {
            Assertion.Fail($"Expected {Assertion.Name(assertions.Expression)} to be an even number, but found {assertions.Subject}.", because);
        }

        return new(assertions);
    }
}

public class CustomAssertionTests
{
    [Fact]
    public void Extension_on_ObjectAssertions_names_the_expression()
    {
        object count = 3;
        var ex = Assert.Throws<VouchException>(() => count.Should().BeEven());
        Assert.Equal("Expected count to be an even number, but found 3.", ex.Message);
    }

    [Fact]
    public void Extension_on_ObjectAssertions_passes()
    {
        object count = 4;
        count.Should().BeEven();
    }

    [Fact]
    public void User_type_with_own_Should_names_the_expression()
    {
        var price = new Money(5m, "USD");
        var ex = Assert.Throws<VouchException>(() => price.Should().BeIn("PHP"));
        Assert.Equal("Expected price to be in PHP, but found USD.", ex.Message);
    }

    [Fact]
    public void Empty_expression_reads_as_value()
    {
        Assert.Equal("value", Assertion.Name(null));
        Assert.Equal("value", Assertion.Name(""));
        Assert.Equal("x", Assertion.Name("x"));
    }

    [Fact]
    public void Extension_failures_are_collected_by_a_scope()
    {
        object a = 1;
        object b = 3;
        var ex = Assert.Throws<VouchException>(() =>
        {
            using var scope = new AssertionScope();
            a.Should().BeEven();
            b.Should().BeEven();
        });
        Assert.Contains("Expected a to be an even number", ex.Message);
        Assert.Contains("Expected b to be an even number", ex.Message);
        Assert.Equal(2, ex.Message.Split("\n\n").Length);
    }

    [Fact]
    public void User_type_failures_are_collected_by_a_scope()
    {
        var first = new Money(1m, "USD");
        var second = new Money(2m, "EUR");
        var ex = Assert.Throws<VouchException>(() =>
        {
            using var scope = new AssertionScope();
            first.Should().BeIn("PHP");
            second.Should().BeIn("PHP");
        });
        Assert.Contains("Expected first to be in PHP", ex.Message);
        Assert.Contains("Expected second to be in PHP", ex.Message);
        Assert.Equal(2, ex.Message.Split("\n\n").Length);
    }

    [Fact]
    public void Because_text_appears_in_the_message()
    {
        object count = 3;
        var ex = Assert.Throws<VouchException>(() => count.Should().BeEven("pairs are needed"));
        Assert.EndsWith("Because pairs are needed.", ex.Message);
    }

    [Fact]
    public void Because_does_not_leak_into_the_next_failure()
    {
        object count = 3;
        Assert.Throws<VouchException>(() => count.Should().BeEven("pairs are needed"));
        var ex = Assert.Throws<VouchException>(() => count.Should().BeEven());
        Assert.DoesNotContain("Because", ex.Message);
    }

    [Fact]
    public void Plain_fail_without_because_has_just_the_message()
    {
        var ex = Assert.Throws<VouchException>(() => Assertion.Fail("custom failure"));
        Assert.Equal("custom failure", ex.Message);
    }

    [Fact]
    public void Fail_with_because_restores_the_outer_reason_under_a_scope()
    {
        Reason.Set("outer");
        try
        {
            Assert.Throws<VouchException>(() =>
            {
                using var scope = new AssertionScope();
                Assertion.Fail("m", "inner");
            });
            Assert.Equal("outer", Reason.Current);
        }
        finally
        {
            Reason.Clear();
        }
    }

    [Fact]
    public void Fail_with_because_restores_the_outer_reason_when_thrown()
    {
        Reason.Set("outer");
        try
        {
            Assert.Throws<VouchException>(() => Assertion.Fail("m", "inner"));
            Assert.Equal("outer", Reason.Current);
        }
        finally
        {
            Reason.Clear();
        }
    }

    [Fact]
    public void Because_shows_in_an_AllSatisfy_item_message()
    {
        var items = new object[] { 3 };
        var ex = Assert.Throws<VouchException>(() =>
            items.Should().AllSatisfy(x => x.Should().BeEven("r")));
        Assert.Contains("Because r", ex.Message);
    }

    [Fact]
    public void Because_shows_in_a_scope_message()
    {
        object count = 3;
        var ex = Assert.Throws<VouchException>(() =>
        {
            using var scope = new AssertionScope();
            count.Should().BeEven("why");
        });
        Assert.Contains("Because why.", ex.Message);
    }

    [Fact]
    public void Expression_is_the_call_site_text_for_built_in_types()
    {
        var text = "a";
        var numbers = new[] { 1 };
        var map = new Dictionary<string, int> { ["a"] = 1 };
        var number = 5;
        var flag = true;
        Func<int> f = () => 1;

        Assert.Equal("text", text.Should().Expression);
        Assert.Equal("numbers", numbers.Should().Expression);
        Assert.Equal("new[] {1}", new[] { 1 }.Should().Expression);
        Assert.Equal("map", map.Should().Expression);
        Assert.Equal("number", number.Should().Expression);
        Assert.Equal("flag", flag.Should().Expression);
        Assert.Equal("f", f.Should().Expression);
    }
}

[Collection("ExceptionFactory")]
public class CustomAssertionExceptionFactoryTests
{
    private sealed class CustomException(string message) : Exception(message);

    [Fact]
    public void Exception_factory_applies_to_a_custom_assertion()
    {
        var previous = VouchConfiguration.ExceptionFactory;
        try
        {
            VouchConfiguration.ExceptionFactory = m => new CustomException(m);
            object count = 3;
            var ex = Assert.Throws<CustomException>(() => count.Should().BeEven());
            Assert.Equal("Expected count to be an even number, but found 3.", ex.Message);
        }
        finally
        {
            VouchConfiguration.ExceptionFactory = previous;
        }
    }
}
