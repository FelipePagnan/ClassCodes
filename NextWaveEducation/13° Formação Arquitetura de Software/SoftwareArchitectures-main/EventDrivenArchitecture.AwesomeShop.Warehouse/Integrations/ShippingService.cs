using EventDrivenArchitecture.AwesomeShop.Warehouse.Events;

namespace EventDrivenArchitecture.AwesomeShop.Warehouse.Integrations
{
    public class ShippingService : IShippingService
    {
        public void ShipOrder(OrderCreatedEvent @event)
        {
            Console.WriteLine("Order is shipped");
        }
    }
}
