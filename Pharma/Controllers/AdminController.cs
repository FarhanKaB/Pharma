using Pharma.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

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
                    return RedirectToAction("Dashboard", "Admin");
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
            return RedirectToAction("Login", "Admin");
        }

        ///////////////////////////////////////////////////////////////////
        ///

        // GET: Admin/Index
        //[Authorize] // Ensure only logged-in admin can access
        public ActionResult Index()
        {
            var medicines = db.Medicines.ToList();
            return View(medicines);
        }

        // GET: Admin/CreateMedicine
        public ActionResult CreateMedicine()
        {
            return View();
        }

        // POST: Admin/CreateMedicine
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMedicine(Medicine medicine)
        {
            if (ModelState.IsValid)
            {
                db.Medicines.Add(medicine);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(medicine);
        }

        // GET: Admin/EditMedicine/5
        public ActionResult EditMedicine(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Medicine medicine = db.Medicines.Find(id);
            if (medicine == null)
            {
                return HttpNotFound();
            }
            return View(medicine);
        }

        // POST: Admin/EditMedicine/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMedicine(Medicine medicine)
        {
            if (ModelState.IsValid)
            {
                db.Entry(medicine).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(medicine);
        }

        // GET: Admin/DeleteMedicine/5
        public ActionResult DeleteMedicine(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Medicine medicine = db.Medicines.Find(id);
            if (medicine == null)
            {
                return HttpNotFound();
            }
            return View(medicine);
        }

        // POST: Admin/DeleteMedicine/5
        [HttpPost, ActionName("DeleteMedicine")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Medicine medicine = db.Medicines.Find(id);
            db.Medicines.Remove(medicine);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Admin/GenerateReport
        public ActionResult GenerateReport()
        {
            var medicines = db.Medicines.ToList();
            // This is a simple example; more complex reporting may require external libraries
            return View(medicines);
        }


        public ActionResult Dashboard()
        {
            return View();
        }


    }
}
