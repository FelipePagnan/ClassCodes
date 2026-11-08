namespace EventDrivenArchitecture.AwesomeShop.Orders.Models
{
    public class AddOrderInputModel
    {
        public string OrderCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public List<OrderItemInputModel> Items { get; set; }
    }

    public class OrderItemInputModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
