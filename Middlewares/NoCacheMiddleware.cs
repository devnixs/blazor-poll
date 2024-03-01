namespace Poll.Middlewares;

public class NoCacheMiddleware
{
    private readonly RequestDelegate _next;

    public NoCacheMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add the no-cache headers to the response
        context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "0";

        // Call the next middleware in the pipeline
        await _next(context);
    }
}
// Extension method used to add the middleware to the HTTP request pipeline.
public static class NoCacheMiddlewareExtensions
{
    public static IApplicationBuilder UseNoCacheMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<NoCacheMiddleware>();
    }
}