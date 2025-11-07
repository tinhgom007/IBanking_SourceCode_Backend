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
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Insufficient balance"));
            }

            // ✅ Kiểm tra xem có Tuition nào chưa thanh toán với CreatedAt trước Tuition hiện tại không
            if (string.IsNullOrEmpty(tuition.CreatedAt))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Tuition CreatedAt is missing"));
            }

            if (!DateTime.TryParse(tuition.CreatedAt, out var currentTuitionCreatedAt))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Tuition CreatedAt format"));
            }

            var unpaidTuitions = await _tuitionServiceConnector.GetTuitions(request.StudentId);
            
            foreach (var otherTuition in unpaidTuitions.Tuitions)
            {
                if (otherTuition.TuitionId == request.TuitionId)
                    continue;

                if (string.IsNullOrEmpty(otherTuition.CreatedAt))
                    continue;

                if (DateTime.TryParse(otherTuition.CreatedAt, out var otherTuitionCreatedAt) &&
                    otherTuitionCreatedAt < currentTuitionCreatedAt)
                {
                    throw new RpcException(new Status(StatusCode.FailedPrecondition, "Cannot create transaction. There are older unpaid tuitions that must be paid first."));
                }
            }

            // ✅ 1. Kiểm tra transaction đang pending
            var existingPending = await _paymentRepository.GetPendingTransaction(request.PayerId, request.TuitionId);
            if (existingPending != null)
            {
                // Gửi lại OTP thay vì tạo giao dịch mới
                await _otpConnector.ResendOTP(getProfile.Email);

                return new CreateTransactionReply
                {
                    PaymentId = existingPending.PaymentId.ToString(),
                    PayerId = existingPending.PayerId,
                    StudentId = existingPending.StudentId,
                    Amount = existingPending.Amount.ToString(),
                    Status = existingPending.Status,
                    CreateAt = existingPending.CreateAt.ToString("o"),
                };
            }

            // ✅ 2. Không có pending -> tạo mới như cũ
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
            catch (InvalidOperationException ex)
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
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid or expired OTP"));
            }

            // 2. Get transaction
            var transaction = await _paymentRepository.GetPaymentById(Guid.Parse(request.PaymentId));
            if (transaction == null || transaction.Status != "pending")
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Transaction not found or not pending"));
            }

            using var dbTransaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

           // 3. Ensure tuition is not already paid
            bool alreadyPaid = await _paymentRepository.HasSuccessfulPayment(transaction.TuitionId);
            if (alreadyPaid)
            {
                transaction.Status = "cancel";
                await _paymentRepository.UpdateTransaction(transaction);

                await dbTransaction.CommitAsync();
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "This tuition was already paid by another account"));
            }

            try
            {
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

            if (payments == null || !payments.Any())
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
        
        public override async Task<PaymentGrpc.SendEmailReply> ResendOtpEmail(ResendOtpEmailRequest request, ServerCallContext context)
        {
            var profile = await _profileConnector.GetProfileAsync();
            var send = await _otpConnector.ResendOTP(profile.Email);
            return new PaymentGrpc.SendEmailReply
            {
                Success = send.Success,
                Message = send.Message
            };
        }
    }
}
