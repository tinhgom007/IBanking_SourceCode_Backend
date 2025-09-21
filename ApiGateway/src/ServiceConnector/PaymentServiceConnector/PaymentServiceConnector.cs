using Grpc.Core;
using PaymentGrpc;

namespace src.ServiceConnector.PaymentServiceConnector
{
    public class PaymentServiceConnector : BaseServiceConnector
    {
        private readonly ServiceConnectorConfig _serviceConnectorConfig;
        public PaymentServiceConnector(IConfiguration configuration) : base(configuration)
        {
            _serviceConnectorConfig = GetServiceConnectorConfig();
        }

        public async Task<GetTransactionHistoryReply> GetTransactionHistory(string studentId, string accessToken)
        {
            using var channel = GetPaymentServiceChannel();
            var client = new PaymentGrpcService.PaymentGrpcServiceClient(channel);
            var request = new GetTransactionHistoryRequest { StudentId = studentId };

            var headers = new Metadata
            {
                { "Authorization", $"Bearer {accessToken}" }
            };

            return await client.GetTransactionHistoryAsync(request, headers);
        }   

        public async Task<CreateTransactionReply> CreateTransaction(string tuitionId, string studentId, string payerId, string accessToken)
        {
            using var channel = GetPaymentServiceChannel();
            var client = new PaymentGrpcService.PaymentGrpcServiceClient(channel);

            var request = new CreateTransactionRequest 
            { 
                TuitionId = tuitionId,
                StudentId = studentId,
                PayerId = payerId,
            };

            var headers = new Metadata
            {
                { "Authorization", $"Bearer {accessToken}" }
            };

            return await client.CreateTransactionAsync(request, headers);
        }

        public async Task<CreateTransactionReply> ConfirmTransaction(string paymentId, string otp, string email, string accessToken)
        {
            using var channel = GetPaymentServiceChannel();
            var client = new PaymentGrpcService.PaymentGrpcServiceClient(channel);

            var request = new ConfirmTransactionRequest
            {
                PaymentId = paymentId,
                Otp = otp,
                Email = email
            };

            var headers = new Metadata
            {
                { "Authorization", $"Bearer {accessToken}" }
            };

            return await client.ConfirmTransactionAsync(request, headers);
        }
    }
}
