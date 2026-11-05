using Portfolio.Application.Contracts;
using Portfolio.Application.Contracts.Repositories;
using Portfolio.Infrastructure.Data.Repositories;

namespace Portfolio.Infrastructure.Data;

public class ApplicationUnitOfWork : UnitOfWork, IApplicationUnitOfWork
{
    private readonly PortfolioDbContext _context;

    public ApplicationUnitOfWork(PortfolioDbContext context) : base(context)
    {
        _context = context;
        AdminUsers = new AdminUserRepository(_context);
        RefreshTokens = new RefreshTokenRepository(_context);
        MediaAssets = new MediaAssetRepository(_context);
        Profiles = new ProfileRepository(_context);
        Experiences = new ExperienceRepository(_context);
        Projects = new ProjectRepository(_context);
        SkillCategories = new SkillCategoryRepository(_context);
        SkillItems = new SkillItemRepository(_context);
        Educations = new EducationRepository(_context);
        Publications = new PublicationRepository(_context);
        Certifications = new CertificationRepository(_context);
        BlogArticles = new BlogArticleRepository(_context);
        ContactMessages = new ContactMessageRepository(_context);
    }

    public IAdminUserRepository AdminUsers { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
    public IMediaAssetRepository MediaAssets { get; }
    public IProfileRepository Profiles { get; }
    public IExperienceRepository Experiences { get; }
    public IProjectRepository Projects { get; }
    public ISkillCategoryRepository SkillCategories { get; }
    public ISkillItemRepository SkillItems { get; }
    public IEducationRepository Educations { get; }
    public IPublicationRepository Publications { get; }
    public ICertificationRepository Certifications { get; }
    public IBlogArticleRepository BlogArticles { get; }
    public IContactMessageRepository ContactMessages { get; }
}

