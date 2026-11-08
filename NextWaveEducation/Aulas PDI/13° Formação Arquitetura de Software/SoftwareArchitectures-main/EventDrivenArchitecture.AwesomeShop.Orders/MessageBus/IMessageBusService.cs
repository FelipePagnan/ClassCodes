namespace EventDrivenArchitecture.AwesomeShop.Orders.MessageBus
{
    public interface IMessageBusService
    {
        void Publish(object data, string routingKey);
    }
}
