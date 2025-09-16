using System.ComponentModel.DataAnnotations;

namespace src.Entities
{
    public class RefreshToken
    {
        public Guid RefreshTokenId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        public Guid UserId { get; set; }
    }
}
