using Pharma.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System;

namespace Pharma.Controllers
{
    public class AdminController : Controller
    {
        private PharmacyContext db = new PharmacyContext();

        // GET: Admin/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Admin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            if (ModelState.IsValid)
            {
                var admin = db.Admins.SingleOrDefault(a => a.Username == username && a.Password == password);
                if (admin != null)
                {
                    Session["AdminID"] = admin.AdminID;
                    Session["Username"] = admin.Username;
                    Session["Email"] = admin.Email;
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }
            return View();
        }

        // GET: Admin/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: Admin/Dashboard
        public ActionResult Dashboard()
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var totalSellers = db.Sellers.Count();
            var totalMedicines = db.Medicines.Sum(m => m.Quantity);
            var totalCustomers = db.Customers.Count();
            var uniqueMedicines = db.Medicines.Select(m => m.MedicineID).Distinct().Count();

            ViewBag.TotalSellers = totalSellers;
            ViewBag.TotalMedicines = totalMedicines;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.UniqueMedicines = uniqueMedicines;

            return View();
        }

        // GET and POST: Admin/Admin_Inventory
        public ActionResult Admin_Inventory()
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            List<Medicine> medicines = db.Medicines.ToList();
            ViewBag.Title = "Medicine Inventory";
            return View(medicines);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Admin_Inventory(Medicine medicine, HttpPostedFileBase ImageFile)
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                // Check if the medicine already exists
                var existingMedicine = db.Medicines.FirstOrDefault(m =>
                    m.MedicineName == medicine.MedicineName &&
                    m.GenericName == medicine.GenericName &&
                    m.Manufacturer == medicine.Manufacturer &&
                    m.DosageForm == medicine.DosageForm &&
                    m.Strength == medicine.Strength);

                if (existingMedicine != null)
                {
                    ModelState.AddModelError("", "This medicine already exists in the inventory.");
                }
                else
                {
                    // Handle file upload
                    if (ImageFile != null && ImageFile.ContentLength > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            ImageFile.InputStream.CopyTo(memoryStream);
                            medicine.ImageData = memoryStream.ToArray();
                        }
                    }

                    // Save medicine to database
                    db.Medicines.Add(medicine);
                    db.SaveChanges();

