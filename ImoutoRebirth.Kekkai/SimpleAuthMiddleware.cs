namespace ImoutoRebirth.Kekkai;

public class SimpleAuthMiddleware : IMiddleware
{
    private readonly IConfiguration _configuration;

    public SimpleAuthMiddleware(IConfiguration configuration) => _configuration = configuration;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var token = _configuration.GetValue<string>("AuthToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            await next(context);
            return;
        }

        context.Request.Query.TryGetValue("token", out var tokenValue);
        if (tokenValue == token)
        {
            await next(context);
            return;
        }
        
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
    }
}