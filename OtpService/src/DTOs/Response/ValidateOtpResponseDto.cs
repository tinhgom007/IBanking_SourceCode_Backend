namespace src.DTOs.Response
{
    public class ValidateOtpResponseDto
    {
        public required string Email { get; set; }
        public required bool IsValid { get; set; }
    }
}