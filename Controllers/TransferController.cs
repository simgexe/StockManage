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
                                    where wt.DestinationWarehouseID = @warehouseID";
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

            return View(new TransferViewModel() { TransferDate = DateTime.Now , SourceWarehouseName = Session["WarehouseName"].ToString() });
        }
        [HttpPost]
        public ActionResult AddTransferRequest(TransferViewModel addTransferRequest)
        {
            
            if (ModelState.IsValid)
            {
                var productexist = logidb.WarehouseStocks.FirstOrDefault(ws => ws.ProductID == addTransferRequest.ProductID && ws.WarehouseID == addTransferRequest.DestinationWarehouseID);
                if (productexist == null || productexist.Quantity < addTransferRequest.Quantity)
                {
                    ModelState.AddModelError("Quantity", "Ürün stokta bulunmamaktadır veya yeterli miktar yoktur.");
                    ViewBag.WarehouseList1 = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName");
                    ViewBag.WarehouseList2 = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName");
                    ViewBag.ProductList = new SelectList(logidb.Products, "ProductID", "ProductName");

                    return View(addTransferRequest);
                }
                logidb.WarehouseTransfers.Add(
                    new Models.WarehouseTransfers()
                    {

                        SourceWarehouseID = addTransferRequest.SourceWarehouseID,
                        DestinationWarehouseID = addTransferRequest.DestinationWarehouseID,
                        ProductID = addTransferRequest.ProductID,
                        Quantity = addTransferRequest.Quantity,
                        TransferDate = addTransferRequest.TransferDate,
                        TransferStatus = addTransferRequest.TransferStatus

                    }
                );
                logidb.SaveChanges();

                return RedirectToAction("TransferRequests", new { message = " başarıyla eklendi." });
            }
            ViewBag.WarehouseList1 = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName");
            ViewBag.WarehouseList2 = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName");
            ViewBag.ProductList = new SelectList(logidb.Products, "ProductID", "ProductName");
            return View(addTransferRequest);
        }


    }
}