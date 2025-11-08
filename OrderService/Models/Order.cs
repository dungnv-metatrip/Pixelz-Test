namespace OrderService.Models
{
    public class Order
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Paid, Failed, SentToProduction
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
