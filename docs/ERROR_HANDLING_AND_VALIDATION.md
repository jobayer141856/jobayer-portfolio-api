# Error Handling and Validation

## 1. Request Validation Pipeline
- Validate every incoming command/request DTO using **FluentValidation** before executing domain/database logic.
- Distinguish between:
  - **Draft Validation:** Requires minimal identity fields (`Title`, valid `Slug`) so the owner can save work-in-progress.
  - **Publish Validation:** Enforces completeness required by the public frontend (e.g., `ShortSummary`, `CoverImage` with non-empty `AltText`, valid external URLs) before allowing `Status` to become `Published`.

## 2. Global Exception Handling (`IExceptionHandler`)
Map exceptions deterministically to HTTP status codes using RFC 9457 `ProblemDetails`:

| Exception / Condition | HTTP Status | Problem Title |
|---|---|---|
| `ValidationException` | `400 Bad Request` | One or more validation errors occurred |
| `UnauthorizedAccessException` | `401 Unauthorized` | Authentication required |
| `ForbiddenAccessException` | `403 Forbidden` | Access denied |
| `NotFoundException` | `404 Not Found` | Resource not found |
| `ConflictException` / Unique Constraint | `409 Conflict` | Resource conflict (e.g., duplicate slug or asset in use) |
| `RateLimitLease` rejected | `429 Too Many Requests` | Too many requests; please try again later |
| Unhandled exception | `500 Internal Server Error` | An unexpected error occurred (no internal details exposed) |

## 3. Logging & Correlation
- Include `traceId` / `correlationId` in every `ProblemDetails` response.
- Log `4xx` validation/not-found outcomes at `Information` or `Warning` level, and `500` exceptions at `Error` level with full structured context in server logs only.
