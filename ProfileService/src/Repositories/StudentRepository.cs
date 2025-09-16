using src.Entities;
using src.Interfaces.IRepositories;
using src.Data;
using Microsoft.EntityFrameworkCore;

namespace src.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ProfileDbContext _context;

        public StudentRepository(ProfileDbContext context)
        {
            _context = context;
        }

        public Task AddStudentAsync(Student student)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            return await _context.Students.ToListAsync();
        }

        public async Task<Student> GetStudentByStudentIdAsync(string studentId)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        public async Task<Student> GetStudentByUserIdAsync(Guid userId)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task UpdateStudentAsync(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }
    }
}
