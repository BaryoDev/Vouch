# Vouch.Time

The FluentAssertions 8 time helpers for [Vouch](https://github.com/BaryoDev/Vouch): `5.Minutes()`, `1.March(2026).At(10, 30)` and the rest. Same names, parameter names and results as FluentAssertions 8 and AwesomeAssertions, checked by differential tests against AwesomeAssertions. No dependencies, not even Vouch.

```sh
dotnet add package Vouch.Time
```

Targets `net8.0` and `net10.0`. The helpers live in namespace `Vouch`, so `using Vouch;` brings them in with `.Should()`.

```csharp
using Vouch;

var timeout = 2.Minutes(30.Seconds());
timeout.Should().Be(TimeSpan.FromSeconds(150));

var start = 1.March(2026).At(10, 30).AsUtc();
start.Should().Be(new DateTime(2026, 3, 1, 10, 30, 0, DateTimeKind.Utc));
2.Hours().After(start).Should().Be(start.AddHours(2));
```

## Members

`FluentTimeSpanExtensions`:

- Build a span from a number: `Ticks`, `Microseconds`, `Nanoseconds` (`int` and `long`), `Milliseconds`, `Seconds`, `Minutes`, `Hours`, `Days` (`int` and `double`).
- Add an offset: `Seconds`, `Minutes`, `Hours` and `Days` with a second `TimeSpan` argument, and `And`.
- Read parts of a span: `Microseconds`, `Nanoseconds`, `TotalMicroseconds`, `TotalNanoseconds`.

`FluentDateTimeExtensions`:

- Build a date: `January` to `December`, as in `5.March(2026)`.
- Set the time: `At(TimeSpan)` and `At(hours, minutes, seconds, milliseconds, microseconds, nanoseconds)` on `DateTime` and `DateTimeOffset`. Only `hours` and `minutes` are required.
- Mark or move: `AsUtc`, `AsLocal`, `WithOffset`, `Before`, `After`, `AddNanoseconds`.
- Read parts of a date: `Microsecond`, `Nanosecond`.

## Resolution

`TimeSpan` and `DateTime` count in ticks of 100 ns. A nanosecond argument is rounded to a tick through double, halves to even, so `99.Nanoseconds()` is one tick, `150.Nanoseconds()` is two and `250.Nanoseconds()` is two. Above about 9e17 ns the result can be a few ticks off the exact nearest tick, the same as FluentAssertions.

## AddMicroseconds

FluentAssertions has `AddMicroseconds` for `DateTime` and `DateTimeOffset`. Vouch.Time leaves it out. On .NET 8 and later, the built-in `AddMicroseconds(double)` wins over an extension method, so the FluentAssertions version is never the one called. The built-in method does the same job.

Members named like a built-in property (`Microsecond`, `Microseconds`, `TotalNanoseconds`) stay. Call them with parentheses, as in `span.Microseconds()`, and the compiler picks the extension.
