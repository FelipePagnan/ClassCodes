using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolidPrinciples.Ocp
{
    public class PaymentService
    {
        public void Process(IOrderPaymentMethod paymentMethod) {
            paymentMethod.Process();
        }
    }

    public interface IOrderPaymentMethod {
        void Process();
    }

    public class CreditCardMethod : IOrderPaymentMethod
    {
        public CreditCardMethod(OrderPaymentInfo paymentInfo)
        {
            
        }
        
        public void Process()
        {
            throw new NotImplementedException();
        }
    }

    public class DebitCardMethod : IOrderPaymentMethod
    {
        public void Process()
        {
            throw new NotImplementedException();
        }
    }

    public class LoyaltyPointsMethod : IOrderPaymentMethod
    {
        public void Process()
        {
            throw new NotImplementedException();
        }
    }

    public class OrderPaymentInfo {
        public Guid OrderId { get; set; }
        public decimal TotalPrice { get; set; }
        public PaymentType Type { get; set; }
    }

    public enum PaymentType {
        Credit = 0,
        Debit = 1,
        LoyaltyPoints = 2
    }
}