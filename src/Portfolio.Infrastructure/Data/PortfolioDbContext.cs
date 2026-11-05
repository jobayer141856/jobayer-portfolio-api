using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Data;

public class PortfolioDbContext : DbContext
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : base(options)
    {
    }

    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Experience> Experiences => Set<Experience>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectImage> ProjectImages => Set<ProjectImage>();
    public DbSet<SkillCategory> SkillCategories => Set<SkillCategory>();
    public DbSet<SkillItem> SkillItems => Set<SkillItem>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<Publication> Publications => Set<Publication>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<BlogArticle> BlogArticles => Set<BlogArticle>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PortfolioDbContext).Assembly);
    }
}

