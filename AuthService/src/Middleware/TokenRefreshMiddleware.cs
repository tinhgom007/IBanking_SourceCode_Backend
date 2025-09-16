using src.Interfaces.IServices;
using System.IdentityModel.Tokens.Jwt;

namespace src.Middleware
{
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenRefreshMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IAuthService authService)
        {
            var accessToken = context.Request.Cookies["AccessToken"];
            var refreshToken = context.Request.Cookies["RefreshToken"];

            if (!string.IsNullOrEmpty(accessToken))
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
                            var newTokens = await authService.RefreshToken(refreshToken, context);

                            context.Response.Cookies.Append("AccessToken", newTokens.AccessToken, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTime.UtcNow.AddMinutes(15)
                            });
                        }
                        catch
                        {
                            
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
