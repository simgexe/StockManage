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
                //bir listeden veri çekip eklediği için düzgün değil. giriş yapan kişinin yeni ürün ekleyebilmesi gerekiyor. 
                var stockexist =logidb.WarehouseStocks
                     .FirstOrDefault(ws => ws.WarehouseID == warehouseID && ws.ProductID == productId && ws.Quantity==quantity);
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
        public ActionResult WarehouseControl(int? warehouseId )
        {
            if (warehouseId == null && logidb.Warehouses.Any())
                warehouseId = logidb.Warehouses.First().WarehouseID;

            var productsInWarehouse = logidb.WarehouseStocks
                .Where(ws => ws.WarehouseID == warehouseId)
                .Select(ws => new LogiManage.ViewModels.WarehouseProductViewModel
                {
                    ProductID = ws.Products.ProductID,
                    ProductName = ws.Products.ProductName,
                    Category = ws.Products.Category,
                    Price = (int)ws.Products.Price,
                    Quantity = (int)ws.Quantity,
                    CriticalStockLevel = (int)ws.Products.CriticalStockLevel

                }).ToList();
            ViewBag.Warehouses = logidb.Warehouses.ToList();
            ViewBag.SelectedWarehouseId = warehouseId;
            ViewBag.Products = logidb.Products.ToList();

            return View(productsInWarehouse);
        }
        public ActionResult Transfer()
        {
            return View();
        }
      
         
    } 
}

         
        