using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Contracts.Repositories;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;

namespace Portfolio.Infrastructure.Data.Repositories;

public class AdminUserRepository : Repository<AdminUser, Guid>, IAdminUserRepository
{
    public AdminUserRepository(PortfolioDbContext context) : base(context) { }

    public async Task<AdminUser?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), ct);
}

public class RefreshTokenRepository : Repository<RefreshToken, Guid>, IRefreshTokenRepository
{
    public RefreshTokenRepository(PortfolioDbContext context) : base(context) { }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
        await _dbSet.FirstOrDefaultAsync(r => r.TokenHash == tokenHash, ct);

    public async Task RevokeFamilyAsync(Guid adminUserId, string? reason, CancellationToken ct = default)
    {
        var activeTokens = await _dbSet.Where(r => r.AdminUserId == adminUserId && r.RevokedAtUtc == null).ToListAsync(ct);
        foreach (var t in activeTokens)
        {
            t.RevokedAtUtc = DateTime.UtcNow;
            t.ReplacedByTokenHash = reason;
        }
    }
}

public class MediaAssetRepository : Repository<MediaAsset, Guid>, IMediaAssetRepository
{
    private readonly PortfolioDbContext _db;
    public MediaAssetRepository(PortfolioDbContext context) : base(context)
    {
        _db = context;
    }

    public async Task<bool> IsAssetInUseAsync(Guid assetId, CancellationToken ct = default)
    {
        bool inProfile = await _db.Profiles.AnyAsync(p => p.ProfileImageAssetId == assetId || p.ResumePdfAssetId == assetId, ct);
        if (inProfile) return true;

        bool inProject = await _db.Projects.AnyAsync(p => p.CoverImageAssetId == assetId, ct);
        if (inProject) return true;

        bool inGallery = await _db.ProjectImages.AnyAsync(g => g.MediaAssetId == assetId, ct);
        if (inGallery) return true;

        bool inExp = await _db.Experiences.AnyAsync(e => e.OrganizationLogoAssetId == assetId, ct);
        if (inExp) return true;

        bool inPub = await _db.Publications.AnyAsync(p => p.PdfAssetId == assetId, ct);
        if (inPub) return true;

        bool inCert = await _db.Certifications.AnyAsync(c => c.BadgeAssetId == assetId, ct);
        if (inCert) return true;

        bool inBlog = await _db.BlogArticles.AnyAsync(b => b.CoverImageAssetId == assetId, ct);
        if (inBlog) return true;

        return false;
    }
}

public class ProfileRepository : Repository<Profile, Guid>, IProfileRepository
{
    public ProfileRepository(PortfolioDbContext context) : base(context) { }

    public async Task<Profile?> GetActiveProfileAsync(CancellationToken ct = default) =>
        await _dbSet
            .Include(p => p.ProfileImageAsset)
            .Include(p => p.ResumePdfAsset)
            .FirstOrDefaultAsync(ct);
}

public class ExperienceRepository : Repository<Experience, Guid>, IExperienceRepository
{
    public ExperienceRepository(PortfolioDbContext context) : base(context) { }

    public async Task<IList<Experience>> GetPublishedOrderedAsync(CancellationToken ct = default) =>
        await _dbSet
            .AsNoTracking()
            .Include(e => e.OrganizationLogoAsset)
            .Where(e => e.Status == ContentStatus.Published)
            .OrderBy(e => e.DisplayOrder)
            .ThenByDescending(e => e.StartDate)
            .ToListAsync(ct);
}

public class ProjectRepository : Repository<Project, Guid>, IProjectRepository
{
    public ProjectRepository(PortfolioDbContext context) : base(context) { }

    public async Task<Project?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await _dbSet
            .Include(p => p.CoverImageAsset)
            .Include(p => p.GalleryImages).ThenInclude(g => g.MediaAsset)
            .FirstOrDefaultAsync(p => p.Slug.ToLower() == slug.ToLower(), ct);

    public async Task<Project?> GetPublishedBySlugAsync(string slug, CancellationToken ct = default) =>
        await _dbSet
            .AsNoTracking()
            .Include(p => p.CoverImageAsset)
            .Include(p => p.GalleryImages).ThenInclude(g => g.MediaAsset)
            .FirstOrDefaultAsync(p => p.Slug.ToLower() == slug.ToLower() && p.Status == ContentStatus.Published, ct);

    public async Task<IList<Project>> GetPublishedProjectsAsync(bool? featured, string? category, string? tag, CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking().Include(p => p.CoverImageAsset).Where(p => p.Status == ContentStatus.Published);

        if (featured.HasValue)
            query = query.Where(p => p.IsFeatured == featured.Value);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category.ToLower() == category.ToLower());

