using src.DTOs.Response;
using src.Entities;

namespace src.Interfaces.IServices
{
    public interface IPaymentService
    {
        public Task<IEnumerable<GetHistoryPaymentResponeDto>> GetHistoryPaymentByPayerId(HttpContext httpContext);
        public Task<Payment> CreatePayment(Payment payment);
    }
}
