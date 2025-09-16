using MailKitClient = MailKit.Net.Smtp.SmtpClient;
using MailKit.Security;
using MimeKit;
using src.Interfaces.IServices;

namespace src.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(
                _configuration["EmailSettings:SenderName"],
                _configuration["EmailSettings:SenderEmail"]
            ));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = "OTP CODE VERIFICATION";

            emailMessage.Body = new TextPart("plain")
            {
                Text = $"Your OTP code is: {otp}. This code will expire in 5 minutes."
            };

            using var client = new MailKitClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            
            await client.ConnectAsync(
                _configuration["EmailSettings:SmtpServer"],
                int.Parse(_configuration["EmailSettings:SmtpPort"]),
                SecureSocketOptions.StartTls
            );

            await client.AuthenticateAsync(
                _configuration["EmailSettings:Username"],
                _configuration["EmailSettings:Password"]
            );

            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}
