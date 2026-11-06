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
            VouchConfiguration.ExceptionFactory = null!;
            var n = 3;
            Assert.Throws<VouchException>(() => n.Should().Be(5));
        });
    }

    [Fact]
    public void A_factory_returning_null_falls_back_to_the_default()
    {
        WithFactory(_ => null!, () =>
        {
            var n = 3;
            Assert.Throws<VouchException>(() => n.Should().Be(5));
        });
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
