using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;
using FluentValidation;
using Portfolio.Application.Contracts;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Exceptions;

namespace Portfolio.Application.Features.Skills;

public record PublicSkillItemDto(Guid Id, string Name, string ContextDescription, int DisplayOrder);
public record PublicSkillCategoryDto(Guid Id, string Name, string Slug, string Description, int DisplayOrder, List<PublicSkillItemDto> Items);

public record AdminSkillItemDto(Guid Id, Guid CategoryId, string Name, string ContextDescription, int DisplayOrder, bool IsVisible);
public record AdminSkillCategoryDto(Guid Id, string Name, string Slug, string Description, int DisplayOrder, bool IsVisible, List<AdminSkillItemDto> Items);

public record CreateSkillCategoryCommand(string Name, string Slug, string Description, int DisplayOrder, bool IsVisible) : ICommand<AdminSkillCategoryDto>;
public record UpdateSkillCategoryCommand(Guid Id, string Name, string Slug, string Description, int DisplayOrder, bool IsVisible) : ICommand<AdminSkillCategoryDto>;
public record DeleteSkillCategoryCommand(Guid Id) : ICommand;

public record CreateSkillItemCommand(Guid CategoryId, string Name, string ContextDescription, int DisplayOrder, bool IsVisible) : ICommand<AdminSkillItemDto>;
public record UpdateSkillItemCommand(Guid Id, Guid CategoryId, string Name, string ContextDescription, int DisplayOrder, bool IsVisible) : ICommand<AdminSkillItemDto>;
public record DeleteSkillItemCommand(Guid Id) : ICommand;

public record ReorderSkillCategoriesCommand(List<Guid> OrderedCategoryIds) : ICommand;
public record ReorderSkillItemsCommand(Guid CategoryId, List<Guid> OrderedItemIds) : ICommand;

public class CreateSkillCategoryValidator : AbstractValidator<CreateSkillCategoryCommand>
{
    public CreateSkillCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(120);
    }
}

