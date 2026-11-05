using Portfolio.Domain.Contracts;
using Portfolio.Domain.Entities;

namespace Portfolio.Application.Contracts.Repositories;

public interface IAdminUserRepository : IRepository<AdminUser, Guid>
{
    Task<AdminUser?> GetByEmailAsync(string email, CancellationToken ct = default);
}

public interface IRefreshTokenRepository : IRepository<RefreshToken, Guid>
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task RevokeFamilyAsync(Guid adminUserId, string? reason, CancellationToken ct = default);
}

public interface IMediaAssetRepository : IRepository<MediaAsset, Guid>
{
    Task<bool> IsAssetInUseAsync(Guid assetId, CancellationToken ct = default);
}

public interface IProfileRepository : IRepository<Profile, Guid>
{
    Task<Profile?> GetActiveProfileAsync(CancellationToken ct = default);
}

public interface IExperienceRepository : IRepository<Experience, Guid>
{
    Task<IList<Experience>> GetPublishedOrderedAsync(CancellationToken ct = default);
}

public interface IProjectRepository : IRepository<Project, Guid>
{
    Task<Project?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Project?> GetPublishedBySlugAsync(string slug, CancellationToken ct = default);
    Task<IList<Project>> GetPublishedProjectsAsync(bool? featured, string? category, string? tag, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);
}

public interface ISkillCategoryRepository : IRepository<SkillCategory, Guid>
{
    Task<IList<SkillCategory>> GetVisibleWithItemsAsync(CancellationToken ct = default);
}

public interface ISkillItemRepository : IRepository<SkillItem, Guid>
{
}

public interface IEducationRepository : IRepository<Education, Guid>
{
    Task<IList<Education>> GetPublishedOrderedAsync(CancellationToken ct = default);
}

public interface IPublicationRepository : IRepository<Publication, Guid>
{
    Task<Publication?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IList<Publication>> GetPublishedOrderedAsync(CancellationToken ct = default);
}

public interface ICertificationRepository : IRepository<Certification, Guid>
{
    Task<IList<Certification>> GetPublishedOrderedAsync(CancellationToken ct = default);
}

public interface IBlogArticleRepository : IRepository<BlogArticle, Guid>
{
    Task<BlogArticle?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<BlogArticle?> GetPublishedBySlugAsync(string slug, CancellationToken ct = default);
    Task<IList<BlogArticle>> GetPublishedArticlesAsync(string? tag, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);
}

public interface IContactMessageRepository : IRepository<ContactMessage, Guid>
{
}

