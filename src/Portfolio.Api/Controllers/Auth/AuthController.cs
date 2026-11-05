using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Application.Contracts.Services;
using Portfolio.Application.Features.Auth;

namespace Portfolio.Api.Controllers.Auth;

[Route("api/v1/auth")]
public class AuthController : BaseApiController
{
    private const string RefreshCookieName = "X-Refresh-Token";

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await SendCommandAsync(new LoginCommand(dto.Email, dto.Password, ip));

        SetRefreshTokenCookie(result.RefreshToken, result.ExpiresAtUtc);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshRequestDto? dto)
    {
        var rawToken = dto?.RefreshToken ?? Request.Cookies[RefreshCookieName];
        if (string.IsNullOrWhiteSpace(rawToken))
            return BadRequest(new { message = "Refresh token is missing." });

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await SendCommandAsync(new RefreshTokenCommand(rawToken, ip));

        SetRefreshTokenCookie(result.RefreshToken, result.ExpiresAtUtc);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        var rawToken = Request.Cookies[RefreshCookieName];
        await SendCommandAsync(new LogoutCommand(rawToken));

        Response.Cookies.Delete(RefreshCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize(Policy = "AdminPolicy")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe([FromServices] ICurrentUserService currentUserService)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var user = await SendQueryAsync(new GetMeQuery(currentUserService.UserId.Value));
        return Ok(user);
    }

    [HttpPost("change-password")]
    [Authorize(Policy = "AdminPolicy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, [FromServices] ICurrentUserService currentUserService)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        await SendCommandAsync(new ChangePasswordCommand(currentUserService.UserId.Value, dto.CurrentPassword, dto.NewPassword));
        return NoContent();
    }

    private void SetRefreshTokenCookie(string token, DateTime expiresAtUtc)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAtUtc
        };
        Response.Cookies.Append(RefreshCookieName, token, cookieOptions);
    }
}

public record RefreshRequestDto(string? RefreshToken);
