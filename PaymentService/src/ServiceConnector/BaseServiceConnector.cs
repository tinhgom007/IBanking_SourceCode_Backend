using Grpc.Net.Client;

namespace src.ServiceConnector
{
    public class ServiceConnectorConfigOption
    {
        public string Endpoint { get; set; } = string.Empty;
    }

    public class ServiceConnectorConfig
    {
        public ServiceConnectorConfigOption ProfileService { get; set; } = new ServiceConnectorConfigOption();
        public ServiceConnectorConfigOption OTPService { get; set; } = new ServiceConnectorConfigOption();
        public ServiceConnectorConfigOption TuitionService { get; set; } = new ServiceConnectorConfigOption();
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
                ProfileService = new ServiceConnectorConfigOption
                {
                    Endpoint = _configuration["ServiceConnector:ProfileService:Endpoint"] ?? string.Empty
                },
                OTPService = new ServiceConnectorConfigOption
                {
                    Endpoint = _configuration["ServiceConnector:OTPService:Endpoint"] ?? string.Empty
                },
                TuitionService = new ServiceConnectorConfigOption
                {
                    Endpoint = _configuration["ServiceConnector:TuitionService:Endpoint"] ?? string.Empty
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

        protected GrpcChannel GetOTPServiceChannel()
            => GetGrpcChannel(GetServiceConnectorConfig().OTPService.Endpoint);

        protected GrpcChannel GetTuitionGrpcChannel()   
            => GetGrpcChannel(GetServiceConnectorConfig().TuitionService.Endpoint);
    }
}
