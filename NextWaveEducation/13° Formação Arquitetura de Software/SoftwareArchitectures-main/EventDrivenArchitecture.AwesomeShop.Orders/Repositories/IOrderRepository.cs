using EventDrivenArchitecture.AwesomeShop.Orders.Enums;

namespace EventDrivenArchitecture.AwesomeShop.Orders.Repositories
{
    public interface IOrderRepository
    {
        void UpdateOrderStatus(int id, OrderStatus status);
    }
}
