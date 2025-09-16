using src.Data;
using src.DTOs.Request;
using src.DTOs.Response;
using src.Entities;

namespace src.Interfaces.IServices
{
    public interface IAuthService
    {
        Task<IEnumerable<User>> GetAllCustomers();
        //Task<bool> Register(RegisterRequestDto registerDto);
        Task<TokenResponseDto> Login(LoginRequestDto loginRequestDto, HttpContext httpContext);
        Task<bool> SignOut(HttpContext httpContext);
        Task<TokenResponseDto>RefreshToken(string refreshToken, HttpContext httpContext);
    }
}
