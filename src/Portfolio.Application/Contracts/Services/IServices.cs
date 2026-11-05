using System.Security.Claims;
using Portfolio.Domain.Entities;

namespace Portfolio.Application.Contracts.Services;

public interface ITokenService
{
    string GenerateAccessToken(AdminUser user);
    RefreshToken GenerateRefreshToken(Guid adminUserId, string? ipAddress);
    string HashToken(string rawToken);
}

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

public record StoredFileResult(string StorageKey, string PublicUrl, long SizeBytes, int? Width, int? Height);

public interface IFileStorageService
{
    Task<StoredFileResult> UploadAsync(Stream stream, string originalFileName, string contentType, string folder, CancellationToken ct = default);
    Task DeleteAsync(string storageKey, CancellationToken ct = default);
}

public interface IEmailService
{
    Task SendContactNotificationAsync(ContactMessage message, CancellationToken ct = default);
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}

