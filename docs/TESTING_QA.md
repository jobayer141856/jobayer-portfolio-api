# Backend Testing and QA

## Testing Goals
Verify that public endpoints never leak unpublished or private data, admin endpoints strictly enforce authorization and validation, database migrations apply cleanly, and rate limiting/caching behave as specified.

## Test Layers

### 1. Unit Tests (`Portfolio.UnitTests`)
- Domain entity invariants (e.g., cannot transition a `Project` to `Published` without required public fields).
- FluentValidation validators for every create/update/publish/contact DTO.
- Slug normalization and reading-time calculation utilities.
- Token generation and password hashing helpers.

### 2. Integration Tests (`Portfolio.IntegrationTests`)
Use `WebApplicationFactory<Program>` and `Testcontainers.PostgreSql` (or dedicated test database):
- **Visibility Isolation Tests:** Seed `Draft`, `Published`, and `Archived` records for `Projects`, `Experiences`, `Publications`, and `Certifications`; assert `GET /api/v1/*` returns **only** `Published` records.
- **Privacy Tests:** Seed `Profile` with `showEmailPublicly = false` and `Education` with `showResultPublicly = false`; assert `GET /api/v1/profile` and `GET /api/v1/educations` omit those fields.
- **Security Boundary Tests:** Call every `/api/v1/admin/*` endpoint without a token and with an expired token; assert `401 Unauthorized`.
- **Cache Eviction Tests:** Fetch `GET /api/v1/projects`, publish a new project via `POST /api/v1/admin/projects`, and verify the subsequent `GET /api/v1/projects` immediately includes the new project.
- **Contact Endpoint Tests:** Verify valid submission returns `201`/`202`, invalid email returns `400 ValidationProblemDetails`, and exceeding the rate limit returns `429 Too Many Requests`.

## Release Checklist
- [ ] `dotnet build --configuration Release -warnaserror` succeeds.
- [ ] All unit and integration tests pass.
- [ ] EF Core migrations apply cleanly from an empty database.
- [ ] OpenAPI specification is generated and matches Angular client models.
- [ ] No secrets exist in `appsettings*.json`.
