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
    public class ProductsController : Controller
    {
       
        LogiManageDbEntities1 logidb= new LogiManageDbEntities1();
        // GET: Products
        [HttpGet]
        public ActionResult ProductAdd()
        {
            ViewBag.CategoryList = new SelectList(logidb.Products.Select(p => p.Category).Distinct().ToList());
            ViewBag.SupplierList = new SelectList(logidb.Suppliers, "SupplierID", "SupplierName");
            return View();
        }
        [HttpPost]
        public ActionResult ProductAdd(AddProductViewModel products)
        {
                var newProduct = new Products
                {
                    ProductName = products.ProductName,
                    Category = products.Category,
                    Price = products.Price,
                    CriticalStockLevel = products.CriticalStockLevel,
                    SupplierID = products.SupplierID,
                    StockQuantity = products.StockQuantity
                };

                logidb.Products.Add(newProduct);
                logidb.SaveChanges();
                return RedirectToAction("Index");
        }
        
        [HttpPost]
        public ActionResult AddProduct(int warehouseID, int productId, int quantity)
        {
            var stockexist = logidb.WarehouseStocks
                .FirstOrDefault(ws => ws.WarehouseID == warehouseID && ws.ProductID == productId);

            if (stockexist != null)
            {
                stockexist.Quantity += quantity;
            }
            else
            {
                var newStock = new WarehouseStocks
                {
                    WarehouseID = warehouseID,
                    Quantity = quantity
                };
                logidb.WarehouseStocks.Add(newStock);
            }

            logidb.SaveChanges();
            return RedirectToAction("WarehouseControl", new { warehouseID });
        }
        [HttpPost]
        public ActionResult DeleteProduct(int warehouseID, int productId)
        {

            var stock = logidb.WarehouseStocks
                .FirstOrDefault(ws => ws.WarehouseID == warehouseID && ws.ProductID == productId);
            if (stock != null)
            {
                logidb.WarehouseStocks.Remove(stock);
                logidb.SaveChanges();
            }

            return RedirectToAction("WarehouseControl", new { warehouseID });
        }
    }
}