using Xunit;

namespace Vouch.Time.Tests;

public class FluentTimeSpanExtensionsTests
{
    [Fact] public void Ticks_of_an_int_is_that_many_ticks() => 5.Ticks().Should().Be(TimeSpan.FromTicks(5));
    [Fact] public void Ticks_of_a_long_is_that_many_ticks() => 5L.Ticks().Should().Be(TimeSpan.FromTicks(5));
    [Fact] public void Ticks_of_a_negative_long_is_negative() => (-5L).Ticks().Should().Be(TimeSpan.FromTicks(-5));
    [Fact] public void Ticks_of_long_max_is_the_largest_span() => long.MaxValue.Ticks().Should().Be(TimeSpan.MaxValue);

    [Fact] public void Microseconds_of_an_int_is_ten_ticks_each() => 3.Microseconds().Should().Be(TimeSpan.FromTicks(30));
    [Fact] public void Microseconds_of_a_long_is_ten_ticks_each() => 3L.Microseconds().Should().Be(TimeSpan.FromTicks(30));
    [Fact] public void Microseconds_of_a_negative_int_is_negative() => (-3).Microseconds().Should().Be(TimeSpan.FromTicks(-30));
    [Fact] public void Microseconds_reads_the_microsecond_part() => TimeSpan.FromTicks(1234567).Microseconds().Should().Be(456);
    [Fact] public void Microseconds_reads_zero_for_a_whole_millisecond() => 5.Milliseconds().Microseconds().Should().Be(0);

    [Fact] public void Nanoseconds_of_an_int_is_one_tick_per_hundred() => 300.Nanoseconds().Should().Be(TimeSpan.FromTicks(3));
    [Fact] public void Nanoseconds_of_a_long_is_one_tick_per_hundred() => 300L.Nanoseconds().Should().Be(TimeSpan.FromTicks(3));
    [Fact] public void Nanoseconds_below_half_a_tick_is_zero() => 49.Nanoseconds().Should().Be(TimeSpan.Zero);
    [Fact] public void Nanoseconds_rounds_to_the_nearest_tick() => 99.Nanoseconds().Should().Be(TimeSpan.FromTicks(1));
    [Theory]
    [InlineData(50, 0)]
    [InlineData(150, 2)]
    [InlineData(250, 2)]
    [InlineData(350, 4)]
    [InlineData(-50, 0)]
    [InlineData(-150, -2)]
    [InlineData(-250, -2)]
    [InlineData(-350, -4)]
    public void Nanoseconds_rounds_halves_to_even(long nanoseconds, long ticks)
    {
        nanoseconds.Nanoseconds().Ticks.Should().Be(ticks);
        ((int)nanoseconds).Nanoseconds().Ticks.Should().Be(ticks);
    }

    [Fact] public void Nanoseconds_of_long_max_goes_through_double() => long.MaxValue.Nanoseconds().Ticks.Should().Be(92233720368547760);
    [Fact] public void Nanoseconds_of_long_min_goes_through_double() => long.MinValue.Nanoseconds().Ticks.Should().Be(-92233720368547760);
    [Fact] public void Nanoseconds_reads_the_nanosecond_part() => TimeSpan.FromTicks(1234567).Nanoseconds().Should().Be(700);

    [Fact] public void TotalMicroseconds_is_ticks_over_ten() => TimeSpan.FromTicks(15).TotalMicroseconds().Should().Be(1.5);
    [Fact] public void TotalNanoseconds_is_ticks_times_a_hundred() => TimeSpan.FromTicks(15).TotalNanoseconds().Should().Be(1500.0);
    [Fact] public void TotalMicroseconds_is_ticks_times_a_tenth() => TimeSpan.FromTicks(1234567).TotalMicroseconds().Should().Be(123456.70000000001);
    [Fact] public void TotalMicroseconds_of_zero_is_zero() => TimeSpan.Zero.TotalMicroseconds().Should().Be(0.0);

