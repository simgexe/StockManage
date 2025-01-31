using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LogiManage.ViewModels;
using System.Web.UI.WebControls;
using LogiManage.Models;
using System.Data.SqlClient;

namespace LogiManage.Controllers
{
    public class TransferController : Controller
    {

        LogiManageDbEntities1 logidb = new LogiManageDbEntities1();

        // GET: Transfer
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
            viewModel.OtherTransfers = new List<TransferViewModel>();
            while (outgoingDr.Read())
            {
                viewModel.OtherTransfers.Add(ReadTransfer(outgoingDr));
            }
            outgoingDr.Close();


            SqlCommand incomingCommand = new SqlCommand(incomingQuery, connection);
            incomingCommand.Parameters.AddWithValue("@warehouseId", (int)Session["WarehouseID"]);
            SqlDataReader incomingDr = incomingCommand.ExecuteReader();
            viewModel.Transfers = new List<TransferViewModel>();
            while (incomingDr.Read())
            {
                viewModel.Transfers.Add(ReadTransfer(incomingDr));
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
        {
            ViewBag.WarehouseList1 = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName");
            ViewBag.WarehouseList2 = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName");
            ViewBag.ProductList = new SelectList(logidb.Products, "ProductID", "ProductName");

            return View(new TransferViewModel() { TransferDate = DateTime.Now, DestinationWarehouseName = Session["WarehouseName"].ToString() });
        }
        [HttpPost]
        public ActionResult AddTransferRequest(TransferViewModel addTransferRequest)
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
                    cmd.Parameters.AddWithValue("@TransferStatus", "Requested");

                    cmd.ExecuteNonQuery();
                }

            }
            return RedirectToAction("TransferRequests");
        }
        public ActionResult ActionResult(int transferid)
        {

            var viewModel = new TransferViewModel();
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
                                    where wt.TransferID = @id";
            connection.Open();
            SqlCommand requestcommand = new SqlCommand(request, connection);
            requestcommand.Parameters.AddWithValue("@id", transferid);
            SqlDataReader requestDr = requestcommand.ExecuteReader();
            while (requestDr.Read())
            {
                viewModel.TransferID = (int)requestDr["TransferID"];
                viewModel.TransferDate = (DateTime)requestDr["TransferDate"];
                viewModel.TransferStatus = (string)requestDr["TransferStatus"];
                viewModel.SourceWarehouseID = (int)requestDr["SourceWarehouseID"];
                viewModel.SourceWarehouseName = (string)requestDr["SourceWarehouseName"];
                viewModel.DestinationWarehouseID = (int)requestDr["DestinationWarehouseID"];
                viewModel.DestinationWarehouseName = (string)requestDr["DestinationWarehouseName"];
                viewModel.ProductID = (int)requestDr["ProductID"];
                viewModel.ProductName = (string)requestDr["ProductName"];
                viewModel.Quantity = (int)requestDr["Quantity"];
            }
            requestDr.Close();
            connection.Close();
            return View(viewModel);
        }
        public ActionResult AcceptTransfer(int transferid)
        {
            var viewModel = new TransferViewModel();
            SqlConnection connection = new SqlConnection("Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True");
            var request = @"Update WarehouseTransfers set TransferStatus = 'Preparing' where TransferID = @id";
            connection.Open();
            SqlCommand requestcommand = new SqlCommand(request, connection);
            requestcommand.Parameters.AddWithValue("@id", transferid);
            requestcommand.ExecuteNonQuery();
            connection.Close();
            return RedirectToAction("TransferRequests");
        }
        public ActionResult RejectTransfer(int transferid)
        { 
            var viewModel = new TransferViewModel();
            SqlConnection connection = new SqlConnection(
                "Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True");
            var request = @"Update WarehouseTransfers set TransferStatus = 'Rejected' where TransferID = @id";
            connection.Open();
            SqlCommand requestcommand = new SqlCommand(request, connection);
            requestcommand.Parameters.AddWithValue("@id", transferid);
            requestcommand.ExecuteNonQuery();
            connection.Close();
            return View(viewModel);


        }
        public ActionResult CompletedTransfer(TransferViewModel completetransfer,int transferid)
           
        { 
            completetransfer.SourceWarehouseID = (int) Session["WarehouseID"];
            using (SqlConnection connection = new SqlConnection("Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True"))
            {
                connection.Open();

                var updateTransferStatus = @"Update WarehouseTransfers set TransferStatus = 'Completed' where TransferID = @id";
                using (SqlCommand command = new SqlCommand(updateTransferStatus, connection))
                {
                    command.Parameters.AddWithValue("@id", transferid);
                    command.ExecuteNonQuery();
                }

                var updateStockQuantity = @"Update WarehouseStocks SET 
                                            WarehouseStocks.Quantity = WarehouseStocks.Quantity - WarehouseTransfers.Quantity   
                                            FROM WarehouseStocks 
                                            JOIN WarehouseTransfers ON WarehouseStocks.WarehouseID = WarehouseTransfers.SourceWarehouseID
                                            WHERE WarehouseTransfers.TransferID = @transferid";
                using (SqlCommand quantityCommand = new SqlCommand(updateStockQuantity, connection))
                {
                    quantityCommand.Parameters.AddWithValue("@warehouseId", (int)Session["WarehouseID"]);
                    quantityCommand.Parameters.AddWithValue("@transferid", transferid);
                    quantityCommand.ExecuteNonQuery();
                }
                connection.Close();
            }
            return RedirectToAction("TransferRequests");
           
        }
        


    }
}