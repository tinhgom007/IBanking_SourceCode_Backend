﻿using Grpc.Core;
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

            var request = new GetProfileRequest { };

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

        public async Task<GetProfileReply> GetStudentByStudentIdAsync(string studentId)
        {
            using var channel = GetProfileServiceChannel();
            var client = new ProfileGrpcService.ProfileGrpcServiceClient(channel);

            var request = new GetStudentByStudentIdRequest
            {
                StudentId = studentId
            };

            return await client.GetStudentByStudentIdAsync(request);
        }
        
        public async Task<SearchStudentIdSuggestRepply> SearchStudentIdSuggest(string partialId)
        {
            using var channel = GetProfileServiceChannel();
            var client = new ProfileGrpcService.ProfileGrpcServiceClient(channel);

            var request = new SearchStudentIdSuggestRequest { PartialId = partialId };
            return await client.SearchStudentIdSuggestAsync(request);
        }
    }
}
