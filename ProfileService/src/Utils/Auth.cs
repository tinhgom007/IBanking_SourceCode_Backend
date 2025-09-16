using System.IdentityModel.Tokens.Jwt;

namespace src.Utils
{
    public class Auth
    {
        private readonly IConfiguration _configuration;

        public Auth(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static Guid? GetUserIdFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                return null;

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null)
                return null;

            if (Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }

    }
}
