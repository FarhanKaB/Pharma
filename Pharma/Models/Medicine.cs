using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pharma.Models
{
    public class Medicine
    {
        public int MedicineID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int Quantity { get; set; }
    }
}