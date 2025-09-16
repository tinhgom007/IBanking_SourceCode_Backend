namespace src.Entities
{
    public class Payment
    {
        public Guid PaymentId { get; set; }
        public Guid TuitionId { get; set; }
        public string StudentId { get; set; }
        public string PayerId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime CreateAt { get; set; }  
        public DateTime UpdateAt { get; set; }
    }
}
