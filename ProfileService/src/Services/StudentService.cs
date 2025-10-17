using src.DTOs.Response;
using src.Entities;
using src.Interfaces.IRepositories;
using src.Interfaces.IServices;
using src.Utils;

namespace src.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        public StudentService(IStudentRepository studentRepository) {
            _studentRepository = studentRepository;
        } 
        public Task AddStudentAsync(Student student)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            return await _studentRepository.GetAllStudentsAsync();
        }

        public Task<GetStudentByIdResponseDto> GetStudentByUserIdAsync(HttpContext httpContext)
        {
            throw new NotImplementedException();
        }

        public async Task<GetStudentByIdResponseDto> GetStudentByStudentIdAsync(string studentId)
        {
            var student = await _studentRepository.GetStudentByStudentIdAsync(studentId);
            
            if (student == null)
            {
                return null;
            }

            return new GetStudentByIdResponseDto
            {
                StudentId = student.StudentId,
                FullName = student.FullName,
                gender = student.gender,
                PhoneNumber = student.PhoneNumber,
                Email = student.Email,
                Balance = student.Balance,
                marjor = student.marjor
            };
        }
        //public async Task<GetStudentByIdResponseDto> GetStudentByUserIdAsync(HttpContext httpContext)
        //{
        //    if (!httpContext.Request.Cookies.ContainsKey("access_token"))
        //    {
        //        throw new Exception("Token does not exist");
        //    }
        //    ;
        //    var userId = Auth.GetUserIdFromToken(httpContext);

        //    if (userId == null)
        //    {
        //        throw new Exception("UserId in token is invalid");
        //    }

        //    var student = await _studentRepository.GetStudentByUserIdAsync(userId.Value);

        //    return new GetStudentByIdResponseDto
        //    {
        //        StudentId = student.StudentId,
        //        FullName = student.FullName,
        //        gender = student.gender,
        //        PhoneNumber = student.PhoneNumber,
        //        Email = student.Email,
        //        Balance = student.Balance,
        //        marjor = student.marjor
        //    };
        //}            
    }
}