public class CreateSkillItemValidator : AbstractValidator<CreateSkillItemCommand>
{
    public CreateSkillItemValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public record GetPublicSkillsQuery : IQuery<IList<PublicSkillCategoryDto>>;
public record GetAdminSkillCategoriesQuery : IQuery<IList<AdminSkillCategoryDto>>;

public class SkillFeatureHandlers :
    IQueryHandler<GetPublicSkillsQuery, IList<PublicSkillCategoryDto>>,
    IQueryHandler<GetAdminSkillCategoriesQuery, IList<AdminSkillCategoryDto>>,
    ICommandHandler<CreateSkillCategoryCommand, AdminSkillCategoryDto>,
    ICommandHandler<UpdateSkillCategoryCommand, AdminSkillCategoryDto>,
    ICommandHandler<DeleteSkillCategoryCommand>,
    ICommandHandler<CreateSkillItemCommand, AdminSkillItemDto>,
    ICommandHandler<UpdateSkillItemCommand, AdminSkillItemDto>,
    ICommandHandler<DeleteSkillItemCommand>,
    ICommandHandler<ReorderSkillCategoriesCommand>,
    ICommandHandler<ReorderSkillItemsCommand>
{
    private readonly IApplicationUnitOfWork _unitOfWork;

    public SkillFeatureHandlers(IApplicationUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IList<PublicSkillCategoryDto>> Handle(GetPublicSkillsQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.SkillCategories.GetVisibleWithItemsAsync(cancellationToken);
        return categories.Select(c => new PublicSkillCategoryDto(
            c.Id,
            c.Name,
            c.Slug,
            c.Description,
            c.DisplayOrder,
            c.Items.Where(i => i.IsVisible).OrderBy(i => i.DisplayOrder).Select(i => new PublicSkillItemDto(
                i.Id, i.Name, i.ContextDescription, i.DisplayOrder
            )).ToList()
        )).ToList();
    }

    public async Task<IList<AdminSkillCategoryDto>> Handle(GetAdminSkillCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.SkillCategories.GetAllAsync(cancellationToken);
        return categories.OrderBy(c => c.DisplayOrder).Select(MapToAdminCategory).ToList();
    }

    public async Task<AdminSkillCategoryDto> Handle(CreateSkillCategoryCommand request, CancellationToken cancellationToken)
    {
        var cat = new SkillCategory
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = request.Slug.ToLowerInvariant().Trim(),
            Description = request.Description.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsVisible = request.IsVisible
        };

        await _unitOfWork.SkillCategories.AddAsync(cat, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return MapToAdminCategory(cat);
    }

    public async Task<AdminSkillCategoryDto> Handle(UpdateSkillCategoryCommand request, CancellationToken cancellationToken)
    {
        var cat = await _unitOfWork.SkillCategories.GetByIdAsync(request.Id, cancellationToken);
        if (cat == null)
            throw new NotFoundException(nameof(SkillCategory), request.Id);

        cat.Name = request.Name.Trim();
        cat.Slug = request.Slug.ToLowerInvariant().Trim();
        cat.Description = request.Description.Trim();
        cat.DisplayOrder = request.DisplayOrder;
        cat.IsVisible = request.IsVisible;

        await _unitOfWork.SaveAsync(cancellationToken);

        return MapToAdminCategory(cat);
    }

    public async Task Handle(DeleteSkillCategoryCommand request, CancellationToken cancellationToken)
    {
        var cat = await _unitOfWork.SkillCategories.GetByIdAsync(request.Id, cancellationToken);
        if (cat == null)
            throw new NotFoundException(nameof(SkillCategory), request.Id);

        _unitOfWork.SkillCategories.Remove(cat);
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    public async Task<AdminSkillItemDto> Handle(CreateSkillItemCommand request, CancellationToken cancellationToken)
    {
        var cat = await _unitOfWork.SkillCategories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (cat == null)
            throw new NotFoundException(nameof(SkillCategory), request.CategoryId);

        var item = new SkillItem
        {
            Id = Guid.NewGuid(),
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            ContextDescription = request.ContextDescription.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsVisible = request.IsVisible
        };

        await _unitOfWork.SkillItems.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return new AdminSkillItemDto(item.Id, item.CategoryId, item.Name, item.ContextDescription, item.DisplayOrder, item.IsVisible);
    }

    public async Task<AdminSkillItemDto> Handle(UpdateSkillItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.SkillItems.GetByIdAsync(request.Id, cancellationToken);
        if (item == null)
            throw new NotFoundException(nameof(SkillItem), request.Id);

        item.CategoryId = request.CategoryId;
        item.Name = request.Name.Trim();
        item.ContextDescription = request.ContextDescription.Trim();
        item.DisplayOrder = request.DisplayOrder;
        item.IsVisible = request.IsVisible;

        await _unitOfWork.SaveAsync(cancellationToken);

        return new AdminSkillItemDto(item.Id, item.CategoryId, item.Name, item.ContextDescription, item.DisplayOrder, item.IsVisible);
    }

    public async Task Handle(DeleteSkillItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.SkillItems.GetByIdAsync(request.Id, cancellationToken);
        if (item == null)
            throw new NotFoundException(nameof(SkillItem), request.Id);

        _unitOfWork.SkillItems.Remove(item);
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    public async Task Handle(ReorderSkillCategoriesCommand request, CancellationToken cancellationToken)
    {
        for (int i = 0; i < request.OrderedCategoryIds.Count; i++)
        {
            var cat = await _unitOfWork.SkillCategories.GetByIdAsync(request.OrderedCategoryIds[i], cancellationToken);
            if (cat != null) cat.DisplayOrder = i;
        }
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    public async Task Handle(ReorderSkillItemsCommand request, CancellationToken cancellationToken)
    {
        for (int i = 0; i < request.OrderedItemIds.Count; i++)
        {
            var item = await _unitOfWork.SkillItems.GetByIdAsync(request.OrderedItemIds[i], cancellationToken);
            if (item != null) item.DisplayOrder = i;
        }
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    private static AdminSkillCategoryDto MapToAdminCategory(SkillCategory c) => new(
        c.Id,
        c.Name,
        c.Slug,
        c.Description,
        c.DisplayOrder,
        c.IsVisible,
        c.Items.OrderBy(i => i.DisplayOrder).Select(i => new AdminSkillItemDto(
            i.Id, i.CategoryId, i.Name, i.ContextDescription, i.DisplayOrder, i.IsVisible
        )).ToList()
    );
}

