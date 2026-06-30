# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build                              # build the solution
dotnet run --project Todo.Web             # run the API
dotnet test                               # run all tests
dotnet test --filter "FullyQualifiedName~CreateTodoListItem_WhenCalled"   # run a single test by name

dotnet tool restore                       # restore local tools (csharpier, dotnet-ef) — required before the commands below
dotnet csharpier .                        # format all code
dotnet csharpier --check .                # verify formatting (CI fails if this fails)

dotnet ef migrations add <Name> --project Todo.Persistence --startup-project Todo.Web
dotnet ef database update --project Todo.Persistence --startup-project Todo.Web
```

CI (`.github/workflows/ci.yaml`) runs `dotnet csharpier --check .` and `dotnet build --configuration Release` + `dotnet test`. Run csharpier before committing or the build will fail on formatting.

The database is SQL Server. The `.devcontainer` provisions it; the connection string named `TodoContext` lives in `Todo.Web/appsettings.Development.json`. Outside the devcontainer, provision SQL Server and update that connection string.

## Architecture

Clean Architecture, .NET 8. Project dependencies point inward — `Todo.Domain` has no dependencies; everything depends toward it:

- **Todo.Domain** — entities only (`TodoList`, `TodoListItem`). No framework dependencies.
- **Todo.Application** — use cases as MediatR CQRS handlers. Defines abstractions the outer layers implement: `IApplicationDbContext`, `IBulkCompletionService`, `ITodoCompletionNotifier`. Depends only on Domain.
- **Todo.Persistence** — EF Core. `TodoContext` implements `IApplicationDbContext`; entity configs in `Configurations/`; migrations in `Migrations/`.
- **Todo.Infrastructure** — non-persistence infrastructure (the background `BulkCompletionService`). Composes Persistence via `AddInfrastructure`.
- **Todo.Web** — ASP.NET Core controllers, SignalR hub, middleware. Depends on Application + Infrastructure.
- **Todo.Application.Tests** — xUnit tests.

Each layer exposes a `DependencyInjection` static class (`AddApplication`, `AddInfrastructure`, `AddPersistence`) wired together in `Todo.Web/Program.cs`. **Exception:** `ITodoCompletionNotifier` is registered directly in `Program.cs` (not in a DI extension) because its SignalR implementation lives in the Web layer.

### CQRS / MediatR

Every operation is a `Command` or `Query` + a `Handler`, organized in feature folders (e.g. `TodoListItems/Commands/CreateTodoListItem/`). Controllers are thin: they inject `IMediator` and only `Send` commands/queries — no business logic. MediatR handlers are auto-registered by assembly scan in `AddApplication`. Handlers depend on `IApplicationDbContext`, never on `TodoContext` directly, which is what lets tests swap in EF InMemory.

### Error handling

Handlers throw `NotFoundException` (in `Application/Common/Exceptions`) when an entity is missing. `ExceptionHandlingMiddleware` (registered first in the pipeline) catches it and returns 404. Don't return `NotFound()` from new handler logic for missing entities — throw `NotFoundException` instead.

### Bulk completion (background jobs + real-time updates)

`BulkCompletionService` (singleton, `Todo.Infrastructure`) completes all incomplete items of a list:
- Jobs are **fire-and-forget** (`Task.Run`) and outlive the HTTP request that started them.
- Each job opens its **own DI scope** via `IServiceScopeFactory` to get a fresh scoped `IApplicationDbContext` (the singleton can't capture a scoped DbContext).
- Items are completed in batches of `BatchSize` (100); each pass re-queries incomplete items.
- Job state lives **in memory** in `ConcurrentDictionary` registries (job progress + cancellation tokens) — it is not persisted, so it does not survive a restart.
- Progress is streamed through the `ITodoCompletionNotifier` abstraction, which keeps Infrastructure free of any SignalR/web dependency.

`Todo.Web/Hubs/` implements that abstraction over SignalR: `TodoCompletionNotifier` broadcasts to the `todo-{todoListId}` group; clients connect to the `/hubs/todo-completion` hub and call `JoinTodoGroup`/`LeaveTodoGroup`. Final lifecycle notifications (completed/failed/cancelled) are sent with `CancellationToken.None` so they go out even when the job token is cancelled.

### Persistence conventions

`TodoContext.OnModelCreating` maps each entity to a table named after its CLR type (singular, e.g. `TodoList`) and applies all `IEntityTypeConfiguration` classes from the assembly. Add new entity config in `Todo.Persistence/Configurations/`.

## Testing

xUnit. Handlers are tested directly (not through HTTP) by constructing a `TodoContext` over `UseInMemoryDatabase(Guid.NewGuid())` — a fresh isolated store per test — and passing it to the handler. Follow the existing `DatabaseContextOptions()` / `PopulateDatabaseContext()` pattern in the test classes.

## API routes

- `api/todolists` and `api/todolists/{todoListId}/items` — CRUD.
- `api/todo-items/{todoListId}/bulk-complete` (POST, returns a `jobId`), `api/todo-items/bulk-jobs/{jobId}/progress` (GET), `api/todo-items/bulk-jobs/{jobId}` (DELETE to cancel).
