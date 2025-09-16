using Grpc.Core;
using Microsoft.AspNetCore.Http;
using src.DTOs.Response;
using src.Interfaces.IRepositories;
using ProfileGrpc;
using src.Utils;

namespace src.Services
{
    public class ProfileGrpcServiceImpl : ProfileGrpcService.ProfileGrpcServiceBase
    {
        private readonly IStudentRepository _studentRepository;

        public ProfileGrpcServiceImpl(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public override async Task<GetProfileReply> GetProfileById(GetProfileRequest request, ServerCallContext context)
        {
            HttpContext httpContext = context.GetHttpContext();
            string? token = null;

            var authHeader = context.RequestHeaders.GetValue("Authorization");
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(token) && httpContext.Request.Cookies.ContainsKey("access_token"))
            {
                token = httpContext.Request.Cookies["access_token"];
            }

            if (string.IsNullOrEmpty(token))
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Token does not exist"));
            }

            var userId = Auth.GetUserIdFromToken(token);
            if (userId == null)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "UserId in token is invalid"));
            }

            var student = await _studentRepository.GetStudentByUserIdAsync(userId.Value);

            return new GetProfileReply
            {
                StudentId = student.StudentId.ToString(),
                FullName = student.FullName,
                Balance = student.Balance.ToString(),
                Gender = student.gender,
                PhoneNumber = student.PhoneNumber,
                Email = student.Email,
                Marjor = student.marjor
            };
        }

        public override async Task<DeductBalanceReply> DeductBalance(DeductBalanceRequest request, ServerCallContext context)
        {
            var student = await _studentRepository.GetStudentByStudentIdAsync(request.StudentId);

            if(student == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Profile not found"));
            }

            if(student.Balance < decimal.Parse(request.Amount))
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Insufficient balance"));
            }

            student.Balance -= decimal.Parse(request.Amount);

            await _studentRepository.UpdateStudentAsync(student);

            return new DeductBalanceReply
            {
                Success = true,
                Message = "Balance deducted successfully",
                NewBalance = student.Balance.ToString()
            };
        }

        public override async Task<BalanceReply> HanldeBalance(BalanceRequest request, ServerCallContext context)
        {
            var student = await _studentRepository.GetStudentByStudentIdAsync(request.StudentId);
            if (student == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Profile not found"));
            }
            if (request.IsAdd)
            {
                student.Balance += decimal.Parse(request.Amount);
            }
            else
            {
                if (student.Balance < decimal.Parse(request.Amount))
                {
                    throw new RpcException(new Status(StatusCode.FailedPrecondition, "Insufficient balance"));
                }
                student.Balance -= decimal.Parse(request.Amount);
            }
            await _studentRepository.UpdateStudentAsync(student);
            return new BalanceReply
            {
                Success = true,
                Message = request.IsAdd ? "Balance added successfully" : "Balance deducted successfully",
                NewBalance = student.Balance.ToString()
            };
        }
    }
}
