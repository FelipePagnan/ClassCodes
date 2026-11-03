using CleanArchitecture.AwesomeShop.Core.Entities;

namespace CleanArchitecture.AwesomeShop.Application.InputModels
{
    public class AddOrderInputModel
    {
        public string OrderCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public List<OrderItemInputModel> Items { get; set; }

        public Order ToEntity()
        {
            return new Order(OrderCode, CustomerName, CustomerEmail, Items.Select(i => new OrderItem(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList());
        }
    }

    public class OrderItemInputModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
