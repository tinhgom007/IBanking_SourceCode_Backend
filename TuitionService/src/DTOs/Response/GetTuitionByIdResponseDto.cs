namespace src.DTOs.Response
{
    public class GetTuitionResponseDto
    {
        public string StudentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string Semester { get; set; }
    }
}
