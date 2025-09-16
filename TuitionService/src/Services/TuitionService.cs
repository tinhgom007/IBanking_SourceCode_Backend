using src.DTOs.Request;
using src.DTOs.Response;
using src.Interfaces.IRepositories;
using src.Interfaces.IServices;

namespace src.Services
{
    public class TuitionService : ITuitionService
    {
        private readonly ITuitionRepository _tuitionRepository;
        //private readonly IMapper _mapper;

        public TuitionService(ITuitionRepository tuitionRepository)
        {
            _tuitionRepository = tuitionRepository;
            //_mapper = mapper;
        }

        public async Task<IEnumerable<GetTuitionResponseDto>> GetAllTuitionByStudentId(string studentId)
        {
            var tuitions = await _tuitionRepository.GetAllTuitionUnpaidByStudentId(studentId);

            var result = tuitions.Select(t => new GetTuitionResponseDto
            {
                StudentId = t.StudentId,
                Amount = t.Amount,
                DueDate = t.DueDate,
                Status = t.Status,
                Semester = t.Semester
            });

            return result;
        }

        public async Task<GetTuitionResponseDto> GetTuitionById(Guid tuitionId)
        {
            var tuitions = await _tuitionRepository.GetTuitionByIdAsync(tuitionId);

            return new GetTuitionResponseDto
            {
                StudentId = tuitions.StudentId,
                Amount = tuitions.Amount,
                Semester = tuitions.Semester,
                DueDate = tuitions.DueDate,
                Status = tuitions.Status,
            };
        }

    }
}
