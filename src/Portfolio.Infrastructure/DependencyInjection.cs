using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Portfolio.Application.Contracts;
using Portfolio.Application.Contracts.Services;
using Portfolio.Infrastructure.Data;
using Portfolio.Infrastructure.Services;

namespace Portfolio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // PostgreSQL DbContext with snake_case naming
        var connectionString = configuration.GetConnectionString("PortfolioDb")
            ?? "Host=localhost;Port=5432;Database=jobayer_portfolio;Username=postgres;Password=postgres";

        services.AddDbContext<PortfolioDbContext>(options =>
        {
            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention();
        });

        // Unit of Work
        services.AddScoped<IApplicationUnitOfWork, ApplicationUnitOfWork>();

        // Services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // JWT Authentication
        var signingKey = configuration["Jwt:SigningKey"] ?? "a-very-long-fallback-secret-key-that-is-at-least-32-bytes-long!";
        var issuer = configuration["Jwt:Issuer"] ?? "jobayer-portfolio-api";
        var audience = configuration["Jwt:Audience"] ?? "jobayer-portfolio-admin";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminPolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Admin");
            });
        });

        return services;
    }
}

