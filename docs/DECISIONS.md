# Backend Architecture and Product Decisions

## Initial Decisions

### DEC-API-001 — Separate Public and Admin Route Groups and DTOs
- **Status:** Accepted
- **Date:** 2026-11-05
- **Context:** Public visitors must never see draft records, internal timestamps, or private fields (such as unapproved CGPA or email), while the admin dashboard needs full entity state.
- **Decision:** Define distinct route groups (`/api/v1/*` vs `/api/v1/admin/*`) and separate Public vs Admin Response DTOs.
- **Consequences:** Impossible to accidentally leak admin-only fields on public endpoints by adding a property to an admin model.

### DEC-API-002 — PostgreSQL with EF Core and Native `jsonb` / Array Columns
- **Status:** Accepted
- **Date:** 2026-11-05
- **Context:** Portfolio entities are relational (Projects, MediaAssets, SkillCategories), while ordered string lists (responsibilities, outcomes, authors, technology tags) benefit from native PostgreSQL `text[]` and `jsonb` without unnecessary join-table overhead.
- **Decision:** Use PostgreSQL + EF Core (`Npgsql`) with relational foreign keys for core entities and `jsonb`/`text[]` for embedded value collections.
- **Consequences:** Clean schema, strong referential integrity on media assets, and fast single-query reads.

### DEC-API-003 — Standardized RFC 9457 ProblemDetails Error Contract
- **Status:** Accepted
- **Date:** 2026-11-05
- **Context:** Both Angular apps need a predictable error format for field validation and domain conflicts.
- **Decision:** Use ASP.NET Core built-in `ProblemDetails` and `ValidationProblemDetails` across all endpoints via a global `IExceptionHandler`.
- **Consequences:** Consistent error parsing in both `jobayer-portfolio-web` and `jobayer-portfolio-admin`.

### DEC-API-004 — Target .NET 10 Framework
- **Status:** Accepted
- **Date:** 2026-11-06
- **Context:** The development environment and reference repository (`MaleFashion-eCommerce`) run on .NET 10 SDK (`net10.0`).
- **Decision:** Target `net10.0` uniformly across all solution projects (`Domain`, `Application`, `Infrastructure`, `Api`, and `Tests`).
- **Consequences:** Consistent language features, C# 13+ support, optimal runtime performance, and alignment with the developer workstation.

### DEC-API-005 — Domain-Driven Design & Repository/Unit-of-Work Pattern from MaleFashion-eCommerce
- **Status:** Accepted
- **Date:** 2026-11-06
- **Context:** The backend architecture must mirror the structured DDD abstractions proved in `MaleFashion-eCommerce` while maintaining Clean Architecture isolation.
- **Decision:** Implement explicit generic `IRepository<TAggregateRoot, TKey>`, `IUnitOfWork`, and aggregate-specific repositories gathered into `IApplicationUnitOfWork`.
- **Consequences:** High cohesion, clean transaction management, decoupled feature handlers, and consistent unit testing via repository mocks.

### DEC-API-006 — Segregated Controller Architecture for REST Endpoints
- **Status:** Accepted
- **Date:** 2026-11-06
- **Context:** The API must strictly divide Public, Admin, and Authentication endpoints to prevent unauthorized access or accidental property leakage.
- **Decision:** Organize endpoints using dedicated ASP.NET Core Web API Controllers partitioned into `Controllers/Public/`, `Controllers/Admin/`, and `Controllers/Auth/`, inheriting from a common `BaseApiController`.
- **Consequences:** Clear OpenAPI tag generation, strict class-level `[Authorize]` policy enforcement, and easily maintainable endpoints.
