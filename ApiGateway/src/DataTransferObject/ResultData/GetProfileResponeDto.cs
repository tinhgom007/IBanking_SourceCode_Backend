namespace src.DataTransferObject.ResultData
{
    public class GetProfileResponeDto
    {
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public string Balance { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Marjor { get; set; } = string.Empty;

        public List<TuitionItem> TuitionItems { get; set; } = new List<TuitionItem>();

    }

    public class TuitionItem
    {
        public string TuitionId { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
    }
}
