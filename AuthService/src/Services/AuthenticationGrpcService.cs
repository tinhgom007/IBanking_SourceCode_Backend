using AuthenticationGrpc;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using src.DTOs.Response;
using src.Entities;
using src.Interfaces.IRepositories;
using src.Repositories;
using src.Utils;
using System.Security.Cryptography;
using System.Text;

namespace src.Services
{
    public class AuthenticationGrpcServiceImpl : AuthenticationGrpcService.AuthenticationGrpcServiceBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly JWT _JWT;

        public AuthenticationGrpcServiceImpl(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, JWT JWT)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _JWT = JWT;
        }

        public override async Task<TokenResponseReply> Login(LoginRequest request, ServerCallContext context)
        {
            var user = await _userRepository.FindUserByUserName(request.UserName);
            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "User không tồn tại"));
            }
            if(user.IsLocked)
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Tài khoản đã bị khoá"));
            }

            if (user.LastFailedLoginAt != null && (DateTime.UtcNow - user.LastFailedLoginAt.Value).TotalHours >= 24)
            {
                await _userRepository.UpdateFailedLoginCount(user.UserId, false);
            }

            //if (!VerifyPassword(request.Password, user.Password))
            //{
            //    await _userRepository.UpdateFailedLoginCount(user.UserId, true);
            //    throw new RpcException(new Status(StatusCode.PermissionDenied, "Sai mật khẩu"));
            //}

            var accessToken = _JWT.GenerateAccessToken(user.UserId, user.Role);

            await _refreshTokenRepository.RevokeAllTokens(user.UserId);
            var refreshToken = await _refreshTokenRepository.GenerateRefreshToken(user.UserId);

            var httpContext = context.GetHttpContext();
            httpContext.Response.Cookies.Append("access_token", accessToken, new CookieOptions { HttpOnly = true });
            httpContext.Response.Cookies.Append("refresh_token", refreshToken.Token, new CookieOptions { HttpOnly = true });

            return new TokenResponseReply
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        public override async Task<TokenResponseReply> RefreshToken(RefreshTokenRequest request, ServerCallContext context)
        {
            var refreshTokenStore = await _refreshTokenRepository.GetByToken(request.RefreshToken);

            if (refreshTokenStore == null || refreshTokenStore.IsRevoked || refreshTokenStore.ExpiresAt <= DateTime.UtcNow)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Invalid refresh token"));
            }

            await _refreshTokenRepository.RevokeToken(request.RefreshToken);

            var user = await _userRepository.FindUserById(refreshTokenStore.UserId);
            var accessToken = _JWT.GenerateAccessToken(user.UserId, user.Role);
            var newRefreshToken = await _refreshTokenRepository.GenerateRefreshToken(user.UserId);

            return new TokenResponseReply
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        public override async Task<SignOutResponseReply> SignOut(SignOutRequest request, ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();
            var accessToken = httpContext.Request.Cookies["access_token"];
            var refreshToken = httpContext.Request.Cookies["refresh_token"];
            // if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            // {
            //     throw new RpcException(new Status(StatusCode.NotFound, "No tokens found in cookies"));
            // }
            var refreshTokenStore = await _refreshTokenRepository.GetByToken(refreshToken);
            // if (refreshTokenStore == null || refreshTokenStore.IsRevoked || refreshTokenStore.ExpiresAt <= DateTime.UtcNow)
            // {
            //     throw new RpcException(new Status(StatusCode.NotFound, "Invalid refresh token"));
            // }
            if (refreshTokenStore != null)
            {
                await _refreshTokenRepository.RevokeAllTokens(refreshTokenStore.UserId);
            }
            httpContext.Response.Cookies.Delete("access_token");
            httpContext.Response.Cookies.Delete("refresh_token");
            return new SignOutResponseReply
            {
                IsSuccess = true
            };
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

    }
}
