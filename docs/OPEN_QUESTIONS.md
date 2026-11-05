# Backend API Open & Resolved Architecture Questions

## Confirmed & Resolved
- [x] **Target .NET version:** Confirmed `.NET 10` (`net10.0`) matching installed SDK 10.0.401 and `MaleFashion-eCommerce` reference.
- [x] **Architecture pattern:** Confirmed Clean Architecture + Domain-Driven Design (DDD) multi-project solution (`Domain`, `Application`, `Infrastructure`, `Api`, `UnitTests`, `IntegrationTests`) following `MaleFashion-eCommerce` patterns.
- [x] **Development Storage:** Local volume storage served at `/uploads/*` behind an abstracted `IFileStorageService`.
- [x] **Development Email:** SMTP via MailKit behind an abstracted `IEmailService`.
- [x] **CQRS & Mapping:** `Cortex.Mediator` + `Mapster`.
- [x] **Presentation:** Partitioned ASP.NET Core Web API Controllers (`Public/`, `Admin/`, `Auth/`).

## Production Deployment Items
- [ ] Confirm production file storage provider (Cloudflare R2, AWS S3, or Azure Blob Storage).
- [ ] Confirm production email delivery service for contact form alerts (Resend, SendGrid, or AWS SES).
- [ ] Confirm whether Cloudflare Turnstile or Google reCAPTCHA v3 should be verified on `POST /api/v1/contact` in production in addition to IP rate limiting and honeypot validation.
- [ ] Confirm whether the Blog endpoints (`/api/v1/blog` and `/api/v1/admin/blog`) should be enabled in the initial public release (supported and feature-flagged).
- [ ] Confirm whether the API should expose a signed preview token endpoint so the admin can preview `Draft` projects on the public frontend before publishing.
