using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using Pharma.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System;

using System.Data.Entity;

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
                TotalOrders = db.Orders.Count(), // Updated to get actual order count
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
                    MedicineName = medicine.MedicineName,
                    Quantity = quantity,
                    Price = medicine.Price,
                    /*Total = quantity * medicine.Price*/ // Ensure Total is calculated
                };

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
                return Json(new { success = false, message = "The requested quantity is not available." });
            }
        }

        // GET: Seller/ViewCart
        public ActionResult ViewCart()
        {
            var cart = Session["Cart"] as List<OrderItem> ?? new List<OrderItem>();
            return View(cart);
        }

        /*public ActionResult ViewReceipts()
        {
            var receipts = db.Orders.Include(o => o.OrderDetails.Select(od => od.Medicine)).ToList();
            return View(receipts);
        }*/

        public ActionResult ViewReceipts()
        {
            var receipts = db.Receipts.Include(r => r.ReceiptItems).ToList();
            return View(receipts);
        }


        // POST: Seller/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(int medicineId, int quantity)
        {
            var cart = Session["Cart"] as List<OrderItem>;
            if (cart != null)
            {
                var item = cart.FirstOrDefault(i => i.MedicineID == medicineId);
                if (item != null)
                {
                    var medicine = db.Medicines.Find(medicineId);
                    if (medicine != null && medicine.Quantity >= quantity)
                    {
                        item.Quantity = quantity;
                        /*item.Total = item.Quantity * item.Price;*/ // Ensure Total is updated
                        return RedirectToAction("ViewCart");
                    }
                    else
                    {
                        TempData["Error"] = "The requested quantity is not available.";
                    }
                }
            }
            return RedirectToAction("ViewCart");
        }

        // POST: Seller/DeleteItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteItem(int medicineId)
        {
            var cart = Session["Cart"] as List<OrderItem>;
            if (cart != null)
            {
                var item = cart.FirstOrDefault(i => i.MedicineID == medicineId);
                if (item != null)
                {
                    cart.Remove(item);
                }
            }
            return RedirectToAction("ViewCart");
        }

        // POST: Seller/PrintReceipt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PrintReceipt(string buyerName, string buyerPhone)
        {
            if (Session["SellerID"] == null)
            {
                TempData["Error"] = "Seller ID is not set. Please log in again.";
                return RedirectToAction("ViewCart");
            }

            var sellerId = (int)Session["SellerID"];
            var cart = Session["Cart"] as List<OrderItem>;

            if (cart != null && cart.Count > 0)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var receipt = new Receipt
                        {
                            DateCreated = DateTime.Now,
                            TotalAmount = cart.Sum(item => item.Total),
                            BuyerName = buyerName,
                            BuyerPhone = buyerPhone,
                            SellerID = sellerId
                        };

                        db.Receipts.Add(receipt);
                        db.SaveChanges();

                        foreach (var item in cart)
                        {
                            var receiptItem = new ReceiptItem
                            {
                                ReceiptID = receipt.ReceiptID,
                                MedicineID = item.MedicineID,
                                MedicineName = item.MedicineName,
                                Quantity = item.Quantity,
                                Price = item.Price,
                                Total = item.Total
                            };
                            db.ReceiptItems.Add(receiptItem);

                            var medicine = db.Medicines.Find(item.MedicineID);
                            if (medicine != null)
                            {
                                medicine.Quantity -= item.Quantity;
                            }
                        }
                        db.SaveChanges();

                        transaction.Commit();

                        using (MemoryStream stream = new MemoryStream())
                        {
                            Document pdfDoc = new Document(PageSize.A4, 25f, 25f, 30f, 30f);
                            PdfWriter.GetInstance(pdfDoc, stream).CloseStream = false;
                            pdfDoc.Open();

                            var titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD);
                            var regularFont = FontFactory.GetFont("Arial", 12, Font.NORMAL);
                            var boldFont = FontFactory.GetFont("Arial", 12, Font.BOLD);

                            pdfDoc.Add(new Paragraph("Pharma Store", titleFont) { Alignment = Element.ALIGN_CENTER });
                            pdfDoc.Add(new Paragraph("123 Pharmacy St., Health City", regularFont) { Alignment = Element.ALIGN_CENTER });
                            pdfDoc.Add(new Paragraph("Phone: (123) 456-7890", regularFont) { Alignment = Element.ALIGN_CENTER });
                            pdfDoc.Add(new Paragraph(" ", regularFont));

                            pdfDoc.Add(new Paragraph("Receipt", titleFont) { Alignment = Element.ALIGN_CENTER });
                            pdfDoc.Add(new Paragraph(" ", regularFont));
                            pdfDoc.Add(new Paragraph("Date: " + DateTime.Now.ToString("MM/dd/yyyy"), regularFont) { Alignment = Element.ALIGN_RIGHT });
                            pdfDoc.Add(new Paragraph("Time: " + DateTime.Now.ToString("HH:mm:ss"), regularFont) { Alignment = Element.ALIGN_RIGHT });
                            pdfDoc.Add(new Paragraph(" "));

                            pdfDoc.Add(new Paragraph($"Buyer: {buyerName}", boldFont));
                            pdfDoc.Add(new Paragraph($"Phone: {buyerPhone}", boldFont));
                            pdfDoc.Add(new Paragraph($"Seller ID: {sellerId}", boldFont));
                            pdfDoc.Add(new Paragraph(" "));

                            PdfPTable table = new PdfPTable(4);
                            table.WidthPercentage = 100;
                            table.SetWidths(new float[] { 3f, 1f, 2f, 2f });

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

                            foreach (var item in cart)
                            {
                                table.AddCell(new PdfPCell(new Phrase(item.MedicineName, regularFont)) { Padding = 5 });
                                table.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString(), regularFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                                table.AddCell(new PdfPCell(new Phrase(item.Price.ToString("C"), regularFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                                table.AddCell(new PdfPCell(new Phrase(item.Total.ToString("C"), regularFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                            }

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

                            Session["Cart"] = null;

                            return File(bytes, "application/pdf", "Receipt.pdf");
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        var errorMessage = $"An error occurred while generating the receipt: {ex.Message}";
                        if (ex.InnerException != null)
                        {
                            errorMessage += $" Inner Exception: {ex.InnerException.Message}";
                            if (ex.InnerException.InnerException != null)
                            {
                                errorMessage += $" Inner Inner Exception: {ex.InnerException.InnerException.Message}";
                            }
                        }
                        TempData["Error"] = errorMessage;
                        return RedirectToAction("ViewCart");
                    }
                }
            }
            else
            {
                TempData["Error"] = "No items in the cart to print.";
                return RedirectToAction("ViewCart");
            }
        }




        // GET: Seller/Orders
        public ActionResult Orders()
        {
            var orders = db.Orders.Include("Customer").Include("OrderDetails.Medicine").Where(o => o.Status == OrderStatus.Pending).ToList();
            return View(orders);
        }

        // POST: Seller/UpdateOrderStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOrderStatus(int orderId)
        {
            var order = db.Orders.Find(orderId);
            if (order != null)
            {
                if (Session["SellerID"] != null)
                {
                    int sellerId = Convert.ToInt32(Session["SellerID"]);
                    order.SellerID = sellerId;
                    order.Status = OrderStatus.OutForDelivery;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Order status updated successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Seller ID not found in session.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Order not found.";
            }
            return RedirectToAction("Orders");
        }



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
