using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using LogiManage.Models;
using LogiManage.ViewModels;
namespace LogiManage.Controllers
{
    public class OperatorController : Controller
    {
       
        // GET: Operator
        LogiManageDbEntities1 logidb = new LogiManageDbEntities1();

        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult StockControl()
        {
            //kendi depomuzda hangi ürünler var

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
        [HttpGet]
        public ActionResult OtherStockControl(int? productID)
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
        public ActionResult TransferRequestss()
        {
            return View();

        }
    }
}