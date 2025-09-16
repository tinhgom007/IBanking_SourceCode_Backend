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

    }
}
