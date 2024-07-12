using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using Pharma.Models;

namespace Pharma.Controllers
{
    public class SellerController : Controller
    {
        private PharmacyContext db = new PharmacyContext();

        // GET: Seller/SellerLogin
        public ActionResult SellerLogin()
        {
            return View();
        }

        // POST: Seller/SellerLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SellerLogin(string username, string password)
        {
            if (ModelState.IsValid)
            {
                var seller = db.Sellers.SingleOrDefault(s => s.Username == username && s.Password == password);
                if (seller != null)
                {
                    Session["SellerID"] = seller.SellerID;
                    Session["Username"] = seller.Username;
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }
            return View();
        }

        // GET: Seller/Dashboard
        public ActionResult Dashboard()
        {
            return View();
        }

        // GET: Seller/ViewMedicines
        public ActionResult ViewMedicines()
        {
            var medicines = db.Medicines.ToList();
            return View(medicines);
        }

        // POST: Seller/AddToList
        [HttpPost]
        public ActionResult AddToList(int medicineId, int quantity)
        {
            var medicine = db.Medicines.Find(medicineId);
            if (medicine != null && medicine.Quantity >= quantity)
            {
                var item = new OrderItem
                {
                    MedicineID = medicine.MedicineID,
                    MedicineName = medicine.MedicineName,
                    Quantity = quantity,
                    Price = medicine.Price
                };

                // Store the item in the session
                if (Session["Cart"] == null)
                {
                    Session["Cart"] = new List<OrderItem>();
                }
                var cart = (List<OrderItem>)Session["Cart"];
                cart.Add(item);

                return RedirectToAction("ViewCart");
            }
            else
            {
                // Handle the case where the medicine is not available or quantity is insufficient
                TempData["Error"] = "The requested quantity is not available.";
                return RedirectToAction("ViewMedicines");
            }
        }

        // GET: Seller/ViewCart
        public ActionResult ViewCart()
        {
            var cart = Session["Cart"] as List<OrderItem> ?? new List<OrderItem>();
            return View(cart);
        }

        // POST: Seller/PrintReceipt
        [HttpPost]
        public ActionResult PrintReceipt()
        {
            var cart = Session["Cart"] as List<OrderItem>;
            if (cart != null && cart.Count > 0)
            {
                // Implement printing logic here
                // Clear the cart after printing
                Session["Cart"] = null;

                // Return a view with the receipt details
                return View("Receipt", cart);
            }
            else
            {
                TempData["Error"] = "No items in the cart to print.";
                return RedirectToAction("ViewCart");
            }
        }
    }
}
