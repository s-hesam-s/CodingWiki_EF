# Local verification — 2026-10-28

- All source projects target .NET 10; solution Release build succeeded.
- The isolated suite now passes 24 model/HTTP preview tests; original CRUD and SQL Server execution remain outside that suite.
- Direct and transitive NuGet audit completed with no known advisories reported for the solution and its separate test/preview projects at this snapshot.
- SDK: 10.0.400 on Windows; the local shared runtime is 10.0.11. Source package upgrades do not upgrade that installed runtime. CI selects the current stable .NET 10 SDK/runtime patch; post-publication CI is a separate check.
- Original hosts, database migrations/seeds, payments and cloud consumers were not executed. Build/fixture results do not establish deployment or live integration readiness.

Reproduce using the commands in the README. Audit results depend on the current advisory feed and network access. Existing compiler/nullability warnings may remain. This snapshot is evidence of the stated checks, not a production-readiness certification.

## Isolated behavior regressions

`dotnet test tests/Behavior.Tests/Behavior.Tests.csproj --configuration Release` passes 24 tests: model validation, relational metadata and an in-process read-only preview using the existing Razor views. The SQL Server provider builds model metadata without a connection string; no database is created, queried, seeded or mutated. Original CRUD/console hosts are not run. CI retains the existing required check name and runs this suite explicitly.
