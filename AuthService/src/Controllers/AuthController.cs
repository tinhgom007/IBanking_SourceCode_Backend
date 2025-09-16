using Microsoft.AspNetCore.Mvc;
using src.DTOs.Request;
using src.Entities;
using src.Interfaces.IServices;

namespace src.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;

        public AuthController(ILogger<AuthController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginRequestDto loginRequestDto)
        {

            try
            {
                var result = await _authService.Login(loginRequestDto, HttpContext);

                return Ok(new
                {
                    statusCode = 200,
                    msg = "Login successfully",
                    metadata = result,
                });
            }
            catch (Exception error)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    msg = error.Message,
                });
            }
        }


        [HttpPost("signout")]
        public async Task<ActionResult> SignOut()
        {
            try
            {
                var result = await _authService.SignOut(HttpContext);

                if (result)
                {
                    return Ok(new
                    {
                        statusCode = 200,
                        msg = "SignOut successfully",
                        metadata = result,
                    });
                } else
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        msg = "SignOut failed",
                    });
                }
            }
            catch (Exception error)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    msg = error.Message,
                });
            }
        }


        [HttpPost("refreshToken")]
        public async Task<ActionResult> RefreshToken(RefreshTokenRequestDto refreshTokenRequestDto)
        {

            try
            {
                var result = await _authService.RefreshToken(refreshTokenRequestDto.RefreshToken, HttpContext);

                return Ok(new
                {
                    statusCode = 200,
                    msg = "Login successfully",
                    metadata = result,
                });
            }
            catch (Exception error)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    msg = error.Message,
                });
            }
        }

    }
}
