using AuthenticationGrpc;
using Grpc.Core;

namespace src.ServiceConnector.AuthServiceConnector
{
    public class AuthServiceConnector : BaseServiceConnector
    {
        private readonly ServiceConnectorConfig _serviceConnectorConfig;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthServiceConnector(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration)
        {
            _serviceConnectorConfig = GetServiceConnectorConfig();
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TokenResponseReply> Login(string userName, string password)
        {
            using var channel = GetAuthServiceChannel();
            var client = new AuthenticationGrpcService.AuthenticationGrpcServiceClient(channel);

            var request = new LoginRequest 
            { 
                UserName = userName, 
                Password = password
            };

            return await client.LoginAsync(request);
        }

        public async Task<TokenResponseReply> RefreshToken(string refreshToken) 
        {
            using var channel = GetAuthServiceChannel();
            var client = new AuthenticationGrpcService.AuthenticationGrpcServiceClient(channel);

            var request = new RefreshTokenRequest 
            { 
                RefreshToken = refreshToken 
            };

            return await client.RefreshTokenAsync(request);
        }

        public async Task<SignOutResponseReply> SignOut()
        {
            using var channel = GetAuthServiceChannel();
            var client = new AuthenticationGrpcService.AuthenticationGrpcServiceClient(channel);

            var request = new SignOutRequest
            {
            };

            // Forward cookies to AuthService via gRPC metadata so it can read tokens
            var httpContext = _httpContextAccessor.HttpContext;
            Metadata? headers = null;
            if (httpContext != null)
            {
                var accessToken = httpContext.Request.Cookies["access_token"];
                var refreshToken = httpContext.Request.Cookies["refresh_token"];
                if (!string.IsNullOrEmpty(accessToken) || !string.IsNullOrEmpty(refreshToken))
                {
                    headers = new Metadata
                    {
                        { "cookie", $"access_token={accessToken}; refresh_token={refreshToken}" }
                    };
                }
            }

            return await client.SignOutAsync(request, headers);
        }
    }
}
