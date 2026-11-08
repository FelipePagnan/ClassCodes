using CleanArchitecture.AwesomeShop.Core.Entities;

namespace CleanArchitecture.AwesomeShop.Core.DomainServices
{
    public interface IOrderDomainService
    {
        bool Validate(Customer customer, Order order);
    }
}