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

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginRequestDto loginRequestDto)
        {

            try
            {
                var result = await _authenticationConnnector.Login(loginRequestDto.Username, loginRequestDto.Password);

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
                                                                              createTransactionRequestDto.amount,
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



    }
}
