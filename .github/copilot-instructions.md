# MountyPython Copilot Instructions

## Toolchain and commands

- The solution targets .NET 10; `global.json` selects Microsoft.Testing.Platform. CI uses .NET `10.0.x` and Node `24.x`.
- Restore and build the full solution:
  ```powershell
  dotnet workload restore
  dotnet restore
  dotnet build MountyPython.slnx --configuration Release
  ```
- Run all .NET tests:
  ```powershell
  dotnet test MountyPython.slnx --configuration Release
  ```
  Run one xUnit test:
  ```powershell
  dotnet test Backend\Shared\Commons.Test\Commons.Test.csproj --filter "FullyQualifiedName~Commons.Test.Domain.GenerateUniqueIDTests.It_Should_Generate_Unique_Id_Using_RNGCrypto"
  ```
- Run the distributed application locally (Docker is required for PostgreSQL, RabbitMQ, and Keycloak):
  ```powershell
  dotnet run --project MountyPython.AppHost\MountyPython.AppHost.csproj
  ```
- For the Angular client, work from `Clients\mountypython.client`. The committed lockfile and `packageManager` field select npm 12:
  ```powershell
  npm ci
  npx ng build
  npx ng test --watch=false
  npx ng test --include=src/app/app.spec.ts --watch=false
  ```
  There is no separate lint script configured.

## Architecture

- `MountyPython.AppHost\AppHost.cs` is the composition root. It provisions PostgreSQL, RabbitMQ, Keycloak, and three API resources: inventory, operative, and notification services. The inventory and operative services each own a separate PostgreSQL database; services communicate asynchronously through RabbitMQ/Rebus.
- The AppHost is also the source of service-discovery configuration. Preserve resource names and their matching connection names: `mountypython-inventorydb`, `mountypython-operativedb`, `rabbit`, and the service names consumed by the Angular proxy (`inventory-services`, `operative-services`, `notification-services`).
- Locally, the Angular app runs as an Aspire JavaScript resource on port 4200. Its `src\proxy.conf.js` maps `/api/catalogs`, `/api/operations`, and notification/SignalR paths to service-discovery environment variables. During publishing, Aspire serves the built client through YARP and routes those same API paths plus Keycloak paths.
- `MountyPython.ServiceDefaults` supplies OpenTelemetry, service discovery, resilient `HttpClient`s, and development-only `/health` and `/alive` endpoints. Every API service calls `AddServiceDefaults()` and should retain that composition.
- Backend projects follow vertical slices: feature modules in `Features\<area>` expose `Map...Endpoints` extension methods that create tagged route groups. `Program.cs` owns startup wiring and maps those modules. Shared cross-cutting infrastructure belongs in `Backend\Shared\Commons`; DTO/contracts projects are kept alongside the owning service area.
- EF Core migrations run on startup of the service, using the MigratorHostedService<TContext> that applies migrations from the service assembly. The service assembly should contain the DbContext and its migrations, not a separate project.

## Repository conventions

- API services use Minimal APIs, JWT authentication/authorization, FluentValidation assembly scanning, OpenAPI/Scalar in development, Serilog setup through `SetupLogging()`, and Rebus handlers discovered from the service assembly. Inventory and operative services also register `MigratorHostedService<TContext>` for database migrations.
- Add database contexts through `SetupNpgsqlServerDbContext<TContext>(connectionName)` rather than configuring EF Core independently; it preserves Aspire connection-string integration and the repository's development-only EF diagnostics.
- Use the existing test stack: xUnit v3 on Microsoft.Testing.Platform, with AwesomeAssertions (`value.Should()...`). Test projects use `<UseMicrosoftTestingPlatformRunner>true</UseMicrosoftTestingPlatformRunner>`.
- The Angular app is NgModule-based, not standalone: components are declared in `AppModule`, and generated components are configured with `standalone: false`. Follow `Clients\mountypython.client\.editorconfig`: two spaces and single quotes in TypeScript.
- CI builds and tests every push. CD deploys `dev-branch` to development, tags the verified commit, then promotes that exact tag from the merge commit's second parent. Keep the CD workflow's production branch filter (`main`) and its production job guard (`master`) aligned when changing branch policy; preserve the AppHost's publish-mode resource and route configuration when changing deployment behavior.

## Key Conventions
- Queries should return directly Aggregate DTO, and not Domain Entities, However Commads can return Domain Entities, and the Controller maps them to DTOs.
- Queries should look like I<<Action>>Query and kept in the same file with the implementation: <<Action>>Query and defined in <<Project>>/Features/<<Feature>>/Queries. Each query in a separate file.
- Commands should look like I<<Action>>Command and kept in the same file with the implementation: <<Action>>Command and defined in <<Project>>/Features/<<Feature>>/Commands. Each command in a separate file.
- Consumers should look like I<<Action>>Consumer and kept in the same file with the implementation: <<Action>>Consumer and defined in <<Project>>/Features/<<Feature>>/Consumers. Each consumer in a separate file.
- Ask before changing the Domain Models.

## Frontend Conventions
- design: mobile-first, desktop-second
- use Signal-Forms for forms, and use NgRx for state management.
- The frontend should be structured in a way that separates concerns and promotes maintainability.
- The frontend should reflect the backend's vertical slice architecture, with feature modules corresponding to backend services.
- Use default icons from Angular Material
- Logo should be a simple SVG based on the jpegs from 'Solution Items'

## Keycloak Conventions
- Use Keycloak for authentication and authorization.
- Create a realm in already defined but empty 'import.json' in the root of the project.
- Create a theme based on Mounty Python's branding'.
- Add 3 test users to the realm, 2 for users/customer (TestUser1, TestUser2) and 1 for manager role (TestManager).