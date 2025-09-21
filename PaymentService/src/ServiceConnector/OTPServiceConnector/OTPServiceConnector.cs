using Grpc.Core;
using OTPGrpc;
using ProfileGrpc;

namespace src.ServiceConnector.OTPServiceConnector
{
    public class OTPServiceConnector : BaseServiceConnector
    {
        private readonly ServiceConnectorConfig _serviceConnectorConfig;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public OTPServiceConnector(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration)
        {
            _serviceConnectorConfig = GetServiceConnectorConfig();
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<SendEmailReply> GenerateOTP(string email)
        {
            using var channel = GetOTPServiceChannel();
            var client = new OtpGrpcService.OtpGrpcServiceClient(channel);

            var request = new GenerateOTPRequest {
                Email = email
            };

            return await client.GenerateOTPAsync(request);
        }

        public async Task<ValidateOTPReply> ValidateOTP(string email, string OTP)
        {
            using var channel = GetOTPServiceChannel();
            var client = new OtpGrpcService.OtpGrpcServiceClient(channel);

            var request = new ValidateOTPRequest {
                Email = email,
                Otp = OTP
            };

            return await client.ValidateOTPAsync(request);
        }

        public async Task<SendEmailReply> SendEmailPaymentSuccess(string email, string amount)
        {
            using var channel = GetOTPServiceChannel();
            var client = new OtpGrpcService.OtpGrpcServiceClient(channel);

            var request = new SendEmailPaymentSuccessRequest
            {
                Email = email,
                Amount = amount
            };

            return await client.SendEmailPaymentSuccessAsync(request);
        }
    }
}
