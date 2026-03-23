# To-Do List POC

🌍 **Live Demo:** [https://todoapp-loic-b5aha8gnfdamhxc4.canadacentral-01.azurewebsites.net/](https://todoapp-loic-b5aha8gnfdamhxc4.canadacentral-01.azurewebsites.net/)

A full-stack To-Do List built as a **Proof of Concept** showcasing modern software engineering practices across .NET 8 and Angular 21.

## Architecture & Engineering Practices

Detailed further in `docs/adr/` (Architecture Decision Records) and `IMPROVEMENTS.md`.

* **Backend (.NET 8)**: Layered architecture (Service Layer) oriented around the "Tell, Don't Ask" principle with clear separation of concerns (SRP, DIP). Includes a *Global Exception Handler* (RFC 7807) to return uniform API errors. EF Core is configured with SQLite for embedded persistence with zero external infrastructure dependencies.
* **Frontend (Angular 21)**: Built with *Standalone* components, leveraging *Signals* for reactive local state with `ChangeDetectionStrategy.OnPush`, and maintaining a clean data flow with a centralized HTTP service and error interceptor.
* **Testing Pyramid**: Unit tests for domain logic (xUnit + NSubstitute + FluentAssertions), integration tests for API endpoints (`WebApplicationFactory` with in-memory SQLite).
* **CI/CD**: GitHub Actions to validate tests and build both projects on every push.

## Project Structure

```
TodoApp/
├── src/
│   ├── TodoApp.Api/           # ASP.NET Core 8 REST API
│   ├── TodoApp.Core/          # Domain entities & interfaces
│   └── TodoApp.Infrastructure/# EF Core, service implementations
├── tests/
│   ├── TodoApp.UnitTests/     # Pure domain logic tests
│   └── TodoApp.IntegrationTests/ # API & service tests (SQLite)
├── client/                    # Angular 21 frontend
│   └── src/app/
│       ├── components/        # Smart & dumb components
│       ├── services/          # API service
│       ├── models/            # TypeScript interfaces
│       └── interceptors/      # HTTP error interceptor
├── docs/
│   ├── adr/                   # Architecture Decision Records
│   └── learning.md            # Technical learnings & patterns
└── IMPROVEMENTS.md            # Future improvements roadmap
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) (includes npm)
- [Angular CLI](https://angular.dev/) (`npm install -g @angular/cli`)

## How to Run

```bash
# 1. Clone the repository
git clone https://github.com/loicndjoyi/TodoListPOC.git
cd TodoListPOC

# 2. Run the Backend (http://localhost:5118)
dotnet run --project src/TodoApp.Api --launch-profile http

# 3. Run the Frontend (http://localhost:4200)
cd client
npm install
ng serve
```

## How to Test

```bash
# Run all .NET tests (unit + integration)
dotnet test

# Type-check the Angular project
cd client
npx tsc --noEmit
```

## Documentation

| Document | Description |
|---|---|
| [`docs/adr/`](docs/adr/) | Architecture Decision Records |
| [`docs/learning.md`](docs/learning.md) | Technical learnings & patterns |
| [`IMPROVEMENTS.md`](IMPROVEMENTS.md) | Future improvements roadmap |
