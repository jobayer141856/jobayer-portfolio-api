using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Domain.Exceptions;

namespace Portfolio.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        var (statusCode, problemDetails) = exception switch
        {
            ValidationException valEx => (
                StatusCodes.Status400BadRequest,
                new ValidationProblemDetails(valEx.Errors)
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    Title = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = valEx.Message,
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                } as ProblemDetails
            ),
            FluentValidation.ValidationException fvEx => (
                StatusCodes.Status400BadRequest,
                new ValidationProblemDetails(fvEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    Title = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = fvEx.Message,
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                } as ProblemDetails
            ),
            NotFoundException nfEx => (
                StatusCodes.Status404NotFound,
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    Title = "Resource not found.",
                    Status = StatusCodes.Status404NotFound,
                    Detail = nfEx.Message,
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                }
            ),
            ConflictException cfEx => (
                StatusCodes.Status409Conflict,
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    Title = "Resource conflict.",
                    Status = StatusCodes.Status409Conflict,
                    Detail = cfEx.Message,
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                }
            ),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                    Title = "Unauthorized.",
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = "Authentication is required to access this resource.",
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                }
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                    Title = "An unexpected error occurred.",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An unexpected error occurred. Please try again later.",
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                }
            )
        };

        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception occurred on path {Path} (TraceId: {TraceId})", context.Request.Path, traceId);
        }
        else
        {
            _logger.LogWarning("Handled domain exception on path {Path}: {Message} (TraceId: {TraceId})", context.Request.Path, exception.Message, traceId);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, problemDetails.GetType()));
    }
}

