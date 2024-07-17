using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pharma.Models;

namespace Pharma.Controllers
{
    public class UserController : Controller
    {
        private PharmacyContext db = new PharmacyContext();

        // GET: User/Login
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
                    /*MedicineID = medicine.MedicineID,*/
                    /*Quantity = quantity,*/
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

/*            db.MedicineRequests.Add(request);
*/            db.SaveChanges();

            TempData["Success"] = "Medicine request submitted successfully.";
            return RedirectToAction("Dashboard");
        }
    }
}
