# Changelog

## Unreleased

Breaking changes come first. All of them land before 1.0, as the roadmap in #20 planned.

### Breaking

- The `BeEquivalentTo` configure callback takes `EquivalencyOptions<TExpectation>`, and the non-generic `EquivalencyOptions` is internal (#2). `x.Should().BeEquivalentTo(y, o => o.WithStrictOrdering())` compiles unchanged. Code that names `EquivalencyOptions` in a callback type does not.
- `NotThrow()` on a `Func<T>` returns `AndWhichConstraint<FunctionAssertions<T>, T>`, so `.Which` is the returned value (#1, #11).
- `Guid`, `char`, `sbyte`, `ushort`, `uint`, `ulong`, `Half`, `TimeOnly`, `Task`, `Task<T>`, `Stream`, `ImmutableArray<T>` and non-generic `IEnumerable` have their own `Should()` overloads (#11, #15, #17, #18). Every call that compiled before still compiles. Code that names the old assertion type, such as `ValueAssertions<Guid>`, does not.
- `MatchRegex(null)` and `BeParsableInto<T>(null)` are now ambiguous between overloads (#14).
- Keyed `BeInAscendingOrder` and `BeInDescendingOrder` return an ordering constraint that converts implicitly to the old `AndConstraint<TSelf>`. Source compiles unchanged, and binaries built against 0.2.0 need a rebuild (#15).
- A NaN or negative delta in `BeCloseTo`, `NotBeCloseTo` and `BeApproximately` throws `ArgumentOutOfRangeException`. Before, `BeApproximately` accepted a NaN delta when the values were equal (#17).
- Messages print a `TimeOnly` in round-trip format ("o"), including when it is nested in an object or a collection (#11).

### Added

- `AssertionScope` collects every failure inside a `using` block and throws one exception on dispose. It supports nested scopes, flows across `await`, keeps parallel tests apart, and has `Discard()` (#8). This builds on a single failure path that every assertion now uses (#7).
- `VouchException` is unsealed, and `VouchConfiguration.ExceptionFactory` lets an adapter package throw the test framework's own exception (#12). `because` stays a single string, and arguments go in by string interpolation.
- A public API for custom assertions: `Vouch.Sdk.Assertion.Fail(message, because)`, `Assertion.Name(expression)`, and `Expression` on each assertion type. Custom assertions obey `AssertionScope`, `because` and the exception factory (#9).
- Equivalency options: `Excluding`, `Including`, `ExcludingMissingMembers`, `WithMapping`, `Using` (for a type or a member), `ComparingByMembers`, `ComparingByValue`, `WithoutStrictOrdering`, `IgnoringCyclicReferences`, `WithMaxDepth` and `RespectingRuntimeTypes` (#10).
- Value formatters: `IValueFormatter`, `VouchConfiguration.Formatters`, `[ValueFormatter]` with `AddFormattersFrom(assembly)`, and `VouchConfiguration.Formatting` (`MaxDepth`, `MaxItems`, `MaxLines`) (#19).
- Strings: occurrence constraints (`Exactly`, `AtLeast`, `AtMost`, `MoreThan`, `LessThan`), `StartWithEquivalentOf`, `EndWithEquivalentOf`, `MatchEquivalentOf`, `BeUpperCased`, `BeLowerCased`, `BeNullOrWhiteSpace`, `NotMatch`, `MatchRegex` and `NotMatchRegex` with a `Regex` or options (1 second timeout), `BeParsableInto`, `NotEndWith`, `ContainAll`, `ContainAny`, `NotContainAll`, `NotContainAny`, and the Not forms (#13, #14).
- Collections: `HaveElementPreceding`, `HaveElementSucceeding`, `ContainEquivalentOf`, `NotBeEquivalentTo`, `SatisfyRespectively`, `Satisfy`, `AllBe`, `AllBeOfType`, `AllBeAssignableTo`, `AllBeEquivalentTo`, `ContainInOrder`, `ContainInConsecutiveOrder`, `NotHaveCount`, `HaveCountLessThan`, `HaveCountLessThanOrEqualTo`, `HaveSameCount`, `IntersectWith`, `NotIntersectWith`, `NotContainNulls`, `ThenBeInAscendingOrder`, `ThenBeInDescendingOrder`, and `HaveElementAt` returning the element as `Which` (#11, #13, #15).
- Dictionaries: `ContainValue`, `ContainValues`, `NotContainValue`, `NotContainValues`, `Contain` and `NotContain` for pairs, `Equal`, `NotEqual`, `WhoseValue` and `NotContainKeys` (#13, #16).
- Numbers, dates and nullables: `BeCloseTo`, `NotBeCloseTo`, `BeNegative`, `BeNaN`, `BeFinite` and their Not forms; `HaveValue` and `NotHaveValue`; `BeSameDateAs`; `HaveYear` through `HaveSecond`; and `BeWithin(span).Before` and `.After` (#17).
- Guid, enum, char, unsigned, `TimeOnly` and `TimeSpan` assertions: `BeEmpty` and `NotBeEmpty` on `Guid`; `BeDefined`, `HaveFlag`, `HaveValue`, `HaveSameNameAs` and `HaveSameValueAs` on enums, with no boxing; `BeCloseTo`, `BePositive` and `BeNegative` on `TimeSpan` (#11).
- Tasks, timing and streams: `CompleteWithinAsync`, `NotCompleteWithinAsync`, `ThrowExactly`, `ThrowExactlyAsync`, `ThrowWithinAsync`, `NotThrowAfter`, `Where` on exception assertions, `WithInnerException` and `WithInnerExceptionExactly`, `ExecutionTime` and `ExecutionTimeOf`, and the stream assertions (#13, #18).

### Changed

- Shared build settings moved to `Directory.Build.props` (#6).
- The README benchmark table shows current Vouch memory figures. One record allocates 1.56 KB, not 960 B, and 100 reordered records 161 KB, not 89 KB. The 5x targets against FluentAssertions still hold.

## 0.2.0

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
