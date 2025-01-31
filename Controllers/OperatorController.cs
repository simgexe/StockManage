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
                                    where  wt.SourceWarehouseID = @warehouseID";

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
                                    where  wt.DestinationWarehouseID = @warehouseID";

            connection.Open();

            SqlCommand outgoingCommand = new SqlCommand(outgoingQuery, connection);
            outgoingCommand.Parameters.AddWithValue("@warehouseId", (int)Session["WarehouseID"]);
            SqlDataReader outgoingDr = outgoingCommand.ExecuteReader();
            OTransferModel.OtherTransfers = new List<TransferViewModel>();
            while (outgoingDr.Read())
            {
                OTransferModel.OtherTransfers.Add(ReadTransfer(outgoingDr));
            }
            outgoingDr.Close();


            SqlCommand incomingCommand = new SqlCommand(incomingQuery, connection);
            incomingCommand.Parameters.AddWithValue("@warehouseId", (int)Session["WarehouseID"]);
            SqlDataReader incomingDr = incomingCommand.ExecuteReader();
            OTransferModel.Transfers = new List<TransferViewModel>();
            while (incomingDr.Read())
            {
                OTransferModel.Transfers.Add(ReadTransfer(incomingDr));
            }
            incomingDr.Close();

            connection.Close();

            return View(OTransferModel);
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
                return RedirectToAction("TransferRequests");
            }

           public ActionResult TransferORequest()
        {
            var viewModell = new TransferListViewModel();
            SqlConnection connection = new SqlConnection("Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True");
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
                                    where wt.TransferStatus ='Requested' and wt.SourceWarehouseID = @warehouseID";
            connection.Open();
            SqlCommand requestcommand = new SqlCommand(request, connection);
            requestcommand.Parameters.AddWithValue("@warehouseId", (int)Session["WarehouseID"]);
            SqlDataReader requestDr = requestcommand.ExecuteReader();

            viewModell.TransferRequests = new List<TransferViewModel>();
            while (requestDr.Read())
            {
                viewModell.TransferRequests.Add(ReadTransfer(requestDr));
            }
            requestDr.Close();

            connection.Close();

            return View(viewModell);
           
        }
        public ActionResult DeliveredTransfer(int transferid)
        {
            var viewModell = new TransferViewModel();
            SqlConnection connection = new SqlConnection("Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True");
            var request = @"Update WarehouseTransfers set TransferStatus = 'Delivered' where TransferID = @id";
            connection.Open();
            SqlCommand requestcommand = new SqlCommand(request, connection);
            requestcommand.Parameters.AddWithValue("@id", transferid);
            requestcommand.ExecuteNonQuery();
            connection.Close();
            return View(viewModell);
        }
        

    }

    }
