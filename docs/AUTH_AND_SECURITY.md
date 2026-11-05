# API Authentication and Security

## 1. Authentication & Token Lifecycle
- **Password Storage:** Hash admin passwords using `Argon2id` or ASP.NET Core `PasswordHasher<TUser>` with high iteration counts. Seed the initial admin account via environment-supplied bootstrap configuration on first startup and force a password change if desired.
- **Access Tokens:** Short-lived JWT (15 minutes) signed with an asymmetric key or 256-bit+ HMAC secret (`Jwt:SigningKey`), containing `sub`, `email`, `role`, and `jti`.
- **Refresh Tokens:** Cryptographically random 64-byte token, stored hashed (`SHA-256`) in the `RefreshTokens` table with `ExpiresAt`, `RevokedAt`, `ReplacedByTokenHash`, and `CreatedByIp`.
- **Token Rotation:** Every call to `POST /api/v1/auth/refresh` revokes the used refresh token and issues a new one. Reuse of an already-revoked token triggers immediate revocation of the entire token family.

## 2. Authorization Policies
- Public routes explicitly carry `[AllowAnonymous]`.
- All `/api/v1/admin/*` route groups enforce `.RequireAuthorization("AdminPolicy")` at the route-group level so individual endpoints cannot accidentally omit authorization.

## 3. Rate Limiting (`Microsoft.AspNetCore.RateLimiting`)
Configure distinct policies by client IP:
- **`ContactPolicy` (`POST /api/v1/contact`):** Token bucket / fixed window (e.g., max 3 submissions per 15 minutes per IP) returning `429 Too Many Requests`.
- **`AuthPolicy` (`POST /api/v1/auth/login`):** Max 5 attempts per 5 minutes per IP + account lockout after 5 consecutive failures.
- **`PublicReadPolicy` (`/api/v1/*`):** Generous sliding window (e.g., 120 requests/minute per IP) to prevent scraping floods.

## 4. CORS Policy
- Configure strict named CORS policies allowing only configured origins:
  - `Cors:PublicOrigin` (e.g., `https://jobayer.dev`) -> `GET`, `POST`, `OPTIONS`
  - `Cors:AdminOrigin` (e.g., `https://admin.jobayer.dev`) -> `GET`, `POST`, `PUT`, `PATCH`, `DELETE`, `OPTIONS` with `AllowCredentials()`
- Never use `AllowAnyOrigin()` in staging or production.

## 5. Input Sanitization & Upload Hardening
- Reject unverified HTML in contact messages.
- Validate uploaded files by inspecting magic bytes (JPEG `FF D8 FF`, PNG `89 50 4E 47`, WEBP `52 49 46 46 ... 57 45 42 50`, PDF `25 50 44 46`), enforce maximum upload size via `[RequestSizeLimit]`, and generate UUID-based storage keys so user-supplied filenames are never used directly on disk.
