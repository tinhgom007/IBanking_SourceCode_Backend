namespace src.Interfaces.IServices
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string UserId);
        Task<bool> ValidateOtpAsync(string UserId, string Otp);
    }
}