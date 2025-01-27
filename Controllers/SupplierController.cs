using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LogiManage.Models;
using LogiManage.ViewModels;
using System.Web.UI.WebControls;


namespace LogiManage.Controllers
{
    public class SupplierController : Controller
    {
        LogiManageDbEntities logidb = new LogiManageDbEntities();
        // GET: Supplier
        [HttpGet]
        public ActionResult Suppliers()
        {
            var suppliers = logidb.Suppliers.Select(s => new LogiManage.ViewModels.SupplierViewModel
            {
                SupplierID = s.SupplierID,
                SupplierName = s.SupplierName,
                ContactName = s.ContactName,
                SupplierAddress = s.SupplierAddress,
                SupplierPhone = s.SupplierPhone,
                SupplierMail = s.SupplierMail
            }).ToList();

            return View(suppliers);
        }
        [HttpGet]
        public ActionResult SupplierUpdate(int supplierID)
        {
            var supplier = logidb.Suppliers.FirstOrDefault(s => s.SupplierID == supplierID);
            if (supplier == null)
                return HttpNotFound("Tedarikçi bulunamadı.");

            return View(supplier);
        }

        [HttpPost]
        public ActionResult SupplierUpdate(int supplierID, Suppliers updatedSupplier)
        {
            var supplier = logidb.Suppliers.FirstOrDefault(s => s.SupplierID == supplierID);
            if (supplier == null)
            {
                return HttpNotFound("Supplier not found");
            }
            supplier.SupplierID = supplierID;
            supplier.SupplierName = updatedSupplier.SupplierName;
            supplier.ContactName = updatedSupplier.ContactName;
            supplier.SupplierAddress = updatedSupplier.SupplierAddress;
            supplier.SupplierPhone = updatedSupplier.SupplierPhone;
            supplier.SupplierMail = updatedSupplier.SupplierMail;

            logidb.SaveChanges();
            return RedirectToAction("Suppliers", new { message = "Tedarikçi başarıyla güncellendi." });
        }

        public ActionResult SupplierDelete(int supplierID)
        {
            var relatedOrdersCount = logidb.Orders.Count(o => o.SupplierID == supplierID);

            if (relatedOrdersCount > 0)
            {
                TempData["ErrorMessage"] = "Bu tedarikçi ile ilişkili siparişler olduğu için silinemiyor.";
                return RedirectToAction("Suppliers");
            }

            var supplier = logidb.Suppliers.FirstOrDefault(s => s.SupplierID == supplierID);
            if (supplier != null)
            {
                logidb.Suppliers.Remove(supplier);
                logidb.SaveChanges();
                TempData["SuccessMessage"] = "Tedarikçi başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Tedarikçi silinirken bir hata oluştu.";
            }

            return RedirectToAction("Suppliers");
        }

        [HttpGet]
        public ActionResult SupplierAdd()
        {
            return View();

        }

        [HttpPost]
        public ActionResult SupplierAdd(SupplierViewModel newsupplier)
        {
           
            if (ModelState.IsValid)
            {
               
                    logidb.Suppliers.Add(
                    new Models.Suppliers()
                    {
                        SupplierName = newsupplier.SupplierName,
                        ContactName = newsupplier.ContactName,
                        SupplierPhone = newsupplier.SupplierPhone,
                        SupplierMail = newsupplier.SupplierMail,
                        SupplierAddress = newsupplier.SupplierAddress                         
                    });
                    
                    logidb.SaveChanges();
                return RedirectToAction("Suppliers", new { message = "Tedarikçi başarıyla eklendi." });
            }
           
            return View(newsupplier);
        }
    }
}