# Observability, Logging, and Health Checks

## 1. Structured Logging (`Serilog`)
- Emit structured JSON logs in staging and production, and readable console logs in development.
- Enrich every log event with `CorrelationId`, `RequestPath`, `RequestMethod`, `StatusCode`, and `ElapsedMs`.
- **PII & Secret Redaction:** Never log `Authorization` headers, cookies, password fields, refresh tokens, or raw visitor contact message bodies.

## 2. Health Checks
Expose two endpoints:
- `GET /health/live`: Liveness probe returning `200 OK` if the HTTP pipeline is responsive.
- `GET /health/ready`: Readiness probe checking PostgreSQL (`AddNpgSql()`) and storage readiness.

## 3. Audit Trail for Admin Actions
- Record structured audit log entries (or `admin_audit_logs` table rows) for state-changing admin operations:
  - `AdminUserId`, `Action` (`Create`, `Update`, `Publish`, `Unpublish`, `Delete`, `Upload`, `Login`), `EntityType`, `EntityId`, `TimestampUtc`, and `IpAddress`.
