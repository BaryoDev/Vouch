# Changelog

## Unreleased

- `Func<ValueTask>` and `Func<ValueTask<T>>` now have their own `Should` overloads, so `Throw`, `NotThrow`, `ThrowAsync` and `NotThrowAsync` see the exception (#22). `.Which` after `NotThrow()` on a `Func<ValueTask<T>>` is now `T` instead of the `ValueTask`, and the sync `Throw` and `NotThrow` run the delegate on a thread-pool thread.
- Failure messages cut each string and each `ToString` result at 200 characters, and sequences that do not know their size are no longer walked to the end (#24). A cut never splits a surrogate pair.
- `Half.NaN.Should().BeLessThan(...)` and the other three ordering assertions on values without a numeric `Should` overload now fail when either side is NaN (#25).
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
