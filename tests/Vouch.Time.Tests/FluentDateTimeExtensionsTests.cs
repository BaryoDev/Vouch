using Xunit;

namespace Vouch.Time.Tests;

public class FluentDateTimeExtensionsTests
{
    private static readonly DateTime Noon = new(2026, 3, 1, 12, 0, 0, DateTimeKind.Unspecified);

    [Fact] public void January_builds_a_date() => 5.January(2026).Should().Be(new DateTime(2026, 1, 5));
    [Fact] public void February_builds_a_date() => 5.February(2026).Should().Be(new DateTime(2026, 2, 5));
    [Fact] public void March_builds_a_date() => 5.March(2026).Should().Be(new DateTime(2026, 3, 5));
    [Fact] public void April_builds_a_date() => 5.April(2026).Should().Be(new DateTime(2026, 4, 5));
    [Fact] public void May_builds_a_date() => 5.May(2026).Should().Be(new DateTime(2026, 5, 5));
    [Fact] public void June_builds_a_date() => 5.June(2026).Should().Be(new DateTime(2026, 6, 5));
    [Fact] public void July_builds_a_date() => 5.July(2026).Should().Be(new DateTime(2026, 7, 5));
    [Fact] public void August_builds_a_date() => 5.August(2026).Should().Be(new DateTime(2026, 8, 5));
    [Fact] public void September_builds_a_date() => 5.September(2026).Should().Be(new DateTime(2026, 9, 5));
    [Fact] public void October_builds_a_date() => 5.October(2026).Should().Be(new DateTime(2026, 10, 5));
    [Fact] public void November_builds_a_date() => 5.November(2026).Should().Be(new DateTime(2026, 11, 5));
    [Fact] public void December_builds_a_date() => 5.December(2026).Should().Be(new DateTime(2026, 12, 5));
    [Fact] public void A_month_helper_gives_an_unspecified_kind() => 1.March(2026).Kind.Should().Be(DateTimeKind.Unspecified);
    [Fact] public void February_29_exists_in_a_leap_year() => 29.February(2024).Day.Should().Be(29);
    [Fact] public void February_29_does_not_exist_in_other_years() => Assert.Throws<ArgumentOutOfRangeException>(() => 29.February(2025));
    [Fact] public void Day_zero_is_rejected() => Assert.Throws<ArgumentOutOfRangeException>(() => 0.March(2026));
    [Fact] public void Day_32_is_rejected() => Assert.Throws<ArgumentOutOfRangeException>(() => 32.March(2026));
    [Fact] public void Year_10000_is_rejected() => Assert.Throws<ArgumentOutOfRangeException>(() => 1.March(10000));
    [Fact] public void Named_arguments_use_the_parameter_names() => FluentDateTimeExtensions.March(day: 1, year: 2026).Should().Be(new DateTime(2026, 3, 1));

    [Fact] public void At_hours_and_minutes_sets_the_time() => 1.March(2026).At(10, 30).Should().Be(new DateTime(2026, 3, 1, 10, 30, 0));
    [Fact] public void At_replaces_an_existing_time_of_day() => Noon.At(10, 30).Should().Be(new DateTime(2026, 3, 1, 10, 30, 0));
    [Fact] public void At_takes_seconds() => 1.March(2026).At(10, 30, 15).Should().Be(new DateTime(2026, 3, 1, 10, 30, 15));
    [Fact] public void At_takes_milliseconds() => 1.March(2026).At(10, 30, 15, 250).Should().Be(new DateTime(2026, 3, 1, 10, 30, 15, 250));
    [Fact] public void At_takes_microseconds() => 1.March(2026).At(0, 0, 0, 0, 7).Ticks.Should().Be(new DateTime(2026, 3, 1).Ticks + 70);
    [Fact] public void At_takes_nanoseconds_in_whole_ticks() => 1.March(2026).At(0, 0, 0, 0, 0, 300).Ticks.Should().Be(new DateTime(2026, 3, 1).Ticks + 3);
    [Fact] public void At_keeps_the_kind() => 1.March(2026).AsUtc().At(10, 30).Kind.Should().Be(DateTimeKind.Utc);
    [Fact]
    public void At_with_named_arguments_uses_the_parameter_names() =>
        1.March(2026).At(hours: 1, minutes: 2, seconds: 3, milliseconds: 4).Should().Be(new DateTime(2026, 3, 1, 1, 2, 3, 4));
    [Fact] public void At_a_time_span_sets_the_time() => 1.March(2026).At(new TimeSpan(10, 30, 0)).Should().Be(new DateTime(2026, 3, 1, 10, 30, 0));
    [Fact] public void At_a_time_span_drops_the_old_time() => Noon.At(TimeSpan.Zero).Should().Be(new DateTime(2026, 3, 1));
    [Fact] public void At_a_time_span_of_a_day_is_the_next_day() => 1.March(2026).At(1.Days()).Should().Be(new DateTime(2026, 3, 2));
    [Fact] public void At_a_time_span_names_its_parameters() => FluentDateTimeExtensions.At(date: 1.March(2026), time: 1.Hours()).Should().Be(new DateTime(2026, 3, 1, 1, 0, 0));
    [Fact] public void At_past_the_last_day_is_rejected() => Assert.Throws<ArgumentOutOfRangeException>(() => 31.December(9999).At(1.Days()));

