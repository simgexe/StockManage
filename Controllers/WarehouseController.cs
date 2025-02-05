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

        // ...
        [HttpGet]
        
        public ActionResult AddOrderRequest()
            {
            var productList = logidb.Products.Select(p => new SelectListItem
            {
                Value = p.ProductID.ToString(),
                Text = p.ProductName
            }).ToList();

            ViewBag.ProductList = new SelectList(productList, "Value", "Text");

            return View();
        }

       
        [HttpPost]
        public ActionResult AddOrderRequest(OrderRequestViewModel addOrderRequest)
        {
            addOrderRequest.WarehouseID = Convert.ToInt32(Session["WarehouseID"]);
            var insertquery = @"INSERT INTO OrderRequests(OrderRequestStatus, ProductID, WarehouseID,RequestQuantity, OrderRequestDate)
                               VALUES (@OrderRequestStatus, @ProductID, @WarehouseID, @RequestQuantity, @OrderRequestDate)  ";

            using (SqlConnection connection = new SqlConnection("Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True"))
            { connection.Open();
                using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(1) FROM Products WHERE ProductID = @ProductID", connection))
                {
                    checkCmd.Parameters.AddWithValue("@ProductID", addOrderRequest.ProductID);
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count == 0)
                    {
                        // Handle the case where the ProductID does not exist
                        ModelState.AddModelError("", "Invalid ProductID.");
                        return View(addOrderRequest);
                    }
                    using (SqlCommand cmd = new SqlCommand(insertquery, connection))
                    {
                       
                        cmd.Parameters.AddWithValue("@OrderRequestStatus", "OrderRequested");
                        cmd.Parameters.AddWithValue("@ProductID", addOrderRequest.ProductID);
                        cmd.Parameters.AddWithValue("@WarehouseID", addOrderRequest.WarehouseID);
                        cmd.Parameters.AddWithValue("@RequestQuantity", addOrderRequest.RequestQuantity);
                        cmd.Parameters.AddWithValue("@OrderRequestDate", DateTime.Now);
                        cmd.ExecuteNonQuery();

                    }
                }
                return RedirectToAction("PurchaseRequests");
            }
        }
        


        public ActionResult PurchaseRequests()
        {
            
            return View();
        }
        

    }
}

         
        