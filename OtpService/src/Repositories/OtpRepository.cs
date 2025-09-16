using src.Interfaces.IRepositories;
using StackExchange.Redis;

namespace src.Repositories
{
    public class OtpRepository : IOtpRepository
    {
        private readonly IConnectionMultiplexer _redis;

        public OtpRepository(IConnectionMultiplexer redis)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        }

        public async Task SaveOtpAsync(string UserId, string Otp, TimeSpan expiry)
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(UserId, Otp, expiry);
        }

        public async Task<string> GetOtpAsync(string UserId)
        {

            var db = _redis.GetDatabase();
            var otp = await db.StringGetAsync(UserId);
            return otp.HasValue ? otp.ToString() : string.Empty;
        }

        public async Task DeleteOtpAsync(string UserId)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(UserId);
        }

    }
}