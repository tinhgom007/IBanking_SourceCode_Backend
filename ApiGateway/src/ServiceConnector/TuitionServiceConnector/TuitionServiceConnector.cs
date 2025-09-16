using Grpc.Core;
using TuitionGrpc;

namespace src.ServiceConnector.TuitionServiceConnector
{
    public class TuitionServiceConnector : BaseServiceConnector
    {
        private readonly ServiceConnectorConfig _serviceConnectorConfig;
        public TuitionServiceConnector(IConfiguration configuration) : base(configuration)
        {
            _serviceConnectorConfig = GetServiceConnectorConfig();
        }

        public async Task<GetTuitionAsyncReply> GetTuitionAsysc(string studentId)
        {
            using var channel = GetTuitionServiceChannel();
            var client = new TuitionGrpcService.TuitionGrpcServiceClient(channel);

            var request = new GetTuitionAsyncRequest { StudentId = studentId };

            return await client.GetTuitionAsync(request);
        }
    }
}
