using Grpc.Net.Client;

namespace src.ServiceConnector
{
    public class ServiceConnectorConfigOption
    {
        public string Endpoint { get; set; } = string.Empty;
    }

    public class ServiceConnectorConfig
    {
        public ServiceConnectorConfigOption AuthService { get; set; } = new ServiceConnectorConfigOption();
        public ServiceConnectorConfigOption ProfileService { get; set; } = new ServiceConnectorConfigOption();
        public ServiceConnectorConfigOption TuitionService { get; set; } = new ServiceConnectorConfigOption();
        public ServiceConnectorConfigOption PaymentService { get; set; } = new ServiceConnectorConfigOption();
    }

    public abstract class BaseServiceConnector
    {
        private readonly IConfiguration _configuration;

        protected BaseServiceConnector(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ServiceConnectorConfig GetServiceConnectorConfig()
        {
            return new ServiceConnectorConfig
            {
                AuthService = new ServiceConnectorConfigOption
                {
                    Endpoint = _configuration["ServiceConnector:AuthService:Endpoint"] ?? string.Empty
                },
                ProfileService = new ServiceConnectorConfigOption
                {
                    Endpoint = _configuration["ServiceConnector:ProfileService:Endpoint"] ?? string.Empty
                },
                TuitionService = new ServiceConnectorConfigOption
                {
                    Endpoint = _configuration["ServiceConnector:TuitionService:Endpoint"] ?? string.Empty
                },
                PaymentService = new ServiceConnectorConfigOption
                {
                    Endpoint = _configuration["ServiceConnector:PaymentService:Endpoint"] ?? string.Empty
                }
            };
        }

        protected GrpcChannel GetGrpcChannel(string endpoint)
        {
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            return Grpc.Net.Client.GrpcChannel.ForAddress(endpoint, new GrpcChannelOptions
            {
                HttpHandler = httpHandler,
                MaxReceiveMessageSize = 50 * 1024 * 1024,
                MaxSendMessageSize = 50 * 1024 * 1024
            });
        }

        protected GrpcChannel GetProfileServiceChannel()
            => GetGrpcChannel(GetServiceConnectorConfig().ProfileService.Endpoint);

        protected GrpcChannel GetAuthServiceChannel()
            => GetGrpcChannel(GetServiceConnectorConfig().AuthService.Endpoint);

        protected GrpcChannel GetTuitionServiceChannel()
            => GetGrpcChannel(GetServiceConnectorConfig().TuitionService.Endpoint);

        protected GrpcChannel GetPaymentServiceChannel()
            => GetGrpcChannel(GetServiceConnectorConfig().PaymentService.Endpoint);
    
    }
}
