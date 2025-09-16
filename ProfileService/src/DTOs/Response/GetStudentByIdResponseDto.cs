using System.ComponentModel.DataAnnotations;

namespace src.DTOs.Response
{
    public class GetStudentByIdResponseDto
    {
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public string gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public decimal Balance { get; set; }
        public string marjor { get; set; }
    }
}
