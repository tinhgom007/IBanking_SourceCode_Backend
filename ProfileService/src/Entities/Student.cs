using System.ComponentModel.DataAnnotations;

namespace src.Entities
{
    public class Student
    {
        [Key]
        [Required]
        [MaxLength(20)]
        public string StudentId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(10)]
        public string gender { get; set; }

        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        [Required]
        public string Email { get; set; }
        public decimal Balance { get; set; }
        public string marjor { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
