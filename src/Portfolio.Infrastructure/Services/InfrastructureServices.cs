using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Portfolio.Application.Contracts.Services;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Exceptions;

namespace Portfolio.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            Algorithm,
            KeySize);

        return $"{Convert.ToHexString(salt)}:{Convert.ToHexString(hash)}";
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var parts = passwordHash.Split(':');
        if (parts.Length != 2) return false;

        try
        {
            byte[] salt = Convert.FromHexString(parts[0]);
            byte[] expectedHash = Convert.FromHexString(parts[1]);

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                Algorithm,
                KeySize);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch
        {
            return false;
        }
    }
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateAccessToken(AdminUser user)
    {
        var key = _config["Jwt:SigningKey"] ?? "a-very-long-fallback-secret-key-that-is-at-least-32-bytes-long!";
        var issuer = _config["Jwt:Issuer"] ?? "jobayer-portfolio-api";
        var audience = _config["Jwt:Audience"] ?? "jobayer-portfolio-admin";
        var expiryMinutes = int.TryParse(_config["Jwt:AccessTokenExpirationMinutes"], out var m) ? m : 15;

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return tokenHandler.WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(Guid adminUserId, string? ipAddress)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var rawToken = Convert.ToBase64String(randomBytes);

        var expiryDays = int.TryParse(_config["Jwt:RefreshTokenExpirationDays"], out var d) ? d : 14;

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            TokenHash = HashToken(rawToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(expiryDays),
            CreatedByIp = ipAddress,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public string HashToken(string rawToken)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadRoot;
    private readonly string _publicBaseUrl;

    public LocalFileStorageService(IConfiguration config)
    {
        var configured = config["Storage:LocalRootPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _uploadRoot = Path.IsPathRooted(configured) ? configured : Path.GetFullPath(configured);
        _publicBaseUrl = config["Storage:PublicBaseUrl"] ?? "/uploads";
        Directory.CreateDirectory(_uploadRoot);
    }

    public async Task<StoredFileResult> UploadAsync(Stream stream, string originalFileName, string contentType, string folder, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        ValidateMagicBytes(stream, ext);

        var subFolder = Path.Combine(_uploadRoot, folder, DateTime.UtcNow.ToString("yyyy"), DateTime.UtcNow.ToString("MM"));
        Directory.CreateDirectory(subFolder);

        var safeFileName = $"{Guid.NewGuid():N}{ext}";
        var physicalPath = Path.Combine(subFolder, safeFileName);
        var relativeUrl = $"{_publicBaseUrl.TrimEnd('/')}/{folder}/{DateTime.UtcNow:yyyy}/{DateTime.UtcNow:MM}/{safeFileName}";

        stream.Position = 0;
        using (var fs = new FileStream(physicalPath, FileMode.Create, FileAccess.Write))
        {
            await stream.CopyToAsync(fs, ct);
        }

        return new StoredFileResult(
            StorageKey: Path.Combine(folder, DateTime.UtcNow.ToString("yyyy"), DateTime.UtcNow.ToString("MM"), safeFileName).Replace('\\', '/'),
            PublicUrl: relativeUrl,
            SizeBytes: stream.Length,
            Width: null,
            Height: null
        );
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        var physicalPath = Path.Combine(_uploadRoot, storageKey.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }
        return Task.CompletedTask;
    }

    private static void ValidateMagicBytes(Stream stream, string extension)
    {
        if (stream.Length < 4)
            throw new ValidationException(new Dictionary<string, string[]> { { "File", new[] { "File is empty or corrupted." } } });

        stream.Position = 0;
        var header = new byte[8];
        int read = stream.Read(header, 0, header.Length);
        stream.Position = 0;

        if (read < 4)
            throw new ValidationException(new Dictionary<string, string[]> { { "File", new[] { "File header could not be read." } } });

        bool valid = extension switch
        {
            ".jpg" or ".jpeg" => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47,
            ".webp" => header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46,
            ".pdf" => header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46,
            ".svg" => true, // Text-based
            _ => false
        };

        if (!valid)
            throw new ValidationException(new Dictionary<string, string[]> { { "File", new[] { $"File signature does not match allowed extension '{extension}'." } } });
    }
}

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendContactNotificationAsync(ContactMessage message, CancellationToken ct = default)
    {
        bool enabled = bool.TryParse(_config["Email:Enabled"], out var en) && en;
        if (!enabled)
        {
            _logger.LogInformation("[Email disabled] Contact message received from {Name} ({Email}): {Subject}", message.SenderName, message.SenderEmail, message.Subject);
            return;
        }

        try
        {
            var mail = new MimeMessage();
            mail.From.Add(new MailboxAddress("Portfolio API", _config["Email:FromAddress"] ?? "noreply@jobayer.dev"));
            mail.To.Add(new MailboxAddress("Admin", _config["Email:AdminNotificationAddress"] ?? "admin@jobayer.dev"));
            mail.Subject = $"[Portfolio Inquiry] {message.Subject ?? "New Message"} - from {message.SenderName}";

            var body = new BodyBuilder
            {
                TextBody = $"You received a new inquiry from {message.SenderName} ({message.SenderEmail}):\n\n{message.MessageBody}"
            };
            mail.Body = body.ToMessageBody();

            using var smtp = new SmtpClient();
            var host = _config["Email:SmtpHost"] ?? "localhost";
            var port = int.TryParse(_config["Email:SmtpPort"], out var p) ? p : 587;
            await smtp.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.Auto, ct);

            var user = _config["Email:SmtpUser"];
            var pass = _config["Email:SmtpPassword"];
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
            {
                await smtp.AuthenticateAsync(user, pass, ct);
            }

            await smtp.SendAsync(mail, ct);
            await smtp.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification for contact message {Id}", message.Id);
        }
    }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(claim?.Value, out var id) ? id : null;
        }
    }

    public string? Email =>
        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value
        ?? _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}