        if (!string.IsNullOrWhiteSpace(tag))
            query = query.Where(p => p.TechnologyTags.Contains(tag));

        return await query.OrderBy(p => p.DisplayOrder).ThenByDescending(p => p.PublishedAtUtc).ToListAsync(ct);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default) =>
        await _dbSet.AnyAsync(p => p.Slug.ToLower() == slug.ToLower() && (!excludeId.HasValue || p.Id != excludeId.Value), ct);
}

public class SkillCategoryRepository : Repository<SkillCategory, Guid>, ISkillCategoryRepository
{
    public SkillCategoryRepository(PortfolioDbContext context) : base(context) { }

    public async Task<IList<SkillCategory>> GetVisibleWithItemsAsync(CancellationToken ct = default) =>
        await _dbSet
            .AsNoTracking()
            .Include(c => c.Items)
            .Where(c => c.IsVisible)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);
}

public class SkillItemRepository : Repository<SkillItem, Guid>, ISkillItemRepository
{
    public SkillItemRepository(PortfolioDbContext context) : base(context) { }
}

public class EducationRepository : Repository<Education, Guid>, IEducationRepository
{
    public EducationRepository(PortfolioDbContext context) : base(context) { }

    public async Task<IList<Education>> GetPublishedOrderedAsync(CancellationToken ct = default) =>
        await _dbSet
            .AsNoTracking()
            .Where(e => e.Status == ContentStatus.Published)
            .OrderBy(e => e.DisplayOrder)
            .ThenByDescending(e => e.StartDate)
            .ToListAsync(ct);
}

public class PublicationRepository : Repository<Publication, Guid>, IPublicationRepository
{
    public PublicationRepository(PortfolioDbContext context) : base(context) { }

    public async Task<Publication?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await _dbSet
            .Include(p => p.PdfAsset)
            .FirstOrDefaultAsync(p => p.Slug.ToLower() == slug.ToLower(), ct);

    public async Task<IList<Publication>> GetPublishedOrderedAsync(CancellationToken ct = default) =>
        await _dbSet
            .AsNoTracking()
            .Include(p => p.PdfAsset)
            .Where(p => p.Status == ContentStatus.Published)
            .OrderBy(p => p.DisplayOrder)
            .ThenByDescending(p => p.PublicationYear)
            .ToListAsync(ct);
}

public class CertificationRepository : Repository<Certification, Guid>, ICertificationRepository
{
    public CertificationRepository(PortfolioDbContext context) : base(context) { }

    public async Task<IList<Certification>> GetPublishedOrderedAsync(CancellationToken ct = default) =>
        await _dbSet
            .AsNoTracking()
            .Include(c => c.BadgeAsset)
            .Where(c => c.Status == ContentStatus.Published)
            .OrderBy(c => c.DisplayOrder)
            .ThenByDescending(c => c.IssueDate)
            .ToListAsync(ct);
}

public class BlogArticleRepository : Repository<BlogArticle, Guid>, IBlogArticleRepository
{
    public BlogArticleRepository(PortfolioDbContext context) : base(context) { }

    public async Task<BlogArticle?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await _dbSet
            .Include(b => b.CoverImageAsset)
            .FirstOrDefaultAsync(b => b.Slug.ToLower() == slug.ToLower(), ct);

    public async Task<BlogArticle?> GetPublishedBySlugAsync(string slug, CancellationToken ct = default) =>
        await _dbSet
            .AsNoTracking()
            .Include(b => b.CoverImageAsset)
            .FirstOrDefaultAsync(b => b.Slug.ToLower() == slug.ToLower() && b.Status == ContentStatus.Published, ct);

    public async Task<IList<BlogArticle>> GetPublishedArticlesAsync(string? tag, CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking().Include(b => b.CoverImageAsset).Where(b => b.Status == ContentStatus.Published);

        if (!string.IsNullOrWhiteSpace(tag))
            query = query.Where(b => b.Tags.Contains(tag));

        return await query.OrderByDescending(b => b.PublishedAtUtc).ToListAsync(ct);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default) =>
        await _dbSet.AnyAsync(b => b.Slug.ToLower() == slug.ToLower() && (!excludeId.HasValue || b.Id != excludeId.Value), ct);
}

public class ContactMessageRepository : Repository<ContactMessage, Guid>, IContactMessageRepository
{
    public ContactMessageRepository(PortfolioDbContext context) : base(context) { }
}

