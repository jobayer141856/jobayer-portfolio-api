# Backend API Open Questions

Confirm these items with the product owner before finalizing production infrastructure:

## Runtime & Architecture
- [ ] Confirm target .NET version (`.NET 8 LTS`, `.NET 9`, or `.NET 10`).
- [ ] Confirm whether Clean Architecture multi-project solution (`Domain`, `Application`, `Infrastructure`, `Api`) or a single-project modular Vertical Slice structure is preferred.

## Infrastructure & Providers
- [ ] Confirm production file storage provider (Cloudflare R2, AWS S3, Azure Blob, or local persistent Docker volume).
- [ ] Confirm email delivery service for contact form alerts (Resend, SendGrid, AWS SES, or SMTP).
- [ ] Confirm whether Cloudflare Turnstile or Google reCAPTCHA v3 should be verified on `POST /api/v1/contact` in addition to IP rate limiting and honeypot validation.

## Content & Feature Flags
- [ ] Confirm whether the Blog endpoints (`/api/v1/blog` and `/api/v1/admin/blog`) should be enabled in the initial release.
- [ ] Confirm whether the API should expose a signed preview token endpoint so the admin can preview `Draft` projects on the public frontend before publishing.
