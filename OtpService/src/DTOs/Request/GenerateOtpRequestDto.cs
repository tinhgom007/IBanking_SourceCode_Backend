using System.ComponentModel.DataAnnotations;

namespace src.DTOs.Request
{
    public class GenerateOtpRequestDto
    {
        [EmailAddress]
        public required string Email { get; set; }
    }
}