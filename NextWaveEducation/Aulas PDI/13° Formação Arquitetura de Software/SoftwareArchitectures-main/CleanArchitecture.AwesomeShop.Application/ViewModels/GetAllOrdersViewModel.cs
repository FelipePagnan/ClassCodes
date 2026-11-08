using CleanArchitecture.AwesomeShop.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.AwesomeShop.Application.ViewModels
{
    public class GetAllOrdersViewModel
    {
        public GetAllOrdersViewModel(List<Order> items)
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
        public OrderOutputItem(Guid id, string orderCode, decimal totalCost, string customerName, string customerEmail)
        {
            Id = id;
            OrderCode = orderCode;
            TotalCost = totalCost;
            CustomerName = customerName;
            CustomerEmail = customerEmail;
        }

        public Guid Id { get; private set; }
        public string OrderCode { get; private set; }
        public decimal TotalCost { get; private set; }
        public string CustomerName { get; private set; }
        public string CustomerEmail { get; private set; }
    }
}
