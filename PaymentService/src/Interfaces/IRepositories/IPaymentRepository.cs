using src.Entities;
using System.Collections.Generic;

namespace src.Interfaces.IRepositories
{
    public interface IPaymentRepository
    {
        Task<Payment> GetPaymentById(Guid paymentId);
        Task<IEnumerable<Payment>> GetPaymentByPayerId(string payerId);
        Task<Payment> CreateTransaction(Payment payment);
        Task<Payment> UpdateTransaction(Payment payment);
        Task<bool> HasSuccessfulPayment(Guid tuitionId);
    }
}
