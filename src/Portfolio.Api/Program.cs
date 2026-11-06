using Cortex.Mediator.DependencyInjection;
using Mapster;
using Microsoft.Extensions.FileProviders;
using Portfolio.Api.Extensions;
using Portfolio.Api.Middleware;
using Portfolio.Application;
using Portfolio.Application.Features.Auth;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Data.Seeds;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog Structured Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Cortex Mediator
builder.Services.AddCortexMediator(
    new[] { typeof(Program), typeof(LoginCommand) },
    options => options.AddDefaultBehaviors()
);

// Mapster
builder.Services.AddMapster();

// Rate Limiting & Output Caching
builder.Services.AddCustomRateLimiting();
builder.Services.AddOutputCache(options =>
{
    options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60);
});

// Controllers with JSON formatting
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// CORS Policies
var publicOrigins = builder.Configuration.GetSection("Cors:PublicOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" };
var adminOrigins = builder.Configuration.GetSection("Cors:AdminOrigins").Get<string[]>() ?? new[] { "http://localhost:4300" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("PublicClient", policy =>
    {
        policy.WithOrigins(publicOrigins)
              .WithMethods("GET", "POST", "OPTIONS")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With");
    });

    options.AddPolicy("AdminClient", policy =>
    {
        policy.WithOrigins(adminOrigins)
              .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "X-Refresh-Token")
              .AllowCredentials();
    });
});

// OpenAPI & Scalar Documentation
builder.Services.AddOpenApi();

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Global Exception Handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Jobayer Portfolio API")
               .WithTheme(ScalarTheme.Moon);
    });

    // Friendly redirects for root and traditional /swagger navigation
    app.MapGet("/", () => Results.Redirect("/scalar/v1"));
    app.MapGet("/swagger", () => Results.Redirect("/scalar/v1"));
    app.MapGet("/swagger/index.html", () => Results.Redirect("/scalar/v1"));
}

// Serve Static Files for Uploads
var rawUploadsPath = builder.Configuration["Storage:LocalRootPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
var uploadsPath = Path.IsPathRooted(rawUploadsPath) ? rawUploadsPath : Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, rawUploadsPath));
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseRouting();

// CORS
app.UseCors(policy =>
{
    policy.SetIsOriginAllowed(origin => publicOrigins.Contains(origin) || adminOrigins.Contains(origin))
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials();
});

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

app.MapControllers();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

// Seed database on startup in development or when requested
if (args.Contains("--seed") || app.Environment.IsDevelopment())
{
    await DataSeeder.SeedAsync(app.Services);
}

app.Run();

public partial class Program { }
