using src.Entities;

namespace src.Interfaces.IRepositories
{
    public interface IRefreshTokenRepository
    {
        Task RevokeToken(string token);
        Task RevokeAllTokens(Guid userId);
        Task<RefreshToken?> GetByToken(string token);
        Task<RefreshToken> GenerateRefreshToken(Guid userId);
    }
}
