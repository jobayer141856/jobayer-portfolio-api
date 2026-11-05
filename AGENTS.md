# Backend API Agent Instructions

## Scope
This repository contains the ASP.NET Core backend REST API (`jobayer-portfolio-api`) only. The public Angular portfolio (`jobayer-portfolio-web`) and the private Angular admin dashboard (`jobayer-portfolio-admin`) are separate applications.

## Before changing code
1. Read `API_PRD.md` and the relevant documents in `docs/`.
2. Inspect the existing .NET version, solution layout, EF Core setup, and dependency injection conventions.
3. Check existing domain entities, application handlers, validators, and endpoint definitions before adding new ones.
4. Verify whether a change affects the public or admin OpenAPI contract.

## Core rules
- Use `API_PRD.md` as product and architectural guidance.
- Keep strict separation between **Public endpoints** (`/api/v1/*`) and **Admin endpoints** (`/api/v1/admin/*`).
- Public endpoints must **never** return records with `Draft`, `Archived`, or non-public status, nor private fields (such as unapproved email addresses, hidden academic CGPA, or visitor messages).
- Every `/api/v1/admin/*` endpoint must require authentication and admin authorization.
- Never commit connection strings, JWT signing keys, admin passwords, SMTP credentials, or storage keys to source control (`appsettings.json`). Use user-secrets or environment variables.
- Validate all incoming requests with FluentValidation / DataAnnotations and return standardized RFC 9457 / RFC 7807 `ProblemDetails` or `ValidationProblemDetails`.
- Never expose stack traces, SQL exceptions, or internal infrastructure details in non-development responses.
- Protect the public contact submission endpoint with strict rate limiting, payload size limits, and spam protection.
- Verify file uploads by inspecting magic bytes / MIME signatures, enforcing size limits, and generating safe storage filenames.
- Maintain EF Core migrations cleanly; never modify an already-applied production migration in place.

## Quality checks
Before completing a change:
1. Run `dotnet format --verify-no-changes` and `dotnet build -warnaserror`.
2. Run unit and integration test suites (`dotnet test`).
3. Verify the generated OpenAPI specification reflects any added or modified DTOs and status codes.
4. Update documentation in `docs/` when endpoints, database schema, or architectural decisions change.

## Handling ambiguity
If business rules, publication workflows, storage providers, or email delivery details are uncertain, document the question in `docs/OPEN_QUESTIONS.md` and ask for clarification rather than inventing a consequential assumption.
