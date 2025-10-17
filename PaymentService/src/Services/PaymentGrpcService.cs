using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using OTPGrpc;
using PaymentGrpc;
using src.Data;
using src.Entities;
using src.Interfaces.IRepositories;
using src.ServiceConnector.OTPServiceConnector;
using src.ServiceConnector.ProfileServiceConnector;
using src.ServiceConnector.TuitionServiceConnector;
using System.Text.Json;


namespace src.Services
{
    public class PaymentGrpcServiceImpl : PaymentGrpcService.PaymentGrpcServiceBase
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ProfileServiceConnector _profileConnector;
        private readonly OTPServiceConnector _otpConnector;
        private readonly TuitionServiceConnector _tuitionServiceConnector;
        private readonly PaymentDbContext _context;

        public PaymentGrpcServiceImpl(IPaymentRepository paymentRepository, 
                                      ProfileServiceConnector profileConnector,
                                      OTPServiceConnector otpServiceConnector,
                                      TuitionServiceConnector tuitionServiceConnector,
                                      PaymentDbContext context
                                      ) 
        {
            _paymentRepository = paymentRepository;
            _profileConnector = profileConnector;
            _otpConnector = otpServiceConnector;
            _tuitionServiceConnector = tuitionServiceConnector;
            _context = context;
        }

        public override async Task<CreateTransactionReply> CreateTransaction(CreateTransactionRequest request, ServerCallContext context)
        {
            var getProfileTask = _profileConnector.GetProfileAsync();
            var tuitionTask = _tuitionServiceConnector.GetTuitionById(request.TuitionId);

            var getProfile = await getProfileTask;
            var tuition = await tuitionTask;

            var balance = decimal.Parse(getProfile.Balance);
            var amount = decimal.Parse(tuition.Amount);

            if (balance < amount)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Insufficient balance"));
            }

            try
            {
                Guid paymentId = Guid.NewGuid();
                await _paymentRepository.CreateTransaction(new Payment
                {
                    PaymentId = paymentId,
                    TuitionId = Guid.Parse(request.TuitionId),
                    StudentId = request.StudentId,
                    PayerId = request.PayerId,
                    Amount = amount,
                    Status = "pending",
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                });

                await _otpConnector.GenerateOTP(getProfile.Email);

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
            catch(InvalidOperationException ex)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
            }
        }


        public override async Task<CreateTransactionReply> ConfirmTransaction(ConfirmTransactionRequest request, ServerCallContext context)
        {
            // 1. Validate OTP
            var otpResult = await _otpConnector.ValidateOTP(request.Email, request.Otp);
            if (!otpResult.IsValid)
            {
                var txn = await _paymentRepository.GetPaymentById(Guid.Parse(request.PaymentId));
                if (txn != null && txn.Status == "pending")
                {
                    txn.Status = "cancel";
                    txn.UpdateAt = DateTime.UtcNow;
                    await _paymentRepository.UpdateTransaction(txn);
                }

                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid or expired OTP"));
            }

            // 2. Get transaction
            var transaction = await _paymentRepository.GetPaymentById(Guid.Parse(request.PaymentId));
            if (transaction == null || transaction.Status != "pending")
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Transaction not found or not pending"));
            }

            using var dbTransaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                // 3. Ensure tuition is not already paid
                bool alreadyPaid = await _paymentRepository.HasSuccessfulPayment(transaction.TuitionId);
                if (alreadyPaid)
                {
                    transaction.Status = "cancel";
                    await _paymentRepository.UpdateTransaction(transaction);
                    throw new RpcException(new Status(StatusCode.FailedPrecondition, "This tuition was already paid by another account"));
                }

                // 4. Deduct balance
                var profileUpdate = await _profileConnector.HanldeBalance(transaction.PayerId, transaction.Amount.ToString(), false);
                if (!profileUpdate.Success)
                {
                    transaction.Status = "cancel";
                    await _paymentRepository.UpdateTransaction(transaction);
                    throw new RpcException(new Status(StatusCode.FailedPrecondition, "Failed to deduct balance"));
                }

                // 5. Update tuition
                var tuitionUpdate = await _tuitionServiceConnector.UpdateStatusTuition(transaction.TuitionId.ToString());
                if (!tuitionUpdate.Success)
                {
                    await _profileConnector.HanldeBalance(transaction.PayerId, transaction.Amount.ToString(), true);
                    transaction.Status = "cancel";
                    await _paymentRepository.UpdateTransaction(transaction);
                    throw new RpcException(new Status(StatusCode.Aborted, "Failed to update tuition, rollback balance"));
                }

                // 6. Mark transaction success
                transaction.Status = "success";
                transaction.UpdateAt = DateTime.UtcNow;
                await _paymentRepository.UpdateTransaction(transaction);

                await dbTransaction.CommitAsync();

                // 7. Side effects (out of transaction)
                _ = _otpConnector.SendEmailPaymentSuccess(request.Email, transaction.Amount.ToString());

                return new CreateTransactionReply
                {
                    PayerId = transaction.PayerId,
                    StudentId = transaction.StudentId,
                    Amount = transaction.Amount.ToString(),
                    Status = transaction.Status,
                    CreateAt = transaction.CreateAt.ToString("o")
                };
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
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
