using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using src.DataTransferObject.Parameter;
using src.DataTransferObject.ResultData;
using src.DTOs.Request;
using src.ServiceConnector.AuthServiceConnector;
using src.ServiceConnector.PaymentServiceConnector;
using src.ServiceConnector.ProfileServiceConnector;
using src.ServiceConnector.TuitionServiceConnector;

namespace src.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api")]
    public class RequestLocationController : ControllerBase
    {
        private readonly ProfileServiceConnector _profileConnector;
        private readonly AuthServiceConnector _authenticationConnnector;
        private readonly TuitionServiceConnector _tuitionServiceConnector;
        private readonly PaymentServiceConnector _paymentServiceConnector;

        public RequestLocationController(ProfileServiceConnector profileConnector, 
                                        AuthServiceConnector authServiceConnector, 
                                        TuitionServiceConnector tuitionServiceConnector,
                                        PaymentServiceConnector paymentServiceConnector)
        {
            _profileConnector = profileConnector;
            _authenticationConnnector = authServiceConnector;
            _tuitionServiceConnector = tuitionServiceConnector;
            _paymentServiceConnector = paymentServiceConnector;
        }

        [HttpPost("sign-in")]
        [AllowAnonymous]
        //[SwaggerOperation(
        //    Summary = "Đăng nhập hệ thống",
        //    Description = "Nhận username và password, trả về token nếu thành công",
        //    Tags = new[] { "Authentication" }
        //)]
        public async Task<ActionResult> Login(LoginRequestDto loginRequestDto)
        {

            try
            {
                var result = await _authenticationConnnector.Login(loginRequestDto.Username, loginRequestDto.Password);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    Secure = true            
                };

                Response.Cookies.Append("access_token", result.AccessToken, cookieOptions);
                Response.Cookies.Append("refresh_token", result.RefreshToken, cookieOptions);



                return Ok(new
                {
                    statusCode = 200,
                    msg = "Login successfully",
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

        [HttpPost("refreshToken")]
        [AllowAnonymous]
        public async Task<ActionResult> RefreshToken(RefreshTokenRequestDto refreshTokenRequestDto)
        {

            try
            {
                var result = await _authenticationConnnector.RefreshToken(refreshTokenRequestDto.RefreshToken);
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    Secure = true            
                };

                Response.Cookies.Append("access_token", result.AccessToken, cookieOptions);
                Response.Cookies.Append("refresh_token", result.RefreshToken, cookieOptions);

                return Ok(new
                {
                    statusCode = 200,
                    msg = "RefreshToken successfully",
                    metadata = result,
                });
            }
            catch (Exception error)
            {
                return BadRequest(new
                {
                    statusCode = 419,
                    msg = error.Message,
                });
            }
        }

        [HttpPost("sign-out")]
        [AllowAnonymous]
        public async Task<ActionResult> SignOut()
        {
            try
            {
                var result = await _authenticationConnnector.SignOut();

                // Ensure cookies are removed on the client by deleting with the same options
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    Secure = true
                };

                Response.Cookies.Delete("access_token", cookieOptions);
                Response.Cookies.Delete("refresh_token", cookieOptions);

                return Ok(new
                {
                    statusCode = 200,
                    msg = "Sign Out successfully",
                    metadata = result,
                });
            }
            catch (Exception error)
            {
                // Even if backend sign-out fails, remove cookies on the client side
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    Secure = true
                };

                Response.Cookies.Delete("access_token", cookieOptions);
                Response.Cookies.Delete("refresh_token", cookieOptions);

                return BadRequest(new
                {
                    statusCode = 400,
                    msg = error.Message,
                });
            }
        }


        [HttpGet("profile")]
        public async Task<ActionResult> GetProfile()
        {
            try
            {
                var profile = await _profileConnector.GetProfileAsync();
                var tuition = await _tuitionServiceConnector.GetTuitionAsysc(profile.StudentId);

                var result = new GetProfileResponeDto
                {
                    StudentId = profile.StudentId,
                    FullName = profile.FullName,
                    Balance = profile.Balance,
                    PhoneNumber = profile.PhoneNumber,
                    Gender = profile.Gender,
                    Email = profile.Email,
                    Marjor = profile.Marjor,
                    TuitionItems = tuition.Tuitions.Select(t => new TuitionItem
                    {
                        TuitionId = t.TuitionId,
                        Amount = t.Amount,
                        DueDate = t.DueDate,
                        Status = t.Status,
                        Semester = t.Semester
                    }).ToList()
                };

                return Ok(new
                {
                    statusCode = 200,
                    msg = "Get profile successfully",
                    metadata = result
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

        [HttpPost("get-studentId")]
        public async Task<ActionResult> GetStudentId(GetStudentIdRequestDto getStudentIdRequestDto)
        {
            try
            {
                var studentIds = await _profileConnector.SearchStudentIdSuggest(getStudentIdRequestDto.StudentId);

                return Ok(new
                {
                    statusCode = 200,
                    msg = "Get StudentID successfully",
                    metadata = studentIds
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

        [HttpPost("get-student-by-studentid")]
        public async Task<ActionResult> GetStudentByStudentId(GetStudentByStudentIdRequestDto request)
        {
            try
            {
                var student = await _profileConnector.GetStudentByStudentIdAsync(request.StudentId);

                if (student == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        msg = "Student Not Found"
                    });
                }

                var result = new
                {
                    StudentId = student.StudentId,
                    FullName = student.FullName,
                    Balance = student.Balance,
                    PhoneNumber = student.PhoneNumber,
                    Gender = student.Gender,
                    Email = student.Email,
                    Marjor = student.Marjor
                };

                return Ok(new
                {
                    statusCode = 200,
                    msg = "Get Student Successfully",
                    metadata = result
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


        [HttpPost("get-tuition-by-studentId")]
        public async Task<ActionResult> GetTuition(GetTuitionRequestDto getTuitionRequestDto)
        {
            try
            {
                var tuition = await _tuitionServiceConnector.GetTuitionAsysc(getTuitionRequestDto.StudentId);

                return Ok(new
                {
                    statusCode = 200,
                    msg = "Get profile successfully",
                    metadata = tuition
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

        [HttpGet("transaction-history")]
        public async Task<ActionResult> GetTransactionHistory()
        {
            try
            {
                var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var profile = await _profileConnector.GetProfileAsync();
                var transactionHistory = await _paymentServiceConnector.GetTransactionHistory(profile.StudentId, accessToken);

                var result = transactionHistory.Transactions.Select(t => new GetTransactionHistoryResponeDto.TransactionItem
                {
                    paymentId = t.PaymentId,
                    payerId = t.PayerId,
                    studentId = t.StudentId,
                    amount = t.Amount,
                    status = t.Status,
                    createAt = t.CreateAt
                }).ToList();

                return Ok(new
                {
                    statusCode = 200,
                    msg = "Get TransactionHistory successfully",
                    metadata = result
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

        [HttpPost("create-transaction")]
        public async Task<ActionResult> CreateTransaction(CreateTransactionRequestDto createTransactionRequestDto)
        {
            try
            {
                var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var result = await _paymentServiceConnector.CreateTransaction(createTransactionRequestDto.tuitionId, 
                                                                              createTransactionRequestDto.studentId, 
                                                                              createTransactionRequestDto.payerId, 
                                                                              accessToken);

                return Ok(new
                {
                    statusCode = 200,
                    msg = "CreateTransaction successfully",
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

        [HttpPost("confirm-transaction")]
        public async Task<ActionResult> ConfirmTransaction(ConfirmTransactionRequestDto confirmTransactionRequestDto)
        {

            try
            {
                var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var result = await _paymentServiceConnector.ConfirmTransaction(confirmTransactionRequestDto.paymentId,
                                                                              confirmTransactionRequestDto.otp,
                                                                              confirmTransactionRequestDto.email,
                                                                              accessToken);

                return Ok(new
                {
                    statusCode = 200,
                    msg = "ConfirmTransaction successfully",
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

        [HttpPost("resend-otp")]
        public async Task<ActionResult> ResendOtp()
        {
            try
            {
                var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var result = await _paymentServiceConnector.ResendOtpEmail(accessToken);

                return Ok(new
                {
                    statusCode = 200,
                    msg = "Resend OTP successfully",
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
