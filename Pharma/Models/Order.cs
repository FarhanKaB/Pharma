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
        public int? SellerID { get; set; }
        public int Status { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }


        // Navigation property to OrderDetails
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }

}