namespace SubastaYa.Api.Middleware;

public class ApiVersionMiddleware
{
    private readonly RequestDelegate _next;

    public ApiVersionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Api-version"] = "1.0";

            return Task.CompletedTask;
        });

        await _next(context);
    }
}