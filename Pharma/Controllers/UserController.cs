using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pharma.Models;


namespace Pharma.Controllers
{
    public class UserController : Controller
    {
        private PharmacyContext db = new PharmacyContext();

        // GET: User/Login & Signup
        public ActionResult UserLogin()
        {
            return View();
        }

        // POST: User/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserLogin(string username, string password)
        {
            if (ModelState.IsValid)
            {
                var user = db.Customers.SingleOrDefault(u => u.Username == username && u.Password == password);
                if (user != null)
                {
                    Session["UserID"] = user.CustomerID;
                    Session["Username"] = user.Username;
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }
            return View();
        }

        // POST: User/ Signup
      [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserRegister(UserReg userReg)
        {
            if (ModelState.IsValid)
            {
                // Check if username is already taken
                var existingUser = db.Customers.FirstOrDefault(u => u.Username == userReg.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Username already exists. Please choose a different one.");
                    return View();
                }

                // Map UserReg to Customer model
                var customer = new Customer
                {
                    Username = userReg.Username,
                    Password = userReg.Password,
                    Email = userReg.Email
                };

                // Add the new customer to the database
                db.Customers.Add(customer);
                db.SaveChanges();


            }
            return RedirectToAction("UserLogin");

        }

        // GET: User/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("UserLogin", "User");
        }

        // GET: User/Dashboard
        public ActionResult Dashboard()
        {
            return View();
        }

        // GET: User/ViewMedicines
        public ActionResult ViewMedicines()
        {
            var medicines = db.Medicines.ToList();
            return View(medicines);
        }

        // POST: User/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(int medicineId, int quantity)
        {
            var medicine = db.Medicines.Find(medicineId);
            if (medicine != null && medicine.Quantity >= quantity)
            {
                var order = new Order
                {
                    CustomerID = (int)Session["UserID"],
                    MedicineID = medicine.MedicineID,
                    Quantity = quantity,
                    OrderDate = DateTime.Now
                };

                db.Orders.Add(order);
                medicine.Quantity -= quantity;
                db.SaveChanges();

                TempData["Success"] = "Order placed successfully.";
            }
            else
            {
                TempData["Error"] = "The requested quantity is not available.";
            }
            return RedirectToAction("ViewMedicines");
        }

        // GET: User/RequestMedicine
        public ActionResult RequestMedicine()
        {
            return View();
        }

        // POST: User/RequestMedicine
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestMedicine(string medicineName)
        {
            var request = new MedicineRequest
            {
                CustomerID = (int)Session["UserID"],
                MedicineName = medicineName,
                RequestDate = DateTime.Now
            };

            db.MedicineRequests.Add(request);
            db.SaveChanges();

            TempData["Success"] = "Medicine request submitted successfully.";
            return RedirectToAction("Dashboard");
        }
    }
}
