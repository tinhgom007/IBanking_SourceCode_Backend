namespace src.DTOs.Response
{
    public class GetHistoryPaymentResponeDto
    {
        public Guid TuitionId { get; set; }
        public string StudentId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }
}
