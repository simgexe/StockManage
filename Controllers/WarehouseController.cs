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
        // ...

        public ActionResult Transfers()
        {
            var viewModel = new TransferListViewModel();
            SqlConnection connection = new SqlConnection("Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True");
            
            // Giden transferler için sorgu
            const string outgoingQuery = @"Select wt.TransferID,
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
                                    where wt.TransferStatus = 'Completed' and wt.SourceWarehouseID = @warehouseID";

            // Gelen transferler için sorgu
            const string incomingQuery = @"Select wt.TransferID,
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
                                    where wt.TransferStatus = 'Completed' and wt.DestinationWarehouseID = @warehouseID";

            connection.Open();
            
            SqlCommand outgoingCommand = new SqlCommand(outgoingQuery, connection);
            outgoingCommand.Parameters.AddWithValue("@warehouseId", (int)Session["WarehouseID"]);
            SqlDataReader outgoingDr = outgoingCommand.ExecuteReader();
            viewModel.Transfers = new List<TransferViewModel>();
            while (outgoingDr.Read())
            {
                viewModel.Transfers.Add(ReadTransfer(outgoingDr));
            }
            outgoingDr.Close();

          
            SqlCommand incomingCommand = new SqlCommand(incomingQuery, connection);
            incomingCommand.Parameters.AddWithValue("@warehouseId", (int)Session["WarehouseID"]);
            SqlDataReader incomingDr = incomingCommand.ExecuteReader();
            viewModel.OtherTransfers = new List<TransferViewModel>();
            while (incomingDr.Read())
            {
                viewModel.OtherTransfers.Add(ReadTransfer(incomingDr));
            }
            incomingDr.Close();
            
            connection.Close();

            return View(viewModel);
        }

        private TransferViewModel ReadTransfer(SqlDataReader dr)
        {
            return new TransferViewModel
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
        }

        public ActionResult TransferRequests()
        {
            var viewModel = new TransferListViewModel();
            SqlConnection connection = new SqlConnection("Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True");
            var request =  @"Select wt.TransferID,
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
                                    where wt.DestinationWarehouseID = @warehouseID";
            connection.Open();
            SqlCommand requestcommand = new SqlCommand(request,connection);
            requestcommand.Parameters.AddWithValue("@warehouseId", (int)Session["WarehouseID"]);
            SqlDataReader requestDr = requestcommand.ExecuteReader();

            viewModel.TransferRequests = new List<TransferViewModel>();
            while (requestDr.Read())
            {
                viewModel.TransferRequests.Add(ReadTransfer(requestDr));
            }
            requestDr.Close();

            connection.Close();

            return View(viewModel);
        }
        [HttpGet]
          public ActionResult AddTransferRequest()
        {   ViewBag.ProductList = new SelectList(logidb.Products, "ProductID", "ProductName");
            ViewBag.WarehouseList = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName");


            return View(new TransferViewModel() { TransferDate = DateTime.Now});
        }
        [HttpPost]
        public ActionResult AddTransferRequest(TransferViewModel model)
        {
            if (ModelState.IsValid)
            {
                var productexist = logidb.WarehouseStocks.FirstOrDefault(ws => ws.ProductID == model.ProductID && ws.WarehouseID == model.SourceWarehouseID);
                if (productexist != null)
                {
                    ModelState.AddModelError("Quantity", "Ürün stokta bulunmamaktadır.");
                    return View(model);
                }
                logidb.WarehouseTransfers.Add(
                    new Models.WarehouseTransfers()
                    {
                        TransferDate = model.TransferDate,
                        TransferStatus = model.TransferStatus,
                        SourceWarehouseID = model.SourceWarehouseID,
                        DestinationWarehouseID = model.DestinationWarehouseID,
                        ProductID = model.ProductID,
                        Quantity = model.Quantity
                    }
                );
                logidb.SaveChanges();

                return RedirectToAction("TransferRequests", new { message = " başarıyla eklendi." });
            }
            return View(model);
        }

        public ActionResult PurchaseRequests()
        {
            return View();
        }
        

    }
}

         
        