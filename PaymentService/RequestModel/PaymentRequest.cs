namespace PaymentService.RequestModel
{
    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string? CardNumber { get; set; } // optional

    }
}
