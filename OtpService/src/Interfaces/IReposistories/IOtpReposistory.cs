namespace src.Interfaces.IRepositories
{
    public interface IOtpRepository
    {
        Task SaveOtpAsync(string UserId, string Otp, TimeSpan expiry);
        Task<string> GetOtpAsync(string UserId);
        Task DeleteOtpAsync(string UserId);
    }
}