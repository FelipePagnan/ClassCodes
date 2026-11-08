using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchitecturalPatterns.AwesomeShop.Core.Enums
{
    public enum OrderStatus
    {
        StartedAndPaymentPending = 1,
        PreparingForDelivery = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5
    }
}
