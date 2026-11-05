using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;
using FluentValidation;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Contracts;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Domain.Exceptions;

namespace Portfolio.Application.Features.Experiences;

public record PublicExperienceDto(
    Guid Id,
    string Organization,
    string? OrganizationLogoUrl,
    string RoleTitle,
    string? EmploymentType,
    string? Location,
    string? WorkArrangement,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsCurrent,
    string Summary,
    List<string> Responsibilities,
    List<string> Outcomes,
    List<string> TechnologyTags,
    int DisplayOrder
);

public record AdminExperienceDto(
    Guid Id,
    string Organization,
    Guid? OrganizationLogoAssetId,
    string? OrganizationLogoUrl,
    string RoleTitle,
    string? EmploymentType,
    string? Location,
    string? WorkArrangement,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsCurrent,
    string Summary,
    List<string> Responsibilities,
    List<string> Outcomes,
    List<string> TechnologyTags,
    int DisplayOrder,
    ContentStatus Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public record CreateExperienceCommand(
    string Organization,
    Guid? OrganizationLogoAssetId,
    string RoleTitle,
    string? EmploymentType,
    string? Location,
    string? WorkArrangement,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsCurrent,
    string Summary,
    List<string> Responsibilities,
    List<string> Outcomes,
    List<string> TechnologyTags,
    int DisplayOrder,
    ContentStatus Status
) : ICommand<AdminExperienceDto>;

public record UpdateExperienceCommand(
    Guid Id,
    string Organization,
    Guid? OrganizationLogoAssetId,
    string RoleTitle,
    string? EmploymentType,
    string? Location,
    string? WorkArrangement,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsCurrent,
    string Summary,
    List<string> Responsibilities,
    List<string> Outcomes,
    List<string> TechnologyTags,
    int DisplayOrder,
    ContentStatus Status
) : ICommand<AdminExperienceDto>;

public record DeleteExperienceCommand(Guid Id) : ICommand;
public record ChangeExperienceStatusCommand(Guid Id, ContentStatus Status) : ICommand;
public record ReorderExperiencesCommand(List<Guid> OrderedIds) : ICommand;

public class CreateExperienceCommandValidator : AbstractValidator<CreateExperienceCommand>
{
    public CreateExperienceCommandValidator()
    {
        RuleFor(x => x.Organization).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RoleTitle).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Summary).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
    }
}

public class UpdateExperienceCommandValidator : AbstractValidator<UpdateExperienceCommand>
{
    public UpdateExperienceCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Organization).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RoleTitle).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Summary).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
    }
}

public record GetPublicExperiencesQuery : IQuery<IList<PublicExperienceDto>>;
public record GetAdminExperiencesQuery : IQuery<IList<AdminExperienceDto>>;
public record GetAdminExperienceByIdQuery(Guid Id) : IQuery<AdminExperienceDto?>;

