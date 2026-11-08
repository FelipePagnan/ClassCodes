using CleanArchitecture.AwesomeShop.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.AwesomeShop.Application.ViewModels
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
            Status = (int) order.Status;
            CreatedAt = order.CreatedAt;
        }

        public Guid Id { get; private set; }
        public string OrderCode { get; private set; }

        public string CustomerName { get; private set; }
        public string CustomerEmail { get; private set; }
        public decimal TotalCost { get; private set; }
        public List<string> Items { get; private set; }
        public int Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
    }
}
