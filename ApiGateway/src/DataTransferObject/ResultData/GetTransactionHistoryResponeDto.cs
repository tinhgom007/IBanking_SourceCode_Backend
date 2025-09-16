namespace src.DataTransferObject.ResultData
{
    public class GetTransactionHistoryResponeDto
    {
        public List<TransactionItem> Transactions { get; set; } = new List<TransactionItem>();

        public class TransactionItem
        {
            public string paymentId { get; set; } = string.Empty;
            public string studentId { get; set; } = string.Empty;
            public string payerId { get; set; } = string.Empty;
            public string amount { get; set; } = string.Empty;
            public string status { get; set; } = string.Empty;
            public string createAt { get; set; } = string.Empty;
        }
    }
}
