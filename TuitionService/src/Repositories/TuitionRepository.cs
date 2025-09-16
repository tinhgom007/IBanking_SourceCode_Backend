using Microsoft.EntityFrameworkCore;
using src.Data;
using src.Entities;
using src.Interfaces.IRepositories;

namespace src.Repositories
{
    public class TuitionRepository : ITuitionRepository
    {
        private readonly TuitionDbContext _context;

        public TuitionRepository(TuitionDbContext context) 
        { 
            _context = context;
        }

        public async Task<IEnumerable<Tuition>> GetAllTuitionUnpaidByStudentId(string studentId)
        {
            return await _context.Tuitions.Where(Tuition => Tuition.StudentId == studentId && Tuition.Status == "Unpaid").ToListAsync();
        }

        public async Task<Tuition> GetTuitionByIdAsync(Guid tuitionId)
        {
            return await _context.Tuitions.FirstOrDefaultAsync(t => t.TuitionId == tuitionId);
        }

        public async Task<Tuition> UpdateTuition(Tuition tuition)
        {
            _context.Tuitions.Update(tuition);
            await _context.SaveChangesAsync();
            return tuition;
        }
    }
}
