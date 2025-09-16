using src.Entities;

namespace src.Interfaces.IRepositories
{
    public interface ITuitionRepository
    {
        Task<Tuition> GetTuitionByIdAsync(Guid tuitionId);
        Task<IEnumerable<Tuition>> GetAllTuitionUnpaidByStudentId(string studentId);
        Task <Tuition>UpdateTuition(Tuition tuition);
    }
}
