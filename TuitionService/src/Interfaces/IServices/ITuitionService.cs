using src.DTOs.Request;
using src.DTOs.Response;
using System.Collections.Generic;

namespace src.Interfaces.IServices
{
    public interface ITuitionService
    {
        Task<GetTuitionResponseDto> GetTuitionById(Guid tuitionId);
        Task<IEnumerable<GetTuitionResponseDto>> GetAllTuitionByStudentId(string studentId);
    }
}
