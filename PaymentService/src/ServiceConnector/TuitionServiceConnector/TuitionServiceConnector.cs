using Grpc.Core;
using TuitionGrpc;

namespace src.ServiceConnector.TuitionServiceConnector
{
    public class TuitionServiceConnector : BaseServiceConnector
    {
        private readonly ServiceConnectorConfig _serviceConnectorConfig;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TuitionServiceConnector(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration)
        {
            _serviceConnectorConfig = GetServiceConnectorConfig();
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UpdateStatusTuitionReply> UpdateStatusTuition(string tuitionId)
        {
            using var channel = GetTuitionGrpcChannel();
            var client = new TuitionGrpcService.TuitionGrpcServiceClient(channel);
            var request = new GetTuitionByIdRequest
            {
                TuitionId = tuitionId
            };
            
            return await client.UpdateStatusTuitionAsync(request);
        }

        public async Task<TuitionItem> GetTuitionById(string tuitionId)
        {
            using var channel = GetTuitionGrpcChannel();
            var client = new TuitionGrpcService.TuitionGrpcServiceClient(channel);
            var request = new GetTuitionByIdRequest
            {
                TuitionId = tuitionId
            };

            return await client.GetTuitionByIdAsync(request);
        }

        public async Task<GetTuitionAsyncReply> GetTuitions(string studentId)
        {
            using var channel = GetTuitionGrpcChannel();
            var client = new TuitionGrpcService.TuitionGrpcServiceClient(channel);
            var request = new GetTuitionAsyncRequest
            {
                StudentId = studentId
            };

            return await client.GetTuitionAsync(request);
        }
    }
}
