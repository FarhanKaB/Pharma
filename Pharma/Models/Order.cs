using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pharma.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public int MedicineID { get; set; }
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Medicine Medicine { get; set; }
    }

}