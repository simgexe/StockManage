using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        public ActionResult GetTransferByStatus(string status)
        {
            if (Session["WarehouseID"] == null)
            {
                return RedirectToAction("Login", "Account"); 
            }

            int warehouseID = Convert.ToInt32(Session["WarehouseID"]);
            var transferList = new List<TransferViewModel>();

            string query = @"
                SELECT wt.TransferID, wt.TransferDate, wt.TransferStatus, 
                       wt.SourceWarehouseID, sw.WarehouseName AS SourceWarehouseName, 
                       wt.DestinationWarehouseID, dw.WarehouseName AS DestinationWarehouseName, 
                       wt.ProductID, p.ProductName, wt.Quantity
                FROM WarehouseTransfers wt
                JOIN Warehouses sw ON wt.SourceWarehouseID = sw.WarehouseID
                JOIN Warehouses dw ON wt.DestinationWarehouseID = dw.WarehouseID
                JOIN Products p ON wt.ProductID = p.ProductID
                WHERE wt.TransferStatus = @status AND wt.SourceWarehouseID = @warehouseID";

            using (SqlConnection connection = new SqlConnection("Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True"))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@status", status);
                    command.Parameters.AddWithValue("@warehouseID", warehouseID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transferList.Add(ReadTransfer(reader));
                        }
                    }
                }
            }
            return View(transferList);
        }
        public ActionResult DeliveredTransfers()
        {
            return GetTransferByStatus("Delivered");
        }
        public ActionResult PreparingTransfers()
        {
            return GetTransferByStatus("Preparing");
        }
        public ActionResult ORejectedTransfers()
        {
            return GetTransferByStatus("ORejected");
        }
        public ActionResult ORequestedTransfers()
        {
            return GetTransferByStatus("ORequested");
        }
        public ActionResult RequestedTransfers()
        {
            return GetTransferByStatus("Requested");
        }
        public ActionResult CompletedTransfers()
        {
            return GetTransferByStatus("Completed");
        }
        public ActionResult UncompletedTransfers()
        {
            return GetTransferByStatus("Uncompleted");
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



        public ActionResult OTransfers()
        {
            var OTransferModel = new TransferListViewModel();
            var outgoingQuery = @"Select wt.TransferID,
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
                                where  wt.SourceWarehouseID = @warehouseID";

            var incomingQuery = @"Select wt.TransferID,
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
                                where  wt.DestinationWarehouseID = @warehouseID";

            using (SqlConnection connection = new SqlConnection("Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True"))
            {
                connection.Open();

                using (SqlCommand outgoingCommand = new SqlCommand(outgoingQuery, connection))
                {
                    outgoingCommand.Parameters.AddWithValue("@warehouseId", (int)Session["WarehouseID"]);
                    using (SqlDataReader outgoingDr = outgoingCommand.ExecuteReader())
                    {
                        OTransferModel.OtherTransfers = new List<TransferViewModel>();
                        while (outgoingDr.Read())
                        {
                            OTransferModel.OtherTransfers.Add(ReadTransfer(outgoingDr));
                        }
                    }
                }

                using (SqlCommand incomingCommand = new SqlCommand(incomingQuery, connection))
                {
                    incomingCommand.Parameters.AddWithValue("@warehouseId", (int)Session["WarehouseID"]);
                    using (SqlDataReader incomingDr = incomingCommand.ExecuteReader())
                    {
                        OTransferModel.Transfers = new List<TransferViewModel>();
                        while (incomingDr.Read())
                        {
                            OTransferModel.Transfers.Add(ReadTransfer(incomingDr));
                        }
                    }
                }
            }

            return View(OTransferModel);
        }

        [HttpGet]
        public ActionResult AddTransferORequest()
        {
            ViewBag.WarehouseList1 = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName");
            ViewBag.WarehouseList2 = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName");
            ViewBag.ProductList = new SelectList(logidb.Products, "ProductID", "ProductName");

            return View(new TransferViewModel() { TransferDate = DateTime.Now, DestinationWarehouseName = Session["WarehouseName"].ToString() });

        }

        [HttpPost]
        public ActionResult AddTransferORequest(TransferViewModel addTransferRequest)
        {
            addTransferRequest.DestinationWarehouseID = Convert.ToInt32(Session["WarehouseID"]);

            var insertquery = @"Insert into WarehouseTransfers (SourceWarehouseID, DestinationWarehouseID, ProductID, Quantity, TransferDate, TransferStatus)
                                values (@SourceWarehouseID, @DestinationWarehouseID, @ProductID, @Quantity, @TransferDate, @TransferStatus)";

            using (SqlConnection connection = new SqlConnection("Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True"))
            {
                using (SqlCommand cmd = new SqlCommand(insertquery, connection))
                {
                    connection.Open();
                    cmd.Parameters.AddWithValue("@SourceWarehouseID", addTransferRequest.SourceWarehouseID);
                    cmd.Parameters.AddWithValue("@DestinationWarehouseID", addTransferRequest.DestinationWarehouseID);
                    cmd.Parameters.AddWithValue("@ProductID", addTransferRequest.ProductID);
                    cmd.Parameters.AddWithValue("@Quantity", addTransferRequest.Quantity);
                    cmd.Parameters.AddWithValue("@TransferDate", addTransferRequest.TransferDate);
                    cmd.Parameters.AddWithValue("@TransferStatus", "ORequested");

                    cmd.ExecuteNonQuery();
                }

            }
            return RedirectToAction("TransferORequest");
            ;
        }

        public ActionResult TransferORequest()
        {
            var viewModell = new TransferListViewModel();
            var request = @"Select wt.TransferID,
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
                                where wt.SourceWarehouseID = @warehouseID";

            using (SqlConnection connection = new SqlConnection("Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True"))
            {
                connection.Open();
                using (SqlCommand requestcommand = new SqlCommand(request, connection))
                {
                    requestcommand.Parameters.AddWithValue("@warehouseId", (int)Session["WarehouseID"]);
                    using (SqlDataReader requestDr = requestcommand.ExecuteReader())
                    {
                        viewModell.TransferRequests = new List<TransferViewModel>();
                        while (requestDr.Read())
                        {
                            viewModell.TransferRequests.Add(ReadTransfer(requestDr));
                        }
                    }
                }
            }

            return View(viewModell);

        }
        public ActionResult SaveDeliveredTransfer(int transferid)
        {
            string query = @"UPDATE WarehouseTransfers SET TransferStatus = 'Delivered' WHERE TransferID = @id";

            using (SqlConnection connection = new SqlConnection("Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True"))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", transferid);
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToAction("TransferORequests");

        }

    }
}