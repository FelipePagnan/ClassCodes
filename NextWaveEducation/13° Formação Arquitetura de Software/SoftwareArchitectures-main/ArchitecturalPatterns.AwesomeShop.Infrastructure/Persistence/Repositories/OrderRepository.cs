using ArchitecturalPatterns.AwesomeShop.Core.Entities;
using ArchitecturalPatterns.AwesomeShop.Core.Enums;
using ArchitecturalPatterns.AwesomeShop.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchitecturalPatterns.AwesomeShop.Infrastructure.Persistence.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        public Task<int> Add(Order order)
        {
            return Task.FromResult(new Random().Next(1, 10000));
        }

        public Task AddUpdate(OrderUpdated orderUpdate)
        {
            return Task.CompletedTask;
        }

        public Task<Order> GetById(int id)
        {
            var order = new Order(
                "1234", 
                "LuisDev", 
                "luisdev@mail.com", 
                new List<OrderItem> { new OrderItem(1, "Product 1", 10.5m, 3) },
                new List<OrderUpdated> { 
                    new OrderUpdated("Order Started", OrderStatus.StartedAndPaymentPending, id),
                    new OrderUpdated("Payment confirmed", OrderStatus.PreparingForDelivery, id),
                    new OrderUpdated("Order is shipped", OrderStatus.Shipped, id),
                    new OrderUpdated("Order is delivered", OrderStatus.Delivered, id)
                });

            return Task.FromResult(order);
        }
    }
}
