# Backend API Product Requirements Document (PRD)

**Project:** Personal Professional Portfolio\
**Application:** ASP.NET Core REST API (`jobayer-portfolio-api`)\
**Framework:** ASP.NET Core Web API (.NET) + PostgreSQL\
**Status:** Draft --- pending product-owner confirmation\
**Document Version:** 0.1\
**Last Updated:** 5 November 2026

------------------------------------------------------------------------

## 1. Product Overview

### 1.1 Product Vision

Build a secure, maintainable, high-performance ASP.NET Core REST API that serves as the single authoritative source of truth for both the public Angular portfolio (`jobayer-portfolio-web`) and the private Angular admin dashboard (`jobayer-portfolio-admin`).

### 1.2 Product Scope

This PRD covers only the **ASP.NET Core backend API** (`jobayer-portfolio-api`).

The overall solution has three separately deployed applications:

1.  `jobayer-portfolio-web` --- public Angular website (consumes public read endpoints and contact submission).
2.  `jobayer-portfolio-admin` --- private Angular admin dashboard (consumes auth and admin CRUD endpoints).
3.  `jobayer-portfolio-api` --- ASP.NET Core REST API, persistence, media storage integration, and email notifications (covered by this PRD).

### 1.3 Product Goals

-   Provide fast, cache-friendly, read-optimized public endpoints that expose only verified, published portfolio content.
-   Provide authenticated, strongly validated admin endpoints for full content lifecycle management (`Draft`, `Published`, `Archived`), reordering, media uploads, and contact inbox management.
-   Enforce data integrity, slug uniqueness, and editorial visibility rules in the backend domain and database layers.
-   Protect the contact form and authentication endpoints against brute force, spam, and abuse.
-   Publish an accurate OpenAPI specification for frontend TypeScript client/model synchronization.

### 1.4 Success Measures

-   Zero leakage of `Draft`/`Archived` records or private fields on public endpoints.
-   100% of `/api/v1/admin/*` endpoints enforce authentication and authorization.
-   All validation errors return standardized RFC 9457 / RFC 7807 `ValidationProblemDetails` responses.
-   Public read endpoints respond with low latency and support HTTP caching (`ETag` / `Cache-Control`).
-   Contact form submissions are persisted reliably and trigger owner notifications without exposing internal mail infrastructure errors.

------------------------------------------------------------------------

## 2. Consumers and Access Tiers

### 2.1 Public Anonymous Consumer (`jobayer-portfolio-web`)

-   **Read-only access** to published portfolio resources (`Profile`, `Experiences`, `Projects`, `Skills`, `Educations`, `Publications`, `Certifications`, and optional `Blog`).
-   **Write access** limited strictly to `POST /api/v1/contact` (visitor inquiry submission) under rate limiting and spam checks.

### 2.2 Authenticated Admin Consumer (`jobayer-portfolio-admin`)

-   Full CRUD, publication status transitions, display-order updates, media library uploads/deletions, contact message management, and account security actions.

------------------------------------------------------------------------

## 3. Functional Requirements by Domain Module

### 3.1 Authentication & Identity (`/api/v1/auth`)

-   Admin login with email/username and password verified via secure password hashing (`Argon2id` or ASP.NET Core Identity `PBKDF2`).
-   Issue short-lived JWT access tokens + rotating refresh tokens (stored hashed in PostgreSQL and delivered via `HttpOnly`, `Secure` cookie or agreed token contract).
-   Support token refresh (`/refresh`), explicit logout/revocation (`/logout`), current session inspection (`/me`), and password update (`/change-password`).
-   Enforce login rate limiting and temporary account lockout after repeated failed attempts.

### 3.2 Profile & Site Metadata (`Profile`)

-   Single singleton/active profile record storing display name, headline, short introduction, full biography, profile photo asset reference, active résumé PDF asset reference, public email + `showEmailPublicly` flag, social profile links, and default SEO metadata.
-   Public `GET /api/v1/profile` omits `publicEmail` unless `showEmailPublicly == true`.

### 3.3 Work Experience (`Experiences`)

-   Fields: `Id`, `Organization`, `OrganizationLogoAssetId`, `RoleTitle`, `EmploymentType`, `Location`, `WorkArrangement`, `StartDate`, `EndDate`, `IsCurrent`, `Summary`, `Responsibilities` (ordered list), `Outcomes` (ordered list), `TechnologyTags`, `DisplayOrder`, `Status` (`Draft`, `Published`, `Archived`), timestamps.
-   Public `GET /api/v1/experiences` returns only `Published` records ordered by `DisplayOrder` / `StartDate DESC`.

### 3.4 Projects & Case Studies (`Projects`)

-   Fields: `Id`, `Slug` (unique index), `Title`, `ShortSummary`, `ProblemContext`, `Goals`, `RoleAndContributions`, `ArchitectureOverview`, `ChallengesAndDecisions`, `Outcomes`, `Category`, `TechnologyTags`, `CoverImageAssetId`, `GalleryImages` (ordered asset references with captions/alt), `LiveDemoUrl`, `RepositoryUrl`, `IsFeatured`, `DisplayOrder`, `Status`, `PublishedAt`, SEO metadata, timestamps.
-   Public endpoints:
    -   `GET /api/v1/projects` (supports optional filters: `featured`, `category`, `tag`).
    -   `GET /api/v1/projects/{slug}` (returns `404` if missing or not `Published`).
    -   Optional slug redirect lookup if a previously published project slug was renamed.

### 3.5 Technical Skills (`Skills`)

