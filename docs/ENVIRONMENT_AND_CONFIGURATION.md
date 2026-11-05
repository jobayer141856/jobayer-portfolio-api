# Backend Environment and Configuration

## Configuration Hierarchy
1. `appsettings.json` (safe defaults, non-secret settings)
2. `appsettings.{Environment}.json` (`Development`, `Staging`, `Production`)
3. User Secrets (`dotnet user-secrets` in local development)
4. Environment Variables / Secret Manager in staging and production

**Rule:** Never commit passwords, JWT keys, database credentials, SMTP keys, or S3 secrets to Git.

## Required Configuration Sections

```json
{
  "ConnectionStrings": {
    "PortfolioDb": "Host=localhost;Port=5432;Database=jobayer_portfolio;Username=postgres;Password=..."
  },
  "Jwt": {
    "Issuer": "jobayer-portfolio-api",
    "Audience": "jobayer-portfolio-admin",
    "SigningKey": "<PROVIDED_VIA_ENV_VAR_MIN_32_BYTES>",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 14
  },
  "Cors": {
    "PublicOrigins": ["http://localhost:4200"],
    "AdminOrigins": ["http://localhost:4300"]
  },
  "Storage": {
    "Provider": "Local",
    "LocalRootPath": "wwwroot/uploads",
    "PublicBaseUrl": "http://localhost:5000/uploads",
    "MaxImageSizeBytes": 5242880,
    "MaxDocumentSizeBytes": 10485760
  },
  "Email": {
    "Enabled": false,
    "Provider": "Smtp",
    "FromAddress": "noreply@example.com",
    "AdminNotificationAddress": "owner@example.com"
  },
  "BootstrapAdmin": {
    "Email": "owner@example.com",
    "InitialPassword": "<PROVIDED_VIA_ENV_VAR_ON_FIRST_RUN>"
  }
}
```

## Startup Options Validation
Bind all configuration sections using `.BindConfiguration(...).ValidateDataAnnotations().ValidateOnStart()` so missing production variables fail fast at startup with a clear log message.
