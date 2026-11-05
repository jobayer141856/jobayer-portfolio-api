using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;
using Portfolio.Domain.ValueObjects;

namespace Portfolio.Infrastructure.Data.Configurations;

public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.ToTable("admin_users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.DisplayName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.PasswordHash).IsRequired();
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.Property(x => x.ReplacedByTokenHash).HasMaxLength(128);
        builder.Property(x => x.CreatedByIp).HasMaxLength(100);

        builder.HasOne(x => x.AdminUser)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(x => x.AdminUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("media_assets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.StorageKey).HasMaxLength(500).IsRequired();
        builder.HasIndex(x => x.StorageKey).IsUnique();
        builder.Property(x => x.PublicUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.AltText).HasMaxLength(300).HasDefaultValue("");
    }
}

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("profiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DisplayName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Headline).HasMaxLength(250).IsRequired();
        builder.Property(x => x.ShortIntro).HasMaxLength(600).IsRequired();
        builder.Property(x => x.PublicEmail).HasMaxLength(256);
        builder.Property(x => x.SeoTitle).HasMaxLength(160);
        builder.Property(x => x.SeoDescription).HasMaxLength(320);

        builder.HasOne(x => x.ProfileImageAsset)
            .WithMany()
            .HasForeignKey(x => x.ProfileImageAssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ResumePdfAsset)
            .WithMany()
            .HasForeignKey(x => x.ResumePdfAssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(x => x.SocialLinks, nb =>
        {
            nb.ToJson("social_links_json");
        });
    }
}

public class ExperienceConfiguration : IEntityTypeConfiguration<Experience>
{
    public void Configure(EntityTypeBuilder<Experience> builder)
    {
        builder.ToTable("experiences");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Organization).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RoleTitle).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EmploymentType).HasMaxLength(80);
        builder.Property(x => x.Location).HasMaxLength(150);
        builder.Property(x => x.WorkArrangement).HasMaxLength(80);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(x => x.OrganizationLogoAsset)
            .WithMany()
            .HasForeignKey(x => x.OrganizationLogoAssetId)
            .OnDelete(DeleteBehavior.Restrict);

        var listComparer = new ValueComparer<List<string>>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property(x => x.Responsibilities)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
            )
            .Metadata.SetValueComparer(listComparer);

        builder.Property(x => x.Outcomes)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
            )
            .Metadata.SetValueComparer(listComparer);
    }
}

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ShortSummary).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LiveDemoUrl).HasMaxLength(500);
        builder.Property(x => x.RepositoryUrl).HasMaxLength(500);
        builder.Property(x => x.SeoTitle).HasMaxLength(160);
        builder.Property(x => x.SeoDescription).HasMaxLength(320);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(x => x.CoverImageAsset)
            .WithMany()
            .HasForeignKey(x => x.CoverImageAssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.GalleryImages)
            .WithOne(g => g.Project)
            .HasForeignKey(g => g.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProjectImageConfiguration : IEntityTypeConfiguration<ProjectImage>
{
    public void Configure(EntityTypeBuilder<ProjectImage> builder)
    {
        builder.ToTable("project_images");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Caption).HasMaxLength(300);

        builder.HasOne(x => x.MediaAsset)
            .WithMany()
            .HasForeignKey(x => x.MediaAssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SkillCategoryConfiguration : IEntityTypeConfiguration<SkillCategory>
{
    public void Configure(EntityTypeBuilder<SkillCategory> builder)
    {
        builder.ToTable("skill_categories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(300);

        builder.HasMany(x => x.Items)
            .WithOne(i => i.Category)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SkillItemConfiguration : IEntityTypeConfiguration<SkillItem>
{
    public void Configure(EntityTypeBuilder<SkillItem> builder)
    {
        builder.ToTable("skill_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ContextDescription).HasMaxLength(300);
    }
}

public class EducationConfiguration : IEntityTypeConfiguration<Education>
{
    public void Configure(EntityTypeBuilder<Education> builder)
    {
        builder.ToTable("educations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Institution).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Degree).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Department).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ResultText).HasMaxLength(100);
        builder.Property(x => x.InstitutionUrl).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
    }
}

public class PublicationConfiguration : IEntityTypeConfiguration<Publication>
{
    public void Configure(EntityTypeBuilder<Publication> builder)
    {
        builder.ToTable("publications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.Title).HasMaxLength(400).IsRequired();
        builder.Property(x => x.Venue).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Doi).HasMaxLength(150);
        builder.Property(x => x.OfficialUrl).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(x => x.PdfAsset)
            .WithMany()
            .HasForeignKey(x => x.PdfAssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(x => x.Authors, nb =>
        {
            nb.ToJson("authors_json");
        });
    }
}

public class CertificationConfiguration : IEntityTypeConfiguration<Certification>
{
    public void Configure(EntityTypeBuilder<Certification> builder)
    {
        builder.ToTable("certifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Issuer).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CredentialId).HasMaxLength(150);
        builder.Property(x => x.CredentialUrl).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(x => x.BadgeAsset)
            .WithMany()
            .HasForeignKey(x => x.BadgeAssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BlogArticleConfiguration : IEntityTypeConfiguration<BlogArticle>
{
    public void Configure(EntityTypeBuilder<BlogArticle> builder)
    {
        builder.ToTable("blog_articles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(x => x.CoverImageAsset)
            .WithMany()
            .HasForeignKey(x => x.CoverImageAssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.ToTable("contact_messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SenderName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.SenderEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(250);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.IpHash).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(300);
    }
}

