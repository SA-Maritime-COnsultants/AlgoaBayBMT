# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AlgoaBayBMT is a Blazor Server (.NET 10, Interactive Server Components) enterprise maritime application for bunker management, crew/vessel compliance, training, billing, regulatory reporting, and emergency (oil spill) response. It uses ASP.NET Core Identity (with roles) and EF Core / SQL Server.

Solution layout (`AlgoaBayBMT.slnx`):
- `AlgoaBayBMT/` — the Blazor Server web app (UI, services, data access, migrations)
- `AlgoaBayBMT.Shared/` — shared class library: domain entity models (`Models/`) and security constants (`Security/`)

There are no automated test projects in this repo.

## Build & Run

```bash
dotnet build AlgoaBayBMT.slnx
dotnet run --project AlgoaBayBMT/AlgoaBayBMT.csproj
```

EF Core migrations (run from `AlgoaBayBMT/`):
```bash
dotnet ef migrations add <Name> --project AlgoaBayBMT.csproj
dotnet ef database update --project AlgoaBayBMT.csproj
```
Migrations are applied automatically at startup via `dbContext.Database.MigrateAsync()` in `Program.cs`.

## Architecture

### Service layer pattern
Almost all business logic lives in `AlgoaBayBMT/Services/*Service.cs`, each with a matching interface in `Services/Interfaces/`, registered as scoped services in `Program.cs`. Request/response DTOs for services live in `Services/Models/`.

- Services that need EF Core **must** take `IDbContextFactory<ApplicationDbContext>` and call `CreateDbContextAsync()` per operation rather than injecting `ApplicationDbContext` directly — this avoids concurrency issues with Blazor Server's single-circuit components.
- Many service methods return `OperationResult` / `OperationResult<T>` (from `AlgoaBayBMT.Shared.Models`) to convey success/failure + messages to the UI instead of throwing.

### Emergency / Oil Spill module
`AlgoaBayBMT/Emergency/OilSpill/` is a self-contained vertical slice (DTOs, Models, Services, Visualization) registered via its own `ServiceCollectionExtensions.AddOilSpillModule()` extension called from `Program.cs`. Use this as the template for adding new self-contained feature modules — don't scatter its types into the top-level `Services`/`Models` folders.

### Data layer
- `Data/ApplicationDbContext.cs` — single `IdentityDbContext<ApplicationUser>` with `DbSet<>` properties grouped by module (training/authoring, bunkering/ports/operators, crewing/compliance, billing/notifications, ISGOTT checklists, oil spill). When adding entities, add the `DbSet` under the relevant comment-grouped section and configure relationships in `OnModelCreating`.
- `Data/ApplicationUser.cs` — extended Identity user (crew rank, SID numbers, approval workflow fields, profile picture, etc.)
- `Data/IdentitySeedData.cs` — seeds roles/initial admin on startup (`IdentitySeedData.EnsureSeedDataAsync`).
- Shared domain entities live in `AlgoaBayBMT.Shared/Models/` (e.g. `CrewingEntities.cs`, `BillingEntities.cs`, `BunkerOperationsEntities.cs`, `TrainingCatalogEntities.cs`, `TrainingProgressEntities.cs`, `Enums.cs`).

### Authorization
- Role constants: `AlgoaBayBMT.Shared.Security.RoleNames` (e.g. `Admin`, `CompanyManager`, `Captain`, `Co`, `Crew`, `Dffe`, `Tnpa`, `Samsa`, etc.), including grouped sets like `RoleNames.CrewCommand` and `RoleNames.AuditAuthorities`.
- Policy constants: `AlgoaBayBMT.Shared.Security.PolicyNames` (e.g. `AdminOnly`, `CrewComplianceManagement`, `TrainingApproval`, `BillingManagement`). Policies are registered in `Program.cs` via `AddAuthorization`.
- When adding new authorization checks, prefer adding/reusing a named policy over inline role checks, and keep role/policy strings centralized in these two files.

### Background services
`Services/Background/` contains `IHostedService` implementations (`CertificateExpiryWatcher`, `NotificationDispatcher`, `ComplianceRevalidator`) registered in `Program.cs` for periodic compliance/notification work.

### Training media & uploads
- Uploaded files live under `wwwroot/uploads` (excluded from `Content` items, included as `None` in the csproj). The directory is created at startup and served via a dedicated `PhysicalFileProvider` at `/uploads` with custom video content-type mappings.
- `TrainingAssetStorageService` handles saving rich-text-editor images and resolving content types; the `/training-media/rte-images` and `/training-media/stream` minimal API endpoints in `Program.cs` are the upload/stream entry points and validate that streamed paths start with `uploads/training`.

### UI / Blazor conventions
- Pages live under `Components/Pages/<Area>/` (Admin, Billing, Bunkering, Crewing, Emergency, Regulatory, Training, TrainingManagement, UserManagement). Layout shell is in `Components/Layout/` (`MainLayout`, `Sidebar`, `TopBar`, `NavMenu`, `GlobalHeader`).
- Use Syncfusion Blazor components (`Syncfusion.Blazor.*` packages) for grids, forms, charts, maps, calendars, rich text editing, etc. — keep new UI consistent with the existing navy/teal "Portflow" enterprise dashboard styling and compact, icon-only action buttons with tooltips on grids.
- The "my-training" page is the visual baseline for restyling work — match its color/spacing conventions for new pages.
- Maintain existing routes, authorization attributes, and business logic when restyling; only change what's requested.

### Licensing
Syncfusion license is registered in `Program.cs` via `Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(...)`. Do not remove/alter this.
