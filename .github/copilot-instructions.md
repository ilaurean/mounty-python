# AI Metering repository instructions

## Scope

This application should implement Energy Meter Analytics based on AI. Should analyse, calculate the max, mediu, max usage per meter or per address, forecast, detect anomalies, checkbox the quality of data using streaming of data (kafka). This application should help decision making, like: when to save energy (battery or other mediums) for future use, when to use energy from the provider.

This should use among a classical UI also using the natural-language querying, RAG from aggregated or raw data, and AI-Agent that uses the tools and analyse the data.

First implementation should be base on Open-Source Models, Agents, Ollamam-Platform.
Future versions should be based on Microsoft Foundry and/or Microsoft AI Search.

## Build, test, and run

- The solution targets .NET 10. Restore workloads and packages before a clean .NET build:
  ```powershell
  dotnet workload restore
  dotnet restore
  dotnet build AiMetering.slnx --configuration Release
  ```
- Run all backend tests:
  ```powershell
  dotnet test Backend\Metering.Tests\Metering.Tests.csproj --configuration Release
  ```
  Run one xUnit.V3 MTP test by its fully qualified name:
  ```powershell
  dotnet test Backend\Metering.Tests\Metering.Tests.csproj --filter "FullyQualifiedName~Metering.Tests.Features.MeterReadings.Commands.GetConsumptionCommandTests.Should_Return_Empty_Consumption_When_No_Readings"
  ```
- The Angular client is under `Clients\metering.client`. Install its locked dependencies with the compatibility flag used by the AppHost, then build or test:
  ```powershell
  cd Clients\metering.client
  npm ci --legacy-peer-deps
  npm run build
  npm test
  ```
  Run one frontend spec with:
  ```powershell
  npm test -- --watch=false --include=src/app/app.spec.ts
  ```
- Start the complete local system through Aspire (Docker is required for PostgreSQL and Kafka):
  ```powershell
  aspire start
  ```
  The AppHost starts the API, Angular dev server, PostgreSQL, Kafka, Kafka UI, and Aspire dashboard. Do not run the AppHost with `dotnet run`.

## Architecture

- `Metering.AppHost` composes the distributed application. It supplies the `meteringdb` PostgreSQL connection and `kafka` connection to `Backend\Metering.Server`, starts the Angular client as an npm application, and creates an Azure Container Apps/YARP deployment topology when publishing.
- `Backend\Metering.Server` is a .NET minimal API. `Program.cs` owns infrastructure registration: Npgsql/EF Core, Service Defaults, migrations, Kafka clients, hosted services, commands, and the endpoint modules. Production database migration is performed by `MigratorHostedService` at application startup.
- Backend behavior is organized as vertical slices beneath `Features`. `MeterReadingsModule` owns the REST endpoints and commands for imports, snapshots, meter IDs, paged readings, and consumption. `MeterReadingStreamingModule` owns the Kafka-backed import endpoint.
- Direct import receives an in-memory JSON list and bulk-upserts it. Streaming import deserializes the request incrementally, publishes readings to Kafka keyed by meter ID, then flows through `KafkaMeterReadingConsumer` (batches of up to 1,000), `MeterReadingIngestionChannel`, and `MeterReadingIngestionService` (500 ms flush window, 60,000-item cap) before validation and bulk upsert.
- The Angular SPA in `Clients\metering.client` calls `/api/meters` through `MeterReadingsService`. During local development its proxy routes API requests to the server; during publishing, YARP serves the static client and proxies `/api/**` to the API.

## Repository conventions

- Add backend operations inside the owning feature slice: define a query/contract and `IXxxCommand` command implementation, register the command in `Program.cs`, and expose it through that slice's `Map...Endpoints` extension method. Commands compose server-side queries through `IMeterReadingRepository.Entities`; the repository returns `AsNoTracking()` queries.
- Preserve meter-reading invariants. Create entities through `MeterReading.Create` and update values with `UpdateValue`; these reject blank meter IDs and non-positive values and normalize times to UTC. Storage has a unique `(MeterId, Time)` key, and bulk upserts may update only `Value`, leaving identity, creation date, and concurrency timestamp intact.
- Both import paths must preserve validation semantics. `MeterReadingValidationFilter` is a singleton with cross-batch per-meter state: forward readings must be non-decreasing, while late readings are accepted only when their value is no greater than the current frontier and do not move that frontier. Keep it in the ingestion pipeline for any new ingestion route.
- Keep streaming work decoupled from request handling: the endpoint publishes to Kafka, the consumer writes batches to the channel, and the ingestion service creates a new scope per database flush because `EmsDbContext` and the repository are scoped.
- Angular uses standalone components, direct Angular Material imports in each component, component SCSS, and signal-based state plus `effect()` for cross-signal reactions. Do not introduce `ReactiveFormsModule`, `FormGroup`, `FormControl`, `toSignal`, `computed`, or NgModules for the meter-reading UI.
- The frontend test runner is Vitest; backend tests use xUnit v3 with Moq and FluentAssertions. No standalone lint command is configured.