public class ExperienceFeatureHandlers :
    IQueryHandler<GetPublicExperiencesQuery, IList<PublicExperienceDto>>,
    IQueryHandler<GetAdminExperiencesQuery, IList<AdminExperienceDto>>,
    IQueryHandler<GetAdminExperienceByIdQuery, AdminExperienceDto?>,
    ICommandHandler<CreateExperienceCommand, AdminExperienceDto>,
    ICommandHandler<UpdateExperienceCommand, AdminExperienceDto>,
    ICommandHandler<DeleteExperienceCommand>,
    ICommandHandler<ChangeExperienceStatusCommand>,
    ICommandHandler<ReorderExperiencesCommand>
{
    private readonly IApplicationUnitOfWork _unitOfWork;

    public ExperienceFeatureHandlers(IApplicationUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IList<PublicExperienceDto>> Handle(GetPublicExperiencesQuery request, CancellationToken cancellationToken)
    {
        var list = await _unitOfWork.Experiences.GetPublishedOrderedAsync(cancellationToken);
        return list.Select(e => new PublicExperienceDto(
            e.Id,
            e.Organization,
            e.OrganizationLogoAsset?.PublicUrl,
            e.RoleTitle,
            e.EmploymentType,
            e.Location,
            e.WorkArrangement,
            e.StartDate,
            e.EndDate,
            e.IsCurrent,
            e.Summary,
            e.Responsibilities,
            e.Outcomes,
            e.TechnologyTags,
            e.DisplayOrder
        )).ToList();
    }

    public async Task<IList<AdminExperienceDto>> Handle(GetAdminExperiencesQuery request, CancellationToken cancellationToken)
    {
        var list = await _unitOfWork.Experiences.GetAllAsync(cancellationToken);
        return list.OrderBy(e => e.DisplayOrder).Select(MapToAdmin).ToList();
    }

    public async Task<AdminExperienceDto?> Handle(GetAdminExperienceByIdQuery request, CancellationToken cancellationToken)
    {
        var exp = await _unitOfWork.Experiences.GetByIdAsync(request.Id, cancellationToken);
        return exp == null ? null : MapToAdmin(exp);
    }

    public async Task<AdminExperienceDto> Handle(CreateExperienceCommand request, CancellationToken cancellationToken)
    {
        var exp = new Experience
        {
            Id = Guid.NewGuid(),
            Organization = request.Organization.Trim(),
            OrganizationLogoAssetId = request.OrganizationLogoAssetId,
            RoleTitle = request.RoleTitle.Trim(),
            EmploymentType = request.EmploymentType,
            Location = request.Location,
            WorkArrangement = request.WorkArrangement,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsCurrent = request.IsCurrent,
            Summary = request.Summary.Trim(),
            Responsibilities = request.Responsibilities ?? new(),
            Outcomes = request.Outcomes ?? new(),
            TechnologyTags = request.TechnologyTags ?? new(),
            DisplayOrder = request.DisplayOrder,
            Status = request.Status,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.Experiences.AddAsync(exp, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return MapToAdmin(exp);
    }

    public async Task<AdminExperienceDto> Handle(UpdateExperienceCommand request, CancellationToken cancellationToken)
    {
        var exp = await _unitOfWork.Experiences.GetByIdAsync(request.Id, cancellationToken);
        if (exp == null)
            throw new NotFoundException(nameof(Experience), request.Id);

        exp.Organization = request.Organization.Trim();
        exp.OrganizationLogoAssetId = request.OrganizationLogoAssetId;
        exp.RoleTitle = request.RoleTitle.Trim();
        exp.EmploymentType = request.EmploymentType;
        exp.Location = request.Location;
        exp.WorkArrangement = request.WorkArrangement;
        exp.StartDate = request.StartDate;
        exp.EndDate = request.EndDate;
        exp.IsCurrent = request.IsCurrent;
        exp.Summary = request.Summary.Trim();
        exp.Responsibilities = request.Responsibilities ?? new();
        exp.Outcomes = request.Outcomes ?? new();
        exp.TechnologyTags = request.TechnologyTags ?? new();
        exp.DisplayOrder = request.DisplayOrder;
        exp.Status = request.Status;
        exp.UpdatedAtUtc = DateTime.UtcNow;

        await _unitOfWork.SaveAsync(cancellationToken);

        return MapToAdmin(exp);
    }

    public async Task Handle(DeleteExperienceCommand request, CancellationToken cancellationToken)
    {
        var exp = await _unitOfWork.Experiences.GetByIdAsync(request.Id, cancellationToken);
        if (exp == null)
            throw new NotFoundException(nameof(Experience), request.Id);

        _unitOfWork.Experiences.Remove(exp);
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    public async Task Handle(ChangeExperienceStatusCommand request, CancellationToken cancellationToken)
    {
        var exp = await _unitOfWork.Experiences.GetByIdAsync(request.Id, cancellationToken);
        if (exp == null)
            throw new NotFoundException(nameof(Experience), request.Id);

        exp.Status = request.Status;
        exp.UpdatedAtUtc = DateTime.UtcNow;
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    public async Task Handle(ReorderExperiencesCommand request, CancellationToken cancellationToken)
    {
        for (int i = 0; i < request.OrderedIds.Count; i++)
        {
            var exp = await _unitOfWork.Experiences.GetByIdAsync(request.OrderedIds[i], cancellationToken);
            if (exp != null)
            {
                exp.DisplayOrder = i;
                exp.UpdatedAtUtc = DateTime.UtcNow;
            }
        }
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    private static AdminExperienceDto MapToAdmin(Experience e) => new(
        e.Id,
        e.Organization,
        e.OrganizationLogoAssetId,
        e.OrganizationLogoAsset?.PublicUrl,
        e.RoleTitle,
        e.EmploymentType,
        e.Location,
        e.WorkArrangement,
        e.StartDate,
        e.EndDate,
        e.IsCurrent,
        e.Summary,
        e.Responsibilities,
        e.Outcomes,
        e.TechnologyTags,
        e.DisplayOrder,
        e.Status,
        e.CreatedAtUtc,
        e.UpdatedAtUtc
    );
}

