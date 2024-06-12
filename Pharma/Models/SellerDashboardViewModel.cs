using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pharma.Models
{
    public class SellerDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalMedicines { get; set; }
/*        public int TotalOrders { get; set; }
*/        public int TotalVisitors { get; set; } // Assuming you have a way to track visitors
        public List<Medicine> LatestMedicines { get; set; }
    }
}