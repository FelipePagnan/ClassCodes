using EventDrivenArchitecture.AwesomeShop.Orders.Enums;

namespace EventDrivenArchitecture.AwesomeShop.Orders.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        public void UpdateOrderStatus(int id, OrderStatus status)
        {
            // Order Status Updated
        }
    }
}
