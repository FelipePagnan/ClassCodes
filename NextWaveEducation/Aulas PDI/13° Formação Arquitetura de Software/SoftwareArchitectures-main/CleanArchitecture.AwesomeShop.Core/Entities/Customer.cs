using CleanArchitecture.AwesomeShop.Core.Enums;
using CleanArchitecture.AwesomeShop.Core.ValueObjects;

namespace CleanArchitecture.AwesomeShop.Core.Entities
{
    public class Customer
    {
        public Customer(string name, string document, CustomerAddress deliveryAddress, CustomerStatus status)
        {
            Id = Guid.NewGuid();
            Name = name;
            Document = document;
            DeliveryAddress = deliveryAddress;
            Status = status;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Document { get; private set; }
        public CustomerAddress DeliveryAddress { get; private set; }
        public CustomerStatus Status { get; private set; }
    }
}
