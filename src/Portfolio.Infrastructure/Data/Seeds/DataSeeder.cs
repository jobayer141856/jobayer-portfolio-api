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

            // Ensure Sample Resume / CV PDF exists
            var sampleCvName = "Jobayer_Hossen_CV.pdf";
            var rawUploadsPath = config["Storage:LocalRootPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var uploadsDir = Path.IsPathRooted(rawUploadsPath) ? rawUploadsPath : Path.GetFullPath(rawUploadsPath);
            Directory.CreateDirectory(uploadsDir);
            var cvFilePath = Path.Combine(uploadsDir, sampleCvName);
            if (!File.Exists(cvFilePath))
            {
                var minimalPdf = "%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj\n3 0 obj<</Type/Page/MediaBox[0 0 612 792]/Parent 2 0 R/Contents 4 0 R/Resources<</Font<</F1 5 0 R>>>>>>endobj\n4 0 obj<</Length 78>>stream\nBT /F1 20 Tf 50 720 Td (Jobayer Hossen - Curriculum Vitae) Tj ET\nendstream\nendobj\n5 0 obj<</Type/Font/Subtype/Type1/BaseFont/Helvetica>>endobj\nxref\n0 6\n0000000000 65535 f\n0000000009 00000 n\n0000000052 00000 n\n0000000101 00000 n\n0000000212 00000 n\n0000000341 00000 n\ntrailer<</Size 6/Root 1 0 R>>\nstartxref\n412\n%%EOF\n";
                await File.WriteAllTextAsync(cvFilePath, minimalPdf);
                logger.LogInformation("Created sample CV PDF at {Path}", cvFilePath);
            }

            var cvAsset = await context.MediaAssets.FirstOrDefaultAsync(m => m.OriginalFileName == sampleCvName);
            if (cvAsset == null)
            {
                cvAsset = new MediaAsset
                {
                    Id = Guid.NewGuid(),
                    OriginalFileName = sampleCvName,
                    StorageKey = sampleCvName,
                    PublicUrl = $"/uploads/{sampleCvName}",
                    ContentType = "application/pdf",
                    SizeBytes = new FileInfo(cvFilePath).Length,
                    AltText = "Jobayer Hossen Curriculum Vitae",
                    CreatedAtUtc = DateTime.UtcNow
                };
                await context.MediaAssets.AddAsync(cvAsset);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded CV MediaAsset record");
            }

            // Seed Profile
            var existingProfile = await context.Profiles.FirstOrDefaultAsync();
            if (existingProfile == null)
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
                    ResumePdfAssetId = cvAsset.Id,
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
            else if (existingProfile.ResumePdfAssetId == null || existingProfile.ResumePdfAssetId == Guid.Empty)
            {
                existingProfile.ResumePdfAssetId = cvAsset.Id;
                existingProfile.UpdatedAtUtc = DateTime.UtcNow;
                logger.LogInformation("Attached CV MediaAsset to existing profile");
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}

