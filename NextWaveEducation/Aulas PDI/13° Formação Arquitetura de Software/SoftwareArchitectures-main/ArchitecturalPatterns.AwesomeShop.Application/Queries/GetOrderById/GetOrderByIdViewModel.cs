using ArchitecturalPatterns.AwesomeShop.Core.Entities;

namespace ArchitecturalPatterns.AwesomeShop.Application.Queries.GetOrderById
{
    public class GetOrderByIdViewModel
    {
        public GetOrderByIdViewModel(Order order)
        {
            Id = order.Id;
            OrderCode = order.OrderCode;
            CustomerName = order.CustomerName;
            CustomerEmail = order.CustomerEmail;
            TotalCost = order.TotalCost;
            Items = order.Items.Select(i => $"Product {i.ProductName}, Quantity {i.Quantity}").ToList();
            Updates = order.Updates.Select(u => $"New State {u.NewStatus} at {u.UpdatedAt}").ToList();
            Status = (int)order.Status;
            CreatedAt = order.CreatedAt;
        }

        public int Id { get; private set; }
        public string OrderCode { get; private set; }

        public string CustomerName { get; private set; }
        public string CustomerEmail { get; private set; }
        public decimal TotalCost { get; private set; }
        public List<string> Items { get; private set; }
        public List<string> Updates { get; private set; }
        public int Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
    }
}
