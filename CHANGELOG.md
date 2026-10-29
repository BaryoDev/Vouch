# Changelog

## Unreleased

- `Func<ValueTask>` and `Func<ValueTask<T>>` now have their own `Should` overloads, so `Throw`, `NotThrow`, `ThrowAsync` and `NotThrowAsync` see the exception (#22).
- Failure messages cap every formatted value at 200 characters, and sequences that do not know their size are no longer walked to the end (#24).
- `BeLessThan`, `BeGreaterThan`, `BeLessThanOrEqualTo`, `BeGreaterThanOrEqualTo` and `BeInRange` fail when either side is NaN (#25).
- `BeEquivalentTo` stays fast on graphs that share references and on large unordered collections of types with their own equality (#23).

## 0.1.0

First release.

- `.Should()` for objects, bools, numbers, nullable numbers, strings, dates (`DateTime`, `DateTimeOffset`, `DateOnly`), collections, dictionaries and delegates.
- `BeEquivalentTo` compares member by member, any order unless `WithStrictOrdering`, and lists every difference with its member path.
- `Throw<T>`, `ThrowAsync<T>`, `NotThrow`, `NotThrowAsync`, with `WithMessage`, `WithParameterName` and `Which`.
- `.And`, `.Which` and `.Subject` chains.
- An optional reason string on most assertions, appended to the failure message.
- One `VouchException` type. Vouch references no test framework. Failures show as plain test failures in xUnit v2, xUnit v3, NUnit and MSTest.
- No runtime dependencies. Targets `net8.0` and `net10.0`.
