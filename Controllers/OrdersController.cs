using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LogiManage.ViewModels;
using System.Web.UI.WebControls;
using LogiManage.Models;

namespace LogiManage.Controllers
{
    public class OrdersController : Controller
    {
        // Diğer birimlerdeki order işlemleri buraya taşınacak.

        // GET: Orders
        LogiManageDbEntities1 logidb = new LogiManageDbEntities1();

        [HttpGet]
        public ActionResult CreateOrder()
        {
            ViewBag.ProductList = new SelectList(logidb.Products, "ProductID", "ProductName");
            ViewBag.CategoryList = new SelectList(
            logidb.Products
                 .Select(p => new { Category = p.Category })
                 .Distinct()
                 .ToList(), "Category",  "Category" );
            var model = new ViewOrderViewModel();
            ViewBag.Price = logidb.Products
                .Where(p => p.ProductID == model.ProductID)
                .Select(p => p.Price)
                .FirstOrDefault();
            ViewBag.SupplierList = new SelectList(logidb.Suppliers, "SupplierID", "SupplierName");
            ViewBag.WarehouseList = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName");


            return View(new ViewOrderViewModel() { OrderDate = DateTime.Now, OrderStatus = "Ordered" });
        }

        [HttpPost]
        public ActionResult CreateOrder(ViewOrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection("Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True"))
                {
                    string getProductPriceQuery = @"SELECT Price FROM Products WHERE ProductID = @ProductID";

                    string insertOrderQuery = @"
                INSERT INTO Orders (OrderDate, OrderStatus, SupplierID, WarehouseID) 
                OUTPUT INSERTED.OrderID
                VALUES (@OrderDate, @OrderStatus, @SupplierID, @WarehouseID)";

                    string insertOrderDetailQuery = @"
                INSERT INTO OrderDetails (OrderID, ProductID, Quantity, UnitPrice) 
                VALUES (@OrderID, @ProductID, @Quantity, @UnitPrice)";

                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        // Ürün fiyatını al
                        SqlCommand getProductPriceCommand = new SqlCommand(getProductPriceQuery, connection, transaction);
                        getProductPriceCommand.Parameters.AddWithValue("@ProductID", model.ProductID);
                        decimal productPrice = (decimal)getProductPriceCommand.ExecuteScalar();

                        // UnitPrice hesapla
                        decimal unitPrice = productPrice * (decimal)model.Quantity;

                        // Orders tablosuna kayıt ekle
                        SqlCommand orderCommand = new SqlCommand(insertOrderQuery, connection, transaction);
                        orderCommand.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                        orderCommand.Parameters.AddWithValue("@OrderStatus", "Ordered");
                        orderCommand.Parameters.AddWithValue("@SupplierID", model.SupplierID);
                        orderCommand.Parameters.AddWithValue("@WarehouseID", model.WarehouseID);
                        int orderId = (int)orderCommand.ExecuteScalar();

                        // OrderDetails tablosuna kayıt ekle
                        SqlCommand orderDetailCommand = new SqlCommand(insertOrderDetailQuery, connection, transaction);
                        orderDetailCommand.Parameters.AddWithValue("@OrderID", orderId);
                        orderDetailCommand.Parameters.AddWithValue("@ProductID", model.ProductID);
                        orderDetailCommand.Parameters.AddWithValue("@Quantity", model.Quantity);
                        orderDetailCommand.Parameters.AddWithValue("@UnitPrice", unitPrice);
                        orderDetailCommand.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Order creation failed.");
                        return View(model);
                    }
                    finally
                    {
                        connection.Close();
                    }
                    return RedirectToAction("Index");
                }
            }

            return View(model);
        }
        public JsonResult GetProductsByCategory(string category)
        {
            var products = logidb.Products
                .Where(p => p.Category == category)
                .Select(p => new { p.ProductID, p.ProductName })
                .ToList();

            return Json(products, JsonRequestBehavior.AllowGet);
        }
       

        public ActionResult ViewOrder()
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
        public ActionResult RejectOrder(int orderRequestid)
        {
            var orderrequest = logidb.OrderRequests.FirstOrDefault(or => or.OrderRequestID == orderRequestid);
            if (orderrequest != null && orderrequest.OrderRequestStatus == "OrderRequested")
            {
                orderrequest.OrderRequestStatus = "OrderRejected";
                logidb.SaveChanges();
            }
            return RedirectToAction("OrderRequests");
        }

    }
}