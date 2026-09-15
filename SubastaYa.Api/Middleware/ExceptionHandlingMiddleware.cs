using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Helpers;

namespace SubastaYa.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ApiException exception)
        {
            _logger.LogWarning(
                exception,
                "[CODE-ERROR] - {Message}",
                exception.Message);

            await WriteErrorAsync(
                context,
                exception.StatusCode,
                exception.Message);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            _logger.LogWarning(
                exception,
                "[CODE-ERROR] - conflicto de concurrencia detectado.");

            await WriteErrorAsync(
                context,
                StatusCodes.Status409Conflict,
                "la subasta fue modificada por otra puja. Actualizá los datos e intentá nuevamente.");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "[CODE-ERROR] - error no controlado en la API.");

            if (context.Response.HasStarted)
            {
                throw;
            }

            await WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "ocurrió un error interno.");
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(
            new
            {
                message = $"[CODE-ERROR] - {message}"
            },
            cancellationToken: context.RequestAborted);
    }
}