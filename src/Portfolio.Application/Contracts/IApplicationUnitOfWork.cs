using Portfolio.Application.Contracts.Repositories;
using Portfolio.Domain.Contracts;

namespace Portfolio.Application.Contracts;

public interface IApplicationUnitOfWork : IUnitOfWork
{
    IAdminUserRepository AdminUsers { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IMediaAssetRepository MediaAssets { get; }
    IProfileRepository Profiles { get; }
    IExperienceRepository Experiences { get; }
    IProjectRepository Projects { get; }
    ISkillCategoryRepository SkillCategories { get; }
    ISkillItemRepository SkillItems { get; }
    IEducationRepository Educations { get; }
    IPublicationRepository Publications { get; }
    ICertificationRepository Certifications { get; }
    IBlogArticleRepository BlogArticles { get; }
    IContactMessageRepository ContactMessages { get; }
}

