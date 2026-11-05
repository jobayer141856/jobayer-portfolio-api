using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;
using FluentValidation;
using Portfolio.Application.Contracts;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Domain.Exceptions;

namespace Portfolio.Application.Features.Educations;

public record PublicEducationDto(
    Guid Id,
    string Institution,
    string Degree,
    string Department,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsCurrent,
    string? ResultText,
    string? Description,
    string? InstitutionUrl,
    int DisplayOrder
);

public record AdminEducationDto(
    Guid Id,
    string Institution,
    string Degree,
    string Department,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsCurrent,
    string? ResultText,
    bool ShowResultPublicly,
    string? Description,
    string? InstitutionUrl,
    int DisplayOrder,
    ContentStatus Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public record CreateEducationCommand(
    string Institution,
    string Degree,
    string Department,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsCurrent,
    string? ResultText,
    bool ShowResultPublicly,
    string? Description,
    string? InstitutionUrl,
    int DisplayOrder,
    ContentStatus Status
) : ICommand<AdminEducationDto>;

public record UpdateEducationCommand(
    Guid Id,
    string Institution,
    string Degree,
    string Department,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsCurrent,
    string? ResultText,
    bool ShowResultPublicly,
    string? Description,
    string? InstitutionUrl,
    int DisplayOrder,
    ContentStatus Status
) : ICommand<AdminEducationDto>;

public record DeleteEducationCommand(Guid Id) : ICommand;
public record ChangeEducationStatusCommand(Guid Id, ContentStatus Status) : ICommand;
public record ReorderEducationsCommand(List<Guid> OrderedIds) : ICommand;

public class CreateEducationCommandValidator : AbstractValidator<CreateEducationCommand>
{
    public CreateEducationCommandValidator()
    {
        RuleFor(x => x.Institution).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Degree).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Department).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StartDate).NotEmpty();
    }
}

public record GetPublicEducationsQuery : IQuery<IList<PublicEducationDto>>;
public record GetAdminEducationsQuery : IQuery<IList<AdminEducationDto>>;
public record GetAdminEducationByIdQuery(Guid Id) : IQuery<AdminEducationDto?>;

public class EducationFeatureHandlers :
    IQueryHandler<GetPublicEducationsQuery, IList<PublicEducationDto>>,
    IQueryHandler<GetAdminEducationsQuery, IList<AdminEducationDto>>,
    IQueryHandler<GetAdminEducationByIdQuery, AdminEducationDto?>,
    ICommandHandler<CreateEducationCommand, AdminEducationDto>,
    ICommandHandler<UpdateEducationCommand, AdminEducationDto>,
    ICommandHandler<DeleteEducationCommand>,
    ICommandHandler<ChangeEducationStatusCommand>,
    ICommandHandler<ReorderEducationsCommand>
{
    private readonly IApplicationUnitOfWork _unitOfWork;

    public EducationFeatureHandlers(IApplicationUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IList<PublicEducationDto>> Handle(GetPublicEducationsQuery request, CancellationToken cancellationToken)
    {
        var list = await _unitOfWork.Educations.GetPublishedOrderedAsync(cancellationToken);
        return list.Select(e => new PublicEducationDto(
            e.Id,
            e.Institution,
            e.Degree,
            e.Department,
            e.StartDate,
            e.EndDate,
            e.IsCurrent,
            e.ShowResultPublicly ? e.ResultText : null,
            e.Description,
            e.InstitutionUrl,
            e.DisplayOrder
        )).ToList();
    }

    public async Task<IList<AdminEducationDto>> Handle(GetAdminEducationsQuery request, CancellationToken cancellationToken)
    {
        var list = await _unitOfWork.Educations.GetAllAsync(cancellationToken);
        return list.OrderBy(e => e.DisplayOrder).Select(MapToAdmin).ToList();
    }

    public async Task<AdminEducationDto?> Handle(GetAdminEducationByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.Educations.GetByIdAsync(request.Id, cancellationToken);
        return item == null ? null : MapToAdmin(item);
    }

    public async Task<AdminEducationDto> Handle(CreateEducationCommand request, CancellationToken cancellationToken)
    {
        var item = new Education
        {
            Id = Guid.NewGuid(),
            Institution = request.Institution.Trim(),
            Degree = request.Degree.Trim(),
            Department = request.Department.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsCurrent = request.IsCurrent,
            ResultText = request.ResultText,
            ShowResultPublicly = request.ShowResultPublicly,
            Description = request.Description,
            InstitutionUrl = request.InstitutionUrl,
            DisplayOrder = request.DisplayOrder,
            Status = request.Status,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.Educations.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return MapToAdmin(item);
    }

    public async Task<AdminEducationDto> Handle(UpdateEducationCommand request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.Educations.GetByIdAsync(request.Id, cancellationToken);
        if (item == null)
            throw new NotFoundException(nameof(Education), request.Id);

        item.Institution = request.Institution.Trim();
        item.Degree = request.Degree.Trim();
        item.Department = request.Department.Trim();
        item.StartDate = request.StartDate;
        item.EndDate = request.EndDate;
        item.IsCurrent = request.IsCurrent;
        item.ResultText = request.ResultText;
        item.ShowResultPublicly = request.ShowResultPublicly;
        item.Description = request.Description;
        item.InstitutionUrl = request.InstitutionUrl;
        item.DisplayOrder = request.DisplayOrder;
        item.Status = request.Status;
        item.UpdatedAtUtc = DateTime.UtcNow;

        await _unitOfWork.SaveAsync(cancellationToken);

        return MapToAdmin(item);
    }

    public async Task Handle(DeleteEducationCommand request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.Educations.GetByIdAsync(request.Id, cancellationToken);
        if (item == null)
            throw new NotFoundException(nameof(Education), request.Id);

        _unitOfWork.Educations.Remove(item);
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    public async Task Handle(ChangeEducationStatusCommand request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.Educations.GetByIdAsync(request.Id, cancellationToken);
        if (item == null)
            throw new NotFoundException(nameof(Education), request.Id);

        item.Status = request.Status;
        item.UpdatedAtUtc = DateTime.UtcNow;
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    public async Task Handle(ReorderEducationsCommand request, CancellationToken cancellationToken)
    {
        for (int i = 0; i < request.OrderedIds.Count; i++)
        {
            var item = await _unitOfWork.Educations.GetByIdAsync(request.OrderedIds[i], cancellationToken);
            if (item != null)
            {
                item.DisplayOrder = i;
                item.UpdatedAtUtc = DateTime.UtcNow;
            }
        }
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    private static AdminEducationDto MapToAdmin(Education e) => new(
        e.Id,
        e.Institution,
        e.Degree,
        e.Department,
        e.StartDate,
        e.EndDate,
        e.IsCurrent,
        e.ResultText,
        e.ShowResultPublicly,
        e.Description,
        e.InstitutionUrl,
        e.DisplayOrder,
        e.Status,
        e.CreatedAtUtc,
        e.UpdatedAtUtc
    );
}

