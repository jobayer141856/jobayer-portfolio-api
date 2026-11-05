# Caching and Performance

## 1. Read Path Optimization
- All public query handlers must use EF Core `.AsNoTracking()` and project directly to public response DTOs via `.Select(...)` so unneeded columns or navigation properties are never materialized.
- Avoid N+1 queries by using explicit projections or `.AsSplitQuery()` where collections (such as project gallery images or skill items) are loaded.

## 2. Output Caching & Cache Tag Invalidation
Use ASP.NET Core `OutputCache` (with in-memory store for single-instance deployment or Redis if scaled horizontally):
- Tag each public endpoint group (`profile`, `experiences`, `projects`, `skills`, `educations`, `publications`, `certifications`, `blog`).
- Whenever an admin command creates, updates, reorders, changes status, or deletes an entity, call `IOutputCacheStore.EvictByTagAsync(tag, cancellationToken)` so public API responses update immediately upon publishing.

## 3. HTTP Conditional Requests & Compression
- Support `ETag` and `Cache-Control: public, max-age=60, stale-while-revalidate=300` on public GET endpoints.
- Enable `Brotli` and `Gzip` response compression for JSON payloads.

## 4. Database Indexing
- Create unique B-tree indexes on `Projects.Slug`, `Publications.Slug`, `BlogArticles.Slug`, and `AdminUsers.Email`.
- Create composite indexes on `(Status, DisplayOrder)` and `(Status, IsFeatured, DisplayOrder)` for fast public queries.
