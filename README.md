# Jobayer Portfolio API (`jobayer-portfolio-api`)

Backend REST API for the personal professional portfolio system, serving both the public Angular portfolio website (`jobayer-portfolio-web`) and the private Angular admin dashboard (`jobayer-portfolio-admin`).

Built with **ASP.NET Core Web API (.NET 10)**, **PostgreSQL 16+**, **Domain-Driven Design (DDD)**, and **Clean Architecture**, adhering to the enterprise CQRS and repository patterns established in [`MaleFashion-eCommerce`](https://github.com/jobayer141856/MaleFashion-eCommerce).

---

## 🏛 Architecture Overview

The solution follows a multi-project Clean Architecture with strict layer isolation:

```
jobayer-portfolio-api/
├── JobayerPortfolio.sln
├── docker-compose.yml
├── docs/
├── src/
│   ├── Portfolio.Domain/            # Entities, Value Objects, Enums, DDD Contracts
│   ├── Portfolio.Application/       # CQRS Features, DTOs, Handlers, FluentValidation, Mappings
│   ├── Portfolio.Infrastructure/    # PostgreSQL EF Core DbContext, Repositories, Unit of Work, Auth, Storage
│   └── Portfolio.Api/               # ASP.NET Core Web API, Partitioned Controllers, Middleware, OutputCache
└── tests/
    ├── Portfolio.UnitTests/         # Domain invariant & command validation unit tests
    └── Portfolio.IntegrationTests/  # Real-database API integration tests
```

### Architectural Principles
- **Strict Separation of Public and Admin Tiers:**
  - `/api/v1/*` — Anonymous, fast, read-optimized endpoints cached with `OutputCache`. Returns zero draft/archived records or sensitive private fields.
  - `/api/v1/auth/*` — Rate-limited admin authentication (JWT access tokens + rotating refresh tokens).
  - `/api/v1/admin/*` — Protected management endpoints requiring `AdminOnly` authorization policy.
- **PostgreSQL Native Optimization:** Native `jsonb` and `text[]` columns for collections without wasteful join tables.
- **RFC 9457 ProblemDetails:** Uniform HTTP error contract across all endpoints.
- **Domain-Driven Design:** Explicit Aggregate Roots, generic Repositories, and Unit of Work.

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker & Docker Compose](https://www.docker.com/) (for PostgreSQL)

### 1. Start PostgreSQL with Docker
```bash
docker-compose up -d
```

### 2. Apply Database Migrations
```bash
dotnet ef database update --project src/Portfolio.Infrastructure --startup-project src/Portfolio.Api
```

### 3. Run the API
```bash
dotnet run --project src/Portfolio.Api
```

The interactive API documentation will be accessible at:
- **Scalar UI:** `https://localhost:5001/scalar/v1`
- **Swagger UI:** `https://localhost:5001/swagger`

---

## 🧪 Testing and Verification

```bash
# Code formatting verification
dotnet format --verify-no-changes

# Build with warnings as errors
dotnet build -warnaserror

# Run all test suites
dotnet test
```

---

## 📚 Documentation Index

Detailed design documents are available in the [`docs/`](./docs) directory:
- [API Endpoints & Contracts](./docs/API_ENDPOINTS_AND_CONTRACTS.md)
- [Authentication & Security](./docs/AUTH_AND_SECURITY.md)
- [Database Schema (PostgreSQL)](./docs/DATABASE_SCHEMA.md)
- [Software Architecture](./docs/SOFTWARE_ARCHITECTURE.md)
- [Technology Stack](./docs/TECH_STACK.md)
- [Caching & Performance](./docs/CACHING_AND_PERFORMANCE.md)
- [Error Handling & Validation](./docs/ERROR_HANDLING_AND_VALIDATION.md)
- [Media & File Storage](./docs/MEDIA_AND_FILE_STORAGE.md)
- [Observability & Logging](./docs/OBSERVABILITY_AND_LOGGING.md)
- [Architecture Decisions Log](./docs/DECISIONS.md)
- [Testing & QA Strategy](./docs/TESTING_QA.md)
- [Deployment](./docs/DEPLOYMENT.md)
- [Environment Configuration](./docs/ENVIRONMENT_AND_CONFIGURATION.md)
- [Open Architecture Questions](./docs/OPEN_QUESTIONS.md)
