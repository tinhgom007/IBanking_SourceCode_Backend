using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using src.Data;
using src.Entities;
using src.Interfaces.IServices;

namespace src.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Student>>> GetAllStudents()
        //{
        //    var users = await _studentService.GetAllStudentsAsync();
        //    return Ok(new
        //    {
        //        statusCode = 200,
        //        msg = "Get All Customer Success",
        //        metadata = users,
        //    });
        //}

        //[HttpGet]
        //public async Task<ActionResult> GetProfileByUserId()
        //{
        //    try
        //    {
        //        var studentProfile = await _studentService.GetStudentByUserIdAsync(HttpContext);

        //        if (studentProfile == null) { 
        //            return NotFound(new
        //            {
        //                statusCode = 404,
        //                msg = "User Not Found"
        //            });
        //        }
        //        else
        //        {
        //            return Ok(new
        //            {
        //                statusCode = 200,
        //                msg = "Get Profile Successfully",
        //                metadata = studentProfile
        //            });
        //        }
        //    }
        //    catch (Exception error)
        //    {
        //        return BadRequest(new
        //        {
        //            statusCode = 400,
        //            msg = error.Message
        //        });
        //    }
        //}

        [HttpGet("{studentId}")]
        public async Task<ActionResult> GetStudentByStudentId(string studentId)
        {
            try
            {
                var student = await _studentService.GetStudentByStudentIdAsync(studentId);

                if (student == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        msg = "Student Not Found"
                    });
                }

                return Ok(new
                {
                    statusCode = 200,
                    msg = "Get Student Successfully",
                    metadata = student
                });
            }
            catch (Exception error)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    msg = error.Message
                });
            }
        }

    }
}
