# Future Improvements

This document lists patterns that were consciously **omitted** from this POC but would be relevant if the scope evolved beyond a single-entity CRUD API.

## 1. CQRS / MediatR
**Our context**: The API has 4 endpoints operating on a single `TodoItem` entity. Each endpoint maps directly to one `TodoService` method.  
**Why it was omitted**: MediatR introduces a Command/Handler indirection per endpoint. With only 4 use cases and no cross-cutting pipeline needs (logging, caching, validation), the `ITaskService` → `TaskService` DI pattern provides the same decoupling with less ceremony.  
**When it would matter here**: If we added user-specific todo lists, category filtering, and search — the service would grow into a "God Service" and splitting into vertical slices via MediatR would restore the SRP.

## 2. DDD — Aggregates & Value Objects
**Our context**: `TodoItem` has a `Title` (string), `IsCompleted` (bool), and timestamps. There are no complex invariants between multiple entities.  
**Why it was omitted**: A `Title` Value Object wrapping a simple string with max-length validation adds a class, equality overrides, and EF Core conversion config — all to protect a single property that is already validated at the DTO level by Data Annotations. The cost/benefit ratio is negative at this scale.  
**What we did keep**: The core TDA principle — `TodoItem.Complete()` and `TodoItem.UpdateTitle()` encapsulate state transitions instead of exposing raw setters.

## 3. Optimistic Concurrency (RowVersion)
**Our context**: The app is single-user. There is no scenario where two users concurrently edit the same `TodoItem`.  
**Why it was omitted**: SQLite does not natively support `rowversion`/`TIMESTAMP`. Simulating it with a `Guid ConcurrencyToken` requires an extra column, a Fluent API `IsConcurrencyToken()` config, and `DbUpdateConcurrencyException` handling — all to protect against a conflict that cannot occur in a single-user POC.  
**When it would matter here**: If we added multi-user support (JWT auth + shared lists), this becomes mandatory to prevent silent overwrites.

## 4. FluentValidation
**Our context**: Validation rules are: `Title` is required and max 200 characters. These are expressed in two Data Annotations on `CreateTodoRequest` and `UpdateTodoRequest`.  
**Why it was omitted**: FluentValidation shines when paired with MediatR's `IPipelineBehavior` for cross-field or async validations (e.g., "title must be unique across all user's todos"). For two simple `[Required]` / `[MaxLength]` rules, the built-in ASP.NET Core model validation is perfectly sufficient and requires zero additional dependencies.  
**When it would matter here**: If we introduced conditional rules like "a completed todo cannot be edited" or async uniqueness checks against the database.

## 5. End-to-End Tests (Playwright)
**Our context**: The UI has 5 user interactions (Add, Edit, Delete, Complete, Uncomplete) and 3 visual states (Loading, Empty, List).  
**Why it was omitted**: We prioritized integration tests (`WebApplicationFactory` + in-memory SQLite) which validate the full API pipeline (routing → controller → service → EF Core → DB) with sub-second execution. Adding Playwright requires a browser runtime in CI, significantly increasing pipeline duration for a POC with no complex multi-step user flows.  
**When it would matter here**: If we added multi-page navigation, drag-and-drop reordering, or authentication flows where the UI state machine becomes non-trivial.

### Proposed E2E Test Cases

| # | Scenario | Steps | Expected Result |
|---|---|---|---|
| 1 | **Add a todo** | Type "Buy milk" → click Add | Item appears in list, counter shows "0 done · 1 remaining" |
| 2 | **Add with validation** | Leave input empty → click Add | "Title is required." error is shown, no item created |
| 3 | **Max length validation** | Type 201 characters → blur | "Title cannot exceed 200 characters." error is shown |
| 4 | **Complete a todo** | Check the checkbox | Strikethrough applied, done counter increments |
| 5 | **Uncomplete a todo** | Uncheck a completed item | Strikethrough removed, remaining counter increments |
| 6 | **Edit a todo** | Click ✏️ → change title → click Update | List reflects the new title, edit form closes |
| 7 | **Cancel edit** | Click ✏️ → click Cancel | Original title preserved, edit form closes |
| 8 | **Delete a todo** | Click 🗑️ | Item removed from list, counters update |
| 9 | **Empty state** | Delete all todos | "🎉 Nothing to do!" message displayed |
| 10 | **Persistence** | Add a todo → refresh the browser | Item still present after reload |
| 11 | **API down** | Stop the backend → refresh | Error banner "Could not load todos. Is the API running?" shown |

## 6. Cloud Design Patterns
**Our context**: The app is a single monolith — one .NET process serves both the API and the Angular static files. There are no inter-service HTTP calls.  
**Why they were omitted**: Patterns like Circuit Breaker (Polly) and Retry protect against transient failures between distributed services. Event Sourcing provides an audit trail of every state change. Neither applies to a single-process app talking directly to an embedded SQLite file.  
**When they would matter here**: If we extracted the todo storage into a separate microservice, or needed a full history of every edit a user made to a task.

## 7. Security (JWT / OAuth2)
**Our context**: There is no concept of "user" in this POC. All todos are globally shared.  
**Why it was omitted**: Authentication and authorization require an identity provider (Azure AD B2C, Auth0), token middleware, policy-based `[Authorize]` attributes, and per-user data isolation (`WHERE UserId = @id`). This is an entire vertical feature, not a quick addition, and it would obscure the core architectural patterns this POC is designed to demonstrate.  
**When it would matter here**: Immediately, if this moved beyond a demo. It is the first production requirement.

## 8. Advanced Search and Filtering
**Our context**: The API returns all `TodoItem` records via `GET /api/todos` with no query parameters. The dataset is expected to be small (personal task list).  
**Why it was omitted**: Adding `?title=buy&status=completed` requires query composition (`IQueryable` filtering), pagination (`Skip/Take`), and potentially Full-Text Search for partial matching. For a list that will realistically contain 10–50 items, client-side filtering is simpler and faster.  
**When it would matter here**: If the app supported multiple users with hundreds of tasks each, server-side filtering and pagination become essential for performance.

---

## Known Limitations

### Mobile Responsiveness
**Our context**: The UI was designed and tested on desktop viewports (≥640px). On smaller screens, the todo item layout (checkbox + title + action buttons) can overflow or wrap awkwardly.  
**Why it wasn't addressed**: The time constraint was spent on architecture, testing, and deployment. CSS media queries and touch-friendly hit targets are a polish step that doesn't affect the core engineering patterns this POC demonstrates.  
**What would be needed**: A `@media (max-width: 480px)` breakpoint to stack the form vertically, hide action button text behind a kebab menu, and increase touch target sizes to 44×44px (WCAG 2.5.5).

### Logging & Observability
**Our context**: The API uses `ILogger` implicitly through the `GlobalExceptionHandler`, but there is no structured logging, no correlation IDs, and no telemetry pipeline.  
**Why it wasn't addressed**: For a POC running on a Free Azure tier with no real traffic, the console output is sufficient for debugging. Adding OpenTelemetry, Application Insights, or Serilog would add dependencies and configuration without a clear consumer of that data.  
**What a production setup would look like**: Serilog with structured JSON output → Azure Application Insights (or OpenTelemetry Collector), with correlation IDs propagated via `HttpContext.TraceIdentifier` for end-to-end request tracing.
