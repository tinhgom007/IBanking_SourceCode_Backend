using System.Net;
using System.Net.Mail;

namespace src.Utils {
    public class Mailer
    {
        private readonly IConfiguration _configuration;

        public Mailer(IConfiguration configuration){
            _configuration = configuration;
        }

        public async Task<bool> SendMail(string email, string sub, string content)
        {
            try
            {
                var fromEmail = new MailAddress(_configuration["MailSettings:Email"], "Your Display Name");
                var toEmail = new MailAddress(email);
                string password = _configuration["MailSettings:Password"];

                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(fromEmail.Address, password);

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.From = fromEmail;
                        mailMessage.To.Add(toEmail);
                        mailMessage.Subject = sub;
                        mailMessage.Body = content;
                        mailMessage.IsBodyHtml = true;

                        await client.SendMailAsync(mailMessage);
                    }
                }
                return true;
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine($"SMTP Error: {smtpEx.Message}");
                return false;
                // throw new Exception("Lỗi gửi email, vui lòng kiểm tra cấu hình SMTP.", smtpEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return false;
                // throw new Exception("Lỗi gửi email, vui lòng kiểm tra cấu hình SMTP.");
            }
        }

    }
}