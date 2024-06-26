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
        public ActionResult UserRegister(UserReg userReg) ///      (Customer userReg) 
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
        
        // Normally, this data would come from a database. Here it's hardcoded for simplicity.
/*        private static List<Medicine> medicines = new List<Medicine>
        {
            new Medicine { MedicineID = 1, Name = "Medicine 1", Description = "Description 1", Price = 10.0m, ImageUrl = "G:\\AUST_LECTURES\\3.2_SD LAB\\Pharma(project)\\Pharma\\image\\img1.jpeg" },
            new Medicine { MedicineID = 2, Name = "Medicine 2", Description = "Description 2", Price = 20.0m, ImageUrl = "G:\\AUST_LECTURES\\3.2_SD LAB\\Pharma(project)\\Pharma\\image\\img1.jpeg" },
            // Add more medicines as needed
        };*/

        public ActionResult ViewMedicines()
        { 
          var medicines = db.Medicines.ToList();                                         ///View Medicines
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


        // POST: User/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int medicineId)
        {
            var medicine = db.Medicines.Find(medicineId);
            if (medicine == null)
            {
                return HttpNotFound();
            }

            List<int> cart;
            if (Session["Cart"] == null)
            {
                cart = new List<int>();
            }
            else
            {
                cart = (List<int>)Session["Cart"];
            }

            cart.Add(medicine.MedicineID);
            Session["Cart"] = cart;

            return new HttpStatusCodeResult(System.Net.HttpStatusCode.OK);
        }

        // GET
        public ActionResult ViewCart()
        {
            List<Medicine> cartMedicines = new List<Medicine>();
            if (Session["Cart"] != null)
            {
                List<int> cart = (List<int>)Session["Cart"];
                cartMedicines = db.Medicines.Where(m => cart.Contains(m.MedicineID)).ToList();
            }

            return View(cartMedicines);
        }
















        /*        // GET
                public ActionResult ViewCart()
                {
                    return View(new List<Medicine>());
                }

                //POST
                [HttpPost]
                [ValidateAntiForgeryToken]
                public ActionResult ViewCart(int medicineId)
                {
                    var medicine = db.Medicines.Find(medicineId);

                    if (medicine == null)
                    {
                        return HttpNotFound();
                    }

                    List<Medicine> cartMedicines = new List<Medicine> { medicine };

                    return View(cartMedicines);
                }*/










    }
}
