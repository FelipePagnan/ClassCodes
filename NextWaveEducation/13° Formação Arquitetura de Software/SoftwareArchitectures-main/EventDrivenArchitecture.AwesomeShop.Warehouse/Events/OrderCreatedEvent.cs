namespace EventDrivenArchitecture.AwesomeShop.Warehouse.Events
{
    public class OrderCreatedEvent
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public List<OrderItemModel> Items { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class OrderItemModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
