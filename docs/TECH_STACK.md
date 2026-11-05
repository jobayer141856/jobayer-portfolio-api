# Backend Technology Stack

**Application:** Portfolio REST API (`jobayer-portfolio-api`)  
**Status:** Proposed baseline

## Stack Overview

| Area | Technology | Purpose |
|---|---|---|
| Runtime & Framework | ASP.NET Core Web API (.NET 10) | HTTP REST API |
| Language | C# (Nullable reference types enabled) | Strongly typed backend logic |
| Database | PostgreSQL 16+ | Primary relational & `jsonb` persistence |
| ORM | Entity Framework Core + `Npgsql` | Data access, LINQ projections, migrations |
| Validation | FluentValidation | Command and request DTO validation |
| CQRS / Mediation | Cortex.Mediator / Mapster | Decoupled feature vertical slices & object mapping |
| Authentication | ASP.NET Core JWT Bearer + Refresh Tokens | Admin authentication & session rotation |
| API Documentation | OpenAPI (`Microsoft.AspNetCore.OpenApi` + Scalar / Swagger UI) | Contract sharing with Angular apps |
| Caching | ASP.NET Core `OutputCache` | Tag-based public endpoint caching & eviction |
| Rate Limiting | `Microsoft.AspNetCore.RateLimiting` | Contact form and login protection |
| Logging | Serilog (`Serilog.AspNetCore`) | Structured JSON and console logging |
| Testing | xUnit + FluentAssertions + Testcontainers (`PostgreSQL`) | Unit and real-database integration tests |
| Containerization | Docker & Docker Compose | Local PostgreSQL + API orchestration |
