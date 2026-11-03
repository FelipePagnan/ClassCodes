using CleanArchitecture.AwesomeShop.Core.Enums;
using CleanArchitecture.AwesomeShop.Core.Events;

namespace CleanArchitecture.AwesomeShop.Core.Entities
{
    public class Order : AggregateRoot
    {
        public Order(string orderCode, string customerName, string customerEmail, List<OrderItem> items) : base()
        {
            Id = Guid.NewGuid();
            OrderCode = orderCode;
            CustomerName = customerName;
            CustomerEmail = customerEmail;
            Items = items;

            TotalCost = items.Sum(i => i.Quantity * i.UnitPrice);
            Status = OrderStatus.StartedAndPaymentPending;
            CreatedAt = DateTime.Now;

            AddEvent(new OrderCreated(Id));
        }

        public Guid Id { get; private set; }
        public string OrderCode { get; private set; }
        public string CustomerName { get; private set; }
        public string CustomerEmail { get; private set; }
        public List<OrderItem> Items { get; private set; }
        public decimal TotalCost { get; private set; }
        public OrderStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime LastUpdatedAt { get; private set; }
    }
}
