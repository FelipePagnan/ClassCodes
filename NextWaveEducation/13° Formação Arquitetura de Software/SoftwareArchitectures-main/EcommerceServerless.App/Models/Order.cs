using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceServerless.App.Models
{
    public class Order
    {
        public int id { get; set; }
        public string orderCode { get; set; }
        public string customerName { get; set; }
        public string customerEmail { get; set; }
        public decimal totalCost { get; set; }
    }
}
