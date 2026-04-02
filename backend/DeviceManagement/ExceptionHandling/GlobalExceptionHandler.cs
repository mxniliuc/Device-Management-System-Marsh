using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace DeviceManagement.ExceptionHandling;

/// <summary>
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private const int ClientClosedRequest = 499;

    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException)
        {
            if (httpContext.RequestAborted.IsCancellationRequested)
            {
                _logger.LogDebug(exception, "Request was cancelled by the client.");
                httpContext.Response.StatusCode = ClientClosedRequest;
                return true;
            }

            _logger.LogWarning(exception, "Operation timed out or was cancelled.");
            await WriteProblemAsync(
                httpContext,
                StatusCodes.Status503ServiceUnavailable,
                "Service Unavailable",
                "The operation timed out. Please try again.",
                exception,
                cancellationToken);
            return true;
        }

        var (status, title, publicMessage) = MapException(exception);

        if (status >= StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        else if (status >= StatusCodes.Status400BadRequest)
            _logger.LogWarning(exception, "Request failed with {Status} for {Method} {Path}", status, httpContext.Request.Method, httpContext.Request.Path);

        await WriteProblemAsync(httpContext, status, title, publicMessage, exception, cancellationToken);
        return true;
    }

    private (int Status, string Title, string Message) MapException(Exception exception)
    {
        return exception switch
        {
            MongoWriteException mwe when mwe.WriteError?.Category == ServerErrorCategory.DuplicateKey =>
                (StatusCodes.Status409Conflict, "Conflict", "A record with the same unique key already exists."),

            MongoException =>
                (StatusCodes.Status503ServiceUnavailable, "Service Unavailable", "The database is temporarily unavailable. Please try again later."),

            TimeoutException =>
                (StatusCodes.Status503ServiceUnavailable, "Service Unavailable", "The operation timed out. Please try again later."),

            BadHttpRequestException =>
                (StatusCodes.Status400BadRequest, "Bad Request", "The request could not be understood."),

            JsonException =>
                (StatusCodes.Status400BadRequest, "Bad Request", "The request body could not be parsed as JSON."),

            ArgumentNullException or ArgumentException =>
                (StatusCodes.Status400BadRequest, "Bad Request", "The request was invalid."),

            FormatException =>
                (StatusCodes.Status400BadRequest, "Bad Request", "The request contained data in an invalid format."),

            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, "Not Found", "The requested resource was not found."),

            UnauthorizedAccessException =>
                (StatusCodes.Status403Forbidden, "Forbidden", "You are not allowed to perform this action."),

            InvalidOperationException =>
                (StatusCodes.Status400BadRequest, "Bad Request", "The operation is not valid in the current state."),

            _ =>
                (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };
    }

    private async Task WriteProblemAsync(
        HttpContext httpContext,
        int statusCode,
        string title,
        string publicMessage,
        Exception exception,
        CancellationToken cancellationToken)
    {
        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = _env.IsDevelopment() ? exception.ToString() : publicMessage,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["requestId"] = httpContext.TraceIdentifier;

        if (_env.IsDevelopment() && statusCode >= StatusCodes.Status500InternalServerError)
            problem.Extensions["exceptionType"] = exception.GetType().FullName;

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
    }
}
