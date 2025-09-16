using src.Entities;

namespace src.Interfaces.IRepositories
{
    public interface IStudentRepository
    {
        Task<Student> GetStudentByStudentIdAsync(string studentId);
        Task<Student> GetStudentByUserIdAsync(Guid userId);
        Task<IEnumerable<Student>> GetAllStudentsAsync();
        Task AddStudentAsync(Student student);
        Task UpdateStudentAsync(Student student);
    }
}
