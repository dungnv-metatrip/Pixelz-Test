using OrderService.Models;

namespace OrderService.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        public IEnumerable<Order> GetAll();

        public IEnumerable<Order> SearchByName(string name);
        public Order? GetById(string id);
        public void Add(Order order);
        public void Update(Order order);
    }
}
