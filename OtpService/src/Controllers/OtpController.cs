using Microsoft.AspNetCore.Mvc;
using src.DTOs.Request;
using src.DTOs.Response;
using src.Interfaces.IServices;

namespace src.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OtpController : ControllerBase
    {
        private readonly IOtpService _otpService;

        public OtpController(IOtpService otpService)
        {
            _otpService = otpService ?? throw new ArgumentNullException(nameof(otpService));
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateOtp(GenerateOtpRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Email is required.");
            }

            var otp = await _otpService.GenerateOtpAsync(request.Email);
            return Ok(new GenerateOtpResponseDto { Email = request.Email });
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateOtp(ValidateOtpRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.Otp))
            {
                return BadRequest("UserId and OTP code are required");
            }

            var isValid = await _otpService.ValidateOtpAsync(request.Email, request.Otp);

            return Ok(new ValidateOtpResponseDto { Email = request.Email, IsValid = isValid });
        }
    }

}