-   Two-level hierarchy: `SkillCategory` (`Id`, `Name`, `Slug`, `Description`, `DisplayOrder`) and `SkillItem` (`Id`, `CategoryId`, `Name`, `ContextDescription`, `IsFeatured`, `DisplayOrder`, `IsVisible`).
-   Public `GET /api/v1/skills` returns visible categories and their visible skills ordered by `DisplayOrder`.

### 3.6 Education (`Educations`)

-   Fields: `Id`, `Institution`, `Degree`, `Department`, `StartDate`, `EndDate`, `IsCurrent`, `ResultText`, `ShowResultPublicly`, `Description`, `Activities`, `InstitutionUrl`, `DisplayOrder`, `Status`.
-   Public `GET /api/v1/educations` omits `ResultText` unless `ShowResultPublicly == true`.

### 3.7 Research Publications (`Publications`)

-   Fields: `Id`, `Slug`, `Title`, `Authors` (ordered list preserving exact citation order), `Venue`, `PublicationYear`, `PublicationDate`, `Doi`, `OfficialUrl`, `PdfAssetId` (only when sharing is permitted), `Abstract`, `DisplayOrder`, `Status`.
-   Public `GET /api/v1/publications` returns only `Published` records.

### 3.8 Certifications & Achievements (`Certifications`)

-   Fields: `Id`, `Title`, `Issuer`, `IssueDate`, `CredentialId`, `CredentialUrl`, `Description`, `BadgeAssetId`, `DisplayOrder`, `Status`.
-   Public `GET /api/v1/certifications` returns only `Published` records.

### 3.9 Blog Articles (`Blog` --- Optional / Feature-Flagged)

-   Fields: `Id`, `Slug` (unique), `Title`, `Summary`, `ContentMarkdown`, `CoverImageAssetId`, `Tags`, `ReadingTimeMinutes` (computed deterministically on save), `Status`, `PublishedAt`, `UpdatedAt`, SEO metadata.
-   Public endpoints enabled when blog feature flag is active.

### 3.10 Visitor Contact Submissions (`Contact` / `Messages`)

-   Public `POST /api/v1/contact`:
    -   Accepts `Name`, `Email`, `Subject` (optional), `Message`, plus optional anti-spam token/honeypot field.
    -   Enforces IP rate limiting, field length validation, and HTML stripping/sanitization.
    -   Persists `ContactMessage` with status `Unread`, UTC timestamp, and hashed/truncated abuse-prevention metadata.
    -   Enqueues an asynchronous email notification to the owner (failure of email provider must not crash or expose internal errors to the visitor once persisted).
-   Admin `/api/v1/admin/messages`:
    -   Paginated listing, filtering by status (`Unread`, `Read`, `Replied`, `Archived`, `Spam`), detail retrieval, status update, and deletion.

### 3.11 Media & Document Storage (`Media`)

-   Admin `POST /api/v1/admin/media/upload` accepts `multipart/form-data`.
-   Allowed types:
    -   Images: `.jpg`, `.jpeg`, `.png`, `.webp`, `.avif`, `.svg` (restricted/sanitized if enabled).
    -   Documents: `.pdf` (for résumé and permitted publication preprints).
-   Validates file size, extension, and binary file signature (magic bytes).
-   Stores files via an abstracted `IFileStorageService` (local volume in dev; S3/Cloudflare R2/Azure Blob in production) and saves metadata in `MediaAssets` table (`Id`, `FileName`, `StorageKey`, `PublicUrl`, `ContentType`, `SizeBytes`, `Width`, `Height`, `AltText`, `CreatedAt`).
-   Prevents deleting a `MediaAsset` if it is currently referenced by a published entity (returns `409 Conflict` with referencing entity info).

### 3.12 SEO Support & Sitemap Feed

-   Provide a lightweight public endpoint (e.g., `GET /api/v1/seo/sitemap-entries`) or direct `/sitemap.xml` generation listing all published static routes, project slugs, and blog slugs with their `LastModified` timestamps.

------------------------------------------------------------------------

## 4. Non-Functional Requirements

-   **Architecture:** Clean Architecture / Vertical Slice hybrid separating Domain, Application (CQRS/handlers + validation), Infrastructure (EF Core PostgreSQL, Storage, Email), and Web API presentation.
-   **Performance & Caching:** Use `AsNoTracking()` on all read queries, response caching / output caching on public GET endpoints, and cache invalidation whenever an admin mutates or publishes content.
-   **Observability:** Structured logging with Serilog/OpenTelemetry, health check endpoints (`/health/live`, `/health/ready`), and correlation IDs (`X-Correlation-Id`).
-   **Security:** Strict CORS allowlist (`jobayer-portfolio-web` and `jobayer-portfolio-admin` origins only), HTTPS enforcement, rate limiting (`Microsoft.AspNetCore.RateLimiting`), and parameterized EF Core queries.

------------------------------------------------------------------------

## 5. Architectural Decisions & Status

| Decision | Accepted Choice | Status |
|---|---|---|
| .NET Version | .NET 10 (`net10.0`) | Accepted (DEC-API-004) |
| Architecture pattern | Clean Architecture + DDD (`MaleFashion-eCommerce` pattern) + Cortex.Mediator / FluentValidation | Accepted (DEC-API-005) |
| Object storage | Local disk in dev (`/uploads`); Cloudflare R2 / AWS S3 ready in prod | Accepted (DEC-API-002) |
| Email provider | SMTP / MailKit in dev; configurable in prod | Accepted |
| Presentation | ASP.NET Core Web API Controllers (`Public/`, `Admin/`, `Auth/`) | Accepted (DEC-API-006) |
| Spam protection | Server-side IP rate limiting + honeypot + input sanitization | Accepted |
