using ArchitecturalPatterns.AwesomeShop.Core.Entities;
using ArchitecturalPatterns.AwesomeShop.Core.Enums;
using MediatR;

namespace ArchitecturalPatterns.AwesomeShop.Application.Commands.AddOrderUpdate
{
    public class AddOrderUpdateCommand : IRequest
    {
        public int OrderId { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }

        public OrderUpdated ToEntity()
        {
            return new OrderUpdated(Description, (OrderStatus)Status, OrderId);
        }
    }
}
