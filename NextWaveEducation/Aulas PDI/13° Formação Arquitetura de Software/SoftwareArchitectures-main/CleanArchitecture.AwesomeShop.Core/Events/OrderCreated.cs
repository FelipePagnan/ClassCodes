namespace CleanArchitecture.AwesomeShop.Core.Events
{
    public class OrderCreated : IEvent
    {
        public OrderCreated(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }
    }
}
