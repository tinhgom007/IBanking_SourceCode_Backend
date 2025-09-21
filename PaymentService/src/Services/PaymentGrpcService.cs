using Grpc.Core;
using OTPGrpc;
using PaymentGrpc;
using src.Entities;
using src.Interfaces.IRepositories;
using src.ServiceConnector.OTPServiceConnector;
using src.ServiceConnector.ProfileServiceConnector;
using src.ServiceConnector.TuitionServiceConnector;


namespace src.Services
{
    public class PaymentGrpcServiceImpl : PaymentGrpcService.PaymentGrpcServiceBase
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ProfileServiceConnector _profileConnector;
        private readonly OTPServiceConnector _otpConnector;
        private readonly TuitionServiceConnector _tuitionServiceConnector;

        public PaymentGrpcServiceImpl(IPaymentRepository paymentRepository, 
                                      ProfileServiceConnector profileConnector,
                                      OTPServiceConnector otpServiceConnector,
                                      TuitionServiceConnector tuitionServiceConnector
                                      ) 
        {
            _paymentRepository = paymentRepository;
            _profileConnector = profileConnector;
            _otpConnector = otpServiceConnector;
            _tuitionServiceConnector = tuitionServiceConnector;
        }

        public override async Task<CreateTransactionReply> CreateTransaction(CreateTransactionRequest request, ServerCallContext context)
        {
            var getProfile = await _profileConnector.GetProfileAsync();
            var tuition = await _tuitionServiceConnector.GetTuitionById(request.TuitionId);

            if (decimal.Parse(getProfile.Balance) < decimal.Parse(tuition.Amount))
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Insufficient balance"));
            }

            await _otpConnector.GenerateOTP(getProfile.Email);

            Guid paymentId = Guid.NewGuid();
            await _paymentRepository.CreateTransaction(new Payment
            {
                PaymentId = paymentId,
                TuitionId = Guid.Parse(request.TuitionId),
                StudentId = request.StudentId,
                PayerId = request.PayerId,
                Amount = decimal.Parse(tuition.Amount),
                Status = "pending",
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            });

            return new CreateTransactionReply
            {
                PaymentId = paymentId.ToString(),
                PayerId = request.PayerId,
                StudentId = request.StudentId,
                Amount = tuition.Amount,
                Status = "pending",
                CreateAt = DateTime.UtcNow.ToString("o")
            };
        }


        public override async Task<CreateTransactionReply> ConfirmTransaction(ConfirmTransactionRequest request, ServerCallContext context)
        {
            var otpResult = await _otpConnector.ValidateOTP(request.Email, request.Otp);
            if (!otpResult.IsValid)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid or expired OTP"));
            }

            var transaction = await _paymentRepository.GetPaymentById(Guid.Parse(request.PaymentId));

            //Check if transaction exists and is still pending
            if (transaction == null || transaction.Status != "pending")
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Transaction not found"));
            }

            // Deduct balance from payer's profile
            var profileUpdate = await _profileConnector.HanldeBalance(transaction.PayerId, transaction.Amount.ToString(), false);

            if(!profileUpdate.Success)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Failed to deduct balance"));
            }

            // Update tuition status
            try
            {
                var tuitionUpdate = await _tuitionServiceConnector.UpdateStatusTuition(transaction.TuitionId.ToString());

                if (!tuitionUpdate.Success)
                {
                    await _profileConnector.HanldeBalance(transaction.PayerId, transaction.Amount.ToString(), true);
                    throw new RpcException(new Status(StatusCode.Aborted, "Failed to update tuition, rollback balance"));
                }

                transaction.Status = "Success";
                transaction.UpdateAt = DateTime.UtcNow;
                await _paymentRepository.UpdateTransaction(transaction);
                await _otpConnector.SendEmailPaymentSuccess(request.Email, transaction.Amount.ToString());

                return new CreateTransactionReply
                {
                    PayerId = transaction.PayerId,
                    StudentId = transaction.StudentId,
                    Amount = transaction.Amount.ToString(),
                    Status = transaction.Status,
                    CreateAt = transaction.CreateAt.ToString("o")
                };
            }
            catch(Exception ex)
            {
                // Rollback balance deduction if tuition update fails
               await _profileConnector.HanldeBalance(transaction.PayerId, transaction.Amount.ToString(), true);
               throw new RpcException(new Status(StatusCode.Internal, $"Transaction failed: {ex.Message}"));
            }
        }

        public override async Task<GetTransactionHistoryReply> GetTransactionHistory(GetTransactionHistoryRequest request, ServerCallContext context)
        {
            var payments = await _paymentRepository.GetPaymentByPayerId(request.StudentId);

            if(payments == null || !payments.Any())
            {
                throw new RpcException(new Status(StatusCode.NotFound, "No transactions found"));
            }

            var transactions = payments.Select(p => new Transaction
            {
                PaymentId = p.PaymentId.ToString(),
                StudentId = p.StudentId,
                PayerId = p.PayerId,
                Amount = p.Amount.ToString(),
                Status = p.Status,
                CreateAt = p.CreateAt.ToString("o"),
                UpdateAt = p.UpdateAt.ToString("o")
            });

            return new GetTransactionHistoryReply
            {
                Transactions = { transactions }
            };

        }
    }
}
