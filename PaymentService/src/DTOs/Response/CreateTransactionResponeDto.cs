namespace src.DTOs.Response
{
    public class CreateTransactionResponeDto
    {
        public string PayerId { get; set; }
        public string StudentId { get; set; }
        public string amount { get; set; }
        public string status { get; set; }
        public string CreateAt { get; set; }
    }
}
