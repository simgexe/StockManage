using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
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

        LogiManageDbEntities1 logidb = new LogiManageDbEntities1();



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
        public ActionResult WarehouseSControl(int? productID)
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
        public ActionResult Transfers()
        {
            SqlConnection connection = new SqlConnection("Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True");
            const string querystring = @"Select wt.TransferID,
                                                    wt.TransferDate,
                                                   wt.TransferStatus,
                                                    wt.SourceWarehouseID,
                                                    sw.WarehouseName as SourceWarehouseName,
                                                    wt.DestinationWarehouseID,
                                                    dw.WarehouseName as DestinationWarehouseName,
                                                    wt.ProductID,
                                                    p.ProductName,
                                                    wt.Quantity
                 from WarehouseTransfers wt 
                join Warehouses sw on wt.SourceWarehouseID = sw.WarehouseID
                join Warehouses dw on wt.DestinationWarehouseID = dw.WarehouseID
                join Products p on wt.ProductID = p.ProductID
                where wt.TransferStatus = 'Completed'";
            SqlCommand command = new SqlCommand(querystring, connection);
            connection.Open();
            SqlDataReader dr = command.ExecuteReader();
            var transfers = new List<TransferViewModel>();
            while (dr.Read())
            {
                var transfer = new TransferViewModel
                {
                    TransferID = (int)dr["TransferID"],
                    TransferDate = (DateTime)dr["TransferDate"],
                    TransferStatus = (string)dr["TransferStatus"],
                    SourceWarehouseID = (int)dr["SourceWarehouseID"],
                    SourceWarehouseName = (string)dr["SourceWarehouseName"],
                    DestinationWarehouseID = (int)dr["DestinationWarehouseID"],
                    DestinationWarehouseName = (string)dr["DestinationWarehouseName"],
                    ProductID = (int)dr["ProductID"],
                    ProductName = (string)dr["ProductName"],
                    Quantity = (int)dr["Quantity"]
                };
                transfers.Add(transfer);
            }
            connection.Close();

            return View(transfers);
        }
        public ActionResult TransferRequests()
        {

            return View();
        }
        public ActionResult PurchaseRequests()
        {
            return View();
        }
        

    }
}

         
        