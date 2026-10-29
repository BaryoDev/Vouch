# Security

## Reporting

**Please do not open a public issue.** Use
[GitHub's private vulnerability reporting](https://github.com/BaryoDev/Vouch/security/advisories/new),
or the contact route in the [BaryoDev policy](https://github.com/BaryoDev/.github/blob/main/SECURITY.md).

Expect an acknowledgement within a few days. This is a small project, so please allow reasonable
time for a fix before disclosing.

## The threat model, in one line

**Vouch runs inside test projects and handles the values the test gives it.** It makes no network
call, launches no process, writes no file and deserialises no types. It has no runtime
dependencies.

## What that means in practice

**Failure messages print the values under test.** If a test asserts on a secret, the secret appears
in the failure output. Vouch does not redact. Each string and each `ToString` result is cut at 200 characters,
but there is no total cap on the output, and cutting is for readability, not for hiding data.

**`BeEquivalentTo` reads members by reflection** and caches them per type. It detects cycles and
has a depth limit. A hostile object graph that makes it hang, overflow the stack or allocate without
bound is **in scope and wanted**, ideally with the smallest graph that shows it.

**`Match`, `MatchRegex` and `WithMessage` run patterns against text.** `MatchRegex` runs the regular
expression you give it, so a pattern that backtracks badly is the caller's pattern. A wildcard
pattern (`*` and `?`) that takes super-linear time on a short input is in scope.

## Not vulnerabilities

- **A failing assertion throws.** That is the library working.
- **A message that contains the value you asserted on.** See above.
- **A regular expression you wrote that is slow.**

## Supported versions

Fixes go to the latest published version on NuGet. There is no long-term support branch.
