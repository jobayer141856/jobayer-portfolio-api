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
