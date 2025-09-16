using Microsoft.EntityFrameworkCore;
using src.DTOs.Request;
using src.DTOs.Response;
using src.Entities;
using src.Interfaces.IRepositories;
using src.Interfaces.IServices;
using src.Utils;
using System.Security.Cryptography;
using System.Text;

namespace src.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly JWT _JWT;
        public AuthService(IUserRepository userRepository, JWT jwt, IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _JWT = jwt;
            _refreshTokenRepository = refreshTokenRepository;
        }
        public async Task<bool> SignOut(HttpContext httpContext)
        {
            if(httpContext.Request.Cookies.ContainsKey("refresh_token"))
            {
                var refreshToken = httpContext.Request.Cookies["refresh_token"];

                await _refreshTokenRepository.RevokeToken(refreshToken);

                httpContext.Response.Cookies.Delete("access_token");
                httpContext.Response.Cookies.Delete("refresh_token");

                return true;
            }

            return false;
        }
        public async Task<TokenResponseDto> Login(LoginRequestDto loginRequestDto, HttpContext httpContext)
        {

            var user = await _userRepository.FindUserByUserName(loginRequestDto.Username) ?? throw new Exception("User không tồn tại");

            //if (!VerifyPassword(loginRequestDto.Password, user.Password))
            //{
            //    throw new Exception("Sai mật khẩu");
            //}
            var accessToken = _JWT.GenerateAccessToken(user.UserId, user.Role);

            await _refreshTokenRepository.RevokeAllTokens(user.UserId);
            var refreshToken = await _refreshTokenRepository.GenerateRefreshToken(user.UserId);

            httpContext.Response.Cookies.Append("access_token", accessToken, new CookieOptions { HttpOnly = true });
            httpContext.Response.Cookies.Append("refresh_token", refreshToken.Token, new CookieOptions { HttpOnly = true });

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };

        }

        public async Task<IEnumerable<User>> GetAllCustomers()
        {
            return await _userRepository.GetAllUsers();
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                var computedHash = Convert.ToBase64String(hash);

                return computedHash == hashedPassword;
            }
        }

        public async Task<TokenResponseDto> RefreshToken(string refreshToken, HttpContext httpContext)
        {
            var refreshTokenStore = await _refreshTokenRepository.GetByToken(refreshToken);

            if(refreshTokenStore == null || refreshTokenStore.IsRevoked || refreshTokenStore.ExpiresAt <= DateTime.UtcNow)
            {
                throw new Exception("Invalid refresh token");
            }

            await _refreshTokenRepository.RevokeToken(refreshToken);

            var user = await _userRepository.FindUserById(refreshTokenStore.UserId);
            var accessToken = _JWT.GenerateAccessToken(user.UserId, user.Role);
            var newRefreshToken = await _refreshTokenRepository.GenerateRefreshToken(user.UserId);

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token
            };
        }
    }
}
