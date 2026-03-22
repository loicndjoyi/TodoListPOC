# To-Do List POC

## Architecture & Engineering Practices

This Proof of Concept (POC) showcases rigorous software engineering practices, detailed further in the `docs/adr/` directory and the `IMPROVEMENTS.md` file.

* **Backend (.NET 8)**: Layered architecture (Service Layer) oriented around the "Tell, Don't Ask" principle with clear separation of concerns (SRP, DIP). Includes a *Global Exception Handler* (RFC 7807) to return uniform API errors. EF Core is configured with SQLite for embedded persistence with zero external infrastructure dependencies.
* **Frontend (Angular 21)**: Built with *Standalone* components, leveraging *Signals* for reactive local state, and maintaining a clean data flow with a centralized HTTP service and error interceptor.
* **Testing Pyramid**: The architecture is structured for fast unit testing (business logic mocking with *NSubstitute*) and reliable API integration testing (`WebApplicationFactory` in-memory).
* **CI/CD**: Continuous Integration is set up with *GitHub Actions* to validate tests and build both the .NET and Angular projects on every update to the main branch.

## How to run the project

```bash
# Run the Backend
cd src/TodoApp.Api
dotnet run

# Run the Backend tests
dotnet test

# Run the Frontend
cd client
npm install
npm run start
```

Please review the **ADRs (Architecture Decision Records)** in `docs/adr/` for the complete justification of the technology choices and scope of this POC.