    [Fact] public void Milliseconds_of_an_int_is_that_many_milliseconds() => 250.Milliseconds().Should().Be(TimeSpan.FromTicks(2_500_000));
    [Fact] public void Milliseconds_of_a_double_keeps_the_fraction() => 1.5.Milliseconds().Should().Be(TimeSpan.FromTicks(15_000));

    [Fact] public void Seconds_of_an_int_is_that_many_seconds() => 90.Seconds().Should().Be(new TimeSpan(0, 1, 30));
    [Fact] public void Seconds_of_a_double_keeps_the_fraction() => 1.5.Seconds().Should().Be(new TimeSpan(0, 0, 0, 1, 500));
    [Fact] public void Seconds_with_an_offset_adds_the_offset() => 1.Seconds(500.Milliseconds()).Should().Be(new TimeSpan(0, 0, 0, 1, 500));

    [Fact] public void Minutes_of_an_int_is_that_many_minutes() => 5.Minutes().Should().Be(new TimeSpan(0, 5, 0));
    [Fact] public void Minutes_of_a_double_keeps_the_fraction() => 1.5.Minutes().Should().Be(new TimeSpan(0, 1, 30));
    [Fact] public void Minutes_with_an_offset_adds_the_offset() => 2.Minutes(30.Seconds()).Should().Be(new TimeSpan(0, 2, 30));
    [Fact] public void Minutes_of_a_negative_int_is_negative() => (-5).Minutes().Should().Be(new TimeSpan(0, -5, 0));

    [Fact] public void Hours_of_an_int_is_that_many_hours() => 2.Hours().Should().Be(new TimeSpan(2, 0, 0));
    [Fact] public void Hours_of_a_double_keeps_the_fraction() => 1.5.Hours().Should().Be(new TimeSpan(1, 30, 0));
    [Fact] public void Hours_with_an_offset_adds_the_offset() => 23.Hours(59.Minutes()).Should().Be(new TimeSpan(23, 59, 0));

    [Fact] public void Days_of_an_int_is_that_many_days() => 3.Days().Should().Be(new TimeSpan(3, 0, 0, 0));
    [Fact] public void Days_of_a_double_keeps_the_fraction() => 1.5.Days().Should().Be(new TimeSpan(1, 12, 0, 0));
    [Fact] public void Days_with_an_offset_adds_the_offset() => 1.Days(2.Hours()).Should().Be(new TimeSpan(1, 2, 0, 0));

    [Fact] public void And_adds_two_spans() => 23.Hours().And(59.Minutes()).Should().Be(new TimeSpan(23, 59, 0));

    [Fact]
    public void Named_arguments_use_the_parameter_names()
    {
        5.Ticks().Should().Be(TimeSpan.FromTicks(5));
        2.Minutes(offset: 30.Seconds()).Should().Be(new TimeSpan(0, 2, 30));
        1.Days().And(offset: 1.Hours()).Should().Be(new TimeSpan(1, 1, 0, 0));
    }

    [Fact] public void Days_of_int_max_overflows() => Assert.Throws<OverflowException>(() => int.MaxValue.Days());
    [Fact] public void Hours_of_int_min_overflows() => Assert.Throws<OverflowException>(() => int.MinValue.Hours());
    [Fact] public void Minutes_of_int_max_still_fits() => int.MaxValue.Minutes().Should().Be(TimeSpan.FromTicks(int.MaxValue * TimeSpan.TicksPerMinute));
    [Fact] public void Minutes_of_NaN_throws_ArgumentException() => Assert.Throws<ArgumentException>(() => double.NaN.Minutes());
    [Fact] public void Seconds_of_infinity_overflows() => Assert.Throws<OverflowException>(() => double.PositiveInfinity.Seconds());
    [Fact] public void Milliseconds_of_double_max_overflows() => Assert.Throws<OverflowException>(() => double.MaxValue.Milliseconds());
    [Fact] public void And_past_the_maximum_overflows() => Assert.Throws<OverflowException>(() => TimeSpan.MaxValue.And(1.Ticks()));
    [Fact] public void Hours_with_an_offset_past_the_maximum_overflows() => Assert.Throws<OverflowException>(() => 1.Hours(TimeSpan.MaxValue));
}
