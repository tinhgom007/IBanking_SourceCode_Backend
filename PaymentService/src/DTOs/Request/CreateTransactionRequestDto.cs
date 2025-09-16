namespace src.DTOs.Request
{
    public class CreateTransactionRequestDto
    {
        public string tuitionId { get; set; }
        public string studentId { get; set; }
        public string payerId { get; set; }
        public string amount { get; set; }
    }
}
