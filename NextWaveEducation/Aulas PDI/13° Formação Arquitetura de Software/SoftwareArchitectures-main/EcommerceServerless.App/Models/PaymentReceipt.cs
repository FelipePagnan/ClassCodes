using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceServerless.App.Models
{
    public class PaymentReceipt
    {
        public string id { get; set; }
        public int orderId { get; set; }
        public DateTime paidAt { get; set; }
    }
}
