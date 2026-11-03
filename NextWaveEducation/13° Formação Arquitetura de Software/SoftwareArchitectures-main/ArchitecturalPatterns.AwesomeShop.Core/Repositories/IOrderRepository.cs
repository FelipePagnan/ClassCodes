using ArchitecturalPatterns.AwesomeShop.Core.Entities;

namespace ArchitecturalPatterns.AwesomeShop.Core.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> GetById(int id);
        Task<int> Add(Order order);
        Task AddUpdate(OrderUpdated orderUpdate);
    }
}
