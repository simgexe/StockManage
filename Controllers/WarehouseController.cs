using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Mvc;
using LogiManage.Models;
using LogiManage.ViewModels;
namespace LogiManage.Controllers
{
    
    public class WarehouseController : Controller
    {
        // GET: WarehouseManager

        LogiManageDbEntities logidb = new LogiManageDbEntities();
        
            

            public ActionResult Index()
            {
             
               return View();
            }
            [HttpPost]
            public ActionResult StockUpdate(int warehouseID, int productId, int quantityChange)
            {
                var stock = logidb.WarehouseStocks
                    .FirstOrDefault(ws => ws.WarehouseID == warehouseID && ws.ProductID == productId);
                if (stock != null)
                {
                    stock.Quantity += quantityChange;
                    if (stock.Quantity < 0)
                        stock.Quantity = 0;

                        logidb.SaveChanges();

                    
                }

                return RedirectToAction("WarehouseControl", new { warehouseID });
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
                    ProductID = productId,
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
       [HttpGet]
       public ActionResult WarehouseSControl(int? productID )
        {
            
           
            

            var productsInWarehouses = logidb.WarehouseStocks
                .Where(ws => ws.ProductID == productID)
                .Select(ws => new LogiManage.ViewModels.WarehouseProductViewModel
                {
                    ProductID = ws.Products.ProductID,
                    ProductName = ws.Products.ProductName,
                    Category = ws.Products.Category,
                    Price = (int)ws.Products.Price,
                    Quantity = (int)ws.Quantity,
                    CriticalStockLevel = (int)ws.Products.CriticalStockLevel,
                    WarehouseName = ws.Warehouses.WarehouseName

                }).ToList();
            ViewBag.Products = logidb.Products.ToList();
            ViewBag.SelectedProductId = productID;
            

            return View(productsInWarehouses);
        }
        public ActionResult Transfers()
        {
            return View();
        }
        public ActionResult TransferRequests()
        {
            return View();
        }
        public ActionResult PurchaseRequests()
        {
            return View();
        }
        [HttpGet]
        public ActionResult WarehouseControl()
        {
            var warehouseID = (int?)Session["WarehouseID"];
            var warehouse = logidb.Warehouses.Where(w => w.WarehouseID == warehouseID);

            var productsInWarehouse = logidb.WarehouseStocks
                .Where(ws => ws.WarehouseID == warehouseID)
                .Select(ws => new LogiManage.ViewModels.WarehouseProductViewModel
                {
                    ProductID = ws.Products.ProductID,
                    ProductName = ws.Products.ProductName,
                    Category = ws.Products.Category,
                    Price = (int)ws.Products.Price,
                    Quantity = (int)ws.Quantity,
                    CriticalStockLevel = (int)ws.Products.CriticalStockLevel
                }).ToList();
  

            return View(productsInWarehouse);
        }
    } 
}

         
        