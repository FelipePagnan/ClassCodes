using HexagonalArchitecture.AwesomeShop.Domain.Entities;

namespace HexagonalArchitecture.AwesomeShop.Application.UseCases.GetAllOrders
{
    public class GetAllOrdersOutput
    {
        public GetAllOrdersOutput(List<Order> items)
        {
            Items = items.Select(o =>
                    new OrderOutputItem(o.Id, o.OrderCode, o.TotalCost, o.CustomerName, o.CustomerEmail)
                    ).ToList();
            GeneratedAt = DateTime.Now;
        }

        public List<OrderOutputItem> Items { get; private set; }
        public DateTime GeneratedAt { get; private set; }
    }

    public class OrderOutputItem
    {
        public OrderOutputItem(int id, string orderCode, decimal totalCost, string customerName, string customerEmail)
        {
            Id = id;
            OrderCode = orderCode;
            TotalCost = totalCost;
            CustomerName = customerName;
            CustomerEmail = customerEmail;
        }

        public int Id { get; private set; }
        public string OrderCode { get; private set; }
        public decimal TotalCost { get; private set; }
        public string CustomerName { get; private set; }
        public string CustomerEmail { get; private set; }
    }
}
