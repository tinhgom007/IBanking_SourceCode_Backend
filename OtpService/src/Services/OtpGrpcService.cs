using Grpc.Core;
using src.Interfaces.IServices;
using src.Interfaces.IRepositories;
using src.Utils;
using src.Templates;
using OTPGrpc;

namespace src.Services
{
    public class OtpGrpcServiceImpl : OtpGrpcService.OtpGrpcServiceBase
    {
        private readonly IConfiguration _configuration;
        private readonly IOtpRepository _otpRepository;
        private readonly Mailer _mailer;
        private readonly IOtpService _otpService;

        public OtpGrpcServiceImpl(IConfiguration configuration, IOtpRepository otpRepository, Mailer mailer, IOtpService otpService)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _otpRepository = otpRepository ?? throw new ArgumentNullException(nameof(otpRepository));
            _mailer = mailer ?? throw new ArgumentNullException(nameof(mailer));
            _otpService = otpService ?? throw new ArgumentNullException(nameof(otpService));
        }

        public override async Task<SendEmailReply> GenerateOTP(GenerateOTPRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Email is required."));
            }

            var otp = GenerateRandomOtp(6);
            await _otpRepository.SaveOtpAsync(request.Email, otp, TimeSpan.FromMinutes(5));
            // await _emailService.SendOtpEmailAsync(request.Email, otp);

            string httpContent = OTPTemplate.Generate(otp);
            var result = await _mailer.SendMail(request.Email, "Your OTP Code", httpContent);

            if (!result)
            {
                return new SendEmailReply { Success = false, Message = "Failed to generate OTP." };
            }

            return new SendEmailReply { Success = true, Message = "OTP generated successfully." };
        }

        public override async Task<ValidateOTPReply> ValidateOTP(ValidateOTPRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Otp))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Email and OTP code are required."));
            }

            var isValid = await _otpService.ValidateOtpAsync(request.Email, request.Otp);
            return new ValidateOTPReply { IsValid = isValid, Message = isValid ? "OTP is valid." : "OTP is invalid." };
        }

        public override async Task<SendEmailReply> ResendOTP(GenerateOTPRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Email is required."));
            }

            var existingOtp = await _otpRepository.GetOtpAsync(request.Email);
            if (!string.IsNullOrEmpty(existingOtp))
            {
                await _otpRepository.DeleteOtpAsync(request.Email);
            }

            var otp = GenerateRandomOtp(6);
            await _otpRepository.SaveOtpAsync(request.Email, otp, TimeSpan.FromMinutes(5));

            string httpContent = OTPTemplate.Generate(otp);
            var result = await _mailer.SendMail(request.Email, "Your OTP Code", httpContent);
            if (!result)
            {
                return new SendEmailReply { Success = false, Message = "Failed to send mail." };
            }

            return new SendEmailReply { Success = true, Message = "OTP resent successfully." };
        }

        public override async Task<SendEmailReply> SendEmailPaymentSuccess(SendEmailPaymentSuccessRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Email is required."));
            }

            string httpContent = PaymentSuccessTemplate.Generate(request.Amount);
            var result = await _mailer.SendMail(request.Email, "Payment Successfully", httpContent);

            if (!result)
            {
                return new SendEmailReply { Success = false, Message = "Failed to send mail." };
            }

            return new SendEmailReply { Success = true, Message = "Send Email successfully." };
        }

        private string GenerateRandomOtp(int length)
        {
            const string chars = "0123456789";
            var random = new Random();
            var otp = new char[length];

            for (int i = 0; i < length; i++)
            {
                otp[i] = chars[random.Next(chars.Length)];
            }

            return new string(otp);
        }
    }
}