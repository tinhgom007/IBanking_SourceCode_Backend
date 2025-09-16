using Grpc.Core;
using ProfileGrpc;

namespace src.ServiceConnector.ProfileServiceConnector
{
    public class ProfileServiceConnector : BaseServiceConnector
    {
        private readonly ServiceConnectorConfig _serviceConnectorConfig;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProfileServiceConnector(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration)
        {
            _serviceConnectorConfig = GetServiceConnectorConfig();
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<GetProfileReply> GetProfileAsync()
        {
            using var channel = GetProfileServiceChannel();
            var client = new ProfileGrpcService.ProfileGrpcServiceClient(channel);

            var request = new GetProfileRequest {};

            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token) && token.StartsWith("Bearer "))
            {
                token = token.Substring("Bearer ".Length).Trim();
            }

            var headers = new Metadata();
            if (!string.IsNullOrEmpty(token))
            {
                headers.Add("Authorization", $"Bearer {token}");
            }

            return await client.GetProfileByIdAsync(request, headers);
        }

        public async Task<DeductBalanceReply> DeductBalance(string studentId, string amount)
        {
            using var channel = GetProfileServiceChannel();
            var client = new ProfileGrpcService.ProfileGrpcServiceClient(channel);

            var request = new DeductBalanceRequest {
                StudentId = studentId,
                Amount = amount
            };

            return await client.DeductBalanceAsync(request);
        }

        public async Task<BalanceReply> HanldeBalance(string studentId, string amount, bool isAdd)
        {
            using var channel = GetProfileServiceChannel();
            var client = new ProfileGrpcService.ProfileGrpcServiceClient(channel);
            var request = new BalanceRequest
            {
                StudentId = studentId,
                Amount = amount,
                IsAdd = isAdd
            };
            return await client.HanldeBalanceAsync(request);
        }
    }
}
