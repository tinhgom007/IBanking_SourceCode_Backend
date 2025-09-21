using System.ComponentModel.DataAnnotations;

namespace src.Entities
{
    public class User
    {
        public Guid UserId { get; set; }

        [MaxLength(100)]
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } = "Customer";
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;
        public int FailedLoginCount { get; set; } = 0;
        public DateTime? LastFailedLoginAt { get; set; } = null;
        public bool IsLocked { get; set; } = false;

    }
}
