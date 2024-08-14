using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security.Policy;
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
        public ActionResult UserRegister(Customer userReg)    ///      (Customer userReg) 
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
                    FullName = userReg.FullName,
                    Address = userReg.Address,
                    Phone = userReg.Phone,
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

        // GET: User/ViewMedicines
        public ActionResult ViewMedicines()
        {
            var medicines = db.Medicines.ToList(); // Load all medicines by default
            return View(medicines);
        }

        // POST: User/ViewMedicines
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ViewMedicines(string searchTerm)
        {
            var medicines = db.Medicines.AsQueryable();

            // Check if the searchTerm is received
            if (!string.IsNullOrEmpty(searchTerm))
            {
                medicines = medicines.Where(m => m.MedicineName.Contains(searchTerm) ||
                                                 m.GenericName.Contains(searchTerm) ||
                                                 m.Manufacturer.Contains(searchTerm) ||
                                                 m.Category.Contains(searchTerm));
            }

            // Return the filtered list of medicines to the view
            return View(medicines.ToList());
        }





        // POST: User/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(List<int> medicineIds, List<int> quantities)
        {
            if (medicineIds == null || !medicineIds.Any())
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "No items in the cart.");
            }

            var cartMedicines = db.Medicines.Where(m => medicineIds.Contains(m.MedicineID)).ToList();

            if (!cartMedicines.Any())
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "Invalid medicines in the cart.");
            }


            int customerId = (int)Session["UserID"];

            // Create a new order
            var order = new Order
            {
                CustomerID = customerId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                TotalPrice = 0, // Initial total price calculations
                OrderDetails = new List<OrderDetail>()
            };

            // Add order details for each medicine in the cart
            for (int i = 0; i < cartMedicines.Count && i < quantities.Count; i++)
            {
                var medicine = cartMedicines[i];
                var quantity = quantities[i];



                var orderDetail = new OrderDetail
                {
                    MedicineID = medicine.MedicineID,
                    Quantity = quantity,
                    Price = medicine.Price
                };
                order.OrderDetails.Add(orderDetail);
            }

            // Calculate the total price of the order
            order.TotalPrice = order.OrderDetails.Sum(od => od.Price * od.Quantity);

            // Save the order to the database
            db.Orders.Add(order);
            db.SaveChanges();

            // Clear the cart after placing the order
            Session["Cart"] = null;

            return RedirectToAction("ViewMedicines"); // Redirect to an order confirmation page
        }

        // Assuming you have a method to get the logged-in customer ID

        // Assuming you have a method to get the logged-in customer ID
        /*        private int GetLoggedInCustomerId()
                {
                    // Replace this with your actual logic to get the logged-in customer ID
                    return 1; // Placeholder
                }

        /*      public ActionResult PlaceOrder(int medicineId, int quantity)
              {
                  var medicine = db.Medicines.Find(medicineId);
                  if (medicine != null && medicine.Quantity >= quantity)
                  {
                      var order = new Order
                      {
                          CustomerID = (int)Session["UserID"],
                          //MedicineID = medicine.MedicineID,

                          OrderDate = DateTime.Now,
                          TotalPrice = medicine.Price * quantity
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
              }*/


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
            decimal totalPrice = 0;

            if (Session["Cart"] != null)
            {
                List<int> cart = (List<int>)Session["Cart"];
                cartMedicines = db.Medicines.Where(m => cart.Contains(m.MedicineID)).ToList();
                totalPrice = cartMedicines.Sum(m => m.Price);
            }
            ViewBag.TotalPrice = totalPrice;
            return View(cartMedicines);
        }

        [HttpPost]
        public ActionResult RemoveFromCart(int id)
        {
            if (Session["Cart"] != null)
            {
                List<int> cart = (List<int>)Session["Cart"];
                cart.Remove(id);
                if (cart.Count == 0)
                {
                    Session["Cart"] = null;
                }
                else
                {
                    Session["Cart"] = cart;
                }
            }

            return Json(new { success = true });
        }


        public new ActionResult Profile()
        {
            // Get the customer ID from the session
            int customerId = (int)Session["UserID"];

            // Fetch the customer details from the database
            var customerDetails = db.Customers.FirstOrDefault(c => c.CustomerID == customerId);

            // Check if customer exists
/*            if (customerDetails == null)
            {
                // Handle the case where the customer is not found
                return HttpNotFound("Customer not found");
            }*/

            // Pass the customer details to the view
            return View(customerDetails);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProfileChange(Customer profcng)
        {
            if (ModelState.IsValid)
            {
                int customerId = (int)Session["UserID"];

                // Find the customer in the database
                var customer = db.Customers.SingleOrDefault(c => c.CustomerID == customerId);

                if (customer != null)
                {
                    customer.FullName = !string.IsNullOrEmpty(profcng.FullName) ? profcng.FullName : customer.FullName;
                    customer.Address = !string.IsNullOrEmpty(profcng.Address) ? profcng.Address : customer.Address;
                    customer.Phone = !string.IsNullOrEmpty(profcng.Phone) ? profcng.Phone : customer.Phone;

                    db.SaveChanges();
                }
            }

            return RedirectToAction("Profile");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PassChange(Customer passcng)    ///      (Customer userReg) 
        {
            if (ModelState.IsValid)
            {

                int customerId = (int)Session["UserID"];

                var customer = db.Customers.SingleOrDefault(c => c.CustomerID == customerId);

                if (customer != null)
                {
                    customer.Password = !string.IsNullOrEmpty(passcng.Password) ? passcng.Password : customer.Password;

                    db.SaveChanges();
                }
            }

            return RedirectToAction("Profile");
        }



        /*        public ActionResult OrderHistory()
                {
                    int customerId = (int)Session["UserID"];

                    var orderMedicines = db.Orders.Where(c => c.CustomerID == customerId).Include(o => o.OrderDetails) .ToList();

                    if (orderMedicines == null || !orderMedicines.Any())
                    {
                        return HttpNotFound("No orders placed yet");
                    }

                    return View(orderMedicines);
                }*/



        [HttpGet]
        public ActionResult OrderHistory()
        {
            int customerId = (int)Session["UserID"];
            var orderMedicines = db.Orders.Where(c => c.CustomerID == customerId).Include(o => o.OrderDetails).ToList();

            return View(orderMedicines);
        }
        [HttpPost]
        public ActionResult OrderHistory(string search)
        {
            int customerId = (int)Session["UserID"];
            OrderStatus status;

            // Check if the search string can be parsed into an OrderStatus enum value
            bool isValidStatus = Enum.TryParse(search, true, out status);

            // Convert the search string to DateTime, assuming the format is 'M/d/yyyy'
            DateTime searchDate;
            bool isValidDate = DateTime.TryParseExact(search, "M/d/yyyy", null, System.Globalization.DateTimeStyles.None, out searchDate);

            var orderMedicines = db.Orders
                                   .Where(c => c.CustomerID == customerId &&
                                               (c.OrderID.ToString().Contains(search) ||
                                               (isValidStatus && c.Status == status) ||
                                               (isValidDate && c.OrderDate.Date == searchDate.Date) ||
                                               c.TotalPrice.ToString().Contains(search)))
                                   .Include(o => o.OrderDetails)
                                   .ToList();

            return View(orderMedicines);
        }









        public ActionResult UserContact()
        {
            return View();
        }

        public ActionResult UserAbout()
        {
            return View();
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
