using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using Pharma.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System;

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
            var viewModel = new SellerDashboardViewModel
            {
                TotalUsers = db.Customers.Count(),
                TotalMedicines = db.Medicines.Count(),
                TotalOrders = 50,
                LatestMedicines = db.Medicines.OrderByDescending(m => m.MedicineID).Take(10).ToList()
            };
            return View(viewModel);
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
                    MedicineName = medicine.Name,
                    Quantity = quantity,
                    Price = medicine.Price,
                    /*Total = quantity * medicine.Price*/
                };

                // Store the item in the session
                if (Session["Cart"] == null)
                {
                    Session["Cart"] = new List<OrderItem>();
                }
                var cart = (List<OrderItem>)Session["Cart"];
                cart.Add(item);

                return Json(new { success = true });
            }
            else
            {
                // Handle the case where the medicine is not available or quantity is insufficient
                return Json(new { success = false, message = "The requested quantity is not available." });
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
        [ValidateAntiForgeryToken]
        public ActionResult PrintReceipt()
        {
            var cart = Session["Cart"] as List<OrderItem>;
            if (cart != null && cart.Count > 0)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    Document pdfDoc = new Document(PageSize.A4, 25f, 25f, 30f, 30f);
                    PdfWriter.GetInstance(pdfDoc, stream).CloseStream = false;
                    pdfDoc.Open();

                    // Add store details
                    var titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD);
                    var regularFont = FontFactory.GetFont("Arial", 12, Font.NORMAL);
                    var boldFont = FontFactory.GetFont("Arial", 12, Font.BOLD);

                    pdfDoc.Add(new Paragraph("Pharma Store", titleFont) { Alignment = Element.ALIGN_CENTER });
                    pdfDoc.Add(new Paragraph("123 Pharmacy St., Health City", regularFont) { Alignment = Element.ALIGN_CENTER });
                    pdfDoc.Add(new Paragraph("Phone: (123) 456-7890", regularFont) { Alignment = Element.ALIGN_CENTER });
                    pdfDoc.Add(new Paragraph(" ", regularFont));

                    // Add receipt details
                    pdfDoc.Add(new Paragraph("Receipt", titleFont) { Alignment = Element.ALIGN_CENTER });
                    pdfDoc.Add(new Paragraph(" ", regularFont));
                    pdfDoc.Add(new Paragraph("Date: " + DateTime.Now.ToString("MM/dd/yyyy"), regularFont) { Alignment = Element.ALIGN_RIGHT });
                    pdfDoc.Add(new Paragraph("Time: " + DateTime.Now.ToString("HH:mm:ss"), regularFont) { Alignment = Element.ALIGN_RIGHT });
                    pdfDoc.Add(new Paragraph(" "));

                    // Add table
                    PdfPTable table = new PdfPTable(4);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 3f, 1f, 2f, 2f });

                    // Add table header
                    PdfPCell cell = new PdfPCell(new Phrase("Medicine Name", boldFont));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 5;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase("Quantity", boldFont));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 5;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase("Price", boldFont));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 5;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase("Total", boldFont));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 5;
                    table.AddCell(cell);

                    // Add table rows
                    foreach (var item in cart)
                    {
                        table.AddCell(new PdfPCell(new Phrase(item.MedicineName, regularFont)) { Padding = 5 });
                        table.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString(), regularFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                        table.AddCell(new PdfPCell(new Phrase(item.Price.ToString("C"), regularFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                        table.AddCell(new PdfPCell(new Phrase(item.Total.ToString("C"), regularFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });

                        // Update medicine quantity in the database
                        var medicine = db.Medicines.Find(item.MedicineID);
                        if (medicine != null)
                        {
                            medicine.Quantity -= item.Quantity;
                        }
                    }

                    // Save changes to the database
                    db.SaveChanges();

                    // Add total row
                    PdfPCell emptyCell = new PdfPCell(new Phrase(""));
                    emptyCell.Border = PdfPCell.NO_BORDER;
                    table.AddCell(emptyCell);
                    table.AddCell(emptyCell);
                    cell = new PdfPCell(new Phrase("Grand Total", boldFont));
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cell.Padding = 5;
                    table.AddCell(cell);

                    decimal grandTotal = cart.Sum(i => i.Total);
                    cell = new PdfPCell(new Phrase(grandTotal.ToString("C"), boldFont));
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cell.Padding = 5;
                    table.AddCell(cell);

                    pdfDoc.Add(table);
                    pdfDoc.Close();

                    byte[] bytes = stream.ToArray();
                    stream.Close();

                    // Clear the cart after printing
                    Session["Cart"] = null;

                    return File(bytes, "application/pdf", "Receipt.pdf");
                }
            }
            else
            {
                TempData["Error"] = "No items in the cart to print.";
                return RedirectToAction("ViewCart");
            }
        }
    }
}
