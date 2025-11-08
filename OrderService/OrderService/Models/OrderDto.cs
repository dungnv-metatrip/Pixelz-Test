namespace OrderService.Models
{
    public class OrderDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; 
    }
}
