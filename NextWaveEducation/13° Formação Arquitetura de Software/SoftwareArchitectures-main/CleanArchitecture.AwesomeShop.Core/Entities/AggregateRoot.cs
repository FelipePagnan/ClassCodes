using CleanArchitecture.AwesomeShop.Core.Events;

namespace CleanArchitecture.AwesomeShop.Core.Entities
{
    public abstract class AggregateRoot
    {
        public AggregateRoot()
        {
            Events = new List<IEvent>();
        }

        public List<IEvent> Events { get; private set; }

        public void AddEvent(IEvent @event) {
            Events.Add(@event);
        }
    }
}
