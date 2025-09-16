namespace src.Entities
{
    public class Tuition
    {
        public Guid TuitionId { get; set; }
        public string StudentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string Semester { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
