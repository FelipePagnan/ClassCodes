using CleanArchitecture.AwesomeShop.Core.Entities;

namespace CleanArchitecture.AwesomeShop.Core.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> GetById(int id);
        Task<List<Order>> GetAll();
        Task<int> Add(Order order);
    }
}
