using src.Interfaces.IRepositories;
using src.Entities;
using src.Data;
using Microsoft.EntityFrameworkCore;

namespace src.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AuthDbContext _context;

        public RefreshTokenRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken> GenerateRefreshToken(Guid userId)
        {
            var refreshToken = new RefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                UserId = userId
            };

            _context.RefreshTokens.Add(refreshToken);
            _context.SaveChanges();

            return refreshToken;
        }

        public async Task<RefreshToken?> GetByToken(string token)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked);
        }

        public async Task RevokeAllTokens(Guid userId)
        {
            var tokens = _context.RefreshTokens.Where(rt => rt.UserId == userId && !rt.IsRevoked);

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task RevokeToken(string tokenStr)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == tokenStr);
            
            if (token != null)
            {
                token.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
