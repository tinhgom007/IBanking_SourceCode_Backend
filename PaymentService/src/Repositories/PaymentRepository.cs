using Microsoft.EntityFrameworkCore;
using src.Data;
using src.Entities;
using src.Interfaces.IRepositories;

namespace src.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _context;

        public PaymentRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> GetPaymentById(Guid paymentId)
        {
            return await _context.Payments.FirstOrDefaultAsync(t => t.PaymentId == paymentId);
        }

        public async Task<Payment> CreateTransaction(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<IEnumerable<Payment>> GetPaymentByPayerId(string payerId)
        {
            return await _context.Payments.Where(t => t.PayerId == payerId).ToListAsync();
        }

        public async Task<Payment> UpdateTransaction(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
            return payment;
        }
    }
}
