using Vouch;
using Xunit;

namespace TimeConsumers;

// Lives outside namespace Vouch to prove the helpers and the core Should() resolve together.
public class ConsumerTests
{
    [Fact]
    public void A_consumer_can_call_Should_on_a_time_span_helper() => 5.Minutes().Should().Be(new TimeSpan(0, 5, 0));

    [Fact]
    public void A_consumer_can_call_Should_on_a_date_helper() => 1.March(2026).At(10, 30).Should().Be(new DateTime(2026, 3, 1, 10, 30, 0));

    [Fact]
    public void A_consumer_can_chain_helpers_into_a_comparison() =>
        2.Hours().And(30.Minutes()).Should().BeGreaterThan(2.Hours());

    [Fact]
    public void A_consumer_can_mix_helpers_with_date_assertions() =>
        1.March(2026).At(10, 30).Should().BeBefore(2.Hours().After(1.March(2026).At(10, 30)));

    [Fact]
    public void A_consumer_can_call_the_part_readers() => 1500.Microseconds().Microseconds().Should().Be(500);
}
