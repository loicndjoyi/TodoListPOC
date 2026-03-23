# Future Improvements

This document lists architectural patterns and elements that were consciously **omitted** from this time-boxed POC, but would be highly relevant in a **production** environment or with a larger development team.

## 1. CQRS / MediatR
**Why it's relevant**: In a distributed system or a larger team, MediatR decouples controllers from logic, simplifies testing, and enables parallel work (Vertical Slices architecture).  
**Why it was omitted**: For a few CRUD endpoints on a single entity, MediatR adds unnecessary ceremony (Commands, Handlers, Validators). A simple DI-injected service provides a much better signal-to-noise ratio.  
**When to adopt**: Once the application grows significantly in use cases or requires cross-cutting pipelines (telemetry, caching).

## 2. DDD (Domain-Driven Design) — Aggregates, Value Objects
**Why it's relevant**: Essential for modeling complex business domains with strong invariants.  
**Why it was omitted**: A generic To-Do List lacks complex business invariants. Applying strict DDD aggregates here would be over-engineering.  
**When to adopt**: When the domain has complex rules (e.g., pricing engines, inventory systems).

## 3. Optimistic Concurrency (RowVersion)
**Why it's relevant**: Essential in multi-user environments to prevent "lost updates".  
**Why it was omitted**: SQLite does not natively support `rowversion`. Simulating it via a Guid updates adds complexity with limited value for a local POC.  
**When to adopt**: When migrating to SQL Server / PostgreSQL in a real production environment.

## 4. FluentValidation (MediatR Pipeline)
**Why it's relevant**: Separates validation from business logic and controllers elegantly via `IPipelineBehavior`.  
**Why it was omitted**: Without MediatR, FluentValidation loses its seamless pipeline integration. Built-in **Data Annotations** on DTOs combined with ASP.NET Core native validation are perfectly sufficient for this scope.  
**When to adopt**: When pairing with MediatR or dealing with complex cross-field/async validations.

## 5. End-to-End Tests (Playwright)
**Why it's relevant**: Guarantees the full user flow works seamlessly (UI → API → DB).  
**Why it was omitted**: The setup overhead for Playwright + Angular is high compared to the immediate value. Priority was given to integration tests which yield higher ROI for a POC.  
**When to adopt**: When CI/CD pipelines require automated UI regression testing for critical paths.

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
- **Circuit Breaker / Retry**: Relevant for inter-service calls (Polly). Not needed for a simple monolith.
- **Event Sourcing**: Excellent for complete audit trails. Severe over-engineering for a To-Do task.

## 7. Security (JWT / OAuth2)
**Why it was omitted**: Kept out of scope to focus on core architecture.  
**Production approach**: Implement JWT middleware with Identity Server or Azure AD B2C, paired with policy-based authorization.

## 8. Advanced Search and Filtering (Partial Title Search)
**Why it's relevant**: Users often need to find tasks by typing a few characters instead of the exact title.
**Why it was omitted**: To adhere strictly to the YAGNI (You Aren't Gonna Need It) principle for a basic POC context. Handling partial searches properly often requires Full-Text Search (FTS) mechanisms or specific database configurations (like `LIKE` operators with lowercasing), which expands the infrastructure scope.
**When to adopt**: When the application grows significantly and users request robust search capabilities across their task lists.
