using EventDrivenArchitecture.AwesomeShop.Warehouse.Events;

namespace EventDrivenArchitecture.AwesomeShop.Warehouse.Integrations
{
    public interface IShippingService
    {
        void ShipOrder(OrderCreatedEvent @event);
    }
}