                    return RedirectToAction("Admin_Inventory");
                }
            }

            // If model  is invalid, return to the same view with validation errors
            List<Medicine> medicines = db.Medicines.ToList();
            ViewBag.Title = "Medicine Inventory";
            return View(medicines);
        }

        // Action to serve image data
        public ActionResult GetImage(int id)
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var medicine = db.Medicines.Find(id);
            if (medicine == null || medicine.ImageData == null)
            {
                return HttpNotFound();
            }
            return File(medicine.ImageData, "image/jpeg");
        }

        // Action to get medicine details
        public ActionResult GetMedicineDetails(int id)
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var medicine = db.Medicines.Find(id);
            if (medicine == null)
            {
                return HttpNotFound();
            }
            return Json(medicine, JsonRequestBehavior.AllowGet);
        }

        // Action to update medicine
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateMedicine(Medicine medicine, HttpPostedFileBase ImageFile)
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                var dbMedicine = db.Medicines.Find(medicine.MedicineID);
                if (dbMedicine == null)
                {
                    return HttpNotFound();
                }

                dbMedicine.MedicineName = medicine.MedicineName;
                dbMedicine.GenericName = medicine.GenericName;
                dbMedicine.Manufacturer = medicine.Manufacturer;
                dbMedicine.DosageForm = medicine.DosageForm;
                dbMedicine.Strength = medicine.Strength;
                dbMedicine.Quantity = medicine.Quantity;
                dbMedicine.Price = medicine.Price;
                dbMedicine.Category = medicine.Category;

                // Only update the ImageData if a new image is provided
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        ImageFile.InputStream.CopyTo(memoryStream);
                        dbMedicine.ImageData = memoryStream.ToArray();
                    }
                }

                db.Entry(dbMedicine).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Admin_Inventory");
            }

            // If model state is invalid, return to the same view with validation errors
            List<Medicine> medicines = db.Medicines.ToList();
            ViewBag.Title = "Medicine Inventory";
            return View(medicines);
        }

        // Action to delete medicine
        [HttpPost]
        public ActionResult DeleteMedicine(int id)
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var medicine = db.Medicines.Find(id);
            if (medicine == null)
            {
                return HttpNotFound();
            }

            db.Medicines.Remove(medicine);
            db.SaveChanges();

            return Json(new { success = true });
        }

        // Display Customer List
        public ActionResult Display_Customer()
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var customers = db.Customers.ToList();
            return View(customers);
        }

        // GET: Create Customer Account
        public ActionResult Create_Customer_Account()
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Customers = db.Customers.ToList();
            return View();
        }

        // POST: Create Customer Account
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create_Customer_Account(Customer customer)
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                // Check if the username, email, or phone number already exists
                var existingCustomer = db.Customers
                    .FirstOrDefault(c => c.Username == customer.Username || c.Email == customer.Email || c.Phone == customer.Phone);

                if (existingCustomer != null)
                {
                    if (existingCustomer.Username == customer.Username)
                    {
                        ModelState.AddModelError("Username", "A customer with this username already exists.");
                    }
                    if (existingCustomer.Email == customer.Email)
                    {
                        ModelState.AddModelError("Email", "A customer with this email already exists.");
                    }
                    if (existingCustomer.Phone == customer.Phone)
                    {
                        ModelState.AddModelError("Phone", "A customer with this phone number already exists.");
                    }
                }
                else
                {
                    db.Customers.Add(customer);
                    db.SaveChanges();
                    return RedirectToAction("Create_Customer_Account");
                }
            }

            ViewBag.Customers = db.Customers.ToList();
            return View(customer);
        }

        // Action to get customer details
        public ActionResult GetCustomerDetails(int id)
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return Json(customer, JsonRequestBehavior.AllowGet);
        }

        // Action to update customer details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCustomer(Customer customer)
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                var dbCustomer = db.Customers.Find(customer.CustomerID);
                if (dbCustomer == null)
                {
                    return HttpNotFound();
                }

                dbCustomer.Username = customer.Username;
                dbCustomer.Email = customer.Email;
                dbCustomer.FullName = customer.FullName;
                dbCustomer.Address = customer.Address;
                dbCustomer.Phone = customer.Phone;

                db.Entry(dbCustomer).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Display_Customer");
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // Action to delete customer
        [HttpPost]
        public ActionResult DeleteCustomer(int id)
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            db.Customers.Remove(customer);
            db.SaveChanges();

            return Json(new { success = true });
        }

        // Action to change customer password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(int customerId, string newPassword)
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var customer = db.Customers.Find(customerId);
            if (customer == null)
            {
                return HttpNotFound();
            }

            // Update customer password (replace with your hashing logic)
            customer.Password = newPassword;

            db.Entry(customer).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Display_Customer");
        }

        // GET: ManageSeller
        public ActionResult ManageSeller()
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var sellers = db.Sellers.ToList();
            return View(sellers);
        }

        // GET: RegisterSellerAccount
        public ActionResult RegisterSellerAccount()
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var sellers = db.Sellers.ToList();
            return View(sellers);
        }

        // POST: RegisterSellerAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterSellerAccount(Seller seller)
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                // Check if the username, email, or phone number already exists
                var existingSeller = db.Sellers
                    .FirstOrDefault(s => s.Username == seller.Username || s.Email == seller.Email || s.Phone == seller.Phone);

                if (existingSeller != null)
                {
                    if (existingSeller.Username == seller.Username)
                    {
                        ModelState.AddModelError("Username", "A seller with this username already exists.");
                    }
                    if (existingSeller.Email == seller.Email)
                    {
                        ModelState.AddModelError("Email", "A seller with this email already exists.");
                    }
                    if (existingSeller.Phone == seller.Phone)
                    {
                        ModelState.AddModelError("Phone", "A seller with this phone number already exists.");
                    }
                }
                else
                {
                    db.Sellers.Add(seller);
                    db.SaveChanges();
                    return RedirectToAction("RegisterSellerAccount");
                }
            }

            // If model state is invalid, return to the same view with validation errors and list of sellers
            var sellers = db.Sellers.ToList();
            return View(sellers);
        }

        // POST: UpdateSeller
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateSeller(Seller seller)
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                var dbSeller = db.Sellers.Find(seller.SellerID);
                if (dbSeller == null)
                {
                    return HttpNotFound();
                }

                dbSeller.Username = seller.Username;
                dbSeller.Email = seller.Email;
                dbSeller.FullName = seller.FullName;
                dbSeller.Phone = seller.Phone;

                db.Entry(dbSeller).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("ManageSeller");
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // POST: DeleteSeller
        [HttpPost]
        public ActionResult DeleteSeller(int id)
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var seller = db.Sellers.Find(id);
            if (seller == null)
            {
                return HttpNotFound();
            }

            db.Sellers.Remove(seller);
            db.SaveChanges();

            return Json(new { success = true });
        }

        // POST: ChangeSellerPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeSellerPassword(int sellerId, string newPassword)
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var seller = db.Sellers.Find(sellerId);
            if (seller == null)
            {
                return HttpNotFound();
            }

            // Update seller password (replace with your hashing logic)
            seller.Password = newPassword;

            db.Entry(seller).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("ManageSeller");
        }

        public ActionResult Profile()
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();

        }
        // POST: ChangeAdminPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeAdminPassword(int adminId, string currentPassword, string newPassword, string confirmPassword)
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var admin = db.Admins.Find(adminId);
            if (admin == null)
            {
                return HttpNotFound();
            }

            // Check if the current password is correct (replace with your hashing logic)
            if (admin.Password != currentPassword)
            {
                ModelState.AddModelError("", "Current password is incorrect.");
                return View("Profile");
            }

            // Check if the new password and confirm password match
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "New password and confirm password do not match.");
                return View("Profile");
            }

            // Update admin password (replace with your hashing logic)
            admin.Password = newPassword;

            db.Entry(admin).State = EntityState.Modified;
            db.SaveChanges();

            TempData["PasswordChangeSuccess"] = "Password changed successfully.";
            return RedirectToAction("Profile");
        }
        public ActionResult Order()
        {
            if (Session["AdminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var orders = db.Orders.Include("OrderDetails").ToList();
            var completedOrders = orders.Where(o => o.Status == OrderStatus.Delivered).ToList();
            var inProcessOrders = orders.Where(o => o.Status == OrderStatus.OutForDelivery).ToList();
            var pendingOrders = orders.Where(o => o.Status == OrderStatus.Pending).ToList();

            ViewBag.CompletedOrders = completedOrders;
            ViewBag.InProcessOrders = inProcessOrders;
            ViewBag.PendingOrders = pendingOrders;

            return View();
        }

        public ActionResult GetOrderDetails(int orderId)
        {
            var order = db.Orders
                          .Include(o => o.OrderDetails.Select(od => od.Medicine))
                          .Include(o => o.Customer)  // Include Customer details
                          .FirstOrDefault(o => o.OrderID == orderId);

            if (order == null)
            {
                return HttpNotFound();
            }

            // Return JSON with necessary data
            return Json(new
            {
                order.OrderID,
                OrderDate = order.OrderDate.ToString("yyyy-MM-dd"),
                order.TotalPrice,
                Customer = new
                {
                    order.Customer.FullName,
                    order.Customer.Email,
                    order.Customer.Phone,
                    order.Customer.Address
                },
                OrderDetails = order.OrderDetails.Select(od => new
                {
                    od.Medicine.MedicineName,
                    od.Medicine.GenericName,
                    od.Medicine.Manufacturer,
                    od.Medicine.DosageForm,
                    od.Medicine.Strength,
                    od.Quantity,
                    od.Price,
                    od.Medicine.Category
                })
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Report()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // Total revenue for the current month
            var totalRevenue = db.Orders
                .Where(o => o.OrderDate.Month == currentMonth && o.OrderDate.Year == currentYear)
                .Sum(o => o.TotalPrice);

            // Most sold medicine for the current month
            var mostSoldMedicineData = db.OrderDetails
                .Where(od => od.Order.OrderDate.Month == currentMonth && od.Order.OrderDate.Year == currentYear)
                .GroupBy(od => od.Medicine)
                .OrderByDescending(g => g.Sum(od => od.Quantity))
                .Select(g => new
                {
                    Medicine = g.Key,
                    Quantity = g.Sum(od => od.Quantity),
                    TotalPrice = g.Sum(od => od.Price * od.Quantity)
                })
                .FirstOrDefault();

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.MostSoldMedicine = mostSoldMedicineData?.Medicine;
            ViewBag.MostSoldQuantity = mostSoldMedicineData?.Quantity ?? 0;
            ViewBag.MostSoldTotalPrice = mostSoldMedicineData?.TotalPrice ?? 0;

            return View();
        }


        // Dispose method
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
