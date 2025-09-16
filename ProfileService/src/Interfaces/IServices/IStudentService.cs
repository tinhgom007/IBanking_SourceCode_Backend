using src.DTOs.Response;
using src.Entities;

namespace src.Interfaces.IServices
{
    public interface IStudentService
    {
        Task<IEnumerable<Student>> GetAllStudentsAsync();
        Task AddStudentAsync(Student student);
        Task<GetStudentByIdResponseDto> GetStudentByUserIdAsync(HttpContext httpContext);
    }
}
