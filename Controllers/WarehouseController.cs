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
        /*.....Stok güncelleme....
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
        }*/

        //...giriş yapılan depodaki ürün miktarlarını kontrol etme 
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
       //....diğer depolardaki ürün miktarlarını kontrol etme....
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

        // ...Sipariş İsteğinde bulunma...
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
        

        //...sipariş isteklerini görme
        public List<OrderRequestViewModel> GetOrderRequests()
        {
            var orderRequestslist = from or in logidb.OrderRequests
                                    join p in logidb.Products on or.ProductID equals p.ProductID
                                    join w in logidb.Warehouses on or.WarehouseID equals w.WarehouseID
                                    select new OrderRequestViewModel
                                    {
                                        OrderRequestID = or.OrderRequestID,
                                        OrderRequestStatus = or.OrderRequestStatus,
                                        ProductID= p.ProductID,
                                        ProductName=p.ProductName,
                                        WarehouseID=w.WarehouseID,
                                        WarehouseName=w.WarehouseName,
                                        RequestQuantity=or.RequestQuantity,
                                        OrderRequestDate=or.OrderRequestDate ??DateTime.Now,

                                    };
            return orderRequestslist.ToList();
        }
        public ActionResult PurchaseRequests() { return View( GetOrderRequests()); }


    }
}

         
        