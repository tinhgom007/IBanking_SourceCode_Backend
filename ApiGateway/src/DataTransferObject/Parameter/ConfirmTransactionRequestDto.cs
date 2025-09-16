namespace src.DataTransferObject.Parameter
{
    public class ConfirmTransactionRequestDto
    {
        public string paymentId { get; set; }
        public string otp { get; set; }
        public string email { get; set; }
    }
}
