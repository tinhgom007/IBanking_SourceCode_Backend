using src.Interfaces.IRepositories;
using src.Interfaces.IServices;
using src.Templates;
using src.Utils;

namespace src.Services
{
    public class OtpService : IOtpService
    {
        private readonly IOtpRepository _otpRepository;
        private readonly IEmailService _emailService;
        private readonly TimeSpan _otpExpiry = TimeSpan.FromMinutes(5);
        private readonly Mailer _mailer;

        public OtpService(IOtpRepository otpRepository, IEmailService emailService, Mailer mailer)
        {
            _otpRepository = otpRepository ?? throw new ArgumentNullException(nameof(otpRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _mailer = mailer;
        }

        public async Task<string> GenerateOtpAsync(string Email)
        {
            var otp = GenerateRandomOtp(6);
            await _otpRepository.SaveOtpAsync(Email, otp, _otpExpiry);
            // await _emailService.SendOtpEmailAsync(Email, otp);

            string httpContent = OTPTemplate.Generate(otp);
            await _mailer.SendMail(Email, "Your OTP Code", httpContent);
            
            return otp;
        }

        public async Task<bool> ValidateOtpAsync(string Email, string Otp)
        {
            var storedOtp = await _otpRepository.GetOtpAsync(Email);

            if (storedOtp == null)
            {
                return false;
            }

            bool isValid = storedOtp == Otp;

            if (isValid)
            {
                await _otpRepository.DeleteOtpAsync(Email);
            }

            return isValid;
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