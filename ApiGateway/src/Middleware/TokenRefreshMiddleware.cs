using src.ServiceConnector.AuthServiceConnector;
using System.IdentityModel.Tokens.Jwt;

namespace src.Middleware
{
    public class TokenRefreshMiddleware : IMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AuthServiceConnector _authService;

        public TokenRefreshMiddleware(RequestDelegate next, AuthServiceConnector authService)
        {
            _next = next;
            _authService = authService;
        }

        public async Task Invoke(HttpContext context)
        {
            var accessToken = context.Request.Cookies["access_token"];
            var refreshToken = context.Request.Cookies["refresh_token"];

            if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                var token = jwtHandler.ReadJwtToken(accessToken);

                var exp = token.Payload.Exp;
                if (exp.HasValue)
                {
                    var expTime = DateTimeOffset.FromUnixTimeSeconds(exp.Value);
                    var now = DateTimeOffset.UtcNow;

                    if (expTime <= now.AddMinutes(1))
                    {
                        try
                        {
                            var newTokens = await _authService.RefreshToken(refreshToken);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[TokenRefreshMiddleware] Refresh failed: {ex.Message}");
                        }
                    }
                }
            }

            await _next(context);
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await next(context);
        }
    }
}
