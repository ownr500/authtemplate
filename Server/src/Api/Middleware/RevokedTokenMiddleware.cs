using API.Core.Services;
using Microsoft.Net.Http.Headers;

namespace API.Middleware;

internal sealed class RevokedTokenMiddleware : IMiddleware
{
    private readonly ITokenService _tokenService;

    public RevokedTokenMiddleware(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Headers
            .TryGetValue(HeaderNames.Authorization, out var header))
        {
            var result = await _tokenService.ValidateAuthHeader(header);
            if (!result)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }

        await next(context);
    }
}