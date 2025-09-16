using Microsoft.IdentityModel.Tokens;
using src.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace src.Utils
{
    public class JWT
    {
        private readonly IConfiguration _configuration;

        public JWT(IConfiguration configuration, AuthDbContext context)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(Guid userId, string role)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var SECRET_KEY = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"])
            );
            var credentials = new SigningCredentials(SECRET_KEY, SecurityAlgorithms.HmacSha256);

            var accessToken = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(accessToken);
        
        }
    }
}
