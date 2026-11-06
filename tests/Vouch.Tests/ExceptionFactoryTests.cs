using Vouch.Sdk;
using Xunit;

namespace Vouch.Tests;

[CollectionDefinition("ExceptionFactory", DisableParallelization = true)]
public class ExceptionFactoryCollection
{
}

[Collection("ExceptionFactory")]
public class ExceptionFactoryTests
{
    private sealed class CustomException : Exception
    {
        public CustomException(string message) : base(message)
        {
        }
    }

    private sealed class DerivedVouchException : VouchException
    {
        public DerivedVouchException(string message) : base(message)
        {
        }
    }

    private static void WithFactory(Func<string, Exception>? factory, Action body)
    {
        var previous = VouchConfiguration.ExceptionFactory;
        try
        {
            VouchConfiguration.ExceptionFactory = factory!;
            body();
        }
        finally
        {
            VouchConfiguration.ExceptionFactory = previous;
        }
    }

    [Fact]
    public void A_registered_factory_decides_the_thrown_type()
    {
        WithFactory(m => new CustomException(m), () =>
        {
            var n = 3;
            var ex = Assert.Throws<CustomException>(() => n.Should().Be(5));
            Assert.Equal("Expected n to be 5, but found 3.", ex.Message);
        });
    }

    [Fact]
    public void The_factory_receives_the_message_with_the_reason()
    {
        WithFactory(m => new CustomException(m), () =>
        {
            var n = 3;
            var ex = Assert.Throws<CustomException>(() => n.Should().Be(5, "it matters"));
            Assert.EndsWith("Because it matters.", ex.Message);
        });
    }

    [Fact]
    public void A_custom_type_works_inside_an_assertion_scope()
    {
        WithFactory(m => new CustomException(m), () =>
        {
            var n = 3;
            var ex = Assert.Throws<CustomException>(() =>
            {
                using var scope = new AssertionScope();
                n.Should().Be(5);
                n.Should().Be(6);
            });
            Assert.Contains("to be 5", ex.Message);
            Assert.Contains("to be 6", ex.Message);
        });
    }

    [Fact]
    public void A_custom_type_works_inside_AllSatisfy()
    {
        WithFactory(m => new CustomException(m), () =>
        {
            var xs = new List<int> { 1, 2, 3 };
            var ex = Assert.Throws<CustomException>(() => xs.Should().AllSatisfy(x => x.Should().BeGreaterThan(1)));
            Assert.Contains("1 did not", ex.Message);
            Assert.Contains("[0]", ex.Message);
        });
    }

    [Fact]
    public void Setting_null_restores_the_default()
    {
        WithFactory(m => new CustomException(m), () =>
        {
            var n = 3;
            Assert.Throws<CustomException>(() => n.Should().Be(5));

            VouchConfiguration.ExceptionFactory = null!;
            Assert.Throws<VouchException>(() => n.Should().Be(5));
        });
    }

    [Fact]
    public void A_factory_returning_null_falls_back_to_the_default()
    {
        WithFactory(m => new CustomException(m), () =>
        {
            var n = 3;
            Assert.Throws<CustomException>(() => n.Should().Be(5));

            VouchConfiguration.ExceptionFactory = _ => null!;
            var ex = Assert.Throws<VouchException>(() => n.Should().Be(5));
            Assert.Equal("Expected n to be 5, but found 3.", ex.Message);
        });
    }

    [Fact]
    public void A_factory_that_throws_falls_back_to_VouchException_with_the_original_message()
    {
        WithFactory(_ => throw new InvalidOperationException("factory broke"), () =>
        {
            var n = 3;
            var ex = Assert.Throws<VouchException>(() => n.Should().Be(5));
            Assert.Equal("Expected n to be 5, but found 3.", ex.Message);
        });
    }

    [Fact]
    public void AllSatisfy_collects_a_failure_thrown_on_another_thread()
    {
        WithFactory(m => new CustomException(m), () =>
        {
            var xs = new List<int> { 1, 2 };
            var ex = Assert.Throws<CustomException>(() =>
                xs.Should().AllSatisfy(inner => Task.Run(() => inner.Should().Be(2)).GetAwaiter().GetResult()));
            Assert.Contains("[0]", ex.Message);
            Assert.Contains("to be 2, but found 1", ex.Message);
            Assert.Equal(1, CountOf(ex.Message, "to be 2, but found 1"));
            Assert.DoesNotContain("[1]", ex.Message);
        });
    }

    [Fact]
    public void A_decorating_factory_does_not_leak_into_AllSatisfy_item_lines()
    {
        WithFactory(m => new CustomException("ADAPTER: " + m), () =>
        {
            var xs = new List<int> { 1, 2, 3 };
            var ex = Assert.Throws<CustomException>(() => xs.Should().AllSatisfy(x => x.Should().BeGreaterThan(1)));
            Assert.StartsWith("ADAPTER: ", ex.Message);
            Assert.Equal(1, CountOf(ex.Message, "ADAPTER: "));
            Assert.Contains("[0]", ex.Message);
        });
    }

    [Fact]
    public void A_factory_returning_a_cached_instance_still_reports_every_item()
    {
        var cached = new CustomException("cached");
        WithFactory(_ => cached, () =>
        {
            var xs = new List<int> { 1, 2 };
            var ex = Assert.Throws<CustomException>(() => xs.Should().AllSatisfy(x => x.Should().BeGreaterThan(5)));
            Assert.Same(cached, ex);

            // The cached instance carries no message of its own, so check the collected items
            // with a factory that returns a fresh item exception but a cached outer one.
        });

        var items = new List<string>();
        var outer = new CustomException("outer");
        WithFactory(m =>
        {
            if (m.Contains("did not"))
            {
                return outer;
            }

            items.Add(m);
            return cached;
        }, () =>
        {
            var xs = new List<int> { 1, 2 };
            Assert.Throws<CustomException>(() => xs.Should().AllSatisfy(x => x.Should().BeGreaterThan(5)));
            Assert.Equal(2, items.Count);
            Assert.Contains("found 1", items[0]);
            Assert.Contains("found 2", items[1]);
        });
    }

    [Fact]
    public void A_nested_scope_throws_the_custom_type_once_with_both_messages()
    {
        WithFactory(m => new CustomException(m), () =>
        {
            var n = 3;
            var ex = Assert.Throws<CustomException>(() =>
            {
                using var outer = new AssertionScope();
                n.Should().Be(1);
                using (var inner = new AssertionScope())
                {
                    n.Should().Be(2);
                }
            });
            Assert.Contains("to be 1", ex.Message);
            Assert.Contains("to be 2", ex.Message);
        });
    }

    private static int CountOf(string text, string part)
    {
        var count = 0;
        var i = 0;
        while ((i = text.IndexOf(part, i, StringComparison.Ordinal)) >= 0)
        {
            count++;
            i += part.Length;
        }

        return count;
    }

    [Fact]
    public void A_subclass_of_VouchException_is_caught_as_VouchException()
    {
        WithFactory(m => new DerivedVouchException(m), () =>
        {
            var n = 3;
            var ex = Assert.Throws<DerivedVouchException>(() => n.Should().Be(5));
            Assert.IsAssignableFrom<VouchException>(ex);
        });
    }
}
