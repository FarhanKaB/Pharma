﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Pharma.Models
{
    public class PharmacyContext : DbContext
    {
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<MedicineRequest> MedicineRequests { get; set; }
        public DbSet<User> Users { get; set; }  // Add this line


        public PharmacyContext() : base("PharmacyDBConnectionString")
        {
        }
    }
}
