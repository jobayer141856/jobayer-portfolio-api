using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Portfolio.Application.Contracts.Services;
using Portfolio.Domain.Entities;
using Portfolio.Domain.ValueObjects;

namespace Portfolio.Infrastructure.Data.Seeds;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<PortfolioDbContext>>();

        try
        {
            await context.Database.MigrateAsync();

            // Seed Admin User
            if (!await context.AdminUsers.AnyAsync())
            {
                var adminEmail = config["BootstrapAdmin:Email"] ?? "admin@jobayer.dev";
                var adminPass = config["BootstrapAdmin:InitialPassword"] ?? "Admin@123456";

                var admin = new AdminUser
                {
                    Id = Guid.NewGuid(),
                    Email = adminEmail,
                    DisplayName = "Jobayer Hossen",
                    PasswordHash = passwordHasher.HashPassword(adminPass),
                    CreatedAtUtc = DateTime.UtcNow
                };

                await context.AdminUsers.AddAsync(admin);
                logger.LogInformation("Seeded bootstrap admin account with email {Email}", adminEmail);
            }

            // Seed Profile
            if (!await context.Profiles.AnyAsync())
            {
                var profile = new Profile
                {
                    Id = Guid.NewGuid(),
                    DisplayName = "Jobayer Hossen",
                    Headline = "Full-Stack Software Engineer (.NET & Angular)",
                    ShortIntro = "Passionate software engineer building resilient, scalable backend architectures and intuitive web applications.",
                    BiographyMarkdown = "Experienced in developing enterprise web applications, Domain-Driven Design (DDD), Clean Architecture, and modern frontend SPAs with Angular.",
                    WorkingApproachMarkdown = "Clean Architecture, test-driven mindset, and continuous integration.",
                    PublicEmail = "contact@jobayer.dev",
                    ShowEmailPublicly = true,
                    SocialLinks = new List<SocialLink>
                    {
                        new() { Platform = "GitHub", Url = "https://github.com/jobayer141856", IconKey = "github", DisplayOrder = 1 },
                        new() { Platform = "LinkedIn", Url = "https://linkedin.com/in/jobayer-hossen", IconKey = "linkedin", DisplayOrder = 2 }
                    },
                    UpdatedAtUtc = DateTime.UtcNow
                };

                await context.Profiles.AddAsync(profile);
                logger.LogInformation("Seeded initial profile record");
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}

