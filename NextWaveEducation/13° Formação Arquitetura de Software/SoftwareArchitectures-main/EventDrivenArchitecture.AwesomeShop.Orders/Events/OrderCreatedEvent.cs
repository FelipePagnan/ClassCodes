using EventDrivenArchitecture.AwesomeShop.Orders.Models;

namespace EventDrivenArchitecture.AwesomeShop.Orders.Events
{
    public class OrderCreatedEvent
    {
        public OrderCreatedEvent(int id, AddOrderInputModel model)
        {
            Id = id;
            CustomerName = model.CustomerName;
            CustomerEmail = model.CustomerEmail;
            Items = model.Items.Select(i => new OrderItemModel(i.ProductId, i.Quantity)).ToList();
            TotalPrice = model.Items.Sum(i => i.Quantity * i.UnitPrice);
        }

        public int Id { get; private set; }
        public string CustomerName { get; private set; }
        public string CustomerEmail { get; private set; }
        public List<OrderItemModel> Items { get; private set; }
        public decimal TotalPrice { get; private set; }
    }

    public class OrderItemModel
    {
        public OrderItemModel(int productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }

        public int ProductId { get; private set; }
        public int Quantity { get; private set; }
    }
}
