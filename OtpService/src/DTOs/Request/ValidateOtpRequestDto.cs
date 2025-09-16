using System.ComponentModel.DataAnnotations;

namespace src.DTOs.Request
{
    public class ValidateOtpRequestDto
    {
        [EmailAddress]
        public required string Email { get; set; }
        public required string Otp { get; set; }
    }
}