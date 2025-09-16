using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using src.Data;
using src.DTOs.Request;
using src.Interfaces.IServices;

namespace src.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class TuitionController : ControllerBase
    {
        private readonly ILogger<TuitionController> _logger;
        private readonly ITuitionService _tuitionService;

        public TuitionController(ILogger<TuitionController> logger, ITuitionService tuitionService)
        {
            _logger = logger;
            _tuitionService = tuitionService;
        }

        [HttpGet("{studentId}")]
        public async Task<ActionResult> GetTuitionByStudentId(string studentId)
        {

            try
            {
                var result = await _tuitionService.GetAllTuitionByStudentId(studentId);

                return Ok(new
                {
                    statusCode = 200,
                    msg = "Get Tuition successfully",
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
