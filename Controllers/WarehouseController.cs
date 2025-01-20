using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LogiManage.Models;
using LogiManage.ViewModels;
namespace LogiManage.Controllers
{
  
        public class WarehouseController : Controller
        {
            // GET: WarehouseManager

            readonly LogiManageDbEntities entity = new LogiManageDbEntities();

            [HttpGet]
            public ActionResult Index(int? warehouseId)
            {
                ViewBag.Warehouses = entity.Warehouses.ToList();
                if (warehouseId == null && entity.Warehouses.Any())
                    warehouseId = entity.Warehouses.First().WarehouseID;

                var productsInWarehouse = entity.WarehouseStocks
                    .Where(ws => ws.WarehouseID == warehouseId)
                    .Select(ws => new LogiManage.ViewModels.WarehouseProductViewModel
                    {
                        ProductID =ws.Products.ProductID,
                        ProductName=ws.Products.ProductName,
                       Category= ws.Products.Category,
                        Price = (int)ws.Products.Price,
                       Quantity= (int)ws.Quantity,
                      CriticalStockLevel= (int)ws.Products.CriticalStockLevel

                    }).ToList();
                ViewBag.SelectedWarehouseId = warehouseId;
            ViewBag.Products = entity.Products.ToList();

                return View(productsInWarehouse);
            }
            [HttpPost]
            public ActionResult StockUpdate(int warehouseID,int productId,int quantityChange)
            {
                var stock = entity.WarehouseStocks
                    .FirstOrDefault(ws => ws.WarehouseID == warehouseID && ws.ProductID == productId);
                if(stock == null)
                {
                    stock.Quantity += quantityChange;
                    if(stock.Quantity < 0)
                    {
                        stock.Quantity = 0;
                        entity.SaveChanges();

                    }
                }

                    return RedirectToAction("Index",new {warehouseID});
            }
            [HttpPost]
            public ActionResult AddProduct(int warehouseID, int productId, int quantity)
            {   
               var  stockexist = entity.WarehouseStocks
                    .FirstOrDefault(ws => ws.WarehouseID == warehouseID && ws.ProductID == productId);
                if (stockexist!= null)
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
                    entity.WarehouseStocks.Add(newStock);
                }
                entity.SaveChanges();
                return RedirectToAction("Index", new {warehouseID});
            }
            [HttpPost]
            public ActionResult DeleteProduct(int warehouseID, int productId)
            {
                /*var warehouse = (from w in entity.Warehouses
                                 join ws in entity.WarehouseStocks on w.WarehouseID equals ws.WarehouseID
                                 select new { w, ws }).ToList();
                */
                var stock = entity.WarehouseStocks
                    .FirstOrDefault(ws => ws.WarehouseID == warehouseID && ws.ProductID == productId);
                if (stock != null)
                { entity.WarehouseStocks.Remove(stock);
                }
                entity.SaveChanges();
                return RedirectToAction("Index", new { warehouseID});
            }
            public ActionResult Transfer() {  return View(); }
            public ActionResult WarehouseControl() { return View(); }
            

        }
    }
