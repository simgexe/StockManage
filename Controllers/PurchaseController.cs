using System;
using System.Data.Entity;

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LogiManage.Models;
using LogiManage.ViewModels;
using Microsoft.Ajax.Utilities;
using System.Runtime.Remoting.Contexts;
using System.Data.SqlClient;

namespace LogiManage.Controllers
{
    public class PurchaseController : Controller
    {
        LogiManageDbEntities logidb = new LogiManageDbEntities();
        // GET: Purchasing
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CreateOrder()
        {
            return View();
        }
        public ActionResult ViewOrder(int? OrderID)
        {
            SqlConnection connection = new SqlConnection("Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True");
            const string querystring = @"Select o.OrderID,
                                                    o.OrderDate,
                                                    o.OrderStatus,
                                                    o.SupplierID,
                                                    s.SupplierName,
                                                    o.WarehouseID,
                                                    w.WarehouseName,
                                                    od.OrderDetailID,
                                                    od.ProductID,
                                                    p. ProductName,
                                                    od. Quantity,
                                                    od.UnitPrice
                 from Orders o 
                join Suppliers s on o.SupplierID = s.SupplierID
                join Warehouses w on o.WarehouseID= w.WarehouseID 
                join OrderDetails od on o.OrderID=od.OrderID
                join Products p on od.ProductID = p.ProductID";


            SqlCommand command = new SqlCommand(querystring, connection);
            connection.Open();
            SqlDataReader dr = command.ExecuteReader();

            var orderDetails = new List<ViewOrderViewModel>();

            while (dr.Read())
            {
                var detail = new ViewOrderViewModel
                {
                    OrderID = dr.GetInt32(0),
                    OrderDate = dr.GetDateTime(1),
                    OrderStatus = dr.GetString(2),
                    SupplierID = dr.GetInt32(3),
                    SupplierName = dr.GetString(4),
                    WarehouseID = dr.GetInt32(5),
                    WarehouseName = dr.GetString(6),
                    OrderDetailID = dr.GetInt32(7),
                    ProductID = dr.GetInt32(8),
                    ProductName = dr.GetString(9),
                    Quantity = dr.IsDBNull(10) ? 0 : Convert.ToSingle(dr[10]),   
                    UnitPrice = dr.IsDBNull(11) ? 0 : dr.GetDecimal(11)          

                };
                orderDetails.Add(detail);
            }
            connection.Close();

            return View(orderDetails);
        }

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
        public ActionResult SupplierAdd(Suppliers suppliers)
        {
            foreach (var key in Request.Form.AllKeys)
            {
                Console.WriteLine($"{key}: {Request.Form[key]}");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    logidb.Suppliers.Add(suppliers);
                    TempData["SuccessMessage"] = "Tedarikçi başarıyla eklendi.";
                    logidb.SaveChanges();
                    return RedirectToAction("Suppliers");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Bir hata oluştu: " + ex.Message);
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Form doğrulama hatası.";
            }
            ViewBag.Message = "Tedarikçi eklenirken bir hata oluştu.";
            return View(suppliers);
        }

    }
}
