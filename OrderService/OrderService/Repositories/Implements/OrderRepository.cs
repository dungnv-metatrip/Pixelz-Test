using OrderService.Models;
using OrderService.Repositories.Interfaces;

namespace OrderService.Repositories.Implements
{
    public class OrderRepository: IOrderRepository
    {
        private readonly List<Order> _orders = new()
    {
        new Order { Id = "6530f980-5325-4eec-adab-cd6cf372099a", Name = "Summer collection", Amount = 99.99m, Email="dung.bkinfo@gmail.com"},
        new Order { Id = "7530f980-5325-4eec-adab-cd6cf372099b", Name = "Winter collection", Amount = 149.50m,Email="dung.bkinfo@gmail.com" },
        new Order { Id = "8530f980-5325-4eec-adab-cd6cf372099c", Name = "Spring collection", Amount = 59.00m,Email="dung.bkinfo@gmail.com" }
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
        public void Add(Order order)
        {
            _orders.Add(order);
        }

    }
}
