using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;
using FluentValidation;
using Portfolio.Application.Contracts;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Exceptions;
using Portfolio.Domain.ValueObjects;

namespace Portfolio.Application.Features.Profile;

public record PublicProfileDto(
    string DisplayName,
    string Headline,
    string ShortIntro,
    string BiographyMarkdown,
    string? WorkingApproachMarkdown,
    string? PublicEmail,
    string? ProfileImageUrl,
    string? ResumePdfUrl,
    List<SocialLink> SocialLinks,
    string? SeoTitle,
    string? SeoDescription
);

public record AdminProfileDto(
    Guid Id,
    string DisplayName,
    string Headline,
    string ShortIntro,
    string BiographyMarkdown,
    string? WorkingApproachMarkdown,
    string? PublicEmail,
    bool ShowEmailPublicly,
    Guid? ProfileImageAssetId,
    string? ProfileImageUrl,
    Guid? ResumePdfAssetId,
    string? ResumePdfUrl,
    List<SocialLink> SocialLinks,
    string? SeoTitle,
    string? SeoDescription,
    DateTime UpdatedAtUtc
);

public record UpdateProfileCommand(
    string DisplayName,
    string Headline,
    string ShortIntro,
    string BiographyMarkdown,
    string? WorkingApproachMarkdown,
    string? PublicEmail,
    bool ShowEmailPublicly,
    Guid? ProfileImageAssetId,
    Guid? ResumePdfAssetId,
    List<SocialLink> SocialLinks,
    string? SeoTitle,
    string? SeoDescription
) : ICommand<AdminProfileDto>;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Headline).NotEmpty().MaximumLength(250);
        RuleFor(x => x.ShortIntro).NotEmpty().MaximumLength(600);
        RuleFor(x => x.BiographyMarkdown).NotEmpty();
        When(x => !string.IsNullOrEmpty(x.PublicEmail), () =>
        {
            RuleFor(x => x.PublicEmail).EmailAddress();
        });
    }
}

public record GetPublicProfileQuery : IQuery<PublicProfileDto?>;
public record GetAdminProfileQuery : IQuery<AdminProfileDto?>;

public class ProfileFeatureHandlers :
    IQueryHandler<GetPublicProfileQuery, PublicProfileDto?>,
    IQueryHandler<GetAdminProfileQuery, AdminProfileDto?>,
    ICommandHandler<UpdateProfileCommand, AdminProfileDto>
{
    private readonly IApplicationUnitOfWork _unitOfWork;

    public ProfileFeatureHandlers(IApplicationUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PublicProfileDto?> Handle(GetPublicProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.Profiles.GetActiveProfileAsync(cancellationToken);
        if (profile == null) return null;

        return new PublicProfileDto(
            profile.DisplayName,
            profile.Headline,
            profile.ShortIntro,
            profile.BiographyMarkdown,
            profile.WorkingApproachMarkdown,
            profile.ShowEmailPublicly ? profile.PublicEmail : null,
            profile.ProfileImageAsset?.PublicUrl,
            profile.ResumePdfAsset?.PublicUrl,
            profile.SocialLinks.OrderBy(x => x.DisplayOrder).ToList(),
            profile.SeoTitle,
            profile.SeoDescription
        );
    }

    public async Task<AdminProfileDto?> Handle(GetAdminProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.Profiles.GetActiveProfileAsync(cancellationToken);
        if (profile == null) return null;

        return MapToAdmin(profile);
    }

    public async Task<AdminProfileDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.Profiles.GetActiveProfileAsync(cancellationToken);
        if (profile == null)
        {
            profile = new Domain.Entities.Profile { Id = Guid.NewGuid() };
            await _unitOfWork.Profiles.AddAsync(profile, cancellationToken);
        }

        profile.DisplayName = request.DisplayName;
        profile.Headline = request.Headline;
        profile.ShortIntro = request.ShortIntro;
        profile.BiographyMarkdown = request.BiographyMarkdown;
        profile.WorkingApproachMarkdown = request.WorkingApproachMarkdown;
        profile.PublicEmail = request.PublicEmail;
        profile.ShowEmailPublicly = request.ShowEmailPublicly;
        profile.ProfileImageAssetId = request.ProfileImageAssetId;
        profile.ResumePdfAssetId = request.ResumePdfAssetId;
        profile.SocialLinks = request.SocialLinks ?? new List<SocialLink>();
        profile.SeoTitle = request.SeoTitle;
        profile.SeoDescription = request.SeoDescription;
        profile.UpdatedAtUtc = DateTime.UtcNow;

        await _unitOfWork.SaveAsync(cancellationToken);

        var refreshed = await _unitOfWork.Profiles.GetActiveProfileAsync(cancellationToken);
        return MapToAdmin(refreshed!);
    }

    private static AdminProfileDto MapToAdmin(Domain.Entities.Profile p) => new(
        p.Id,
        p.DisplayName,
        p.Headline,
        p.ShortIntro,
        p.BiographyMarkdown,
        p.WorkingApproachMarkdown,
        p.PublicEmail,
        p.ShowEmailPublicly,
        p.ProfileImageAssetId,
        p.ProfileImageAsset?.PublicUrl,
        p.ResumePdfAssetId,
        p.ResumePdfAsset?.PublicUrl,
        p.SocialLinks,
        p.SeoTitle,
        p.SeoDescription,
        p.UpdatedAtUtc
    );
}

