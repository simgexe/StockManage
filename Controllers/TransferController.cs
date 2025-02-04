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
        public List<TransferViewModel> GetTransfers()
        {
            var warehouseId = 0;
            if (int.TryParse(Session["WarehouseID"].ToString(), out int warehouseIdKontrol))
                warehouseId = warehouseIdKontrol;

            var transferList = from wt in logidb.WarehouseTransfers
                               join p in logidb.Products on wt.ProductID equals p.ProductID
                               join sw in logidb.Warehouses on wt.SourceWarehouseID equals sw.WarehouseID
                               join dw in logidb.Warehouses on wt.DestinationWarehouseID equals dw.WarehouseID
                               select new TransferViewModel
                               {
                                   TransferID = wt.TransferID,
                                   SourceWarehouseID = sw.WarehouseID,
                                   DestinationWarehouseID = dw.WarehouseID,
                                   ProductID = wt.ProductID ?? 0,
                                   Quantity = wt.Quantity ?? 0,
                                   TransferDate = wt.TransferDate ?? DateTime.Now,
                                   TransferStatus = wt.TransferStatus,
                                   ProductName = p.ProductName,
                                   SourceWarehouseName = sw.WarehouseName,
                                   DestinationWarehouseName = dw.WarehouseName
                               };
            return transferList.ToList();
        }
        public ActionResult Transfers()
        {
           /* var viewModel = new TransferListViewModel();
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

            return View(viewModel);*/
           return View(GetTransfers());
        }
        public ActionResult TransferRequests()
        {
            return View(GetTransfers());
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
        
        
    
        
    // ------------------BUTONLAR-----------------------------------------------------------------
        public ActionResult AcceptO(int transferid)
        {
            var transfer = logidb.WarehouseTransfers.FirstOrDefault(t => t.TransferID == transferid);
            if (transfer != null && transfer.TransferStatus == "ORequested")
            {
                transfer.TransferStatus = "Requested";
                logidb.SaveChanges();
            }
            return RedirectToAction("TransferRequests");
        }  //Requested yapmak için operatörden gelen istekleri

        public ActionResult RejectO(int transferid)
        {
            var transfer = logidb.WarehouseTransfers.FirstOrDefault(t => t.TransferID == transferid);
            if (transfer != null && transfer.TransferStatus == "ORequested")
            {
                transfer.TransferStatus = "ORejected";
                logidb.SaveChanges();
            }
            return RedirectToAction("TransferRequests");
        }  //ORejected yapmak için

        public ActionResult Completed(TransferViewModel completetransfer, int transferId)
        {
            completetransfer.SourceWarehouseID = (int)Session["WarehouseID"];
            using (SqlConnection connection = new SqlConnection("Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True"))
            {
                connection.Open();

                var updateTransferStatus = @"Update WarehouseTransfers set TransferStatus = 'Completed' where TransferID = @id";
                using (SqlCommand command = new SqlCommand(updateTransferStatus, connection))
                {
                    command.Parameters.AddWithValue("@id", transferId);
                    command.ExecuteNonQuery();
                }

                var updateSourceStockQuantity = @"Update WarehouseStocks SET 
                                            WarehouseStocks.Quantity = WarehouseStocks.Quantity + WarehouseTransfers.Quantity   
                                            FROM WarehouseStocks 
                                            JOIN WarehouseTransfers ON WarehouseStocks.WarehouseID = WarehouseTransfers.SourceWarehouseID
                                            WHERE WarehouseTransfers.TransferID = @transferid";
                var updateDestinationStockQuantity = @"Update WarehouseStocks SET 
                                            WarehouseStocks.Quantity = WarehouseStocks.Quantity-WarehouseTransfers.Quantity   
                                            FROM WarehouseStocks 
                                            JOIN WarehouseTransfers ON WarehouseStocks.WarehouseID = WarehouseTransfers.DestinationWarehouseID
                                            WHERE WarehouseTransfers.TransferID = @transferid";
                using (SqlCommand quantityCommand = new SqlCommand(updateSourceStockQuantity, connection))
                {
                    quantityCommand.Parameters.AddWithValue("@warehouseId", (int)Session["WarehouseID"]);
                    quantityCommand.Parameters.AddWithValue("@transferid", transferId);
                    quantityCommand.ExecuteNonQuery();
                }
                using (SqlCommand quantityCommand = new SqlCommand(updateDestinationStockQuantity, connection))
                {
                    quantityCommand.Parameters.AddWithValue("@warehouseId", (int)Session["WarehouseID"]);
                    quantityCommand.Parameters.AddWithValue("@transferid", transferId);
                    quantityCommand.ExecuteNonQuery();
                }
                connection.Close();
            }
            return RedirectToAction("TransferRequests");
        }  //Completed yapmak için. quantityde arttırma azaltma işlemleri
        public ActionResult Uncompleted(int transferId)
        {
            var transfer = logidb.WarehouseTransfers.FirstOrDefault(t => t.TransferID == transferId);
            if (transfer != null && transfer.TransferStatus == "Delivered")
            {
                transfer.TransferStatus = "Uncompleted";
                logidb.SaveChanges();
            }
            return RedirectToAction("TransferRequests");
        }
        public ActionResult Accept(int transferId)
        {
            var transfer = logidb.WarehouseTransfers.FirstOrDefault(t => t.TransferID == transferId);
            if (transfer != null && transfer.TransferStatus == "Requested")
            {
                transfer.TransferStatus = "Preparing";
                logidb.SaveChanges();
            }
            return RedirectToAction("TransferRequests");
        }  //Preparing yapmak için baska warehousetan gelen istekleri
        public ActionResult Reject(int transferId)
        {
            var transfer = logidb.WarehouseTransfers.FirstOrDefault(t => t.TransferID == transferId);
            if (transfer != null && transfer.TransferStatus == "ORequested")
            {
                transfer.TransferStatus = "Rejected";
                logidb.SaveChanges();
            }
            return RedirectToAction("TransferRequests");
        }  //Rejected yapmak için
    } }