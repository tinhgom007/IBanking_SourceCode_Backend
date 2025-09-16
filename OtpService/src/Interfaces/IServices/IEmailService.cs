namespace src.Interfaces.IServices
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string to, string otp);
    }
}