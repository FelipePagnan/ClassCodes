using ArchitecturalPatterns.AwesomeShop.Core.Entities;
using MediatR;

namespace ArchitecturalPatterns.AwesomeShop.Application.Commands.AddOrder
{
    public class AddOrderCommand : IRequest<int>
    {
        public string OrderCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public List<AddOrderCommandItem> Items { get; set; }

        public Order ToEntity()
        {
            return new Order(OrderCode, CustomerName, CustomerEmail, Items.Select(i => new OrderItem(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList());
        }
    }

    public class AddOrderCommandItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
