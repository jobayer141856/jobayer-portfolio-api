# Backend API Deployment

## Containerized Deployment (`Docker`)
- Package `jobayer-portfolio-api` as a multi-stage Linux Docker image (`mcr.microsoft.com/dotnet/sdk` for build/publish -> `mcr.microsoft.com/dotnet/aspnet` non-root runtime user).
- Expose HTTP port `8080` behind a TLS-terminating reverse proxy or cloud load balancer (Nginx, Caddy, Cloudflare, AWS ALB, Azure Container Apps, or Render/Railway/VPS).

## Database Migrations in Release Pipeline
- Run EF Core migrations as a controlled pre-deployment step or migration bundle (`efbundle`) before routing traffic to new API containers.
- Take an automated PostgreSQL backup (`pg_dump` or managed snapshot) prior to applying schema migrations in production.

## Health Checks & Readiness
- `GET /health/live`: Confirms the ASP.NET Core process is running.
- `GET /health/ready`: Verifies PostgreSQL connectivity and storage configuration before the container receives traffic.

## Reverse Proxy & Forwarded Headers
- Configure `UseForwardedHeaders()` with trusted proxy IPs so client IP rate limiting (`ContactPolicy`, `AuthPolicy`) and HTTPS scheme generation work accurately behind a reverse proxy.
