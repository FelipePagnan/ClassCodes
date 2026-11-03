using ArchitecturalPatterns.AwesomeShop.Core.Enums;

namespace ArchitecturalPatterns.AwesomeShop.Core.Entities
{
    public class OrderUpdated
    {
        public OrderUpdated(string description, OrderStatus newStatus, int orderId)
        {
            Description = description;
            NewStatus = newStatus;
            OrderId = orderId;
            UpdatedAt = DateTime.Now;
        }

        public int Id { get; private set; }
        public string Description { get; private set; }
        public OrderStatus NewStatus { get; private set; }
        public int OrderId { get; private set; }
        public DateTime UpdatedAt { get; private set; }
    }
}
