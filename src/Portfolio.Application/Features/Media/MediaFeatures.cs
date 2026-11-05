using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Contracts;
using Portfolio.Application.Contracts.Services;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Exceptions;

namespace Portfolio.Application.Features.Media;

public record MediaAssetDto(
    Guid Id,
    string OriginalFileName,
    string StorageKey,
    string PublicUrl,
    string ContentType,
    long SizeBytes,
    int? Width,
    int? Height,
    string AltText,
    DateTime CreatedAtUtc
);

public record UploadMediaAssetCommand(
    Stream FileStream,
    string OriginalFileName,
    string ContentType,
    string Folder,
    string? AltText
) : ICommand<MediaAssetDto>;

public record DeleteMediaAssetCommand(Guid Id) : ICommand;
public record UpdateMediaAssetAltTextCommand(Guid Id, string AltText) : ICommand;

public record GetAdminMediaAssetsQuery(int PageNumber = 1, int PageSize = 20) : IQuery<PaginatedResult<MediaAssetDto>>;
public record GetAdminMediaAssetByIdQuery(Guid Id) : IQuery<MediaAssetDto?>;

public class MediaFeatureHandlers :
    ICommandHandler<UploadMediaAssetCommand, MediaAssetDto>,
    ICommandHandler<DeleteMediaAssetCommand>,
    ICommandHandler<UpdateMediaAssetAltTextCommand>,
    IQueryHandler<GetAdminMediaAssetsQuery, PaginatedResult<MediaAssetDto>>,
    IQueryHandler<GetAdminMediaAssetByIdQuery, MediaAssetDto?>
{
    private readonly IApplicationUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public MediaFeatureHandlers(IApplicationUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<MediaAssetDto> Handle(UploadMediaAssetCommand request, CancellationToken cancellationToken)
    {
        var result = await _fileStorageService.UploadAsync(
            request.FileStream,
            request.OriginalFileName,
            request.ContentType,
            request.Folder,
            cancellationToken
        );

        var asset = new MediaAsset
        {
            Id = Guid.NewGuid(),
            OriginalFileName = request.OriginalFileName,
            StorageKey = result.StorageKey,
            PublicUrl = result.PublicUrl,
            ContentType = request.ContentType,
            SizeBytes = result.SizeBytes,
            Width = result.Width,
            Height = result.Height,
            AltText = request.AltText?.Trim() ?? string.Empty,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.MediaAssets.AddAsync(asset, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return MapToDto(asset);
    }

    public async Task Handle(DeleteMediaAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await _unitOfWork.MediaAssets.GetByIdAsync(request.Id, cancellationToken);
        if (asset == null)
            throw new NotFoundException(nameof(MediaAsset), request.Id);

        bool inUse = await _unitOfWork.MediaAssets.IsAssetInUseAsync(request.Id, cancellationToken);
        if (inUse)
            throw new ConflictException("Cannot delete media asset because it is currently referenced by one or more entities.");

        await _fileStorageService.DeleteAsync(asset.StorageKey, cancellationToken);

        _unitOfWork.MediaAssets.Remove(asset);
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    public async Task Handle(UpdateMediaAssetAltTextCommand request, CancellationToken cancellationToken)
    {
        var asset = await _unitOfWork.MediaAssets.GetByIdAsync(request.Id, cancellationToken);
        if (asset == null)
            throw new NotFoundException(nameof(MediaAsset), request.Id);

        asset.AltText = request.AltText.Trim();
        await _unitOfWork.SaveAsync(cancellationToken);
    }

    public async Task<PaginatedResult<MediaAssetDto>> Handle(GetAdminMediaAssetsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.MediaAssets.Query();
        var total = query.Count();
        var items = query
            .OrderByDescending(a => a.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = items.Select(MapToDto).ToList();
        return PaginatedResult<MediaAssetDto>.Create(dtos, total, request.PageNumber, request.PageSize);
    }

    public async Task<MediaAssetDto?> Handle(GetAdminMediaAssetByIdQuery request, CancellationToken cancellationToken)
    {
        var asset = await _unitOfWork.MediaAssets.GetByIdAsync(request.Id, cancellationToken);
        return asset == null ? null : MapToDto(asset);
    }

    private static MediaAssetDto MapToDto(MediaAsset a) => new(
        a.Id,
        a.OriginalFileName,
        a.StorageKey,
        a.PublicUrl,
        a.ContentType,
        a.SizeBytes,
        a.Width,
        a.Height,
        a.AltText,
        a.CreatedAtUtc
    );
}

