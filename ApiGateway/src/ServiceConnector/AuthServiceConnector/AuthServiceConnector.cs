using AuthenticationGrpc;

namespace src.ServiceConnector.AuthServiceConnector
{
    public class AuthServiceConnector : BaseServiceConnector
    {
        private readonly ServiceConnectorConfig _serviceConnectorConfig;
        public AuthServiceConnector(IConfiguration configuration) : base(configuration)
        {
            _serviceConnectorConfig = GetServiceConnectorConfig();
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

            return await client.SignOutAsync(request);
        }
    }
}