    [Fact]
    public void At_on_an_offset_keeps_the_offset()
    {
        var value = new DateTimeOffset(2026, 3, 1, 12, 0, 0, TimeSpan.FromHours(2)).At(10, 30);
        value.DateTime.Should().Be(new DateTime(2026, 3, 1, 10, 30, 0));
        value.Offset.Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void At_on_an_offset_takes_every_part()
    {
        var value = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.FromHours(-5)).At(1, 2, 3, 4, 5, 600);
        value.DateTime.Ticks.Should().Be(new DateTime(2026, 3, 1, 1, 2, 3, 4).Ticks + 50 + 6);
        value.Offset.Should().Be(TimeSpan.FromHours(-5));
    }

    [Fact] public void At_rejects_hour_24() => Assert.Throws<ArgumentOutOfRangeException>(() => 1.March(2026).At(24, 0));
    [Fact] public void At_rejects_a_negative_hour() => Assert.Throws<ArgumentOutOfRangeException>(() => 1.March(2026).At(-1, 0));
    [Fact] public void At_rejects_minute_60() => Assert.Throws<ArgumentOutOfRangeException>(() => 1.March(2026).At(0, 60));
    [Fact] public void At_rejects_second_60() => Assert.Throws<ArgumentOutOfRangeException>(() => 1.March(2026).At(0, 0, 60));
    [Fact] public void At_rejects_millisecond_1000() => Assert.Throws<ArgumentOutOfRangeException>(() => 1.March(2026).At(0, 0, 0, 1000));
    [Fact] public void At_rejects_microsecond_1000() => Assert.Throws<ArgumentOutOfRangeException>(() => 1.March(2026).At(0, 0, 0, 0, 1000));
    [Fact] public void At_rejects_a_negative_microsecond() => Assert.Throws<ArgumentOutOfRangeException>(() => 1.March(2026).At(0, 0, 0, 0, -1));
    [Fact] public void At_rejects_nanosecond_1000() => Assert.Throws<ArgumentOutOfRangeException>(() => 1.March(2026).At(0, 0, 0, 0, 0, 1000));
    [Fact] public void At_rejects_a_negative_nanosecond() => Assert.Throws<ArgumentOutOfRangeException>(() => 1.March(2026).At(0, 0, 0, 0, 0, -1));
    [Fact] public void At_rejects_hour_24_on_an_offset() => Assert.Throws<ArgumentOutOfRangeException>(() => new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero).At(24, 0));
    [Fact] public void At_rejects_nanosecond_1000_on_an_offset() => Assert.Throws<ArgumentOutOfRangeException>(() => new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero).At(0, 0, 0, 0, 0, 1000));
    [Fact] public void At_rounds_nanoseconds_halves_to_even() => 1.March(2026).At(0, 0, 0, 0, 0, 250).Ticks.Should().Be(new DateTime(2026, 3, 1).Ticks + 2);
    [Fact] public void At_rounds_nanosecond_999_up_to_a_microsecond() => 1.March(2026).At(23, 59, 59, 999, 999, 999).Should().Be(2.March(2026));
    [Fact] public void At_on_an_offset_that_puts_utc_before_year_1_is_rejected() => Assert.Throws<ArgumentOutOfRangeException>(() => new DateTimeOffset(1, 1, 1, 12, 0, 0, TimeSpan.FromHours(2)).At(0, 0, 0, 0, 0, 150));

    [Fact]
    public void AsUtc_marks_the_kind_without_moving_the_clock()
    {
        var value = Noon.AsUtc();
        value.Kind.Should().Be(DateTimeKind.Utc);
        value.Ticks.Should().Be(Noon.Ticks);
    }

    [Fact]
    public void AsLocal_marks_the_kind_without_moving_the_clock()
    {
        var value = Noon.AsLocal();
        value.Kind.Should().Be(DateTimeKind.Local);
        value.Ticks.Should().Be(Noon.Ticks);
    }

    [Fact] public void AsUtc_overrides_a_local_kind() => Noon.AsLocal().AsUtc().Kind.Should().Be(DateTimeKind.Utc);

    [Fact] public void Before_goes_back_by_the_span() => 2.Days().Before(3.March(2026)).Should().Be(1.March(2026));
    [Fact] public void After_goes_forward_by_the_span() => 2.Days().After(1.March(2026)).Should().Be(3.March(2026));
    [Fact] public void Before_keeps_the_kind() => 1.Hours().Before(Noon.AsUtc()).Kind.Should().Be(DateTimeKind.Utc);
    [Fact] public void After_keeps_the_kind() => 1.Hours().After(Noon.AsLocal()).Kind.Should().Be(DateTimeKind.Local);
    [Fact] public void Before_past_the_minimum_is_rejected() => Assert.Throws<ArgumentOutOfRangeException>(() => 1.Days().Before(DateTime.MinValue));
    [Fact] public void After_past_the_maximum_is_rejected() => Assert.Throws<ArgumentOutOfRangeException>(() => 1.Days().After(DateTime.MaxValue));
    [Fact] public void Before_names_its_parameters() => FluentDateTimeExtensions.Before(timeDifference: 1.Days(), sourceDateTime: 2.March(2026)).Should().Be(1.March(2026));

    [Fact] public void Microsecond_of_a_date_time_reads_the_part() => new DateTime(2026, 3, 1).AddTicks(1234567).Microsecond().Should().Be(456);
    [Fact] public void Microsecond_of_an_offset_reads_the_part() => new DateTimeOffset(new DateTime(2026, 3, 1).AddTicks(1234567), TimeSpan.Zero).Microsecond().Should().Be(456);
    [Fact] public void Nanosecond_of_a_date_time_reads_the_part() => new DateTime(2026, 3, 1).AddTicks(1234567).Nanosecond().Should().Be(700);
    [Fact] public void Nanosecond_of_an_offset_reads_the_part() => new DateTimeOffset(new DateTime(2026, 3, 1).AddTicks(1234567), TimeSpan.Zero).Nanosecond().Should().Be(700);

    [Fact] public void AddNanoseconds_adds_whole_ticks() => Noon.AddNanoseconds(300).Ticks.Should().Be(Noon.Ticks + 3);
    [Fact] public void AddNanoseconds_below_half_a_tick_changes_nothing() => Noon.AddNanoseconds(49).Ticks.Should().Be(Noon.Ticks);
    [Fact] public void AddNanoseconds_rounds_to_the_nearest_tick() => Noon.AddNanoseconds(99).Ticks.Should().Be(Noon.Ticks + 1);
    [Fact] public void AddNanoseconds_rounds_halves_to_even() => Noon.AddNanoseconds(250).Ticks.Should().Be(Noon.Ticks + 2);
    [Fact] public void AddNanoseconds_rounds_negative_halves_to_even() => Noon.AddNanoseconds(-250).Ticks.Should().Be(Noon.Ticks - 2);
    [Fact] public void AddNanoseconds_rounding_up_past_the_last_tick_is_rejected() => Assert.Throws<ArgumentOutOfRangeException>(() => DateTime.MaxValue.AddNanoseconds(99));
    [Fact] public void AddNanoseconds_can_go_back() => Noon.AddNanoseconds(-300).Ticks.Should().Be(Noon.Ticks - 3);
    [Fact] public void AddNanoseconds_keeps_the_kind() => Noon.AsUtc().AddNanoseconds(300).Kind.Should().Be(DateTimeKind.Utc);
    [Fact] public void AddNanoseconds_multiplies_by_a_hundredth_in_double() => new DateTime(42).AddNanoseconds(4811602766166610294L).Ticks.Should().Be(48116027661666146);
    [Fact] public void AddNanoseconds_past_the_end_names_the_parameter_t() => Assert.Throws<ArgumentOutOfRangeException>(() => DateTime.MaxValue.AddNanoseconds(99)).ParamName.Should().Be("t");
    [Fact] public void AddNanoseconds_on_an_offset_past_the_end_names_the_parameter_t() => Assert.Throws<ArgumentOutOfRangeException>(() => DateTimeOffset.MaxValue.AddNanoseconds(99)).ParamName.Should().Be("t");
    [Fact] public void At_hour_24_has_no_parameter_name() => Assert.Throws<ArgumentOutOfRangeException>(() => 1.March(2026).At(24, 0)).ParamName.Should().BeNull();
    [Fact] public void At_millisecond_1000_names_millisecond() => Assert.Throws<ArgumentOutOfRangeException>(() => 1.March(2026).At(0, 0, 0, 1000)).ParamName.Should().Be("millisecond");
    [Fact] public void At_microsecond_1000_names_microseconds() => Assert.Throws<ArgumentOutOfRangeException>(() => 1.March(2026).At(0, 0, 0, 0, 1000)).ParamName.Should().Be("microseconds");
    [Fact] public void AddNanoseconds_of_long_max_adds_the_rounded_ticks() => Noon.AddNanoseconds(long.MaxValue).Ticks.Should().Be(Noon.Ticks + 92233720368547760);
    [Fact]
    public void AddNanoseconds_on_an_offset_adds_whole_ticks()
    {
        var value = new DateTimeOffset(2026, 3, 1, 12, 0, 0, TimeSpan.FromHours(2)).AddNanoseconds(300);
        value.DateTime.Ticks.Should().Be(new DateTime(2026, 3, 1, 12, 0, 0).Ticks + 3);
        value.Offset.Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void AddNanoseconds_of_long_max_on_an_offset_adds_the_rounded_ticks() =>
        new DateTimeOffset(2026, 3, 1, 12, 0, 0, TimeSpan.Zero).AddNanoseconds(long.MaxValue).Ticks.Should().Be(Noon.Ticks + 92233720368547760);

    [Fact]
    public void WithOffset_on_an_unspecified_date_keeps_the_clock()
    {
        var value = Noon.WithOffset(TimeSpan.FromHours(2));
        value.DateTime.Should().Be(Noon);
        value.Offset.Should().Be(TimeSpan.FromHours(2));
    }

    [Fact] public void WithOffset_on_a_utc_date_with_a_zero_offset_works() => Noon.AsUtc().WithOffset(TimeSpan.Zero).Offset.Should().Be(TimeSpan.Zero);
    [Fact]
    public void WithOffset_ignores_the_kind()
    {
        var utc = Noon.AsUtc().WithOffset(TimeSpan.FromHours(2));
        utc.DateTime.Ticks.Should().Be(Noon.Ticks);
        utc.Offset.Should().Be(TimeSpan.FromHours(2));
        Noon.AsLocal().WithOffset(TimeSpan.FromHours(-5)).Offset.Should().Be(TimeSpan.FromHours(-5));
    }

    [Fact] public void WithOffset_that_leaves_the_range_is_rejected() => Assert.Throws<ArgumentOutOfRangeException>(() => DateTime.MaxValue.AsUtc().WithOffset(TimeSpan.FromHours(-14)));
    [Fact] public void WithOffset_of_part_of_a_minute_is_rejected() => Assert.Throws<ArgumentException>(() => Noon.WithOffset(TimeSpan.FromSeconds(30)));
    [Fact] public void WithOffset_of_fifteen_hours_is_rejected() => Assert.Throws<ArgumentOutOfRangeException>(() => Noon.WithOffset(TimeSpan.FromHours(15)));
    [Fact] public void WithOffset_of_fourteen_hours_works() => Noon.WithOffset(TimeSpan.FromHours(14)).Offset.Should().Be(TimeSpan.FromHours(14));
}
