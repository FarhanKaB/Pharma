using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pharma.Models
{
    public class OrderItem
    {
        public int MedicineID { get; set; }
        public string MedicineName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price;
    }
}
