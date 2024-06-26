using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pharma.Models
{
    public class Medicine
    {
        [Key]
        public int MedicineID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public DateTime ExpiryDate{ get; set; }
        public int Quantity {  get; set; }
        public byte[] ImgUrl { get; set; }
    }
}