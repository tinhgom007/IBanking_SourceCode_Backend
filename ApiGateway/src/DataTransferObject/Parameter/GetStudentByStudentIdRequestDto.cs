using System.ComponentModel.DataAnnotations;

namespace src.DataTransferObject.Parameter
{
    public class GetStudentByStudentIdRequestDto
    {
        [Required]
        public string StudentId { get; set; }
    }
}
