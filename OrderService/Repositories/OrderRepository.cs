using OrderService.Models;

namespace OrderService.Repositories
{
    public class OrderRepository
    {
        private readonly List<Order> _orders = new()
    {
        new Order { Id = "1", Name = "Summer Collection", Amount = 99.99m },
        new Order { Id = "2", Name = "Winter Lookbook", Amount = 149.50m },
        new Order { Id = "3", Name = "Ecommerce Studio Test", Amount = 59.00m }
    };

        public IEnumerable<Order> GetAll() => _orders;

        public IEnumerable<Order> SearchByName(string name)
            => _orders.Where(o => o.Name.Contains(name, StringComparison.OrdinalIgnoreCase));

        public Order? GetById(string id) => _orders.FirstOrDefault(o => o.Id == id);

        public void Update(Order order)
        {
            var idx = _orders.FindIndex(o => o.Id == order.Id);
            if (idx >= 0) _orders[idx] = order;
        }

    }
}
