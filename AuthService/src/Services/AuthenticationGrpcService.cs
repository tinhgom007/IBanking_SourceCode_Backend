using AuthenticationGrpc;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using src.DTOs.Response;
using src.Entities;
using src.Interfaces.IRepositories;
using src.Repositories;
using src.Utils;

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

            //if (!VerifyPassword(loginRequestDto.Password, user.Password))
            //{
            //    throw new Exception("Sai mật khẩu");
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



    }
}
